#nullable disable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public partial class ListGroupHeaderPresenter : Microsoft.UI.Xaml.Controls.ContentPresenter
	{
		void OnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
		{
			var element = VisualTreeHelper.GetParent(this) as FrameworkElement;
			while (element != null)
			{
				var list = element as Microsoft.UI.Xaml.Controls.ListView;
				if (list != null)
					element = list.SemanticZoomOwner;

				if (element == null)
					break;

				var zoom = element as SemanticZoom;
				if (zoom != null)
				{
					zoom.ToggleActiveView();

					var grid = zoom.ZoomedOutView as GridView;
					grid?.MakeVisible(new SemanticZoomLocation { Item = DataContext });

					return;
				}

				element = VisualTreeHelper.GetParent(element) as FrameworkElement;
			}
		}
	}
}