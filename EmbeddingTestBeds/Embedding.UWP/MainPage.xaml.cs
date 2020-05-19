using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls;
using Embedding.XF;
using System.Maui.Platform.UWP;

namespace Embedding.UWP
{
	public sealed partial class MainPage : Page
	{
		readonly System.Maui.ContentPage _page4;

		public MainPage()
		{
			InitializeComponent();

			HelloFlyout.Content = new Hello().CreateFrameworkElement();

			_page4 = new Page4();
		}

		void NavToUWPPage(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(Page2));
		}

		void NavToFormsPage4(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(_page4);
		}

		void NavToFormsPage3(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(new Page3());
		}

		void NavToAlertsAndActionSheets(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(new AlertsAndActionSheets());
		}
	}
}