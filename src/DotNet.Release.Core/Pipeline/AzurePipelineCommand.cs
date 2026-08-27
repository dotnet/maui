namespace DotNet.Release.Core;

/// <summary>
/// Formats Azure Pipelines logging commands.
/// </summary>
/// <remarks>
/// <para>
/// The tool emits exactly <b>two</b> pipeline variables, and no others:
/// </para>
/// <list type="number">
/// <item><see cref="BarIdVariable"/>, from <c>release plan</c>, because the
/// <c>darc gather-drop</c> step that follows needs the BAR ID that step discovered.</item>
/// <item><see cref="PackagesToPublishVariable"/>, from <c>release filter</c>, because the
/// publish task is skipped by an Azure DevOps <c>condition</c>, and a condition can only
/// read a variable.</item>
/// </list>
/// <para>
/// The intent was one. The second is unavoidable rather than convenient: the alternative is
/// to hand <c>1ES.PublishNuget@1</c> an empty directory, and its glob failing on zero
/// matches is a worse outcome than one extra variable. Everything else — channel, feed,
/// package identities, hashes — travels in the plan file. The current pipeline sets four
/// output variables and threads them through <c>stageDependencies</c> expressions, which is
/// why so many of its steps restate the same values.
/// </para>
/// <para>
/// The plan's own hash is deliberately <i>not</i> emitted here. The pipeline computes it
/// with <c>Get-FileHash</c> rather than trusting the tool to declare the value that pins it.
/// </para>
/// </remarks>
public static class AzurePipelineCommand
{
    /// <summary>The one variable this tool sets.</summary>
    public const string BarIdVariable = "BarId";

    /// <summary>The variable the publish task's condition reads to skip an empty push.</summary>
    public const string PackagesToPublishVariable = "NuGetPackagesToPublish";

    /// <summary>Formats a <c>task.setvariable</c> logging command.</summary>
    public static string SetVariable(string name, string value, bool isOutput = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        var flags = isOutput ? ";isOutput=true" : string.Empty;
        return $"##vso[task.setvariable variable={name}{flags}]{Escape(value)}";
    }

    /// <summary>Formats the <c>BarId</c> output variable emitted by <c>release plan</c>.</summary>
    public static string SetBarId(int barBuildId) =>
        SetVariable(BarIdVariable, barBuildId.ToString(System.Globalization.CultureInfo.InvariantCulture), isOutput: true);

    /// <summary>Formats the flag the publish task's condition reads.</summary>
    public static string SetPackagesToPublish(bool hasPackages) =>
        SetVariable(PackagesToPublishVariable, hasPackages ? "true" : "false");

    /// <summary>
    /// Escapes characters Azure Pipelines would otherwise treat as command syntax.
    /// </summary>
    private static string Escape(string value) => value
        .Replace("%", "%AZP25", StringComparison.Ordinal)
        .Replace("\r", "%0D", StringComparison.Ordinal)
        .Replace("\n", "%0A", StringComparison.Ordinal)
        .Replace("]", "%5D", StringComparison.Ordinal)
        .Replace(";", "%3B", StringComparison.Ordinal);
}
