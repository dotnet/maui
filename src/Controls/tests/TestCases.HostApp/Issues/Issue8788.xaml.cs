using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 8788, "Shell Tab is still visible after set Tab.IsVisible to false", PlatformAffected.Android)]
	public partial class Issue8788 : Shell
	{
		public Issue8788()
		{
			InitializeComponent();
		}

		private void MenuItem_Clicked(object sender, EventArgs e)
		{
#if ANDROID
			Tab2.IsVisible = true;
			Tab3.IsVisible = true;
			Tab1.IsVisible = false;
#else
			Tab1.IsVisible = false;
			Tab2.IsVisible = true;
			Tab3.IsVisible = true;
#endif
		}
	}

	public partial class MainTabPage : ContentPage
	{
		public MainTabPage()
		{
			Content = new VerticalStackLayout()
			{
				new Label(){
					Text = "Page Loaded in first Tab",
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					TextDecorations = TextDecorations.Underline,

				},
			};
		}
	}

	public partial class SecondTabPage : ContentPage
	{
		public SecondTabPage()
		{
			Content = new VerticalStackLayout()
			{
				new Label(){
						Text = "Page Loaded in Second Tab",
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						TextDecorations = TextDecorations.Underline,

					},
			};
		}
	}
	public partial class ThirdTabPage : ContentPage
	{
		public ThirdTabPage()
		{
			Content = new VerticalStackLayout()
			{
				new Label(){
						Text = "Page Loaded in Third Tab",
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						TextDecorations = TextDecorations.Underline,

					},
			};
		}
	}
}
