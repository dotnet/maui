using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		UITextField GetPlatformControl(EntryHandler handler) =>
			(UITextField)handler.PlatformView;

		Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).Text = text;

		int GetPlatformCursorPosition(EntryHandler entryHandler)
		{
			var textField = GetPlatformControl(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.BeginningOfDocument, textField.SelectedTextRange.Start);

			return -1;
		}

		int GetPlatformSelectionLength(EntryHandler entryHandler)
		{
			var textField = GetPlatformControl(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.SelectedTextRange.Start, textField.SelectedTextRange.End);

			return -1;
		}

		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<ViewCell, ViewCellRenderer>();
					handlers.AddHandler<TextCell, TextCellRenderer>();
					handlers.AddHandler<ListView, ListViewRenderer>();
					handlers.AddHandler<TableView, TableViewRenderer>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<EntryCell, EntryCellRenderer>();
				});
			});
		}

		[Fact]
		public async Task NextListView()
		{
			SetupBuilder();

			var entry1 = new Entry
			{
				Text = "Entry 1",
				ReturnType = ReturnType.Next
			};

			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemsSource = new List<EntryCell>
				{
					new EntryCell () {Text="EntryCell1"},
					new EntryCell () {Text="EntryCell2"},
					new EntryCell () {Text="EntryCell3"},
				}
			};

			var layout = new VerticalStackLayout()
			{
				entry1,
				listView,
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				var firstResponder = layout.ToPlatform().FindFirstResponder();
				var field = firstResponder as UITextField;
				Assert.NotNull(field);
				Assert.True(field.Text == "EntryCell1");
				await Task.Delay(10);
			});
		}

		[Fact]
		public async Task NextListView2()
		{
			SetupBuilder();

			ObservableCollection<Entry> data = new ObservableCollection<Entry>()
			{
				new Entry {Text="cat" },
				new Entry {Text="dog" },
				new Entry {Text="catdog" },
			};

			var entry1 = new Entry
			{
				Text = "Entry 1",
				ReturnType = ReturnType.Next
			};

			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				//ItemTemplate = new DataTemplate() {
				//	new ViewCell (),
				//}

				//listView.DataTemplate = new DataTemplate() {
				//	new ViewCell() {
				//		View = new Entry(),
				//	};
				//};

				ItemTemplate = new DataTemplate(() =>
				{
					return new ViewCell()
					{
						View = new Entry ()
					};
				}),
				ItemsSource = data
			};

			var layout = new VerticalStackLayout()
			{
				entry1,
				listView,
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				var firstResponder = layout.ToPlatform().FindFirstResponder();
				var field = firstResponder as UITextField;
				Assert.NotNull(field);
				Assert.True(field.Text == "cat");
				await Task.Delay(10);
			});
		}

		[Fact]
		public async Task NextTableView()
		{
			SetupBuilder();

			var entry1 = new Entry
			{
				Text = "Entry 1",
				ReturnType = ReturnType.Next
			};

			var tableView = new TableView()
			{
				Root = new TableRoot("Table Title") {
					new TableSection ("Section 1 Title") {
						new EntryCell {
							Text = "EntryCell1",
						},
						new EntryCell {
							Text = "EntryCell2",
						},
						new EntryCell {
							Text = "EntryCell3",
						}
					},
				}
			};

			var layout = new VerticalStackLayout()
			{
				entry1,
				tableView,
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				var firstResponder = layout.ToPlatform().FindFirstResponder();
				var field = firstResponder as UITextField;
				Assert.NotNull(field);
				Assert.True(field.Text == "EntryCell1");
				await Task.Delay(10);
			});
		}
	}
}
