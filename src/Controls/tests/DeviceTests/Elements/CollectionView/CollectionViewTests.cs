using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	[Category(TestCategory.CollectionView)]
	public partial class CollectionViewTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();

					handlers.AddHandler<CollectionView, CollectionViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});
		}

		[Fact]
		public async Task ItemsSourceDoesNotLeak()
		{
			SetupBuilder();

			IList logicalChildren = null;
			WeakReference weakReference = null;
			var collectionView = new CollectionView
			{
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				var data = new ObservableCollection<string>()
				{
					"Item 1",
					"Item 2",
					"Item 3"
				};
				weakReference = new WeakReference(data);
				collectionView.ItemsSource = data;
				await Task.Delay(100);

				// Get ItemsView._logicalChildren
				var flags = BindingFlags.NonPublic | BindingFlags.Instance;
				logicalChildren = typeof(ItemsView).GetField("_logicalChildren", flags).GetValue(collectionView) as IList;
				Assert.NotNull(logicalChildren);

				// Replace with cloned collection
				collectionView.ItemsSource = new ObservableCollection<string>(data);
				await Task.Delay(100);
			});

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.NotNull(weakReference);
			Assert.False(weakReference.IsAlive, "ObservableCollection should not be alive!");
			Assert.NotNull(logicalChildren);
			Assert.True(logicalChildren.Count <= 3, "_logicalChildren should not grow in size!");
		}
	}
}