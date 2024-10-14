using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.Bugzilla, 41038, "FlyoutPage loses menu icon on iOS after reusing NavigationPage as Detail")]
public class Bugzilla41038 : TestFlyoutPage
{
	NavigationPage _navPage;

	protected override void Init()
	{
		Title = "Main";

		var btnViewA = new Button() { Text = "ViewA", AutomationId = "ViewA" };
		btnViewA.Clicked += Button_Clicked;

		var btnViewB = new Button() { Text = "ViewB", AutomationId = "ViewB" };
		btnViewB.Clicked += Button_Clicked;

		var stack = new StackLayout();
		stack.Children.Add(btnViewA);
		stack.Children.Add(btnViewB);

		var root = new ContentPage() { Title = "Flyout", Content = stack };

		_navPage = new NavigationPage(new ViewA());

		Flyout = root;
		Detail = _navPage;

	}

	private async void Button_Clicked(object sender, EventArgs e)
	{
		Page root = _navPage.Navigation.NavigationStack[0];

		await _navPage.Navigation.PopToRootAsync(false);

		Page newRoot = null;

		var btn = (Button)sender;
		if (btn.Text == "ViewA")
			newRoot = new ViewA();
		else
			newRoot = new ViewB();


		await _navPage.Navigation.PushAsync(newRoot);
		_navPage.Navigation.RemovePage(root);
		IsPresented = false;
	}

	public class ViewA : ContentPage
	{
		public ViewA()
		{
			Title = "ViewA";
			Content = new Label()
			{
				Text = "Verify that the hamburger icon is visible. Click the icon and switch to ViewB. If the icon does not disappear, the test has passed.",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			};
		}
	}

	public class ViewB : ContentPage
	{
		public ViewB()
		{
			Title = "ViewB";
			Content = new Label() { Text = "View B" };
		}
	}
}
