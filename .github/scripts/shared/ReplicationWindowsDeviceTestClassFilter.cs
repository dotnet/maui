#nullable enable
#pragma warning disable CA2255

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.DotNet.XHarness.TestRunners.Common;

internal static class ReplicationWindowsDeviceTestClassFilter
{
	const string ArgumentPrefix = "--maui-replication-include-class=";
	const string ClassMetadataKey = "MauiReplicationWindowsIncludeClassBase64";
	const string MethodMetadataKey = "MauiReplicationWindowsIncludeMethodBase64";
	const string ExactMethodSelectorMetadataKey = "MauiReplicationWindowsExactMethodSelector";
	static readonly Regex AllowedClass = new(
		@"^Microsoft\.Maui\.DeviceTests\.[A-Za-z_][A-Za-z0-9_]{0,255}$",
		RegexOptions.CultureInvariant);
	static readonly Regex AllowedMethod = new(
		@"^[A-Za-z_][A-Za-z0-9_]{0,255}$",
		RegexOptions.CultureInvariant);

	internal static string? SelectedClass { get; private set; }

	internal static string? SelectedMethod { get; private set; }

	internal static bool UsesExactMethodSelector { get; private set; }

	[ModuleInitializer]
	internal static void Initialize()
	{
		var metadataClass = ReadMetadataSelector(ClassMetadataKey);
		var selectedMethod = ReadMetadataSelector(MethodMetadataKey);
		var exactMethodSelector = string.Equals(
			ReadMetadataValue(ExactMethodSelectorMetadataKey),
			"true",
			StringComparison.Ordinal);
		string? commandLineClass = null;
		foreach (var argument in Environment.GetCommandLineArgs())
		{
			if (!argument.StartsWith(ArgumentPrefix, StringComparison.Ordinal))
				continue;

			if (commandLineClass is not null)
				throw new InvalidOperationException("The packaged device-test class selector was supplied more than once.");

			commandLineClass = argument.Substring(ArgumentPrefix.Length);
		}

		if (metadataClass is not null &&
			commandLineClass is not null &&
			!string.Equals(metadataClass, commandLineClass, StringComparison.Ordinal))
		{
			throw new InvalidOperationException("The packaged device-test class selectors do not match.");
		}
		var selectedClass = metadataClass ?? commandLineClass;

		if (exactMethodSelector && (selectedClass is null || selectedMethod is null))
			throw new InvalidOperationException("The exact packaged device-test selector requires both class and method metadata.");
		if (!exactMethodSelector && selectedMethod is not null)
			throw new InvalidOperationException("Packaged device-test method metadata requires exact selector mode.");
		if (selectedClass is null && selectedMethod is null)
			return;

		if (selectedClass is null ||
			!AllowedClass.IsMatch(selectedClass) ||
			Regex.IsMatch(selectedClass, @"\.Issue[1-9][0-9]*(?:Tests)?$", RegexOptions.CultureInvariant))
		{
			throw new InvalidOperationException("The packaged device-test class selector is invalid.");
		}
		if (selectedMethod is not null && !AllowedMethod.IsMatch(selectedMethod))
			throw new InvalidOperationException("The packaged device-test method selector is invalid.");

		SelectedClass = selectedClass;
		SelectedMethod = selectedMethod;
		UsesExactMethodSelector = exactMethodSelector;

		// XHarness treats this legacy option as a class inclusion filter.
		Environment.SetEnvironmentVariable("NUNIT_SKIPPED_CLASSES", selectedClass);
		Console.WriteLine("[MAUI replication] Packaged device-test class filter: " + selectedClass);
		if (selectedMethod is not null)
		{
			var fullyQualifiedMethod = selectedClass + "." + selectedMethod;
			Environment.SetEnvironmentVariable("NUNIT_SKIPPED_METHODS", fullyQualifiedMethod);
			Console.WriteLine("[MAUI replication] Packaged device-test method filter: " + fullyQualifiedMethod);
		}
		else
		{
			Environment.SetEnvironmentVariable("NUNIT_SKIPPED_METHODS", null);
		}

		RefreshApplicationOptions(selectedClass, selectedMethod);
	}

	internal static void RefreshApplicationOptions(string selectedClass, string? selectedMethod)
	{
		// Refresh the pinned XHarness singleton after installing trusted selectors so
		// selection does not depend on which consumer initialized it first.
		var options = new ApplicationOptions();
		var expectedMethod = selectedMethod is null
			? null
			: selectedClass + "." + selectedMethod;
		if (options.ClassMethodFilters.Count != 1 ||
			!string.Equals(options.ClassMethodFilters.Single(), selectedClass, StringComparison.Ordinal) ||
			options.SingleMethodFilters.Count != (expectedMethod is null ? 0 : 1) ||
			(expectedMethod is not null &&
				!string.Equals(options.SingleMethodFilters.Single(), expectedMethod, StringComparison.Ordinal)))
		{
			throw new InvalidOperationException("The packaged device-test selectors were not accepted by XHarness.");
		}

		ApplicationOptions.Current = options;
	}

	static string? ReadMetadataSelector(string key)
	{
		var encoded = ReadMetadataValue(key);
		if (encoded is null)
			return null;
		if (encoded.Length == 0)
			throw new InvalidOperationException("The packaged device-test selector metadata is empty.");

		try
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
		}
		catch (FormatException ex)
		{
			throw new InvalidOperationException("The packaged device-test selector metadata is invalid.", ex);
		}
	}

	static string? ReadMetadataValue(string key)
	{
		string? value = null;
		foreach (var attribute in typeof(ReplicationWindowsDeviceTestClassFilter)
			.Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
		{
			if (!string.Equals(attribute.Key, key, StringComparison.Ordinal))
				continue;
			if (value is not null)
				throw new InvalidOperationException("The packaged device-test selector metadata was supplied more than once.");
			value = attribute.Value;
		}
		return value;
	}
}
