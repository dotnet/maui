#nullable enable
#pragma warning disable CA2255

using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

internal static class ReplicationWindowsDeviceTestClassFilter
{
	const string ArgumentPrefix = "--maui-replication-include-class=";
	static readonly Regex AllowedClass = new(
		@"^Microsoft\.Maui\.DeviceTests\.[A-Za-z_][A-Za-z0-9_]{0,255}$",
		RegexOptions.CultureInvariant);

	[ModuleInitializer]
	internal static void Initialize()
	{
		string? selectedClass = null;
		foreach (var argument in Environment.GetCommandLineArgs())
		{
			if (!argument.StartsWith(ArgumentPrefix, StringComparison.Ordinal))
				continue;

			if (selectedClass is not null)
				throw new InvalidOperationException("The packaged device-test class selector was supplied more than once.");

			selectedClass = argument.Substring(ArgumentPrefix.Length);
		}

		if (selectedClass is null)
			return;

		if (!AllowedClass.IsMatch(selectedClass) ||
			Regex.IsMatch(selectedClass, @"\.Issue[1-9][0-9]*(?:Tests)?$", RegexOptions.CultureInvariant))
		{
			throw new InvalidOperationException("The packaged device-test class selector is invalid.");
		}

		// XHarness treats this legacy option as a class inclusion filter.
		Environment.SetEnvironmentVariable("NUNIT_SKIPPED_CLASSES", selectedClass);
		Console.WriteLine("[MAUI replication] Packaged device-test class filter: " + selectedClass);
	}
}
