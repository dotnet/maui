using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/EffectiveVisualExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.EffectiveVisualExtensions']/Docs" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class EffectiveVisualExtensions
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/EffectiveVisualExtensions.xml" path="//Member[@MemberName='IsDefault']/Docs" />
		public static bool IsDefault(this IVisual visual) => visual == VisualMarker.Default;
		/// <include file="../../docs/Microsoft.Maui.Controls/EffectiveVisualExtensions.xml" path="//Member[@MemberName='IsMatchParent']/Docs" />
		public static bool IsMatchParent(this IVisual visual) => visual == VisualMarker.MatchParent;
		/// <include file="../../docs/Microsoft.Maui.Controls/EffectiveVisualExtensions.xml" path="//Member[@MemberName='IsMaterial']/Docs" />
		public static bool IsMaterial(this IVisual visual) => visual == VisualMarker.Material;
	}
}