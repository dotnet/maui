using System.Threading.Tasks;
using Android.Graphics.Drawables;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LayoutTests
	{
		[Fact, Category(TestCategory.Layout)]
		public async Task NestedButtonHasExpectedIconPosition()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();
					handlers.AddHandler<Layout, LayoutHandler>();
					handlers.AddHandler<Button, ButtonHandler>();
				});
			});

			var button = new Button()
			{
				Text = "Hello",
				ImageSource = "red.png",
				ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 8)
			};

			var page = new ContentPage()
			{
				Content = new HorizontalStackLayout()
				{
					button
				}
			};

			await InvokeOnMainThreadAsync(async () =>
				await page.ToPlatform(MauiContext).AttachAndRun(() =>
				{
					var handler = CreateHandler<ButtonHandler>(button);

					var platformButton = (AppCompatButton)handler.PlatformView;

					Drawable[] drawables = TextViewCompat.GetCompoundDrawablesRelative(platformButton);
					var rightDrawable = drawables[2];

					// Assert that the image is on the right
					Assert.NotNull(drawables[2]);
				})
			);
		}

		void ValidateInputTransparentOnPlatformView(IView view)
		{
			var handler = view.ToHandler(MauiContext);
			if (handler.PlatformView is LayoutViewGroup lvg)
			{
				Assert.Equal(view.InputTransparent, lvg.InputTransparent);
				if (handler.ContainerView is WrapperView wv)
					Assert.False(wv.InputTransparent);

				return;
			}

			if (view.InputTransparent)
			{
				Assert.True(view.ToPlatform(MauiContext) is WrapperView wv && wv.InputTransparent);
			}
			else
			{
				Assert.True(view.ToPlatform(MauiContext) is not WrapperView wv || !wv.InputTransparent);
			}
		}
	}
}
