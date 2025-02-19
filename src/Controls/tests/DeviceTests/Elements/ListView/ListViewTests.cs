using System;
using System.Collections.Generic;
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
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<EntryCell, EntryCellRenderer>();
					handlers.AddHandler<ViewCell, ViewCellRenderer>();
					handlers.AddHandler<TextCell, TextCellRenderer>();
					handlers.AddHandler<ListView, ListViewRenderer>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Entry, EntryHandler>();
				});
			});
		}


		[Fact
#if ANDROID
			(Skip = "https://github.com/dotnet/maui/issues/24701")
#endif
		]
		public async Task ChangingTemplateTypeDoesNotCrash()
		{
			SetupBuilder();
			ObservableCollection<string> data1 = new ObservableCollection<string>()
			{
				"cat",
				"dog",
			};
			ObservableCollection<string> data2 = new ObservableCollection<string>()
			{
				"dog",
				"cat",
			};

			var template1 = new DataTemplate(() =>
			{
				return new ViewCell()
				{
					View = new Label()
				};
			});

			var template2 = new DataTemplate(() =>
			{
				return new ViewCell()
				{
					View = new Entry()
				};
			});

			var listView = new ListView()
			{
				HasUnevenRows = true,
				ItemTemplate = new FunctionalDataTemplateSelector((item, container) =>
				{
					return item.ToString() == "cat" ? template1 : template2;
				}),
				IsGroupingEnabled = true,
				ItemsSource = new ObservableCollection<ObservableCollection<string>>() { data1 }
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(new VerticalStackLayout() { listView }, async (handler) =>
			{
				listView.ItemsSource = new ObservableCollection<ObservableCollection<string>>() { data2 };
				await Task.Delay(5000);
			});
		}

		[Fact]
		public async Task RemovingFirstItemOfListViewDoesntCrash()
		{
			SetupBuilder();
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
			SetupBuilder();
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
			SetupBuilder();
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
			SetupBuilder();
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
			SetupBuilder();
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

		[Fact("Cells Do Not Leak"
#if !WINDOWS
			, Skip = "Skip for now on other platforms, due to how cells are recycled this does not pass."
#endif
		)]
		public async Task CellsDoNotLeak()
		{
			SetupBuilder();

			var references = new List<WeakReference>();
			var listView = new ListView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new TextCell();
					references.Add(new(cell));
					return cell;
				})
			};

			await CreateHandlerAndAddToWindow<ListViewRenderer>(listView, async _ =>
			{
				listView.ItemsSource = new[] { 1, 2, 3 };
				await Task.Delay(100);
				ValidatePlatformCells(listView);
				listView.ItemsSource = null;
				await Task.Delay(100);
				ValidatePlatformCells(listView);
			});

			await AssertionExtensions.WaitForGC(references.ToArray());
		}

		[Fact("Cells Repopulate After Null ItemsSource")]
		public async Task CellsRepopulateAfterNullItemsSource()
		{
			SetupBuilder();

			List<TextCell> cells = null;

			var listView = new ListView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new TextCell();
					cell.SetBinding(TextCell.TextProperty, new Binding("."));
					cells?.Add(cell);
					return cell;
				})
			};

			await CreateHandlerAndAddToWindow<ListViewRenderer>(listView, async _ =>
			{
				listView.ItemsSource = new[] { 1, 2, 3 };
				await Task.Delay(100);
				ValidatePlatformCells(listView);
				listView.ItemsSource = null;
				await Task.Delay(100);
				ValidatePlatformCells(listView);

				// Now track the new cells
				cells = new();
				listView.ItemsSource = new[] { 4, 5, 6 };
				await Task.Delay(100);
				ValidatePlatformCells(listView);

				Assert.Equal(3, cells.Count);
				Assert.Equal("4", cells[0].Text);
				Assert.Equal("5", cells[1].Text);
				Assert.Equal("6", cells[2].Text);
			});
		}

		class FunctionalDataTemplateSelector : DataTemplateSelector
		{
			public Func<object, BindableObject, DataTemplate> Selector { get; }

			public FunctionalDataTemplateSelector(Func<object, BindableObject, DataTemplate> selectTemplate)
			{
				Selector = selectTemplate;
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				return Selector.Invoke(item, container);
			}
		}
	}
}
