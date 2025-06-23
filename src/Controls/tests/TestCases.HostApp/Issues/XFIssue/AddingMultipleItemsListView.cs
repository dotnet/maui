namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Adding Multiple Items to a ListView", PlatformAffected.All)]

public class AddingMultipleItemsListView : TestContentPage
{
	protected override void Init()
	{
		Title = "Hours";
		var exampleViewModel = new ExampleViewModel();
		BindingContext = exampleViewModel;

		var listView = new ListView
		{
			ItemTemplate = new DataTemplate(typeof(CustomViewCell)),
			HeightRequest = 400,
			VerticalOptions = LayoutOptions.Start
		};

		listView.SetBinding(ListView.ItemsSourceProperty, new Binding("Jobs", BindingMode.TwoWay));

		var addOneJobButton = new Button
		{
			Text = "Add One",
			AutomationId = "Add One"
		};
		addOneJobButton.SetBinding(Button.CommandProperty, new Binding("AddOneCommand"));

		var addTwoJobsButton = new Button
		{
			Text = "Add Two",
			AutomationId = "Add Two"
		};
		addTwoJobsButton.SetBinding(Button.CommandProperty, new Binding("AddTwoCommand"));

		var layout = new VerticalStackLayout() { Spacing = 10 };
		layout.Children.Add(listView);
		layout.Children.Add(addOneJobButton);
		layout.Children.Add(addTwoJobsButton);
		Content = layout;
	}

	public class CustomViewCell : ViewCell
	{
		public CustomViewCell()
		{
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			var jobId = new Label
			{
				FontSize = 20,
				WidthRequest = 105,
				VerticalOptions = LayoutOptions.Center,

				HorizontalOptions = LayoutOptions.StartAndExpand
			};
			jobId.SetBinding(Label.TextProperty, "JobId");
			jobId.SetBinding(Label.AutomationIdProperty, "JobId");

			var jobName = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				WidthRequest = 175,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};
			jobName.SetBinding(Label.TextProperty, "JobName");
			jobName.SetBinding(Label.AutomationIdProperty, "JobName");

			var hours = new Label
			{
				WidthRequest = 45,
				VerticalOptions = LayoutOptions.Center,
				HorizontalTextAlignment = TextAlignment.End,
				HorizontalOptions = LayoutOptions.EndAndExpand,

			};
#pragma warning restore CS0612
#pragma warning restore CS0618
			hours.SetBinding(Label.TextProperty, new Binding("Hours", BindingMode.OneWay, new DoubleStringConverter()));

			var hlayout = new StackLayout
			{
				Children = {
					jobId,
					jobName,
					hours
				},
				Orientation = StackOrientation.Horizontal,
			};

			View = hlayout;
		}
	}
}
