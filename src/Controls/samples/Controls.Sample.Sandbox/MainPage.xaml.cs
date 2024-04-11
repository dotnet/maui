using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private void ClearGrid_Clicked(object sender, EventArgs e)
	{
		Stopwatch sw = Stopwatch.StartNew();

		contentGrid.Clear();

		sw.Stop();

		info.Text = $"Clearing grid took: {sw.ElapsedMilliseconds} ms";
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		const int rowCount = 30;
		const int columnCount = 30;

		Stopwatch sw = Stopwatch.StartNew();
		contentGrid.Clear();

		for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
		{
			for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
			{
				Label label = new Label() { Text = $"[{columnIndex}x{rowIndex}]" };
				contentGrid.Add(label, column: columnIndex, row: rowIndex);
			}
		}

		sw.Stop();
		info.Text = $"Clearing grid took: {sw.ElapsedMilliseconds} ms";
	}
}