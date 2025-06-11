namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 3318, "[MAC] ScrollTo method is not working in Xamarin.Forms for mac platform", PlatformAffected.macOS)]

public class Issue3318 : TestContentPage
{
	protected override void Init()
	{
		var stackLayout = new StackLayout();

		var list = Enumerable.Range(0, 40).Select(c => $"Item {c}").ToArray();
		var listview = new ListView
		{
			ItemsSource = list,
			ItemTemplate = new DataTemplate(() =>
			{
				var viewCell = new ViewCell();
				var itemTemplateLabel = new Label() { HeightRequest = 30 };
				itemTemplateLabel.SetBinding(Label.TextProperty, new Binding("."));
				itemTemplateLabel.SetBinding(Label.AutomationIdProperty, new Binding("."));
				viewCell.View = itemTemplateLabel;
				return viewCell;
			})

		};

		var swShouldAnimate = new Switch();
		var lblShouldAnimate = new Label { Text = "Should Animate?" };

		var btnMakeVisible = new Button { Text = "Make Visible" };
		btnMakeVisible.Clicked += (s, e) =>
		{
			listview.ScrollTo(list[19], ScrollToPosition.MakeVisible, swShouldAnimate.IsToggled);
		};

		var btnCenter = new Button { Text = "Center" };
		btnCenter.Clicked += (s, e) =>
		{
			listview.ScrollTo(list[19], ScrollToPosition.Center, swShouldAnimate.IsToggled);
		};

		var btnStart = new Button { Text = "Start" };
		btnStart.Clicked += (s, e) =>
		{
			listview.ScrollTo(list[19], ScrollToPosition.Start, swShouldAnimate.IsToggled);
		};

		var btnEnd = new Button { Text = "End", AutomationId = "End" };
		btnEnd.Clicked += (s, e) =>
		{
			listview.ScrollTo(list[19], ScrollToPosition.End, swShouldAnimate.IsToggled);
		};

		stackLayout.Children.Add(btnMakeVisible);
		stackLayout.Children.Add(btnCenter);
		stackLayout.Children.Add(btnStart);
		stackLayout.Children.Add(btnEnd);

		var shouldAnimateContainer = new StackLayout { Orientation = StackOrientation.Horizontal };
		shouldAnimateContainer.Children.Add(swShouldAnimate);
		shouldAnimateContainer.Children.Add(lblShouldAnimate);

		stackLayout.Children.Add(shouldAnimateContainer);
		stackLayout.Children.Add(listview);

		Content = stackLayout;
	}
}
