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
}