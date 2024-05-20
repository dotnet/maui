using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21967, "CollectionView causes invalid measurements on resize", PlatformAffected.Android)]
public partial class Issue21967 : ContentPage
{
	public readonly record struct Data(string Text, string AutomationId);
	public Issue21967()
	{
		InitializeComponent();
		cv.ItemsSource =
			new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" }
			.Select((x, i) =>
			{
				string text = x;

				if (i == 1)
				{
					// Generate long text that would measure really big to ensure everything stays measured the
					text = Enumerable.Range(0, 1000).Select(x => x.ToString()).Aggregate((x, y) => x + " " + y);
				}
				return new Data(text, x);
			})
			.ToList();


		button.Clicked += (_, _) =>
		{
			if (cv.WidthRequest == 200)
			{
				cv.WidthRequest = 100;
			}
			else
			{
				cv.WidthRequest = 200;
			}
		};

		buttonFullSize.Clicked += (_, _) =>
		{
			cv.WidthRequest = Microsoft.Maui.Primitives.Dimension.Unset;
		};
	}
}