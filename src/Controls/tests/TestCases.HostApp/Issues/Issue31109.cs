namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 31109, "FlexLayout items with Dynamic Width are not updating correctly on orientation change or scroll in Android", PlatformAffected.Android)]
	public class Issue31109 : TestContentPage
	{
		FlexLayout _flexLayout;
		Label _statusLabel;
		double _currentWidth = 100;

		protected override void Init()
		{
			_flexLayout = new FlexLayout
			{
				Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
				Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
				BackgroundColor = Colors.LightGray,
			};

			var item1 = CreateItem("Item1", 100);
			var item2 = CreateItem("Item2", 150);
			var item3 = CreateItem("Item3", 200);

			_flexLayout.Add(item1);
			_flexLayout.Add(item2);
			_flexLayout.Add(item3);

			_statusLabel = new Label
			{
				AutomationId = "StatusLabel",
				Text = "Width: 100, 150, 200",
			};

			var changeButton = new Button
			{
				Text = "Change Widths",
				AutomationId = "ChangeWidthsButton",
			};
			changeButton.Clicked += OnChangeWidths;

			Content = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					_statusLabel,
					changeButton,
					_flexLayout,
				}
			};
		}

		BoxView CreateItem(string automationId, double width)
		{
			return new BoxView
			{
				AutomationId = automationId,
				WidthRequest = width,
				HeightRequest = 50,
				Color = Colors.CornflowerBlue,
				Margin = 5,
			};
		}

		void OnChangeWidths(object sender, EventArgs e)
		{
			_currentWidth += 50;

			foreach (var child in _flexLayout.Children)
			{
				if (child is BoxView box)
				{
					box.WidthRequest = _currentWidth;
				}
			}

			_statusLabel.Text = $"Width: {_currentWidth}";
		}
	}
}
