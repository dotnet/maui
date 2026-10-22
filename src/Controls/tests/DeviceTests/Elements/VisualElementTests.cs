using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
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
		[Category(TestCategory.Lifecycle)]
#if IOS || MACCATALYST
		[Trait(RendererHandlerVariant.NavigationViewVariantTraitName, RendererHandlerVariant.NavigationRenderer)] // See RendererHandlerVariant.cs
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
							RegisterNavigationPageHandler(handlers);
#if WINDOWS || ANDROID
							handlers.AddHandler<Toolbar, ToolbarHandler>();
#endif
						});
			}

			// Extracted so an iOS/MacCatalyst-only subclass can swap in NavigationRenderer,
			// letting every NewWindowCollection test run against both the NavigationPage
			// renderer and handler. See VisualElementNavigationHandlerTests.iOS.cs and
			// RendererHandlerVariant.cs.
			protected virtual void RegisterNavigationPageHandler(IMauiHandlersCollection handlers)
			{
#if IOS || MACCATALYST
				handlers.AddHandler<NavigationPage, NavigationRenderer>();
#else
				handlers.AddHandler<NavigationPage, NavigationViewHandler>();
#endif
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

				await AssertEventually(() => loaded == 2 && unloaded == 2);
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

				await CreateHandlerAndAddToWindow<IElementHandler>(navPage, async (handler) =>
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

				await CreateHandlerAndAddToWindow<IElementHandler>(navPage, async (handler) =>
				{
					Assert.Equal(0, loaded);
					Assert.Equal(0, unloaded);

					await navPage.PushAsync(page);

					Assert.Equal(1, loaded);
					Assert.Equal(0, unloaded);

					await navPage.PopAsync();

					await AssertEventually(() => loaded == 1 && unloaded == 1);
				});
			}
		}
	}
}