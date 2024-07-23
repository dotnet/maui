using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18857, "ImageButton Padding & Ripple effect stops working with .NET 8", PlatformAffected.Android)]
	public partial class Issue18857 : ContentPage
	{
		public Issue18857()
		{
			InitializeComponent();
			UpdateImageButtonBackground();
		}

		void OnUpdateBackgroundButtonClicked(object sender, EventArgs e)
		{
			UpdateImageButtonBackground();
		}

		void OnRemoveBackgroundButtonClicked(object sender, EventArgs e)
		{
			BackgroundImageButton.Background = null;
		}

		void UpdateImageButtonBackground()
		{
			Color startColor = Colors.GreenYellow;
			Color endColor = Colors.Yellow;

			BackgroundImageButton.Background = new LinearGradientBrush
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