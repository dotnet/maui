using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.CarouselView)]
	public partial class CarouselViewTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CarouselView, CarouselViewHandler>();
					handlers.AddHandler<IContentView, ContentViewHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});
		}

		[Fact]
		public async Task CarouselViewDataTemplateSelectorSelectorNoCrash()
		{
			SetupBuilder();

			ObservableCollection<int> data = new ObservableCollection<int>()
			{
				1,
				2,
			};

			var template1 = new DataTemplate(() =>
			{
				return new Grid()
				{
					new Label()
				};
			});

			var template2 = new DataTemplate(() =>
			{
				return new Grid()
				{
					new Label()
				};
			});

			var carouselView = new CarouselView()
			{
				ItemTemplate = new CustomDataTemplateSelectorSelector
				{
					Template1 = template1,
					Template2 = template2
				},
				ItemsSource = data
			};

			var layout = new Grid()
			{
				carouselView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await Task.Delay(100);
				Assert.NotNull(handler.PlatformView);
			});
		}

		[Fact]
		public async Task HiddenCarouselViewNoCrash()
		{
			SetupBuilder();

			ObservableCollection<int> data = new ObservableCollection<int>()
			{
				1,
				2,
			};

			var template = new DataTemplate(() =>
			{
				return new Grid()
				{
					new Label()
				};
			});

			var carouselView = new CarouselView()
			{
				ItemTemplate = template,
				ItemsSource = data
			};

			var layout = new Grid()
			{
				carouselView,
			};

			layout.IsVisible = false;

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await Task.Delay(100);
				Assert.NotNull(handler.PlatformView);
			});
		}

#if !ANDROID //https://github.com/dotnet/maui/pull/24610
		[Fact]
		public async void DisconnectedCarouselViewDoesNotHookCollectionViewChanged()
		{
			SetupBuilder();

			CollectionChangedObservableCollection<int> data = new CollectionChangedObservableCollection<int>()
			{
				1,
				2,
			};

			var template = new DataTemplate(() =>
			{
				return new Grid()
				{
					new Label()
				};
			});

			var carouselView = new CarouselView()
			{
				ItemTemplate = template,
				ItemsSource = data
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async (handler) =>
			{
				await Task.Delay(100);
				Assert.NotNull(handler.PlatformView);
				Assert.False(data.IsCollectionChangedEventEmpty);
			});

			carouselView.Handler?.DisconnectHandler();

			Assert.True(data.IsCollectionChangedEventEmpty);
		}
#endif
	}

	internal class CustomDataTemplateSelectorSelector : DataTemplateSelector
	{
		public DataTemplate Template1 { get; set; }
		public DataTemplate Template2 { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var value = (int)item;

			if (value == 1)
				return Template1;
			else
				return Template2;
		}
	}

	internal class CollectionChangedObservableCollection<T> : ObservableCollection<T>, INotifyCollectionChanged
	{
		NotifyCollectionChangedEventHandler collectionChanged;

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { collectionChanged += value; base.CollectionChanged += value; }
			remove { collectionChanged -= value; base.CollectionChanged -= value; }
		}

		public bool IsCollectionChangedEventEmpty => collectionChanged is null;
	}
}
