using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui
{
	public class PageViewController : ContainerViewController
	{
		readonly PageHandler _pageHandler;

		public PageViewController(IView page, PageHandler pageHandler)
		{
			CurrentView = page;
			Context = pageHandler.MauiContext!;

			LoadFirstView(page);
			_pageHandler = pageHandler;
		}

		protected override UIView CreateNativeView(IElement view)
		{
			return (PageView)PageHandler.FactoryMapper[nameof(PageHandler.Factory.CreateNativeView)]
				.Invoke(_pageHandler, (IView)view, null)!;
		}

		public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
		{
			if (CurrentView?.Handler is ElementHandler handler)
			{
				var application = handler.GetRequiredService<IApplication>();
				application?.ThemeChanged();
			}

			base.TraitCollectionDidChange(previousTraitCollection);
		}
	}
}