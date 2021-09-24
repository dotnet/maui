using System.Collections.Generic;
using Microsoft.UI.Xaml;
using WSize = Windows.Foundation.Size;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public delegate SizeRequest? GetDesiredSizeDelegate(NativeViewWrapperRenderer renderer, double widthConstraint, double heightConstraint);

	public delegate WSize? ArrangeOverrideDelegate(NativeViewWrapperRenderer renderer, WSize finalSize);

	public delegate WSize? MeasureOverrideDelegate(NativeViewWrapperRenderer renderer, WSize availableSize);

	public static class LayoutExtensions
	{
		public static void Add(this IList<View> children, FrameworkElement view, GetDesiredSizeDelegate getDesiredSizeDelegate = null, ArrangeOverrideDelegate arrangeOverrideDelegate = null,
							   MeasureOverrideDelegate measureOverrideDelegate = null)
		{
			children.Add(view.ToView(getDesiredSizeDelegate, arrangeOverrideDelegate, measureOverrideDelegate));
		}

		public static View ToView(this FrameworkElement view, GetDesiredSizeDelegate getDesiredSizeDelegate = null, ArrangeOverrideDelegate arrangeOverrideDelegate = null,
								  MeasureOverrideDelegate measureOverrideDelegate = null)
		{
			return new NativeViewWrapper(view, getDesiredSizeDelegate, arrangeOverrideDelegate, measureOverrideDelegate);
		}
	}
}