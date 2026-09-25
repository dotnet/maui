#if MAUI_UI_EVIDENCE
namespace Maui.Controls.Sample;

sealed class UiEvidenceLayoutControlsPage : ContentPage
{
	public UiEvidenceLayoutControlsPage()
	{
		Title = "UI Evidence - Layout";
		BackgroundColor = Color.FromArgb("#F5F7FA");

		var heading = new Label
		{
			Text = "Layout evidence",
			AutomationId = "UiEvidenceHeading",
			FontSize = 24,
			FontAttributes = FontAttributes.Bold,
			TextColor = Color.FromArgb("#172B4D")
		};

		var entry = new Entry
		{
			Text = "Deterministic value",
			AutomationId = "UiEvidenceEntry",
			FontSize = 16,
			HeightRequest = 48,
			BackgroundColor = Colors.White,
			TextColor = Colors.Black
		};

		var button = new Button
		{
			Text = "Primary action",
			AutomationId = "UiEvidencePrimaryButton",
			HeightRequest = 48,
			BackgroundColor = Color.FromArgb("#005FB8"),
			TextColor = Colors.White,
			CornerRadius = 8
		};

		var details = new Grid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star)
			},
			ColumnSpacing = 12
		};
		details.Add(CreateCard("Left card", "Stable content A", "UiEvidenceLeftCard"), 0);
		details.Add(CreateCard("Right card", "Stable content B", "UiEvidenceRightCard"), 1);

		var footer = new Label
		{
			Text = "Evidence fixture ready",
			AutomationId = "UiEvidenceFooter",
			HorizontalTextAlignment = TextAlignment.Center,
			FontSize = 14,
			TextColor = Color.FromArgb("#42526E")
		};

		var ready = new Label
		{
			Text = "Ready",
			AutomationId = "UiEvidenceReady",
			FontSize = 12,
			TextColor = Color.FromArgb("#006644"),
			HorizontalTextAlignment = TextAlignment.End
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(24),
				Spacing = 18,
				Children =
				{
					heading,
					new Label
					{
						Text = "This trusted page exercises common layout and control paths.",
						FontSize = 16,
						TextColor = Color.FromArgb("#42526E")
					},
					entry,
					button,
					details,
					footer,
					ready
				}
			}
		};
	}

	static Border CreateCard(string title, string body, string automationId) =>
		new()
		{
			AutomationId = automationId,
			Padding = new Thickness(16),
			BackgroundColor = Colors.White,
			Stroke = Color.FromArgb("#DFE1E6"),
			StrokeThickness = 1,
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
			Content = new VerticalStackLayout
			{
				Spacing = 8,
				Children =
				{
					new Label
					{
						Text = title,
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						TextColor = Color.FromArgb("#172B4D")
					},
					new Label
					{
						Text = body,
						FontSize = 14,
						TextColor = Color.FromArgb("#42526E")
					}
				}
			}
		};
}
#endif
