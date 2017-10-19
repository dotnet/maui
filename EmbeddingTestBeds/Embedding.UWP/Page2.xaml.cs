using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Embedding.XF;
using Xamarin.Forms.Platform.UWP;

namespace Embedding.UWP
{
	public sealed partial class Page2 : Page
	{
		public Page2()
		{
			InitializeComponent();
		}

		void Button_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(new Page3());
		}
	}
}