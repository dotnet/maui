using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 15460, "ScrollView on ios not being correctly sized when changes occur within the ScrollView content.", PlatformAffected.iOS)]
	public class ScrollViewButtonInteractionPage : TestContentPage
	{		
		Command _toggleBoxCommand;
		int _boxHeight = 800;
		int _buttonHeight = 50;

		public ScrollViewButtonInteractionPage()
		{

		}

		protected override void Init()
		{
			Title = "ScrollView Hit Tests";

			var header = new Label() { AutomationId = "HeaderLabel", HeightRequest = 100, BackgroundColor = Colors.Green, Text = "Header" };
			var footer = new Label() { AutomationId = "FooterLabel", HeightRequest = 100, BackgroundColor = Colors.Yellow, Text = "Footer" };
			var redBox = new BoxView() { AutomationId = "RedBox", IsVisible = false, HeightRequest = _boxHeight, Color = Colors.Red };
			var blueBox = new BoxView() { AutomationId = "BlueBox", IsVisible = false, HeightRequest = _boxHeight, Color = Colors.Blue };

			_toggleBoxCommand = new Command<BoxView>((parameter) =>
			{
				parameter.IsVisible = !parameter.IsVisible;
				header.Text = $"{parameter.AutomationId} IsVisible: {parameter.IsVisible}";
			});
			
			var scrollView = new ScrollView();
			scrollView.Content = new VerticalStackLayout()
			{
				Children =
				{					
					redBox,
					CreateButton("RedButton" ,"Toggle Red Box", redBox),
					blueBox,
					CreateButton("BlueButton", "Toggle Blue Box", blueBox)
				}
			};

			Grid.SetRow(header, 0);
			Grid.SetRow(scrollView, 1);
			Grid.SetRow(footer, 2);

			Content = new Grid()
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Star),
					new RowDefinition(GridLength.Auto)
				},
				Children =
				{
					header,
					scrollView,
					footer
				}
			};
		}

		private Button CreateButton(string automationId, string label, BoxView box)
		{
			var btn = new Button()
			{
				AutomationId = automationId,
				HeightRequest = _buttonHeight,
				Text = label,
				Command = _toggleBoxCommand,
				CommandParameter = box
			};

			return btn;
		}
	}
}
