using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.Generic;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1672, "UWP: Button: AccessKey support", PlatformAffected.UWP)]
	public class Issue1672 : TestTabbedPage
	{
		ContentPage _tab2;

		protected override void Init ()
		{
			Title = "Issue 1672";
			var label1 = new Label
			{
				Text = "Access  key A"
			};
			label1.On<PlatformConfiguration.Windows>().SetAccessKey("A");

			var label2 = new Label
			{
				Text = "Access  key B"
			};
			label2.On<PlatformConfiguration.Windows>().SetAccessKey("B");
			
			var button1 = new Button() 
			{
				Text = "Access key C"
			};
			button1.On<PlatformConfiguration.Windows>().SetAccessKey("C");

			var button2 = new Button() 
			{
				Text = "Toggle Access key I/X on tab 2"
			};
			button2.On<PlatformConfiguration.Windows>().SetAccessKey("D");

			var button3 = new Button()
			{
				Text = "Access key E, placement Left"
			};
			button3.On<PlatformConfiguration.Windows>()
				.SetAccessKey("E")
				.SetAccessKeyPlacement(AccessKeyPlacement.Left);

			var button4 = new Button()
			{
				Text = "Access key F, placement Right."
			};
			button4.On<PlatformConfiguration.Windows>()
				.SetAccessKey("F")
				.SetAccessKeyPlacement(AccessKeyPlacement.Right);

			var button5 = new Button()
			{
				Text = "Add new Tab", Margin = new Thickness(20)
			};
			button5.On<PlatformConfiguration.Windows>()
				.SetAccessKey("G")
				.SetAccessKeyPlacement(AccessKeyPlacement.Top)
				.SetAccessKeyHorizontalOffset(20)
				.SetAccessKeyVerticalOffset(40);

			button1.Clicked += ButtonClicked;
			button2.Clicked += ToggleAccessKeyOnSecondTab;
			button3.Clicked += ButtonClicked;
			button4.Clicked += ButtonClicked;
			button5.Clicked += (sender, e) => AddTab($"New tab {Children.Count}", $"{Children.Count}");

			var layout = new StackLayout()
			{
				Children = { label1, label2, button1, button2, button3, button4, button5 }
			};

			var tab1 = new ContentPage()
			{
				Title = "Tab1",
				Content = layout
			};
			tab1.On<PlatformConfiguration.Windows>().SetAccessKey("H");

			_tab2 = new ContentPage() { Title = "Tab2", Content = new StackLayout() { Children = { new Label() { Text = "Inside tab2" } } } };
			_tab2.On<PlatformConfiguration.Windows>().SetAccessKey("I");
			
			var tab3 = new ContentPage() { Title = "Tab3", Content = new StackLayout() { Children = { new Label() { Text = "Inside tab3" } } } };
			tab3.On<PlatformConfiguration.Windows>().SetAccessKey("J");
			
			Children.Add(tab1);
			Children.Add(_tab2);
			Children.Add(tab3);

		}

		IPlatformElementConfiguration<PlatformConfiguration.Windows, VisualElement> AddTab(string title, string accessKey)
		{
			var tab = new ContentPage() { Title = title, Content = new StackLayout() { Children = { new Label() { Text = $"Inside {title} with access key {accessKey}" } } } };
			Children.Add(tab);
			return tab.On<PlatformConfiguration.Windows>().SetAccessKey(accessKey);			
		}

		void ToggleAccessKeyOnSecondTab(object sender, System.EventArgs e)
		{
			var tab = _tab2.On<PlatformConfiguration.Windows>();
			tab.SetAccessKey(tab.GetAccessKey() == "I" ? "X" : "I");
		}

		void ButtonClicked(object sender, System.EventArgs e)
		{
			var button = sender as Button;
			DisplayAlert("Button clicked", $"Clicked {button?.Text}", "OK");
		}
	}
}
