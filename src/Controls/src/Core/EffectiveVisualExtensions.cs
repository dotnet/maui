#nullable disable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>Extension methods for checking visual type.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class EffectiveVisualExtensions
	{
		/// <param name="visual">The visual parameter.</param>
		public static bool IsDefault(this IVisual visual) => visual == VisualMarker.Default;
		/// <param name="visual">The visual parameter.</param>
		public static bool IsMatchParent(this IVisual visual) => visual == VisualMarker.MatchParent;
		/// <param name="visual">The visual parameter.</param>
		public static bool IsMaterial(this IVisual visual) => false; // visual == VisualMarker.Material;
	}
}