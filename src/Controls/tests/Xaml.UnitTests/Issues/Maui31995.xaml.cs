using System;
using System.Collections.Generic;
namespace Microsoft.Maui.Controls.Xaml.UnitTests.Issues;
#nullable enable

public partial class Maui31995 : ContentPage
{
	public Maui31995() => InitializeComponent();

	int count = 0;
	public List<object> ItemsSource = new();  // Added.



	private void OnCounterClicked(object? sender, EventArgs e)
	{
		count++;
	}
}