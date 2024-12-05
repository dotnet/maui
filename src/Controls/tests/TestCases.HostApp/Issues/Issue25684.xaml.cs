namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25684, "[WinUI] Editor width is not updated when setting styles for native view", PlatformAffected.UWP)]
	public partial class Issue25684 : TestContentPage
	{

		public Issue25684()
		{
			InitializeComponent();

		}

		protected override void Init()
		{

		}
	}

	public class CustomStyleEditor : Editor
	{

#if WINDOWS
		protected override void OnHandlerChanged()
		{
			// Hide editor border and underline.
			var platformView = Handler?.PlatformView as Microsoft.UI.Xaml.Controls.TextBox;

			if (platformView != null)
			{
				ApplyTextBoxStyle(platformView);
			}

			base.OnHandlerChanged();
		}

		void ApplyTextBoxStyle(Microsoft.UI.Xaml.Controls.TextBox textbox)
		{

			var textBoxStyle = new Microsoft.UI.Xaml.Style(typeof(Microsoft.UI.Xaml.Controls.TextBox));
			textBoxStyle.Setters.Add(new Microsoft.UI.Xaml.Setter() { Property = Microsoft.UI.Xaml.Controls.Control.BorderBrushProperty, Value = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)) });
			textBoxStyle.Setters.Add(new Microsoft.UI.Xaml.Setter() { Property = Microsoft.UI.Xaml.Controls.Control.BorderThicknessProperty, Value = new Thickness(0) });
			textbox.Resources.Add(typeof(Microsoft.UI.Xaml.Controls.TextBox), textBoxStyle);
		}
#endif
	}
}