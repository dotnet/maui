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
#pragma warning disable CS0618 // Type or member is obsolete
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


		[Fact]
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
#pragma warning disable CS0618 // Type or member is obsolete
				return new ViewCell()
				{
					View = new Label()
				};
#pragma warning restore CS0618 // Type or member is obsolete
			});

			var template2 = new DataTemplate(() =>
			{
#pragma warning disable CS0618 // Type or member is obsolete
				return new ViewCell()
				{
					View = new Entry()
				};
#pragma warning restore CS0618 // Type or member is obsolete
			});

#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete

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

#pragma warning disable CS0618 // Type or member is obsolete
			var listView = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
#pragma warning disable CS0618 // Type or member is obsolete
					return new ViewCell()
					{
						View = new VerticalStackLayout()
						{
							new Label()
						}
					};
#pragma warning restore CS0618 // Type or member is obsolete
				}),
				ItemsSource = data
			};
#pragma warning restore CS0618 // Type or member is obsolete

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
#pragma warning disable CS0618 // Type or member is obsolete
			var listView = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete
				})
			};
#pragma warning restore CS0618 // Type or member is obsolete

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

#pragma warning disable CS0618 // Type or member is obsolete
			var listView = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
#pragma warning disable CS0618 // Type or member is obsolete
					var cell = new EntryCell();
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
					cell.SetBinding(EntryCell.TextProperty, "Value");
#pragma warning restore CS0618 // Type or member is obsolete
					return cell;
				}),
				ItemsSource = data
			};
#pragma warning restore CS0618 // Type or member is obsolete

			var layout = new VerticalStackLayout()
			{
				listView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await Task.Yield();
#pragma warning disable CS0618 // Type or member is obsolete
				var entryCell = listView.TemplatedItems[0] as EntryCell;
#pragma warning restore CS0618 // Type or member is obsolete

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

#pragma warning disable CS0618 // Type or member is obsolete
			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemTemplate = new DataTemplate(() =>
				{
#pragma warning disable CS0618 // Type or member is obsolete
					return new ViewCell()
					{
						View = new VerticalStackLayout()
						{
							new Label()
						}
					};
#pragma warning restore CS0618 // Type or member is obsolete
				}),
				HasUnevenRows = true,
				ItemsSource = data
			};
#pragma warning restore CS0618 // Type or member is obsolete

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

#pragma warning disable CS0618 // Type or member is obsolete
			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemTemplate = new DataTemplate(() =>
				{
#pragma warning disable CS0618 // Type or member is obsolete
					return new ViewCell
					{
						View = new VerticalStackLayout
						{
							new Label()
						}
					};
#pragma warning restore CS0618 // Type or member is obsolete
				}),
				ItemsSource = data
			};
#pragma warning restore CS0618 // Type or member is obsolete

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
#pragma warning disable CS0618 // Type or member is obsolete
			var listView = new ListView
			{
				ItemTemplate = new DataTemplate(() =>
				{
#pragma warning disable CS0618 // Type or member is obsolete
					var cell = new TextCell();
#pragma warning restore CS0618 // Type or member is obsolete
					references.Add(new(cell));
					return cell;
				})
			};
#pragma warning restore CS0618 // Type or member is obsolete

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

#pragma warning disable CS0618 // Type or member is obsolete
			List<TextCell> cells = null;
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
			var listView = new ListView
			{
				ItemTemplate = new DataTemplate(() =>
				{
#pragma warning disable CS0618 // Type or member is obsolete
					var cell = new TextCell();
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
					cell.SetBinding(TextCell.TextProperty, new Binding("."));
#pragma warning restore CS0618 // Type or member is obsolete
					cells?.Add(cell);
					return cell;
				})
			};
#pragma warning restore CS0618 // Type or member is obsolete

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
#pragma warning restore CS0618 // Type or member is obsolete
}
