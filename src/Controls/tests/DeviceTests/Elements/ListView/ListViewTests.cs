using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Category(TestCategory.ListView)]
	public partial class ListViewTests : ControlsHandlerTestBase
	{
		protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder) =>
			base.ConfigureBuilder(mauiAppBuilder)
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<EntryCell, EntryCellRenderer>();
					handlers.AddHandler<ViewCell, ViewCellRenderer>();
					handlers.AddHandler<TextCell, TextCellRenderer>();
					handlers.AddHandler<ListView, ListViewRenderer>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});

		[Fact]
		public async Task RemovingFirstItemOfListViewDoesntCrash()
		{
			ObservableCollection<string> data = new ObservableCollection<string>()
			{
				"cat",
				"dog",
				"catdog"
			};

			var listView = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
					return new ViewCell()
					{
						View = new VerticalStackLayout()
						{
							new Label()
						}
					};
				}),
				ItemsSource = data
			};

			var layout = new VerticalStackLayout()
			{
				listView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await Task.Delay(100);
				ValidatePlatformCells(listView);
				data.RemoveAt(0);
				await Task.Delay(100);
				ValidatePlatformCells(listView);
				data.Insert(1, "new");
				data.Insert(2, "record");
				await Task.Delay(100);
				ValidatePlatformCells(listView);
				data.RemoveAt(0);
				await Task.Delay(100);
				ValidatePlatformCells(listView);
			});
		}

		[Fact(DisplayName = "ReAssigning ListView in VSL Crashes")]
		public async Task ReAssigninListViewInVSLCrashes()
		{
			var listView = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
					return new ViewCell()
					{
						View = new VerticalStackLayout()
						{
							new Label()
							{
								Text = "Cat"
							}
						}
					};
				})
			};

			var layout = new VerticalStackLayout()
			{
				listView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				listView.ItemsSource = Enumerable.Range(1, 1);
				await Task.Delay(100);
				ValidatePlatformCells(listView);
				listView.ItemsSource = Enumerable.Range(1, 2);
				await Task.Delay(100);
				ValidatePlatformCells(listView);
			});
		}

		[Fact]
		public async Task EntryCellBindingCorrectlyUpdates()
		{
			var vm = new EntryCellBindingCorrectlyUpdatesVM();
			var data = new[]
			{
				vm
			};

			var listView = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new EntryCell();
					cell.SetBinding(EntryCell.TextProperty, "Value");
					return cell;
				}),
				ItemsSource = data
			};

			var layout = new VerticalStackLayout()
			{
				listView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await Task.Yield();
				var entryCell = listView.TemplatedItems[0] as EntryCell;

				// Initial Value is correct
				Assert.Equal(vm.Value, entryCell.Text);

				// Validate that the binding stays operational
				for (int i = 0; i < 3; i++)
				{
					vm.ChangeValue();
					await Task.Yield();
					Assert.Equal(vm.Value, entryCell.Text);
				}
			});
		}
		class EntryCellBindingCorrectlyUpdatesVM : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public string Value { get; set; }

			public void ChangeValue()
			{
				Value = Guid.NewGuid().ToString();
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
			}
		}

		[Fact]
		public async Task ClearItemsListViewDoesntCrash()
		{
			ObservableCollection<string> data = new ObservableCollection<string>()
			{
				"cat",
				"dog",
				"catdog"
			};

			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemTemplate = new DataTemplate(() =>
				{
					return new ViewCell()
					{
						View = new VerticalStackLayout()
						{
							new Label()
						}
					};
				}),
				HasUnevenRows = true,
				ItemsSource = data
			};

			var layout = new VerticalStackLayout()
			{
				listView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await Task.Delay(100);
				ValidatePlatformCells(listView);
				data.Clear();
				await Task.Delay(100);
			});
		}

		[Fact
#if WINDOWS
			(Skip = "Failing")
#endif
			]
		public async Task NullTemplateDoesntCrash()
		{
			ObservableCollection<string> data = new ObservableCollection<string>()
			{
				"cat",
				"dog",
				"catdog"
			};

			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemTemplate = new DataTemplate(() =>
				{
					return new ViewCell
					{
						View = new VerticalStackLayout
						{
							new Label()
						}
					};
				}),
				ItemsSource = data
			};

			var layout = new VerticalStackLayout
			{
				listView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				await Task.Delay(100);
				ValidatePlatformCells(listView);
			});

			// Default ctor
			await InvokeOnMainThreadAsync(() => listView.ItemTemplate = new DataTemplate());

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				await Task.Delay(100);
				ValidatePlatformCells(listView);
			});

			// Return null
			await InvokeOnMainThreadAsync(() => listView.ItemTemplate = null);

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				await Task.Delay(100);
				ValidatePlatformCells(listView);
			});
		}
	}
}