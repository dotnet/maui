using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 15810, "TapGestureRecognizer Tapped events not worked in Windows Platform", PlatformAffected.UWP)]
	public class Issue15810 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			var customView = new CustomView
			{
				AutomationId = "CustomView"
			};

			var infoLabel = new Label
			{
				AutomationId = "InfoLabel"
			};

			Grid.SetRow(customView, 0);
			grid.Children.Add(customView);

			Grid.SetRow(infoLabel, 1);
			grid.Children.Add(infoLabel);

			customView.Tapped += (sender, args) =>
			{
				infoLabel.Text = "Tapped";
			};

			Content = grid;
		}
	}

	internal class CustomView : ContentView
	{
		public CustomView()
		{
			var absoluteLayout = new AbsoluteLayout();
			Content = absoluteLayout;

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += OnBoxTapped;
			GestureRecognizers.Add(tapGestureRecognizer);
		}

		public EventHandler Tapped;

		private void OnBoxTapped(object sender, EventArgs e)
		{
			BackgroundColor = BackgroundColor == Colors.Red ? Colors.Green : Colors.Red;
			Tapped?.Invoke(this, EventArgs.Empty);
		}
	}
}
