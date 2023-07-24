 using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		const string EntryTest = nameof(EntryTest);
		const string EntryToClick = "EntryToClick";
		const string EntryToClick2 = "EntryToClick2";
		const string CreateTopTabButton = "CreateTopTabButton";
		const string CreateBottomTabButton = "CreateBottomTabButton";

		const string EntrySuccess = "EntrySuccess";
		const string ResetKeyboard = "Hide Keyboard";
		const string ResetKeyboard2 = "Hide Keyboard 2";

		public MainPage()
		{
			InitializeComponent();
			this.Content = CreateEntryInsetView();
		}


		View CreateEntryInsetView()
		{
			ScrollView view = null;
			var content = new Grid();

			view = new ScrollView()
			{
				Content = content
			};

			content.AddRowDefinition(new RowDefinition());
			content.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });
			content.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });
			content.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });
			content.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });
			content.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });
			content.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });
			content.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });
			content.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });
			content.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });
			content.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });

			content.AddChild(new Label() { AutomationId = EntrySuccess, VerticalOptions = LayoutOptions.Fill, Text = "Click the entry and it should scroll up and stay visible. Click off entry and this label should still be visible" },
					0, 0);

			content.AddChild(new Button() { Text = "Change Navbar Visible", Command = new Command(() => Shell.SetNavBarIsVisible(view.Parent, !(Shell.GetNavBarIsVisible(view.Parent)))) }, 0, 1);
			content.AddChild(new Button()
			{
				Text = "Push On Page",
				Command = new Command(() => Navigation.PushAsync(new ContentPage() { Content = CreateEntryInsetView() }))
			}, 0, 2);
			content.AddChild(new Button() { Text = "Reset", Command = new Command(() => { }) }, 0, 3);
			content.AddChild(new Button()
			{
				Text = ResetKeyboard

			}, 0, 4);
			content.AddChild(new Entry()
			{
				AutomationId = EntryToClick
			}, 0, 5);
			content.AddChild(new Button()
			{
				Text = ResetKeyboard,
				AutomationId = ResetKeyboard2

			}, 0, 6);
			content.AddChild(new Button()
			{
				Text = "Top Tab",
				AutomationId = CreateTopTabButton,
			}, 0, 7);
			content.AddChild(new Button()
			{
				Text = "Bottom Tab",
				AutomationId = CreateBottomTabButton,
			}, 0, 8);
			content.AddChild(new Entry()
			{
				AutomationId = EntryToClick2
			}, 0, 9);

			return view;
		}
	}


	public static class GridExtension
	{
		public static void AddChild(this Grid grid, View view, int column, int row, int columnspan = 1, int rowspan = 1)
		{
			if (row < 0)
			{
				throw new ArgumentOutOfRangeException("row");
			}
			if (column < 0)
			{
				throw new ArgumentOutOfRangeException("column");
			}
			if (rowspan <= 0)
			{
				throw new ArgumentOutOfRangeException("rowspan");
			}
			if (columnspan <= 0)
			{
				throw new ArgumentOutOfRangeException("columnspan");
			}
			if (view == null)
			{
				throw new ArgumentNullException("view");
			}

			Grid.SetRow(view, row);
			Grid.SetRowSpan(view, rowspan);
			Grid.SetColumn(view, column);
			Grid.SetColumnSpan(view, columnspan);
			grid.Children.Add(view);
		}
	}
}