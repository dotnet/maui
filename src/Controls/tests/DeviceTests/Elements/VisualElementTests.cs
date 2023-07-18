using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
#if IOS || MACCATALYST
using NavigationViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.VisualElement)]
	public partial class VisualElementTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task CanCreateHandler()
		{
			var image = new Image();
			await CreateHandlerAsync<ImageHandler>(image);
		}

		[Fact]
		public async Task SettingHandlerDoesNotThrow()
		{
			var image = new Image();
			var handler = await CreateHandlerAsync<ImageHandler>(image);
			image.Handler = handler;
		}

#if ANDROID || IOS || MACCATALYST
		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
		public class NewWindowCollection : ControlsHandlerTestBase
		{
			protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder builder)
			{

				return
					base
						.ConfigureBuilder(builder)
						.ConfigureMauiHandlers(handlers =>
						{
							handlers.AddHandler<NavigationPage, NavigationViewHandler>();
#if WINDOWS || ANDROID
							handlers.AddHandler<Toolbar, ToolbarHandler>();
#else
							handlers.AddHandler<NavigationPage, Controls.Handlers.Compatibility.NavigationRenderer>();
#endif
						});
			}

			[Fact]
			public async Task LoadedAndUnloadedFire()
			{
				var editor = new Editor();

				int unloaded = 0;
				int loaded = 0;
				editor.Loaded += (_, __) => loaded++;
				editor.Unloaded += (_, __) => unloaded++;

				await CreateHandlerAndAddToWindow<EditorHandler>(editor, (handler) =>
				{
					Assert.Equal(1, loaded);
					Assert.Equal(0, unloaded);
					return Task.CompletedTask;
				});

				Assert.Equal(1, loaded);
				Assert.Equal(1, unloaded);
			}

			[Fact]
			public async Task LoadedAndUnloadedFireWhenParentRemoved()
			{
				var editor = new Editor();
				var layout = new VerticalStackLayout()
				{
					editor
				};

				var parentLayout = new VerticalStackLayout()
				{
					layout
				};

				int unloaded = 0;
				int loaded = 0;
				editor.Loaded += (_, __) => loaded++;
				editor.Unloaded += (_, __) => unloaded++;

				await CreateHandlerAndAddToWindow<LayoutHandler>(parentLayout, async (handler) =>
				{
					parentLayout.Remove(layout);
					await OnUnloadedAsync(layout);
					await OnUnloadedAsync(editor);

					Assert.Equal(1, loaded);
					Assert.Equal(1, unloaded);

					parentLayout.Add(layout);
					await OnLoadedAsync(layout);
					await OnLoadedAsync(editor);

					Assert.Equal(2, loaded);
					Assert.Equal(1, unloaded);
				});

				await AssertionExtensions.Wait(() => loaded == 2 && unloaded == 2);

				Assert.Equal(2, loaded);
				Assert.Equal(2, unloaded);
			}

			[Fact]
			public async Task NavigatedToFiresAfterLoaded()
			{
				var navPage = new NavigationPage(new ContentPage());
				var page = new ContentPage();

				int loaded = 0;
				bool loadedFired = false;

				page.Loaded += (_, __) => loaded++;
				page.NavigatedTo += (_, __) => loadedFired = (loaded == 1);

				await CreateHandlerAndAddToWindow<NavigationViewHandler>(navPage, async (handler) =>
				{
					await navPage.PushAsync(page);
					Assert.True(loadedFired);
				});
			}

			[Fact]
			public async Task LoadedFiresOnPushedPage()
			{
				var navPage = new NavigationPage(new ContentPage());
				var page = new ContentPage();

				int unloaded = 0;
				int loaded = 0;
				page.Loaded += (_, __) => loaded++;
				page.Unloaded += (_, __) => unloaded++;

				await CreateHandlerAndAddToWindow<NavigationViewHandler>(navPage, async (handler) =>
				{
					Assert.Equal(0, loaded);
					Assert.Equal(0, unloaded);

					await navPage.PushAsync(page);

					Assert.Equal(1, loaded);
					Assert.Equal(0, unloaded);

					await navPage.PopAsync();

					await AssertionExtensions.Wait(() => loaded == 1 && unloaded == 1);
					Assert.Equal(1, loaded);
					Assert.Equal(1, unloaded);
				});
			}
		}
	}
}