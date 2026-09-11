using System;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Pages
{
	public partial class ProgressBarPage
	{
		public ProgressBarPage()
		{
			InitializeComponent();
		}

		void OnProgressToClicked(object? sender, EventArgs args)
		{
			ProgressToBar.ProgressTo(1.0, 1000, Easing.Linear);
		}
	}
}