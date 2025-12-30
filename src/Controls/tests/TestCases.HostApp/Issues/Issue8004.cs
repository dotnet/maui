namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 8004, "Add a ScaleXTo and ScaleYTo animation extension method", PlatformAffected.All)]
	public class Issue8004 : TestContentPage
	{
		BoxView _boxView;
		const string AnimateBoxViewButton = "AnimateBoxViewButton";
		const string BoxToScale = "BoxToScale";

		protected override void Init()
		{
			var label = new Label
			{
				Text = "Click the button below to animate the BoxView using individual ScaleXTo and ScaleYTo extension methods.",
				TextColor = Colors.Black,
				AutomationId = "TestReady"
			};

			var button = new Button
			{
				AutomationId = AnimateBoxViewButton,
				Text = "Animate BoxView",
				//Commented out due to tap does not perform while automation on the Mac platform when setting VerticalOptions.				
				// BackgroundColor = Colors.Black,
				// TextColor = Colors.White,
				// VerticalOptions = LayoutOptions.EndAndExpand
			};

			button.Clicked += AnimateButton_Clicked;

			_boxView = new BoxView
			{
				AutomationId = BoxToScale,
				BackgroundColor = Colors.Blue,
				WidthRequest = 200,
				HeightRequest = 100,
				HorizontalOptions = LayoutOptions.Center
			};

			var grid = new Grid();

			Grid.SetRow(label, 0);
			Grid.SetRow(_boxView, 1);
			Grid.SetRow(button, 2);

			grid.Children.Add(label);
			grid.Children.Add(_boxView);
			grid.Children.Add(button);
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			Content = grid;
		}

		void AnimateButton_Clicked(object sender, EventArgs e)
		{
			_boxView.ScaleYToAsync(2, 250, Easing.CubicInOut);
			_boxView.ScaleXToAsync(1.5, 400, Easing.BounceOut);
		}
	}
}
