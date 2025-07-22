using Microsoft.Maui.Graphics;
using Tizen.UIExtensions.NUI;
using TColor = Tizen.UIExtensions.Common.Color;

namespace Microsoft.Maui.Platform
{
	public class MauiRefreshLayout : RefreshLayout
	{
		IPlatformViewHandler? _contentHandler;

		public void UpdateContent(IView? content, IMauiContext? mauiContext)
		{
			Content = null;
			_contentHandler?.Dispose();
			_contentHandler = null;

			if (content != null && mauiContext != null)
			{
				var contentView = content.ToPlatform(mauiContext);
				if (content.Handler is IPlatformViewHandler thandler)
				{
					_contentHandler = thandler;
				}
				Content = contentView;
			}
		}

		public void UpdateIsRefreshing(IRefreshView view)
		{
			IsRefreshing = view.IsRefreshing;
		}

		public void UpdateRefreshColor(IRefreshView view)
		{
			IconColor = view.RefreshColor?.ToPlatform() ?? TColor.Default;
		}

		public void UpdateBackground(IRefreshView view)
		{
			IconBackgroundColor = view.Background.ToColor()?.ToPlatform() ?? TColor.Default;
		}
	}
}
