#nullable enable

using System;
using System.Linq;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;

/// <summary>
/// Reads the branches of a generated <c>UpdateComponent()</c>. Each live instance records the generation
/// it is at, and the patch picks the branch diffed from that generation; instances at an unrecorded
/// generation get the default branch, diffed from the previous generation.
/// </summary>
static class UpdateComponentText
{
	const string Guard = "if (__xihrFrom != ";
	const string Case = "if (__xihrFrom == ";

	/// <summary>The patch from the previous generation, applied to instances without a matching branch.</summary>
	public static string DefaultPatch(string updateComponent)
	{
		var lines = updateComponent.Split('\n').Select(line => line.TrimEnd('\r')).ToArray();
		var guard = Array.FindIndex(lines, line => line.TrimStart('\t').StartsWith(Guard, StringComparison.Ordinal));
		if (guard < 0)
			return updateComponent;
		var branchIndent = Indent(lines[guard]) + 1;
		var end = Array.FindIndex(lines, guard, line => line.Contains("XamlComponentRegistry.Register(this, \"__xihr_generation:", StringComparison.Ordinal));
		var start = Array.FindLastIndex(lines, end, end - guard, line => Indent(line) == branchIndent && line.Trim() == "else");
		return string.Join("\n", lines[((start < 0 ? guard : start) + 1)..end]);
	}

	/// <summary>The number of branches for instances at a recorded older generation.</summary>
	public static int BaselineCaseCount(string updateComponent) =>
		updateComponent.Split(Case).Length - 1;

	static int Indent(string line) => line.Length - line.TrimStart('\t').Length;
}
