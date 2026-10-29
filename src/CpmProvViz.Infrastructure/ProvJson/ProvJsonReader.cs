using System.Text.Json;
using CpmProvViz.Application.Ports;
using CpmProvViz.Domain.Prov;
using Attribute = CpmProvViz.Domain.Prov.Attribute;

namespace CpmProvViz.Infrastructure.ProvJson;

/// <summary>
/// Reads PROV-JSON (W3C Member Submission, 2013).
/// </summary>
/// <remarks>
/// Temporary: PROV and CPM handling moves to the Java provenance service (CPF-Toolbox). Don't
/// extend this.
/// </remarks>
public sealed class ProvJsonReader : IProvDocumentReader
{
    private static readonly (string Section, ElementKind Kind)[] ElementSections =
    [
        ("entity", ElementKind.Entity),
        ("activity", ElementKind.Activity),
        ("agent", ElementKind.Agent),
    ];

    // The keys of each relation's two main arguments in PROV-JSON, and of its other arguments that
    // name something, in PROV-DM order. Map by key, never by position.
    private static readonly (string Section, RelationType Type, string Source, string Target, string[] Participants)[]
        RelationSections =
    [
        ("wasGeneratedBy", RelationType.WasGeneratedBy, "prov:entity", "prov:activity", []),
        ("used", RelationType.Used, "prov:activity", "prov:entity", []),
        ("wasInformedBy", RelationType.WasInformedBy, "prov:informed", "prov:informant", []),
        ("wasStartedBy", RelationType.WasStartedBy, "prov:activity", "prov:trigger", ["prov:starter"]),
        ("wasEndedBy", RelationType.WasEndedBy, "prov:activity", "prov:trigger", ["prov:ender"]),
        ("wasInvalidatedBy", RelationType.WasInvalidatedBy, "prov:entity", "prov:activity", []),
        ("wasDerivedFrom", RelationType.WasDerivedFrom, "prov:generatedEntity", "prov:usedEntity",
            ["prov:activity", "prov:generation", "prov:usage"]),
        ("wasAttributedTo", RelationType.WasAttributedTo, "prov:entity", "prov:agent", []),
        ("wasAssociatedWith", RelationType.WasAssociatedWith, "prov:activity", "prov:agent", ["prov:plan"]),
        ("actedOnBehalfOf", RelationType.ActedOnBehalfOf, "prov:delegate", "prov:responsible", ["prov:activity"]),
        ("wasInfluencedBy", RelationType.WasInfluencedBy, "prov:influencee", "prov:influencer", []),
        ("specializationOf", RelationType.SpecializationOf, "prov:specificEntity", "prov:generalEntity", []),
        ("alternateOf", RelationType.AlternateOf, "prov:alternate1", "prov:alternate2", []),
        ("hadMember", RelationType.HadMember, "prov:collection", "prov:entity", []),
        ("mentionOf", RelationType.MentionOf, "prov:specificEntity", "prov:generalEntity", ["prov:bundle"]),
    ];

    private static readonly HashSet<string> KnownSections = new[] { "prefix", "bundle" }
        .Concat(ElementSections.Select(s => s.Section))
        .Concat(RelationSections.Select(s => s.Section))
        .ToHashSet();

    private const string BlankIdentifierPrefix = "_:";

    public async Task<ProvDocument> ReadAsync(Stream input, CancellationToken cancellationToken)
    {
        JsonDocument json;
        try
        {
            json = await JsonDocument.ParseAsync(input, cancellationToken: cancellationToken);
        }
        catch (JsonException e)
        {
            throw new UnreadableDocumentException("The document is not valid JSON.", e);
        }

        using (json)
        {
            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new UnreadableDocumentException("A PROV-JSON document must be a JSON object.");
            }

            if (root.EnumerateObject().Any() && !root.EnumerateObject().Any(p => KnownSections.Contains(p.Name)))
            {
                throw new UnreadableDocumentException("The JSON object has none of the PROV-JSON sections.");
            }

            CheckSections(root);
            var namespaces = new Namespaces(ReadPrefixes(root));
            return new ProvDocument(
                namespaces.Declared,
                ReadElements(root, namespaces),
                ReadRelations(root, namespaces),
                ReadBundles(root, namespaces));
        }
    }

    private static List<Bundle> ReadBundles(JsonElement root, Namespaces documentNamespaces)
    {
        var bundles = new List<Bundle>();
        if (ObjectSection(root, "bundle") is not { } section)
        {
            return bundles;
        }

        foreach (var bundle in section.EnumerateObject())
        {
            if (bundle.Value.ValueKind != JsonValueKind.Object)
            {
                throw new UnreadableDocumentException($"The content of bundle '{bundle.Name}' must be a JSON object.");
            }

            CheckSections(bundle.Value);
            var namespaces = new Namespaces(ReadPrefixes(bundle.Value), documentNamespaces);
            bundles.Add(new Bundle(
                namespaces.Resolve(bundle.Name),
                namespaces.Declared,
                ReadElements(bundle.Value, namespaces),
                ReadRelations(bundle.Value, namespaces)));
        }

        return bundles;
    }

    private static Dictionary<string, string> ReadPrefixes(JsonElement container)
    {
        var prefixes = new Dictionary<string, string>();
        if (ObjectSection(container, "prefix") is { } section)
        {
            foreach (var declaration in section.EnumerateObject())
            {
                if (declaration.Value.ValueKind != JsonValueKind.String)
                {
                    throw new UnreadableDocumentException($"Prefix '{declaration.Name}' must map to a namespace IRI string.");
                }

                prefixes[declaration.Name] = declaration.Value.GetString()!;
            }
        }

        return prefixes;
    }

    private static List<Element> ReadElements(JsonElement container, Namespaces namespaces)
    {
        var elements = new List<Element>();
        foreach (var (sectionName, kind) in ElementSections)
        {
            if (ObjectSection(container, sectionName) is not { } section)
            {
                continue;
            }

            foreach (var record in section.EnumerateObject())
            {
                elements.Add(new Element(namespaces.Resolve(record.Name), kind, ReadAttributes(record.Value, namespaces)));
            }
        }

        return elements;
    }

    private static List<Relation> ReadRelations(JsonElement container, Namespaces namespaces)
    {
        var relations = new List<Relation>();
        foreach (var (sectionName, type, sourceKey, targetKey, participantKeys) in RelationSections)
        {
            if (ObjectSection(container, sectionName) is not { } section)
            {
                continue;
            }

            foreach (var record in section.EnumerateObject())
            {
                // Blank identifiers carry no semantics in PROV-JSON and should be ignored.
                var id = record.Name.StartsWith(BlankIdentifierPrefix, StringComparison.Ordinal)
                    ? null
                    : namespaces.Resolve(record.Name);
                var source = Argument(record.Value, sourceKey, namespaces);
                var target = Argument(record.Value, targetKey, namespaces);
                var participants = participantKeys
                    .Select(key => Argument(record.Value, key, namespaces) is { } named
                        ? new Participant(namespaces.Resolve(key), named)
                        : null)
                    .OfType<Participant>()
                    .ToList();

                // An argument that is not read as a reference stays visible as an attribute.
                var readArguments = new List<string>();
                if (source is not null)
                {
                    readArguments.Add(sourceKey);
                }

                if (target is not null)
                {
                    readArguments.Add(targetKey);
                }

                readArguments.AddRange(participants.Select(p => p.Name.Text));
                relations.Add(new Relation(
                    type,
                    id,
                    source,
                    target,
                    participants,
                    ReadAttributes(record.Value, namespaces, except: readArguments.ToArray())));
            }
        }

        return relations;
    }

    private static QualifiedName? Argument(JsonElement record, string key, Namespaces namespaces) =>
        record.ValueKind == JsonValueKind.Object
        && record.TryGetProperty(key, out var value)
        && value.ValueKind == JsonValueKind.String
            ? namespaces.Resolve(value.GetString()!)
            : null;

    private static List<Attribute> ReadAttributes(
        JsonElement record, Namespaces namespaces, params string[] except)
    {
        var attributes = new List<Attribute>();
        if (record.ValueKind != JsonValueKind.Object)
        {
            return attributes;
        }

        foreach (var attribute in record.EnumerateObject())
        {
            if (except.Contains(attribute.Name))
            {
                continue;
            }

            // A key may hold one value or an array of values.
            IEnumerable<JsonElement> items = attribute.Value.ValueKind == JsonValueKind.Array
                ? attribute.Value.EnumerateArray()
                : [attribute.Value];
            attributes.Add(new Attribute(
                namespaces.Resolve(attribute.Name),
                items.Select(item => ReadValue(item, namespaces)).OfType<AttributeValue>().ToList()));
        }

        return attributes;
    }

    /// <summary>
    /// Reads a string, number or boolean, or a typed or language-tagged literal <c>{"$": …}</c>.
    /// Returns <c>null</c> for any other shape.
    /// </summary>
    private static AttributeValue? ReadValue(JsonElement value, Namespaces namespaces) => value.ValueKind switch
    {
        JsonValueKind.Object when value.TryGetProperty("$", out var literal) && IsScalar(literal) =>
            ReadLiteral(value, Text(literal), namespaces),
        _ when IsScalar(value) => new AttributeValue(Text(value), null, null, null),
        _ => null,
    };

    private static AttributeValue ReadLiteral(JsonElement literal, string text, Namespaces namespaces)
    {
        var datatype = StringProperty(literal, "type") is { } type ? namespaces.Resolve(type) : null;
        var qualifiedName = datatype is not null && Datatypes.IsQualifiedName(datatype) ? namespaces.Resolve(text) : null;
        return new AttributeValue(text, datatype, StringProperty(literal, "lang"), qualifiedName);
    }

    private static bool IsScalar(JsonElement value) =>
        value.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False;

    // A string as is; a number or boolean as its JSON text.
    private static string Text(JsonElement value) =>
        value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText();

    private static string? StringProperty(JsonElement record, string key) =>
        record.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    /// <summary>Checks that every known section of a document or bundle is a JSON object.</summary>
    private static void CheckSections(JsonElement container)
    {
        foreach (var name in KnownSections)
        {
            ObjectSection(container, name);
        }
    }

    /// <summary>
    /// Returns the named section of a document or bundle, or <c>null</c> if it has none.
    /// Throws if the section is not a JSON object.
    /// </summary>
    private static JsonElement? ObjectSection(JsonElement container, string name)
    {
        if (!container.TryGetProperty(name, out var section))
        {
            return null;
        }

        return section.ValueKind == JsonValueKind.Object
            ? section
            : throw new UnreadableDocumentException($"The '{name}' section must be a JSON object.");
    }
}
