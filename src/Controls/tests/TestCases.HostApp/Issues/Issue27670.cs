
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;
using Page = Microsoft.Maui.Controls.Page;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27670, "NavigationPage fails to push the modal page via PushModalAsync after using PageSheet", PlatformAffected.iOS)]

public partial class Issue27670Shell : Shell
{
	public Issue27670Shell()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		ShellContent shellContent = new ShellContent
		{
			ContentTemplate = new DataTemplate(typeof(Issue27670)),
			Route = "MainPage"
		};

		Items.Add(shellContent);
	}

	public class Issue27670 : ContentPage
	{
		public Issue27670()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "1. Navigate to page that opens modal",
						AutomationId = "Button",
						Command = new Command(() => Shell.Current.Navigation.PushAsync(new Issue27670Page1()))
					}
				}
			};
		}
	}

	internal class Issue27670Page1 : ContentPage
	{
		public Issue27670Page1()
		{
			Content = new VerticalStackLayout()
			{
				Children =
				{
					new Button()
					{
						AutomationId = "ModalButton",
						Text = "2. Open Modal", Command = new Command(() =>
						Shell.Current.Navigation.PushModalAsync(
							new Issue27670PageSheet(new Issue27670Page2())
						)), HorizontalOptions = LayoutOptions.Center
					},
				}
			};
		}
	}

	public class Issue27670PageSheet : NavigationPage
	{
		public Issue27670PageSheet(Page root) : base(root)
		{
			On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);

		}
	}

	public class Issue27670Page2 : ContentPage
	{
		public Issue27670Page2()
		{
			Content = new VerticalStackLayout()
			{
				Children =
				{
					new Button()
					{
						Text = "3. Navigate",
						AutomationId = "NavigateButton",
						VerticalOptions = LayoutOptions.Start,
						HorizontalOptions = LayoutOptions.Center,
						Command = new Command(() => Shell.Current.Navigation.PushAsync(new Issue27670Page3()))
					}
				}
			};
		}
	}

	public class Issue27670Page3 : ContentPage
	{
		public Issue27670Page3()
		{
			Content = new Label() { AutomationId = "Label", Text = "4. Swipe this page down quickly when the page appears" };
		}
	}
}









