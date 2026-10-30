using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Platform;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Picker, DatePicker and TimePicker: one test per mapped property, on real widgets.</summary>
	public class PickerHandlerTests
	{
		static string Css(global::Gtk.Widget widget) => GtkStyle.For(widget).ToCss();

		static Picker NewPicker(params string[] items)
		{
			var picker = new Picker();
			foreach (var item in items)
				picker.Items.Add(item);
			return picker;
		}

		// ------------------------------------------------------------------ Picker

		[GtkFact]
		public void PickerMapsItemsAndSelectedIndex() => GtkTestHost.Run(() =>
		{
			var picker = NewPicker("one", "two", "three");
			picker.SelectedIndex = 1;
			var widget = GtkTestApp.CreateWidget<MauiGtkPicker>(picker);

			Assert.Equal(3, widget.ItemCount);
			Assert.Equal("two", widget.GetItem(1));
			Assert.Equal(1, widget.SelectedIndex);

			picker.SelectedIndex = 2;
			Assert.Equal(2, widget.SelectedIndex);

			picker.Items.Add("four");
			Assert.Equal(4, widget.ItemCount);

			picker.Items.Clear();
			Assert.Equal(0, widget.ItemCount);
			Assert.Equal(-1, widget.SelectedIndex);
		});

		[GtkFact]
		public void PickerWritesTheUserSelectionBack() => GtkTestHost.Run(() =>
		{
			var picker = NewPicker("a", "b", "c");
			var widget = GtkTestApp.CreateWidget<MauiGtkPicker>(picker);

			widget.DropDown.Selected = 3;
			Assert.Equal(2, picker.SelectedIndex);
		});

		[GtkFact]
		public void PickerMapsTitleAsThePlaceholderRow() => GtkTestHost.Run(() =>
		{
			var picker = NewPicker("a", "b");
			picker.Title = "Pick one";
			var widget = GtkTestApp.CreateWidget<MauiGtkPicker>(picker);

			Assert.Equal("Pick one", widget.Title);
			Assert.Equal(2, widget.ItemCount);
			Assert.Equal(-1, widget.SelectedIndex);
			Assert.Equal(0u, widget.DropDown.Selected);

			picker.SelectedIndex = 1;
			Assert.Equal(1, widget.SelectedIndex);
			Assert.Equal(2u, widget.DropDown.Selected);

			picker.SelectedIndex = -1;
			Assert.Equal(0u, widget.DropDown.Selected);

			picker.Title = "Other";
			Assert.Equal("Other", widget.Title);
		});

		[GtkFact]
		public void PickerMapsTitleColor() => GtkTestHost.Run(() =>
		{
			var picker = NewPicker("a");
			picker.TitleColor = Colors.Orange;
			picker.TextColor = Colors.Blue;
			var widget = GtkTestApp.CreateWidget<MauiGtkPicker>(picker);

			// Nothing selected: the title colour; with a selection: the text colour.
			Assert.Contains("color: rgba(255,165,0,1)", Css(widget));

			picker.SelectedIndex = 0;
			Assert.Contains("color: rgba(0,0,255,1)", Css(widget));
		});

		[GtkFact]
		public void PickerMapsTextStyle() => GtkTestHost.Run(() =>
		{
			var picker = NewPicker("a");
			picker.TextColor = Colors.Red;
			picker.FontSize = 22;
			picker.FontAttributes = FontAttributes.Bold;
			picker.CharacterSpacing = 3;
			var widget = GtkTestApp.CreateWidget<MauiGtkPicker>(picker);

			var css = Css(widget);
			Assert.Contains("color: rgba(255,0,0,1)", css);
			Assert.Contains("font-size: 22px", css);
			Assert.Contains("font-weight: 700", css);
			Assert.Contains("letter-spacing: 3px", css);
		});

		[GtkFact]
		public void PickerMapsHorizontalAndVerticalTextAlignment() => GtkTestHost.Run(() =>
		{
			var picker = NewPicker("a");
			picker.Title = "t";
			picker.HorizontalTextAlignment = TextAlignment.End;
			picker.VerticalTextAlignment = TextAlignment.Start;
			var widget = GtkTestApp.CreateWidget<MauiGtkPicker>(picker);

			Assert.Equal(-1, widget.SelectedIndex);
		});

		[GtkFact]
		public void PickerMapsBackgroundAndIsEnabled() => GtkTestHost.Run(() =>
		{
			var picker = NewPicker("a");
			picker.Background = new SolidColorBrush(Colors.Yellow);
			picker.IsEnabled = false;
			var widget = GtkTestApp.CreateWidget<MauiGtkPicker>(picker);

			Assert.Contains("background-color: rgba(255,255,0,1)", Css(widget));
			Assert.False(widget.Sensitive);
		});

		[GtkLinuxFact]
		public void PickerMapsIsOpen() => GtkTestHost.Run(() =>
		{
			var picker = NewPicker("a", "b");
			using var host = new WindowHost(picker, 300, 200);
			host.Pump();

			picker.IsOpen = true;
			host.Pump();
			var widget = (MauiGtkPicker)host.Widget(picker);
			Assert.True(widget.Popover?.Visible ?? false);

			picker.IsOpen = false;
			host.Pump();
			Assert.False(widget.Popover?.Visible ?? false);
		});

		// -------------------------------------------------------------- DatePicker

		[GtkFact]
		public void DatePickerMapsDateAndFormat() => GtkTestHost.Run(() =>
		{
			var picker = new DatePicker { Date = new DateTime(2024, 5, 17), Format = "yyyy-MM-dd" };
			var widget = GtkTestApp.CreateWidget<MauiGtkDatePicker>(picker);

			Assert.Equal(new DateTime(2024, 5, 17), widget.Date);
			Assert.Equal("2024-05-17", widget.Label);

			picker.Format = "dd/MM/yyyy";
			Assert.Equal("17/05/2024", widget.Label);

			picker.Date = new DateTime(2025, 1, 2);
			Assert.Equal("02/01/2025", widget.Label);
		});

		[GtkFact]
		public void DatePickerMapsMinimumAndMaximumDate() => GtkTestHost.Run(() =>
		{
			var picker = new DatePicker
			{
				MinimumDate = new DateTime(2024, 1, 1),
				MaximumDate = new DateTime(2024, 12, 31),
				Date = new DateTime(2024, 6, 1),
			};
			var widget = GtkTestApp.CreateWidget<MauiGtkDatePicker>(picker);

			Assert.Equal(new DateTime(2024, 1, 1), widget.MinimumDate);
			Assert.Equal(new DateTime(2024, 12, 31), widget.MaximumDate);
			Assert.Equal(new DateTime(2024, 6, 1), widget.Date);

			// The widget never holds a date outside the range.
			widget.Date = new DateTime(2030, 1, 1);
			Assert.Equal(new DateTime(2024, 12, 31), widget.Date);
		});

		[GtkFact]
		public void DatePickerWritesTheUserDateBack() => GtkTestHost.Run(() =>
		{
			var picker = new DatePicker { Date = new DateTime(2024, 5, 17) };
			var widget = GtkTestApp.CreateWidget<MauiGtkDatePicker>(picker);

#pragma warning disable CS0612
			widget.Calendar.SelectDay(GLib.DateTime.FromSystemDateTime(new DateTime(2024, 5, 20)));
#pragma warning restore CS0612
			Assert.Equal(new DateTime(2024, 5, 20), picker.Date);
		});

		[GtkFact]
		public void DatePickerMapsTextStyle() => GtkTestHost.Run(() =>
		{
			var picker = new DatePicker { TextColor = Colors.Blue, FontSize = 18, CharacterSpacing = 2 };
			var widget = GtkTestApp.CreateWidget<MauiGtkDatePicker>(picker);

			var css = Css(widget);
			Assert.Contains("color: rgba(0,0,255,1)", css);
			Assert.Contains("font-size: 18px", css);
			Assert.Contains("letter-spacing: 2px", css);
		});

		[GtkFact]
		public void DatePickerBuildsAPopoverWithACalendar() => GtkTestHost.Run(() =>
		{
			var widget = GtkTestApp.CreateWidget<MauiGtkDatePicker>(new DatePicker());
			Assert.Same(widget.Calendar, widget.Editor.Child);
			Assert.Same(widget.Editor, widget.Popover);
		});

		[GtkLinuxFact]
		public void DatePickerMapsIsOpen() => GtkTestHost.Run(() =>
		{
			var picker = new DatePicker();
			using var host = new WindowHost(picker, 300, 200);
			host.Pump();

			picker.IsOpen = true;
			host.Pump();
			Assert.True(((MauiGtkDatePicker)host.Widget(picker)).Active);

			picker.IsOpen = false;
			host.Pump();
			Assert.False(((MauiGtkDatePicker)host.Widget(picker)).Active);
		});

		// -------------------------------------------------------------- TimePicker

		[GtkFact]
		public void TimePickerMapsTimeAndFormat() => GtkTestHost.Run(() =>
		{
			var picker = new TimePicker { Time = new TimeSpan(14, 35, 0), Format = "HH:mm" };
			var widget = GtkTestApp.CreateWidget<MauiGtkTimePicker>(picker);

			Assert.Equal(new TimeSpan(14, 35, 0), widget.Time);
			Assert.Equal(14, widget.Hours.ValueAsInt);
			Assert.Equal(35, widget.Minutes.ValueAsInt);
			Assert.Equal("14:35", widget.Label);

			picker.Format = "hh:mm tt";
			Assert.Equal(new DateTime(2000, 1, 1, 14, 35, 0).ToString("hh:mm tt"), widget.Label);

			picker.Time = new TimeSpan(7, 5, 0);
			Assert.Equal(7, widget.Hours.ValueAsInt);
			Assert.Equal(5, widget.Minutes.ValueAsInt);
		});

		[GtkFact]
		public void TimePickerWritesTheUserTimeBack() => GtkTestHost.Run(() =>
		{
			var picker = new TimePicker { Time = new TimeSpan(9, 0, 0) };
			var widget = GtkTestApp.CreateWidget<MauiGtkTimePicker>(picker);

			widget.Hours.Value = 18;
			Assert.Equal(new TimeSpan(18, 0, 0), picker.Time);
			widget.Minutes.Value = 45;
			Assert.Equal(new TimeSpan(18, 45, 0), picker.Time);
		});

		[GtkFact]
		public void TimePickerMapsTextStyle() => GtkTestHost.Run(() =>
		{
			var picker = new TimePicker { TextColor = Colors.Green, FontSize = 16, CharacterSpacing = 1 };
			var widget = GtkTestApp.CreateWidget<MauiGtkTimePicker>(picker);

			var css = Css(widget);
			Assert.Contains("color: rgba(0,128,0,1)", css);
			Assert.Contains("font-size: 16px", css);
			Assert.Contains("letter-spacing: 1px", css);
		});

		[GtkLinuxFact]
		public void TimePickerMapsIsOpen() => GtkTestHost.Run(() =>
		{
			var picker = new TimePicker();
			using var host = new WindowHost(picker, 300, 200);
			host.Pump();

			picker.IsOpen = true;
			host.Pump();
			Assert.True(((MauiGtkTimePicker)host.Widget(picker)).Active);

			picker.IsOpen = false;
			host.Pump();
			Assert.False(((MauiGtkTimePicker)host.Widget(picker)).Active);
		});

		[GtkFact]
		public void PickersLayOutWithoutComplaints() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();

			var stack = new VerticalStackLayout
			{
				Spacing = 6,
				Children =
				{
					NewPicker("a", "b"),
					new DatePicker { Date = new DateTime(2024, 1, 1) },
					new TimePicker { Time = new TimeSpan(10, 30, 0) },
				},
			};

			using var host = new WindowHost(stack, 400, 300);
			host.Pump();

			foreach (var child in stack.Children)
				Assert.True(host.Bounds((IView)child).Height > 0, child.GetType().Name + " has no height");

			GtkTestHost.AssertNoComplaints();
		});
	}
}
