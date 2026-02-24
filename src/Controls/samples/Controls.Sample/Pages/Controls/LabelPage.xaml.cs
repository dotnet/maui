using System;
using Maui.Controls.Sample.ViewModels;
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

		void TapGestureRecognizer_Tapped(object sender, EventArgs e)
		{
			SetRandomBackgroundColor(GestureSpan);
		}

		void ChangeFormattedString_Clicked(object sender, EventArgs e)
		{
			labelFormattedString.FormattedText = new FormattedString
			{
				Spans =
				{
					new Span
					{
						Text = "Testing"
					},
					new Span
					{
						Text = "Bold",
						FontAttributes = FontAttributes.Bold
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
		void OnLink4Tapped(object sender, EventArgs e)
		{
			SetRandomBackgroundColor(Link4);
		}

		void OnLink5Tapped(object sender, EventArgs e)
		{
			SetRandomBackgroundColor(Link5);
		}

		void SetRandomBackgroundColor(Span span)
		{
			var oldColor = span.BackgroundColor;

			Color newColor = _colors[_rand.Next(_colors.Length)];

			while (oldColor == newColor)
			{
				newColor = _colors[_rand.Next(_colors.Length)];
			}

			span.BackgroundColor = newColor;
		}
	}
}