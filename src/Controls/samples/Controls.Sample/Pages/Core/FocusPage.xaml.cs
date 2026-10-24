using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class FocusPage
	{
		public FocusPage()
		{
			InitializeComponent();
		}

		void OnFocusClicked(object? sender, EventArgs e)
		{
			FocusEntry.Focus();
		}

		void OnUnfocusClicked(object? sender, EventArgs e)
		{
			FocusEntry.Unfocus();
		}

		void OnFocusEntryFocusChanged(object? sender, Microsoft.Maui.Controls.FocusEventArgs e)
		{
			InfoLabel.Text += e.IsFocused ? "Focused" + Environment.NewLine : "Unfocused" + Environment.NewLine;
		}
	}
}