using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

public partial class Issue19214_FullScrollView : ContentPage
{
	public Issue19214_FullScrollView()
	{
		InitializeComponent();
	}

	async void PopButtonPressed(object sender, EventArgs e)
	{
		await Navigation.PopAsync(true);
	}
}