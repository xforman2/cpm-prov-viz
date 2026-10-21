namespace CpmProvViz.Domain.Cpm;

/// <summary>
/// The known CPM types, each a role an element plays in a provenance component's traversal
/// information.
/// </summary>
public enum CpmRole
{
    MainActivity,
    ForwardConnector,
    BackwardConnector,
    SpecForwardConnector,
    SenderAgent,
    ReceiverAgent,
    CurrentAgent,
}
