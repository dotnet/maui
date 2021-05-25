#nullable enable
using Microsoft.Maui.Graphics;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif MONOANDROID
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : IFrameworkElementHandler
	{
		public static PropertyMapper<IPage, PageHandler> PageMapper = new PropertyMapper<IPage, PageHandler>(FrameworkElementMapper)
		{
			[nameof(IPage.Title)] = MapTitle,
			[nameof(IPage.Content)] = MapContent,
			[nameof(IVisual.Background)] = MapBackground,
			Actions = {
					[nameof(IArrangeable.InvalidateMeasure)] = MapInvalidateMeasure
				}
		};

		public PageHandler() : base(PageMapper)
		{

		}

		public PageHandler(PropertyMapper? mapper = null) : base(mapper ?? PageMapper)
		{

		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return new Size(widthConstraint, heightConstraint);
		}

		public static void MapBackground(IFrameworkElementHandler handler, IPage page)
		{
			((NativeView?)handler.NativeView)?.UpdateBackground(page);
		}

		public static void MapInvalidateMeasure(IFrameworkElementHandler handler, IPage page)
		{
			((NativeView?)handler.NativeView)?.InvalidateMeasure(page);
		}
	}
}
