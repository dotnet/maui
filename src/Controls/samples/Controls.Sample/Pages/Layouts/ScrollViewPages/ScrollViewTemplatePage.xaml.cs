using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.ScrollViewPages
{
	public partial class ScrollViewTemplatePage
	{
		int count = 0;

		public ScrollViewTemplatePage()
		{
			InitializeComponent();
		}

		private void OnCounterClicked(object? sender, EventArgs e)
		{
			count++;

			if (count == 1)
				CounterBtn.Text = $"Clicked {count} time";
			else
				CounterBtn.Text = $"Clicked {count} times";
		}
	}

	public class ScrollViewTemplatePageModel
	{
		public int Spacing { get; set; } = 25;

		public LayoutOptions VerticalAlignment { get; set; } = LayoutOptions.Center;

		public Thickness ScrollViewPadding { get; set; } = Thickness.Zero;

		public Color ContentBackground { get; set; } = Colors.Transparent;
	}
}