#nullable enable
using System;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Test seam: raised each time a platform transform is rebuilt from a view's transform properties (iOS, Mac Catalyst
	/// and Windows rebuild the whole transform per update), so tests can verify that one animation frame rebuilds it once.
	/// </summary>
	internal static class TransformationDiagnostics
	{
		internal static Action<IView>? Rebuilt { get; set; }

		internal static void OnRebuilt(IView view) => Rebuilt?.Invoke(view);
	}
}
