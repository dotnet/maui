using System;
using System.ComponentModel;

namespace Xamarin.Forms
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class EffectiveVisualExtensions
	{
		public static bool IsDefault(this IVisual visual) => visual == VisualMarker.Default;
		public static bool IsMatchParent(this IVisual visual) => visual == VisualMarker.MatchParent;
		public static bool IsMaterial(this IVisual visual) => visual == VisualMarker.Material;
	}
}