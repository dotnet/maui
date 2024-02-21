using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.DeviceTests.TestCases;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		static UITextField GetPlatformControl(EntryHandler handler) =>
			(UITextField)handler.PlatformView;

		static Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		static void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).Text = text;

		static int GetPlatformCursorPosition(EntryHandler entryHandler)
		{
			var textField = GetPlatformControl(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.BeginningOfDocument, textField.SelectedTextRange.Start);

			return -1;
		}

		static int GetPlatformSelectionLength(EntryHandler entryHandler)
		{
			var textField = GetPlatformControl(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.SelectedTextRange.Start, textField.SelectedTextRange.End);

			return -1;
		}

		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
		public class ScrollTests : ControlsHandlerTestBase
		{
			[Fact]
			public async Task ScrollSkipsAlert()
			{
				var entry = new Entry();

				EnsureHandlerCreated(builder =>
				{
					builder.ConfigureMauiHandlers(handler =>
					{
						handler.AddHandler<VerticalStackLayout, LayoutHandler>();
						handler.AddHandler<Entry, EntryHandler>();
					});
				});

				var layout = new VerticalStackLayout
			{
				entry
			};

				layout.WidthRequest = 300;
				layout.HeightRequest = 800;

				await InvokeOnMainThreadAsync(async () =>
				{
					var contentViewHandler = CreateHandler<LayoutHandler>(layout);
					await contentViewHandler.PlatformView.AttachAndRun(async () =>
					{
						var initialEntryBox = entry.GetBoundingBox();
						var layoutPlat = layout.ToPlatform();
						var win = layoutPlat.Window;
						if (win is UIWindow uIWindow)
						{
							var alert = UIAlertController.Create("Popup Alert", "This is a popup", UIAlertControllerStyle.Alert);

							alert.AddTextField((textfield) =>
							{
								textfield.Placeholder = "Placeholder Text";
							});

							await uIWindow.RootViewController.PresentViewControllerAsync(alert, true);

							var finalEntryBox = entry.GetBoundingBox();

							var taskCompletion = new TaskCompletionSource<bool>();

							uIWindow.RootViewController.DismissViewController(true, () => { taskCompletion.SetResult(true); });

							await taskCompletion.Task;

							Assert.True(initialEntryBox == finalEntryBox);
							Assert.False(KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling);
						}
					});
				});
			}
		}

		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
		public class NextKeyboardTests : ControlsHandlerTestBase
		{
			void SetupNextBuilder()
			{
				EnsureHandlerCreated(builder =>
				{
					builder.ConfigureMauiHandlers(handlers =>
					{
						handlers.AddHandler<ListView, ListViewRenderer>();
						handlers.AddHandler<TableView, TableViewRenderer>();
						handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
						handlers.AddHandler<Entry, EntryHandler>();
						handlers.AddHandler<EntryCell, EntryCellRenderer>();
					});
				});
			}

			[Fact]
			public async Task NextListView()
			{
				SetupNextBuilder();

				var entry1 = new Entry
				{
					Text = "Entry 1",
					ReturnType = ReturnType.Next
				};

				var listView = new ListView()
				{
					ItemTemplate = new DataTemplate(() =>
					{
						var cell = new EntryCell();
						cell.SetBinding(EntryCell.TextProperty, ".");
						return cell;
					}),
					ItemsSource = Enumerable.Range(0, 10).Select(i => $"EntryCell {i}").ToList()
				};

				var layout = new VerticalStackLayout()
				{
					entry1,
					listView,
				};

				layout.HeightRequest = 1000;
				layout.WidthRequest = 300;

				await InvokeOnMainThreadAsync(async () =>
				{
					var contentViewHandler = CreateHandler<LayoutHandler>(layout);
					await contentViewHandler.PlatformView.AttachAndRun(() =>
					{
						KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
						var firstResponder = layout.ToPlatform().FindFirstResponder();
						var field = firstResponder as UITextField;
						Assert.NotNull(field);
						Assert.True(field.Text == "EntryCell 0");
					});
				});
			}

			[Fact]
			public async Task NextTableView()
			{
				SetupNextBuilder();

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

				layout.HeightRequest = 1000;
				layout.WidthRequest = 300;

				await InvokeOnMainThreadAsync(async () =>
				{
					var contentViewHandler = CreateHandler<LayoutHandler>(layout);
					await contentViewHandler.PlatformView.AttachAndRun(() =>
					{
						KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
						var firstResponder = layout.ToPlatform().FindFirstResponder();
						var field = firstResponder as UITextField;
						Assert.NotNull(field);
						Assert.True(field.Text == "EntryCell1");
					});
				});
			}
		}

		[Category(TestCategory.Entry)]
		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
		public partial class EntryTestsWithWindow : ControlsHandlerTestBase
		{
			[Theory]
			[ClassData(typeof(ControlsPageTypesTestCases))]
			public async Task NextMovesToNextEntry(ControlsPageTypesTestCase page)
			{
				bool isFocused = false;
				EnsureHandlerCreated(builder =>
				{
					ControlsPageTypesTestCases.Setup(builder);
					builder.ConfigureMauiHandlers(handlers =>
					{
						handlers.AddHandler(typeof(Entry), typeof(EntryHandler));
					});
				});

				var entry1 = new Entry
				{
					Text = "Entry 1",
					ReturnType = ReturnType.Next
				};

				var entry2 = new Entry
				{
					Text = "Entry 2",
					ReturnType = ReturnType.Next
				};

				ContentPage contentPage = new ContentPage()
				{
					Content = new VerticalStackLayout()
					{
						entry1,
						entry2
					}
				};

				Page rootPage = ControlsPageTypesTestCases.CreatePageType(page, contentPage);

				await CreateHandlerAndAddToWindow(rootPage, async () =>
				{
					KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform());
					await AssertEventually(() => entry2.IsFocused);
					isFocused = entry2.IsFocused;
				});

				Assert.True(isFocused, $"{page} failed to focus the second entry DANG");
			}
		}
	}
}