using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls;
using global::Windows.UI.Xaml.Input;
using global::Windows.UI.Xaml.Media;

namespace System.Maui.Platform.UWP
{
	public class ListGroupHeaderPresenter : global::Windows.UI.Xaml.Controls.ContentPresenter
	{
		void OnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
		{
			var element = VisualTreeHelper.GetParent(this) as FrameworkElement;
			while (element != null)
			{
				var list = element as global::Windows.UI.Xaml.Controls.ListView;
				if (list != null)
					element = list.SemanticZoomOwner;

				if (element == null)
					break;

				var zoom = element as SemanticZoom;
				if (zoom != null)
				{
					zoom.ToggleActiveView();

					var grid = zoom.ZoomedOutView as GridView;
					if (grid != null)
					{
						grid.MakeVisible(new SemanticZoomLocation { Item = DataContext });
					}

					return;
				}

				element = VisualTreeHelper.GetParent(element) as FrameworkElement;
			}
		}
	}
}