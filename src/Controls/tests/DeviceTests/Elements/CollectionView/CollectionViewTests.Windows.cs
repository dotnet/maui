using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Xunit;
using WSetter = Microsoft.UI.Xaml.Setter;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CollectionViewTests
	{
		[Fact(DisplayName = "CollectionView Disconnects Correctly")]
		public async Task CollectionViewHandlerDisconnects()
		{
			SetupBuilder();

			ObservableCollection<string> data = new ObservableCollection<string>()
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};

			var collectionView = new CollectionView()
			{
				ItemTemplate = new Controls.DataTemplate(() =>
				{
					return new VerticalStackLayout()
					{
						new Label()
					};
				}),
				SelectionMode = SelectionMode.Single,
				ItemsSource = data
			};

			var layout = new VerticalStackLayout()
			{
				collectionView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, (handler) =>
			{
				// Validate that no exceptions are thrown
				var collectionViewHandler = (IElementHandler)collectionView.Handler;
				collectionViewHandler.DisconnectHandler();

				((IElementHandler)handler).DisconnectHandler();

				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task ValidateItemContainerDefaultHeight()
		{
			SetupBuilder();
			ObservableCollection<string> data = new ObservableCollection<string>()
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};

			var collectionView = new CollectionView()
			{
				ItemTemplate = new Controls.DataTemplate(() =>
				{
					return new VerticalStackLayout()
					{
						new Label()
					};
				}),
				ItemsSource = data
			};

			var layout = new VerticalStackLayout()
			{
				collectionView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await Task.Delay(100);
				ValidateItemContainerStyle(collectionView);
			});
		}

		void ValidateItemContainerStyle(CollectionView collectionView)
		{
			var handler = (CollectionViewHandler)collectionView.Handler;
			var control = handler.PlatformView;

			var minHeight = control.ItemContainerStyle.Setters
				.OfType<WSetter>()
				.FirstOrDefault(X => X.Property == FrameworkElement.MinHeightProperty).Value;

			Assert.Equal(0d, minHeight);
		}
	}
}