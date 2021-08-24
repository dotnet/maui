using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Maui.Controls.Sample
{
    public partial class XamlApp
    {
        static XamlApp()
		{
            // just replace the entire factory and then override the parts you want
            PageHandler.PageViewFactory = new MyPageFactory();


            ScrollViewHandler.FactoryMapper[nameof(ScrollViewHandler.ScrollViewFactory.CreateNativeView)] =
                (h, v) =>
                {
                    return new UIScrollView() { BackgroundColor = UIColor.Purple };
                };


            PageHandler.FactoryMapper
                .AppendToCreatedMapping<IView, PageHandler>(nameof(PageHandler.PageViewFactory.CreateNativeView),
                (h, v, r) =>
                {
                    if (r is UIView view)
                        view.BackgroundColor = UIColor.Red;

                    return r;
                });
        }

        public class MyPageFactory : PageHandler.Factory
		{
			public override PageView CreateNativeView(PageHandler pageHandler, IView scrollView)
			{
				return base.CreateNativeView(pageHandler, scrollView);
			}

			public override PageViewController CreateViewController(PageHandler pageHandler, IView scrollView)
			{
				return base.CreateViewController(pageHandler, scrollView);
			}

		}
    }
}
