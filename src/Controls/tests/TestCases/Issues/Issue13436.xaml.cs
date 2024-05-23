using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 13436,
		"[Bug] Java.Lang.IllegalArgumentException in CarouselView adjusting PeekAreaInsets in OnSizeAllocated using XF 5.0",
		PlatformAffected.Android)]
	public partial class Issue13436 : TestContentPage
	{
		public Issue13436()
		{
			InitializeComponent();
		}

		double _prevWidth;

		protected override void OnAppearing()
		{
			base.OnAppearing();

			Carousel.ItemsSource = new List<Issue13436Model>
			{
				new Issue13436Model
				{
					Name = "N1",
					Desc = "D1",
					Color = Colors.Yellow
				},
				new Issue13436Model
				{
					Name = "N2",
					Desc = "D2",
					Color = Colors.Orange
				},
				new Issue13436Model
				{
					Name = "N3",
					Desc = "D3",
					Color = Colors.AliceBlue
				}
			};
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			if (Math.Abs(width - _prevWidth) < .1)
			{
				return;
			}

			_prevWidth = width;
			Carousel.PeekAreaInsets = width * .15;
		}

		protected override void Init()
		{
		}
	}
}