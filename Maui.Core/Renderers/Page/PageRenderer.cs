using System;
using System.Collections.Generic;
using System.Text;
#if __IOS__
using NativeView = UIKit.UIView;
#elif __MACOS__
using NativeView = AppKit.NSView;
#elif MONOANDROID
using NativeView = Android.Views.View;
#elif NETCOREAPP
using NativeView = System.Windows.Controls.UserControl;
#else
using NativeView = System.Object;
#endif

namespace System.Maui.Platform
{
	public partial class PageRenderer : AbstractViewRenderer<IPage, NativeView>
	{
		public static PropertyMapper<IPage> PageRendererMapper = new PropertyMapper<IPage>(ViewRenderer.ViewMapper)
		{
		};

		public PageRenderer() : base(PageRendererMapper)
		{

		}

		public PageRenderer(PropertyMapper mapper) : base(mapper)
		{
		}
		
	}
}
