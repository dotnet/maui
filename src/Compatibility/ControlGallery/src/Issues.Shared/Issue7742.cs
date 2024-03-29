﻿using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
using System.Linq;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7742, "(iOS) Changing ItemTemplate does not work as expected", PlatformAffected.iOS)]
	public class Issue7742 : TestContentPage
	{
		CollectionView _collectionView;

		public Issue7742()
		{
			Title = "Issue 7742";
		}

		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "Click the Button. If all cells render correctly, then the test passes."
			};

			var button = new Button
			{
				Text = "Change ItemTemplate",
				AutomationId = "TemplateBtn"
			};

			button.Clicked += OnButton1Clicked;

			_collectionView = new CollectionView
			{
				BackgroundColor = Colors.LightGreen,
				SelectionMode = SelectionMode.None,
				HeightRequest = 500
			};

			var lines = new List<Issue7742Model>();

			for (int i = 0; i < 30; i++)
			{
				lines.Add(new Issue7742Model() { Text = $"Item {i}" });
			}

			_collectionView.ItemsSource = lines;

			var stack = new StackLayout();

			stack.Children.Add(instructions);
			stack.Children.Add(button);
			stack.Children.Add(_collectionView);

			Content = stack;
		}

		void OnButton1Clicked(object sender, EventArgs e)
		{
			_collectionView.ItemTemplate = CreateDataGridTemplate(2);
		}

		DataTemplate CreateDataGridTemplate(int columns)
		{
			var template = new DataTemplate(() =>
			{
				var grid = new Grid() { Padding = new Thickness(0), Margin = 0, RowSpacing = 0, ColumnSpacing = 0 };

				grid.RowDefinitions.Clear();
				grid.ColumnDefinitions.Clear();
				grid.Children.Clear();
				grid.RowDefinitions.Add(new RowDefinition() { Height = 40 });
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
				Label cell;
				cell = new Label() { };
				cell.SetBinding(Label.TextProperty, "Text");
				cell.FontSize = 20;
				cell.FontAttributes = FontAttributes.Bold;
				cell.BackgroundColor = Colors.LightBlue;
				grid.Add(cell, 0, 0);

				for (int i = 0; i < columns; i++)
				{
					grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
					cell = new Label() { };
					cell.Text = "Col:" + i;
					cell.FontAttributes = FontAttributes.Bold;
					cell.BackgroundColor = Colors.Beige;
					grid.Add(cell, i + 1, 0);
				}
				return grid;
			});
			return template;
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue7742Model
	{
		public string Text { get; set; }
	}
}