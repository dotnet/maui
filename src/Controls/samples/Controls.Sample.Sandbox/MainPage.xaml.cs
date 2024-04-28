using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}
		private void PointerGestureRecognizer_PointerEntered(object sender, PointerEventArgs e)
		{
			debug.Text += "\nEnter";
		}

		private void PointerGestureRecognizer_PointerExited(object sender, PointerEventArgs e)
		{
			debug.Text += "\nExit";
		}

		private void PointerGestureRecognizer_PointerMoved(object sender, PointerEventArgs e)
		{
			debug.Text += "\nMove";
		}

		private void PointerGestureRecognizer_PointerPressed(object sender, PointerEventArgs e)
		{
			debug.Text += "\nPress";
		}

		private void PointerGestureRecognizer_PointerReleased(object sender, PointerEventArgs e)
		{
			debug.Text += "\nRelease";
		}
	}
}