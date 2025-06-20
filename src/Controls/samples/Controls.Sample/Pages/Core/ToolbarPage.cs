using System;
using System.Threading.Tasks;
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

	void Button_Clicked(object sender, EventArgs e)
	{
		secondary4.IsEnabled = !secondary4.IsEnabled;
	}

	void Button_Clicked1(object sender, EventArgs e)
	{
		primary1.IsEnabled = !primary1.IsEnabled;
	}

	void Button_Clicked2(object sender, EventArgs e)
	{
		Task.Delay(5000).ContinueWith(t =>
		{
			Dispatcher.Dispatch(() =>
			{
				secondary2.IsEnabled = !secondary2.IsEnabled;
			});
		});
	}
}