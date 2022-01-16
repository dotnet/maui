#nullable enable

#if WINDOWS || ANDROID || IOS
using System;
using System.Collections.Generic;
using System.Text;

#if WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif ANDROID
using PlatformView = Android.Views.View;
#elif IOS
using PlatformView = UIKit.UIView;
#endif

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
#if WINDOWS
	public abstract partial class ViewRenderer<TElement, TNativeView> : VisualElementRenderer<TElement, TNativeView>, INativeViewHandler
#else
	public abstract partial class ViewRenderer<TElement, TNativeView> : VisualElementRenderer<TElement>, INativeViewHandler
#endif
		where TElement : View, IView
		where TNativeView : PlatformView
	{
		public ViewRenderer(IMauiContext mauiContext) : this(mauiContext, VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{

		}

		internal ViewRenderer(IMauiContext context, IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(context, mapper, commandMapper)
		{
		}
	}
}
#endif