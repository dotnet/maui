#pragma warning disable CS0618 // ListView, TableView and cells are obsolete but supported.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Items;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>The legacy ListView and TableView with cells, on real widgets.</summary>
	public class LegacyListTests
	{
		static IEnumerable<string> RowTexts(GtkListViewHandler handler) =>
			handler.BoundHosts.OrderBy(h => h.ListItem!.Position).Select(h => GtkTestHost.Find<global::Gtk.Label>(h).First().Text);

		[GtkFact]
		public void ListViewShowsTextCellsFromTheTemplate() => GtkTestHost.Run(() =>
		{
			var list = new ListView
			{
				ItemsSource = new[] { "one", "two", "three" },
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new TextCell();
					cell.SetBinding(TextCell.TextProperty, ".");
					cell.SetBinding(TextCell.DetailProperty, "Length");
					return cell;
				}),
			};
			using var host = new ControlsWindowHost(new ContentPage { Content = list }, 400, 300);
			host.Pump(10);

			var handler = Assert.IsType<GtkListViewHandler>(list.Handler);
			Assert.Equal(3, handler.Source!.Count);
			Assert.Equal(new[] { "one", "two", "three" }, RowTexts(handler));
			// The detail is a second label bound to the cell.
			Assert.Contains("3", handler.BoundHosts.SelectMany(h => GtkTestHost.Find<global::Gtk.Label>(h)).Select(l => l.Text));
			GtkTestHost.AssertNoComplaints();
		});

		[GtkFact]
		public void ListViewWithoutATemplateShowsToString() => GtkTestHost.Run(() =>
		{
			var list = new ListView { ItemsSource = new object[] { 1, "b" } };
			using var host = new ControlsWindowHost(new ContentPage { Content = list }, 400, 300);
			host.Pump(10);

			Assert.Equal(new[] { "1", "b" }, RowTexts((GtkListViewHandler)list.Handler!));
		});

		[GtkFact]
		public void ListViewViewCellsShowTheirView() => GtkTestHost.Run(() =>
		{
			var list = new ListView
			{
				ItemsSource = new[] { "a", "b" },
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label { HeightRequest = 24 };
					label.SetBinding(Label.TextProperty, ".");
					return new ViewCell { View = label };
				}),
			};
			using var host = new ControlsWindowHost(new ContentPage { Content = list }, 400, 300);
			host.Pump(10);

			Assert.Equal(new[] { "a", "b" }, RowTexts((GtkListViewHandler)list.Handler!));
		});

		[GtkFact]
		public void SelectionAndTapsReachTheListView() => GtkTestHost.Run(() =>
		{
			var list = new ListView { ItemsSource = new[] { "a", "b", "c" } };
			using var host = new ControlsWindowHost(new ContentPage { Content = list }, 400, 300);
			host.Pump(10);
			var handler = (GtkListViewHandler)list.Handler!;

			object? selected = null, tapped = null;
			list.ItemSelected += (_, e) => selected = e.SelectedItem;
			list.ItemTapped += (_, e) => tapped = e.Item;

			// The user selects a row.
			handler.Selection!.SelectItem(1, true);
			host.Pump();
			Assert.Equal("b", list.SelectedItem);
			Assert.Equal("b", selected);

			// The app selects a row.
			list.SelectedItem = "c";
			host.Pump();
			Assert.Equal(2u, handler.Selection.Selected);

			// Activating a row taps it.
			GLib.Signal.Emit(handler.List!, "activate", 0u);
			Assert.Equal("a", tapped);
		});

		[GtkFact]
		public void ListViewHeaderFooterAndGrouping() => GtkTestHost.Run(() =>
		{
			var group = new List<string> { "x", "y" };
			var list = new ListView
			{
				Header = "head",
				Footer = "foot",
				IsGroupingEnabled = true,
				ItemsSource = new List<List<string>> { group },
			};
			using var host = new ControlsWindowHost(new ContentPage { Content = list }, 400, 300);
			host.Pump(10);
			var handler = (GtkListViewHandler)list.Handler!;

			Assert.Equal("head", ((Label)handler.HeaderHost!.View!).Text);
			Assert.Equal("foot", ((Label)handler.FooterHost!.View!).Text);
			Assert.Equal(GtkRowKind.GroupHeader, handler.Source!.RowAt(0).Kind);
			Assert.Equal(3, handler.Source.Count);
		});

		[GtkFact]
		public void SwitchAndEntryCellsStayBoundToTheCell() => GtkTestHost.Run(() =>
		{
			var switchCell = new SwitchCell { Text = "wifi", On = false };
			var entryCell = new EntryCell { Label = "name", Text = "abc", Placeholder = "p" };
			var table = new TableView
			{
				Root = new TableRoot { new TableSection("Settings") { switchCell, entryCell } },
			};
			using var host = new ControlsWindowHost(new ContentPage { Content = table }, 400, 300);
			host.Pump(10);
			var handler = (GtkTableViewHandler)table.Handler!;

			// Title row + two cells.
			Assert.Equal(3, handler.Hosts.Count);
			Assert.Equal("Settings", ((Label)handler.Hosts[0].View!).Text);

			var toggle = GtkTestHost.Find<global::Gtk.Switch>(handler.Hosts[1]).Single();
			Assert.False(toggle.Active);
			switchCell.On = true;
			host.Pump();
			Assert.True(toggle.Active);
			toggle.Active = false;
			host.Pump();
			Assert.False(switchCell.On);

			var entry = GtkTestHost.Find<global::Gtk.Entry>(handler.Hosts[2]).Single();
			Assert.Equal("abc", entry.Text);
			entry.Text = "changed";
			host.Pump();
			Assert.Equal("changed", entryCell.Text);
		});

		[GtkFact]
		public void TappingATableCellRunsItsTapBehaviour() => GtkTestHost.Run(() =>
		{
			int taps = 0;
			var cell = new TextCell { Text = "tap me", Command = new Command(() => taps++) };
			var table = new TableView { Root = new TableRoot { new TableSection { cell } } };
			using var host = new ControlsWindowHost(new ContentPage { Content = table }, 400, 300);
			host.Pump(10);
			var handler = (GtkTableViewHandler)table.Handler!;

			var click = GtkTestHost.ControllerOf<global::Gtk.GestureClick>(handler.Hosts.Single());
			Assert.NotNull(click);
			GLib.Signal.Emit(click!, "released", 1, 1.0, 1.0);
			Assert.Equal(1, taps);
		});
	}
}
