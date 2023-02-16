using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Tizen.UIExtensions.NUI;
using IMeasurable = Tizen.UIExtensions.Common.IMeasurable;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class VisualElementRenderer<TElement> : ViewGroup, IPlatformViewHandler, IMeasurable
		where TElement : Element, IView
	{
		object? IElementHandler.PlatformView => Children.Count > 0 ? Children[0] : null;

		static partial void ProcessAutoPackage(IElement element)
		{
			if (element?.Handler?.PlatformView is not ViewGroup viewGroup)
				return;

			viewGroup.Children.Clear();

			if (element is not IVisualTreeElement vte)
				return;

			var mauiContext = element?.Handler?.MauiContext;
			if (mauiContext == null)
				return;

			foreach (var child in vte.GetVisualChildren())
			{
				if (child is IElement childElement)
					viewGroup.Children.Add(childElement.ToPlatform(mauiContext));
			}
		}

		public void UpdateLayout()
		{
			if (Element != null)
				this.InvalidateMeasure(Element);
		}

		TSize IMeasurable.Measure(double availableWidth, double availableHeight)
		{
			if (Children.Count > 0 && Children[0] is IMeasurable measurable)
			{
				return measurable.Measure(availableWidth, availableHeight);
			}
			else
			{
				return NaturalSize2D.ToCommon();
			}
		}
	}
}
