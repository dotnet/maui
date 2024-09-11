using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues
{
	public partial class Issue8244 : Shell
	{
		public Issue8244()
		{
			InitializeComponent();
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
}