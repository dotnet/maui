using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class EditorPage
	{
		public EditorPage()
		{
			InitializeComponent();

			UpdateEditorBackground();
		}

		void OnEditorCompleted(object sender, EventArgs e)
		{
			var text = ((Editor)sender).Text;
			DisplayAlertAsync("Completed", text, "Ok");
		}

		void OnEditorFocused(object sender, FocusEventArgs e)
		{
			var text = ((Editor)sender).Text;
			DisplayAlertAsync("Focused", text, "Ok");
		}

		void OnEditorUnfocused(object sender, FocusEventArgs e)
		{
			var text = ((Editor)sender).Text;
			DisplayAlertAsync("Unfocused", text, "Ok");
		}

		void OnUpdateBackgroundButtonClicked(object sender, System.EventArgs e)
		{
			UpdateEditorBackground();
		}

		void OnClearBackgroundButtonClicked(object sender, System.EventArgs e)
		{
			BackgroundEditor.Background = null;
		}

		void UpdateEditorBackground()
		{
			Random rnd = new Random();
			Color startColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			Color endColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);

			BackgroundEditor.Background = new LinearGradientBrush
			{
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = startColor },
					new GradientStop { Color = endColor, Offset = 1 }
				}
			};
		}
	}
}