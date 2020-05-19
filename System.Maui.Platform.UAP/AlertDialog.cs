using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	public class AlertDialog : ContentDialog
	{
		public Windows.UI.Xaml.Controls.ScrollBarVisibility VerticalScrollBarVisibility { get; set; }

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// The child template name is derived from the default style
			// https://msdn.microsoft.com/en-us/library/windows/apps/mt299120.aspx
			var scrollName = "ContentScrollViewer";
			if (GetTemplateChild(scrollName) is ScrollViewer contentScrollViewer)
				contentScrollViewer.VerticalScrollBarVisibility = VerticalScrollBarVisibility;
		}
	}
}