// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Diagnostic IDs for the Microsoft.Maui.Essentials.AI package.
/// </summary>
internal static class DiagnosticIds
{
	/// <summary>
	/// Experimental API diagnostic IDs.
	/// </summary>
	/// <remarks>
	/// All Essentials.AI experiments share a single diagnostic ID so consumers
	/// only need one suppression to opt in: <c>&lt;NoWarn&gt;MAUIAI0001&lt;/NoWarn&gt;</c>.
	/// Individual constants exist per feature area so that APIs can be graduated
	/// to stable independently in the future by assigning distinct IDs.
	/// </remarks>
	internal static class Experiments
	{
		internal const string EssentialsAI = "MAUIAI0001";
	}
}
