using System.Collections.ObjectModel;
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
					handlers.AddHandler<ViewCell, ViewCellRenderer>();
					handlers.AddHandler<ListView, ListViewRenderer>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
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
	}
}