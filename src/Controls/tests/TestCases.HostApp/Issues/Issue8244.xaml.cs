using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 8244, "[iOS]Custom font icon is not rendered in a shell tabbar tab",
	PlatformAffected.iOS)]
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
					AutomationId="TabBarIcon",
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

	static class IconFont
	{
		public const string Laptop = "\uf109";
		public const string LaptopFile = "\ue51d";
		public const string LaptopCode = "\uf5fc";
		public const string LaptopMedical = "\uf812";
		public const string Plus = "\u002b";
	}
}
