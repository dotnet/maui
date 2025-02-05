namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 9440, "Flyout closes with two or more taps", PlatformAffected.Android)]
public class Issue9440 : TestShell
{
	const string Test1 = "Test 1";
	const string Test2 = "Test 2";
	protected override void Init()
	{
		this.AddFlyoutItem(CreatePage(Test1), Test1);
		this.AddFlyoutItem(CreatePage(Test2), Test2);

		ContentPage CreatePage(string title)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var label = new Label
			{
				TextColor = Colors.Black,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HorizontalTextAlignment = TextAlignment.End
			};
#pragma warning restore CS0618 // Type or member is obsolete
			label.BindingContext = this;
			label.SetBinding(Label.TextProperty, "FlyoutIsPresented");
			return new ContentPage
			{
				Title = title,
				Content = new ScrollView
				{
					Content = label
				}
			};
		}
	}
}
