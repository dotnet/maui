using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 17642, "Button Click Ripple Effect Not Working In Android", PlatformAffected.Android)]
	public partial class Issue17642 : ContentPage
	{
		readonly Random _random;

		public Issue17642()
		{
			InitializeComponent();

			_random = new Random();
		}

		void OnAddClicked(object sender, EventArgs args)
		{
			var linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(GetRandomInt(), GetRandomInt()),
				EndPoint = new Point(GetRandomInt(), GetRandomInt()),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = GetRandomColor() },
					new GradientStop { Color = GetRandomColor() }
				}
			};

			DynamicBackground.Background = linearGradientBrush;
		}

		void OnRemoveClicked(object sender, EventArgs args)
		{
			DynamicBackground.Background = null;
		}

		double GetRandomInt()
		{
			return _random.NextDouble() * 1;
		}

		Color GetRandomColor()
		{
			return Color.FromRgb(_random.Next(256), _random.Next(256), _random.Next(256));
		}
	}
}