namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 30353, "FlyoutPage.IsPresentedChanged is not raised")]
public class Bugzilla30353 : TestFlyoutPage
{
	protected override void Init()
	{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		var lbl = new Label
		{
			HorizontalOptions = LayoutOptions.CenterAndExpand,
			VerticalOptions = LayoutOptions.CenterAndExpand,
			Text = "Detail"
		};
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		var lblMaster = new Label
		{
			HorizontalOptions = LayoutOptions.CenterAndExpand,
			VerticalOptions = LayoutOptions.CenterAndExpand,
			Text = "Flyout"
		};
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		var btn = new Button()
		{
			Text = "DetailToggle"
		};
		var btn1 = new Button()
		{
			Text = "FlyoutToggle"
		};

		btn.Clicked += (object sender, EventArgs e) => IsPresented = !IsPresented;
		btn1.Clicked += (object sender, EventArgs e) => IsPresented = !IsPresented;

		var stacklayout = new StackLayout();
		stacklayout.Children.Add(lbl);
		stacklayout.Children.Add(btn);

		var stacklayout1 = new StackLayout();
		stacklayout1.Children.Add(lblMaster);
		stacklayout1.Children.Add(btn1);

		Flyout = new ContentPage
		{
			Title = "IsPresentedChanged Test",
			BackgroundColor = Colors.Green,
			Content = stacklayout1
		};
		Detail = new ContentPage
		{
			BackgroundColor = Colors.Gray,
			Content = stacklayout
		};
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
		IsPresentedChanged += (s, e) =>
			lblMaster.Text = lbl.Text = string.Format("The Flyout is now {0}", IsPresented ? "visible" : "invisible");
	}
}
