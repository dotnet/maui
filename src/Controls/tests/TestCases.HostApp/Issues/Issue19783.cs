using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 19783, "ShellContent IsVisible=True Does not display Title")]
	public class Issue19783 : Shell
	{
		public Issue19783()
		{
			var shellContent = new ShellContent()
			{
				ContentTemplate = new DataTemplate(typeof(Issue19783_WelcomePage))
			};
			var shellContent2 = new ShellContent()
			{
				ContentTemplate = new DataTemplate(typeof(Issue19783_ProfilePage))
			};
			this.Items.Add(new FlyoutItem()
			{
				Title = "Welcome Page",
				Items =
				{
					shellContent
				}
			});
			this.Items.Add(new FlyoutItem()
			{
				Title = "Profile Page",
				IsVisible = false,
				Items =
				{
					shellContent2
				}
			});
			Button button = new Button();
			button.Text = "Toggle Profile Page";
			button.AutomationId = "ToggleProfilePage";
			button.Clicked += (sender, e) =>
			{
				this.Items[1].IsVisible = true;
			};
			this.FlyoutFooter = button;
		}
	}

	public class Issue19783_WelcomePage : ContentPage
	{
		StackLayout stackLayout;
		public Issue19783_WelcomePage()
		{
			Title = "Welcome";
			stackLayout = new StackLayout();
			var button = new Button();
			button.Text = "click to open pane";
			button.AutomationId = "OpenPane";
			button.Clicked += (sender, e) =>
			{
				Shell.Current.FlyoutIsPresented = true;
			};
			var label = new Label { Text = "Welcome to the Shell" };
			stackLayout.Children.Add(label);
			stackLayout.Children.Add(button);
			Content = stackLayout;
		}
	}

	public class Issue19783_ProfilePage : ContentPage
	{
		public Issue19783_ProfilePage()
		{
			Title = "Profile";
			Content = new Label { Text = "Profile" };
		}
	}
}
