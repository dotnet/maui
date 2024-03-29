﻿using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 21501, "ListView: Button in ItemTemplate breaks SelectedItem", PlatformAffected.Android)]
	public class Bugzilla21501 : ContentPage
	{
		public Bugzilla21501()
		{
			var stringList = new List<string>() { "abc", "xyz", "todo" };

			var resultLabel = new Label() { Text = "A" };

			var listView = new ListView();
			listView.ItemsSource = stringList;
			listView.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");

				var button = new Button() { Text = "Test" };

				return new ViewCell
				{
					View = new StackLayout
					{
						Padding = new Thickness(0, 5),
						Orientation = StackOrientation.Horizontal,
						Children = { label, button }
					}
				};
			});

			listView.ItemSelected += (sender, args) =>
			{
				resultLabel.Text = resultLabel.Text + "!";
			};

			var layout = new StackLayout()
			{
				Orientation = StackOrientation.Vertical,
				Children = { listView, resultLabel }
			};

			Content = layout;
		}
	}
}

