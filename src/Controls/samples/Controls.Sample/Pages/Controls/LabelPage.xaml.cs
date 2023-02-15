using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class LabelPage
	{
		readonly Color[] _colors = new Color[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow, Colors.Brown, Colors.Purple, Colors.Orange, Colors.Gray };
		readonly Random _rand = new Random();

		public LabelPage()
		{
			InitializeComponent();

			BindingContext = new LabelViewModel();
		}

		void ClickGestureRecognizer_Clicked(System.Object sender, System.EventArgs e)
		{
			var label = sender as Label;

			if (label == null)
				return;

			var rnd = new System.Random();
			GestureSpan.TextColor = Color.FromRgb((byte)rnd.Next(0, 254), (byte)rnd.Next(0, 254), (byte)rnd.Next(0, 254));

			if (sender is Span span)
				span.TextColor = Color.FromRgb((byte)rnd.Next(0, 254), (byte)rnd.Next(0, 254), (byte)rnd.Next(0, 254));
		}

		void ChangeFormattedString_Clicked(object sender, System.EventArgs e)
		{
			labelFormattedString.FormattedText = new FormattedString
			{
				Spans =
				{
					new Span
					{
						Text = "Testing"
					}
				}
			};
		}

		void OnLink1Tapped(object sender, EventArgs e)
		{
			SetRandomBackgroundColor(Link1);
		}

		void OnLink2Tapped(object sender, EventArgs e)
		{
			SetRandomBackgroundColor(Link2);
		}

		void OnLink3Tapped(object sender, EventArgs e)
		{
			SetRandomBackgroundColor(Link3);
		}

		void SetRandomBackgroundColor(Span span)
		{
			var oldColor = span.BackgroundColor;

			Color newColor;
			do
			{
				newColor = _colors[_rand.Next(_colors.Length)];
			} while (oldColor == newColor);

			span.BackgroundColor = newColor;
		}
	}
}