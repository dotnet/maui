namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24434, "Modifying a layout while view isn't part of the Window fails to update the layout visually",
		PlatformAffected.iOS)]
	public class Issue24434 : NavigationPage
	{
		public Issue24434() : base(new TestPage())
		{

		}

		public class TestPage : ContentPage
		{
			public TestPage()
			{
				VerticalStackLayout vsl = new VerticalStackLayout();

				vsl.Add(new Button()
				{
					Text = "Click me and you should see a label appear beneath me",
					LineBreakMode = LineBreakMode.WordWrap,
					AutomationId = "ClickMe",
					Command = new Command(async () =>
					{
						var secondPage = new ContentPage() { Content = new Label() { Text = "I should just disappear" } };

						await Navigation.PushAsync(secondPage);
						await Navigation.PushModalAsync(new ContentPage() { Content = new Label() { Text = "I should just disappear" } });

						// Ensure that the VSL is unloaded otherwise the test isn't really valid
						if (!vsl.IsLoaded)
						{
							vsl.Add(new Label { Text = "Hello, World!", AutomationId = "Success" });
						}

						await Navigation.PopModalAsync();
						await Navigation.PopAsync();
					})
				});

				Content = vsl;
			}
		}
	}
}
