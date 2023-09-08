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

		void PointerGestureRecognizer_PointerReleased(System.Object sender, Microsoft.Maui.Controls.PointerEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("PointerGestureRecognizer_PointerReleased");
		}

		void PointerGestureRecognizer_PointerEntered(System.Object sender, Microsoft.Maui.Controls.PointerEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("PointerGestureRecognizer_PointerEntered");
		}

		void PointerGestureRecognizer_PointerExited(System.Object sender, Microsoft.Maui.Controls.PointerEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("PointerGestureRecognizer_PointerExited");
		}

		void PointerGestureRecognizer_PointerMoved(System.Object sender, Microsoft.Maui.Controls.PointerEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("PointerGestureRecognizer_PointerMoved");
		}

		void PointerGestureRecognizer_PointerPressed(System.Object sender, Microsoft.Maui.Controls.PointerEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("PointerGestureRecognizer_PointerPressed");
		}

		void ContentView_Unloaded(System.Object sender, System.EventArgs e)
		{
		}
	}
}