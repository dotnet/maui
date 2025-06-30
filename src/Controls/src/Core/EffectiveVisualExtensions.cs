#nullable disable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/EffectiveVisualExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.EffectiveVisualExtensions']/Docs/*" />
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