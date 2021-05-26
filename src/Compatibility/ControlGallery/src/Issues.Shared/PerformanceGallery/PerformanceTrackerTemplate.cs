using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
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
				BackgroundColor = Colors.Blue,
				TextColor = Colors.White,
				HorizontalTextAlignment = TextAlignment.Center,
				HeightRequest = 25,
				AutomationId = ScenarioId
			};
			scenarioLabel.SetBinding(Label.TextProperty, new Binding(nameof(PerformanceTracker.Scenario), source: RelativeBindingSource.TemplatedParent));
			Children.Add(scenarioLabel);

			var renderTimeLabel = new Label
			{
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				HorizontalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 25,
				AutomationId = ActualId
			};
			renderTimeLabel.SetBinding(Label.TextProperty, new Binding(nameof(PerformanceTracker.RenderTime), source: RelativeBindingSource.TemplatedParent));

			var expectedLabel = new Label
			{
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				HorizontalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 25,
				AutomationId = ExpectedId
			};
			expectedLabel.SetBinding(Label.TextProperty, new Binding(nameof(PerformanceTracker.ExpectedRenderTime), source: RelativeBindingSource.TemplatedParent));

			var outcomeLabel = new Label
			{
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				HorizontalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 25,
				AutomationId = OutcomeId
			};
			outcomeLabel.SetBinding(Label.TextProperty, new Binding(nameof(PerformanceTracker.Outcome), source: RelativeBindingSource.TemplatedParent));

			var horStack = new StackLayout { Orientation = StackOrientation.Horizontal };
			horStack.Children.Add(renderTimeLabel);
			horStack.Children.Add(expectedLabel);
			horStack.Children.Add(outcomeLabel);

			Children.Add(horStack);

			Children.Add(new ContentPresenter());
		}
	}
}
