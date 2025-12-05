using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 31106, "[MacCatalyst] Picker dialog closes automatically with VoiceOver/Keyboard", PlatformAffected.macOS)]
	public partial class Issue31106 : ContentPage
	{
		private List<string> _items;
		private int _openCount = 0;
		private int _closeCount = 0;

		public Issue31106()
		{
			InitializeComponent();

			_items = new List<string>
			{
				"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5"
			};

			TestPicker.ItemsSource = _items;
			TestPicker.SelectedIndexChanged += OnPickerSelectedIndexChanged;

			// Monitor when picker opens/closes
			TestPicker.Focused += OnPickerFocused;
			TestPicker.Unfocused += OnPickerUnfocused;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
			{
				LogMessage("Page appeared");
				CaptureState("OnAppearing");
			});
		}

		private void OnOpenPickerClicked(object sender, EventArgs e)
		{
			LogMessage("=== OPENING PICKER PROGRAMMATICALLY ===");
			TestPicker.Focus();

			Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
			{
				CaptureState("AfterProgrammaticOpen");
			});
		}

		private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedIndex = TestPicker.SelectedIndex;
			var selectedItem = selectedIndex >= 0 ? _items[selectedIndex] : "None";

			LogMessage($"SelectedIndexChanged: Index={selectedIndex}, Item={selectedItem}");
			StatusLabel.Text = $"Status: Selected '{selectedItem}' (Index {selectedIndex})";
		}

		private void OnPickerFocused(object sender, FocusEventArgs e)
		{
			_openCount++;
			LogMessage($"=== PICKER FOCUSED (Open #{_openCount}) ===");

			Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
			{
				LogMessage($"  IsFocused: {TestPicker.IsFocused}");
				LogMessage($"  SelectedIndex: {TestPicker.SelectedIndex}");
			});
		}

		private void OnPickerUnfocused(object sender, FocusEventArgs e)
		{
			_closeCount++;
			LogMessage($"=== PICKER UNFOCUSED (Close #{_closeCount}) ===");

			Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
			{
				LogMessage($"  IsFocused: {TestPicker.IsFocused}");
				LogMessage($"  SelectedIndex: {TestPicker.SelectedIndex}");

				// Check for premature close
				if (_openCount == _closeCount && _closeCount > 0)
				{
					LogMessage($"⚠️  Dialog closed {_closeCount} time(s) - check if premature!");
				}
			});
		}

		private void CaptureState(string context)
		{
			Debug.WriteLine($"=== STATE CAPTURE: {context} ===");
			Debug.WriteLine($"  SelectedIndex: {TestPicker.SelectedIndex}");
			Debug.WriteLine($"  IsFocused: {TestPicker.IsFocused}");
			Debug.WriteLine($"  Items Count: {_items.Count}");
			Debug.WriteLine($"  Open Count: {_openCount}");
			Debug.WriteLine($"  Close Count: {_closeCount}");
			Debug.WriteLine("=== END STATE CAPTURE ===");

			LogMessage($"State captured at {context}");
		}

		private void LogMessage(string message)
		{
			var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
			var logLine = $"[{timestamp}] {message}";
			Debug.WriteLine(logLine);

			// Update UI log (keep last few lines)
			Dispatcher.Dispatch(() =>
			{
				var existingLog = LogLabel.Text;
				var lines = existingLog.Split('\n');
				var newLog = logLine;

				if (lines.Length > 0 && !string.IsNullOrEmpty(lines[0]))
				{
					// Keep last 5 lines
					var linesToKeep = Math.Min(4, lines.Length);
					for (int i = 0; i < linesToKeep; i++)
					{
						newLog += "\n" + lines[i];
					}
				}

				LogLabel.Text = newLog;
			});
		}
	}
}
