namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.ManualTest, "ReduceInvalidateMeasure", "https://github.com/dotnet/maui/pull/21801", PlatformAffected.iOS)]
	public partial class ReduceInvalidateMeasure : ContentPage
	{
		int _currentLineBreakMode;

		public ReduceInvalidateMeasure()
		{
			InitializeComponent();
		}

		void OnUpdateTextButtonClicked(object sender, EventArgs e)
		{
			UpdateTextLabel.Text += " consectetur adipiscing elit";
		}

		void OnUpdateSizeButtonClicked(object sender, EventArgs e)
		{
			UpdateSizeLabel.WidthRequest += 50;
		}

		void OnUpdateFontSizeButtonClicked(object sender, EventArgs e)
		{
			UpdateFontSizeLabel.FontSize += 4;
		}

		void OnUpdateLineBreakModeButtonClicked(object sender, EventArgs e)
		{
			Array lineBreakModes = Enum.GetValues(typeof(LineBreakMode));

			if (_currentLineBreakMode >= lineBreakModes.Length)
				_currentLineBreakMode = 0;

			UpdateLineBreakModeLabel.LineBreakMode = (LineBreakMode)lineBreakModes.GetValue(_currentLineBreakMode);

			_currentLineBreakMode++;
		}

		void OnUpdateLineHeightButtonClicked(object sender, EventArgs e)
		{
			UpdateLineHeightLabel.LineHeight++;
		}

		void OnUpdateVisibilityButtonClicked(object sender, EventArgs e)
		{
			UpdateVisibilityLabel.IsVisible = !UpdateVisibilityLabel.IsVisible;
		}
	}
}