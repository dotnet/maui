using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	const int rowCount = 30;
	const int columnCount = 30;

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
		Stopwatch sw = Stopwatch.StartNew();
		contentGrid.Clear();

		for (int n = 0; n < rowCount; n++)
		{
			contentGrid.RowDefinitions.Add(new RowDefinition());
		}

		for (int n = 0; n < columnCount; n++)
		{
			contentGrid.ColumnDefinitions.Add(new ColumnDefinition());
		}

		int i = 0;
		Label[] views = new Label[rowCount * columnCount];

		for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
		{

			for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
			{
				Label view = new Label() { Text = $"[{columnIndex}x{rowIndex}]" };
				// Button view = new() { Text = $"[{columnIndex}x{rowIndex}]" };
				// contentGrid.Add(view, column: columnIndex, row: rowIndex);

				views[i] = view;
				i++;

				contentGrid.SetRow(view, rowIndex);
				contentGrid.SetRowSpan(view, 1);

				contentGrid.SetColumn(view, columnIndex);
				contentGrid.SetColumnSpan(view, 1);
			}
		}

		contentGrid.AddBulk(views);

		sw.Stop();
		info.Text = $"Grid was created in: {sw.ElapsedMilliseconds} ms";
	}

	private async void BatchGenerate_ClickedAsync(object sender, EventArgs e)
	{
		Stopwatch sw = Stopwatch.StartNew();

		long sumMs = 0;
		int batchSize = int.Parse(BatchSize.Text);

		for (int i = 0; i < batchSize; i++)
		{
			sw.Reset();
			sw.Start();

			contentGrid.Clear();

			for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
			{
				for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
				{
					 Label view = new() { Text = $"[{columnIndex}x{rowIndex}]" };
					//Button view = new() { Text = $"[{columnIndex}x{rowIndex}]" };
					contentGrid.Add(view, column: columnIndex, row: rowIndex);
				}
			}

			sw.Stop();
			sumMs += sw.ElapsedMilliseconds;

			int runs = i + 1;

			info.Text = $"Grid was created {runs} times and it took {sumMs} ms in total. Avg run took {Math.Round(sumMs / (double)runs, 2)} ms";

			await Task.Delay(1000).ConfigureAwait(true);
		}
	}

}