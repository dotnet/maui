using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class MauiRefreshContainer : RefreshContainer
	{
		public void UpdateContent(IView? content, IMauiContext? mauiContext)
		{
			if (content != null && mauiContext != null)
			{
				var contentView = content.ToNative(mauiContext);
				Content = contentView;
			}
		}
	}
}