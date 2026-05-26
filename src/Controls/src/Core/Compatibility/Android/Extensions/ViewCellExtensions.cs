#nullable disable
using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal static class ViewCellExtensions
	{
		public static bool IsInViewCell(this VisualElement element)
		{
			var parent = element.Parent;
			while (parent != null)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				if (parent is ViewCell)
				{
					return true;
				}
#pragma warning restore CS0618 // Type or member is obsolete
				parent = parent.Parent;
			}

			return false;
		}
	}
}