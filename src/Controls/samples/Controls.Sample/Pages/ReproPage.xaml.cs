using System;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ReproPage
	{
		public ReproPage() => InitializeComponent();

		private void OnCounterClicked(object sender, EventArgs e)
		{
			Rect b = this.MyStack.Bounds;
			IView v = this.MyStack;

			this.CounterLabel.Text = $"StackLayout.Bounds: {b.Width} x {b.Height}\r\nIView size: {v.Width} x {v.Height}";
		}
	}
}
