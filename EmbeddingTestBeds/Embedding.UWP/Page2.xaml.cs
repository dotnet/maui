using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls;
using Embedding.XF;
using System.Maui.Platform.UWP;

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