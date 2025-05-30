namespace Facepunch.AssemblySchema;

static class InternalUtility
{
    /// <summary>
    /// Removes the largest shared leading whitespace (spaces, tabs, or both) from all non-empty lines.
    /// </summary>
    public static string Unindent(this string input)
    {
        var lines = input.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();

        var minIndent = lines
            .Where(l => l.Trim().Length > 0)
            .Select(l => l.Length - l.TrimStart().Length)
            .DefaultIfEmpty(0)
            .Min();

        for (int i = 0; i < lines.Length; i++)
            if (lines[i].Length >= minIndent)
                lines[i] = lines[i].Substring(minIndent);

        return string.Join("\n", lines);
    }
}

