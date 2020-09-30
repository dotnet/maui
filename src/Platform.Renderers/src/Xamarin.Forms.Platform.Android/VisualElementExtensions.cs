using System;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Platform.Android
{
	public static class VisualElementExtensions
	{
		public static IVisualElementRenderer GetRenderer(this VisualElement self)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			IVisualElementRenderer renderer = Platform.GetRenderer(self);

			return renderer;
		}

		internal static bool UseLegacyColorManagement<T>(this T element) where T : VisualElement, IElementConfiguration<T>
		{
			// Determine whether we're letting the VSM handle the colors or doing it the old way
			// or disabling the legacy color management and doing it the old-old (pre 2.0) way
			return !element.HasVisualStateGroups()
					&& element.OnThisPlatform().GetIsLegacyColorModeEnabled();
		}


		internal static bool IsAttachedToRoot(this VisualElement Element)
		{
			var elementRenderer = Element.GetRenderer();
			if ((elementRenderer as ILifeCycleState)?.MarkedForDispose == true)
				return false;

			Page root = Element as Page;
			var parent = Element.RealParent;
			while (root == null && parent != null)
			{
				root = parent as Page;
				parent = parent?.RealParent;
			}

			while (!Application.IsApplicationOrNull(root.RealParent))
			{
				root = (Page)root.RealParent;
				if (root.GetRenderer() is ILifeCycleState lcs)
				{
					if (lcs.MarkedForDispose)
						return false;
				}
			}

			return root.RealParent != null &&
				((root.GetRenderer() as ILifeCycleState)?.MarkedForDispose != true);
		}
	}
}