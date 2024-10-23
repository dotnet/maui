#nullable disable
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	public sealed partial class AlertDialog : ContentDialog
	{
		public UI.Xaml.Controls.ScrollBarVisibility VerticalScrollBarVisibility { get; set; }

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