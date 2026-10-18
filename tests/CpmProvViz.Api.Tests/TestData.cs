namespace CpmProvViz.Api.Tests;

/// <summary>Files in the repository's <c>testdata/</c> folder.</summary>
internal static class TestData
{
    public static string Root { get; } = FindRoot();

    public static string PathOf(string relativePath) => Path.Combine(Root, relativePath);

    private static string FindRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "CpmProvViz.slnx")))
            {
                return Path.Combine(dir.FullName, "testdata");
            }
        }

        throw new InvalidOperationException("Could not find the repository root (CpmProvViz.slnx).");
    }
}
