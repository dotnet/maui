using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	internal class PerformanceTrackerTemplate : StackLayout
	{
		public const string ScenarioId = "ScenarioId";
		public const string ExpectedId = "ExpectedId";
		public const string ActualId = "ActualId";
		public const string OutcomeId = "OutcomeId";

		public PerformanceTrackerTemplate()
		{
			var scenarioLabel = new Label
			{
				BackgroundColor = Color.Blue,
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.Center,
				HeightRequest = 25,
				AutomationId = ScenarioId
			};
			scenarioLabel.SetBinding(Label.TextProperty, new TemplateBinding(nameof(PerformanceTracker.Scenario)));
			Children.Add(scenarioLabel);

			var renderTimeLabel = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 25,
				AutomationId = ActualId
			};
			renderTimeLabel.SetBinding(Label.TextProperty, new TemplateBinding(nameof(PerformanceTracker.RenderTime)));

			var expectedLabel = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 25,
				AutomationId = ExpectedId
			};
			expectedLabel.SetBinding(Label.TextProperty, new TemplateBinding(nameof(PerformanceTracker.ExpectedRenderTime)));

			var outcomeLabel = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 25,
				AutomationId = OutcomeId
			};
			outcomeLabel.SetBinding(Label.TextProperty, new TemplateBinding(nameof(PerformanceTracker.Outcome)));

			var horStack = new StackLayout { Orientation = StackOrientation.Horizontal };
			horStack.Children.Add(renderTimeLabel);
			horStack.Children.Add(expectedLabel);
			horStack.Children.Add(outcomeLabel);

			Children.Add(horStack);

			Children.Add(new ContentPresenter());
		}
	}
}
