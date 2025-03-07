using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages;

public partial class ToolbarPage
{
	public ToolbarPage()
	{
		InitializeComponent();
	}

	void ItemClicked(object sender, EventArgs e)
	{
		if (sender is ToolbarItem tbi)
		{
			menuLabel.Text = $"You clicked on ToolbarItem: {tbi.Text}";
		}
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		secondary4.IsEnabled = !secondary4.IsEnabled;
	}

	private void Button_Clicked1(object sender, EventArgs e)
	{
		primary1.IsEnabled = !primary1.IsEnabled;
	}
}