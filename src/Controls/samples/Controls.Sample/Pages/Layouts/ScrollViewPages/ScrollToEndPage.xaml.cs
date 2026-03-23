using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.ScrollViewPages
{
	public partial class ScrollToEndPage
	{
		public ScrollToEndPage()
		{
			InitializeComponent();
		}

		async void OnButtonClicked(object? sender, EventArgs e)
		{
			await scrollView.ScrollToAsync(finalLabel, ScrollToPosition.End, true);
		}

		void OnScrollViewScrolled(object? sender, ScrolledEventArgs e)
		{
			Console.WriteLine($"ScrollX: {e.ScrollX}, ScrollY: {e.ScrollY}");
		}
	}
}