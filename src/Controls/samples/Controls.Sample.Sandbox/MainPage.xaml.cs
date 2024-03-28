using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	private Label[] _labels;

	public MainPage()
	{
		InitializeComponent();

		_labels = [label1, label2, label3, label4];
	}

	private void OnTap(object? sender, TappedEventArgs e)
	{
		Color textColor = GetTextColor(sender);
		results.Add(new Label() { Text = "OnTap called", TextColor = textColor });
	}

	private void OnDoubleTap(object? sender, TappedEventArgs e)
	{
		Color textColor = GetTextColor(sender);
		results.Add(new Label() { Text = "OnDoubleTap called", TextColor = textColor });
	}

	private Color GetTextColor(object? sender)
	{
		foreach (Label label in _labels)
		{
			if (sender == label)
			{
				return label.TextColor;
			}
		}

		return Color.Parse("Gray");
	}
}