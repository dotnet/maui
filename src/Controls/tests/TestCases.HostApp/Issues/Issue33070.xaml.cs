using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33070, "Fix Android drawable mutation crash", PlatformAffected.Android)]
public partial class Issue33070 : ContentPage
{
	private readonly Color[] _testColors = new[]
	{
		Colors.Red, Colors.Blue, Colors.Green, Colors.Purple,
		Colors.Orange, Colors.Pink, Colors.Cyan, Colors.Yellow
	};

	private int _colorIndex = 0;
	private int _rapidChangeCounter = 0;

	public Issue33070()
	{
		InitializeComponent();
	}

	private void OnChangeActivityIndicatorColor(object sender, EventArgs e)
	{
		try
		{
			_colorIndex = (_colorIndex + 1) % _testColors.Length;
			var newColor = _testColors[_colorIndex];
			ActivityIndicatorTest.Color = newColor;
			ActivityIndicatorStatus.Text = $"Color changed to {newColor}";
		}
		catch (Exception ex)
		{
			ActivityIndicatorStatus.Text = $"ERROR: {ex.Message}";
		}
	}

	private void OnChangeEntryColor(object sender, EventArgs e)
	{
		try
		{
			_colorIndex = (_colorIndex + 1) % _testColors.Length;
			var newColor = _testColors[_colorIndex];
			EntryTest.TextColor = newColor;
			EntryStatus.Text = $"Color changed to {newColor}";
		}
		catch (Exception ex)
		{
			EntryStatus.Text = $"ERROR: {ex.Message}";
		}
	}

	private void OnChangeSwitchColors(object sender, EventArgs e)
	{
		try
		{
			_colorIndex = (_colorIndex + 1) % _testColors.Length;
			var newThumbColor = _testColors[_colorIndex];
			var newTrackColor = _testColors[(_colorIndex + 1) % _testColors.Length];

			SwitchTest.ThumbColor = newThumbColor;
			SwitchTest.OnColor = newTrackColor;

			SwitchStatus.Text = $"Thumb: {newThumbColor}, Track: {newTrackColor}";
		}
		catch (Exception ex)
		{
			SwitchStatus.Text = $"ERROR: {ex.Message}";
		}
	}

	private void OnChangeSearchBarColors(object sender, EventArgs e)
	{
		try
		{
			_colorIndex = (_colorIndex + 1) % _testColors.Length;
			var newTextColor = _testColors[_colorIndex];
			var newPlaceholderColor = _testColors[(_colorIndex + 1) % _testColors.Length];
			var newCancelColor = _testColors[(_colorIndex + 2) % _testColors.Length];

			SearchBarTest.TextColor = newTextColor;
			SearchBarTest.PlaceholderColor = newPlaceholderColor;
			SearchBarTest.CancelButtonColor = newCancelColor;

			SearchBarStatus.Text = $"Colors changed successfully";
		}
		catch (Exception ex)
		{
			SearchBarStatus.Text = $"ERROR: {ex.Message}";
		}
	}

	private async void OnRunRapidChangesTest(object sender, EventArgs e)
	{
		const int totalIterations = 50;
		_rapidChangeCounter = 0;

		try
		{
			RapidChangesStatus.Text = "Running...";

			for (int i = 0; i < totalIterations; i++)
			{
				var color1 = _testColors[i % _testColors.Length];
				var color2 = _testColors[(i + 1) % _testColors.Length];
				var color3 = _testColors[(i + 2) % _testColors.Length];
				var color4 = _testColors[(i + 3) % _testColors.Length];

				// Rapidly change all controls
				ActivityIndicatorTest.Color = color1;
				EntryTest.TextColor = color2;
				SwitchTest.ThumbColor = color3;
				SwitchTest.OnColor = color4;
				SearchBarTest.TextColor = color1;
				SearchBarTest.PlaceholderColor = color2;
				SearchBarTest.CancelButtonColor = color3;

				_rapidChangeCounter++;

				if (i % 10 == 0)
				{
					RapidChangesProgress.Text = $"Iteration {i + 1}/{totalIterations}";
					await Task.Delay(10); // Small delay to allow UI update
				}
			}

			RapidChangesStatus.Text = $"✅ Completed {_rapidChangeCounter} iterations";
			RapidChangesProgress.Text = "No crashes!";
		}
		catch (Exception ex)
		{
			RapidChangesStatus.Text = $"❌ Failed at iteration {_rapidChangeCounter}";
			RapidChangesProgress.Text = $"Error: {ex.Message}";
		}
	}
}
