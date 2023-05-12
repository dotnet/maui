using System.Threading.Tasks;
using Android.Graphics.Drawables;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LayoutTests
	{
		[Fact, Category(TestCategory.Layout)]
		public async Task NestedButtonHasExpectedIconPosition()
		{
			var layout = new HorizontalStackLayout();

			var button = new Button()
			{
				Text = "Hello",
				ImageSource = "red.png",
				ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 80)
			};

			layout.Children.Add(button);

			int position = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<ButtonHandler>(button);

				var platformButton = (AppCompatButton)handler.PlatformView;

				Drawable[] drawables = TextViewCompat.GetCompoundDrawablesRelative(platformButton);
				var bounds = drawables[2].Bounds;
				return bounds.Left;
			});

			Assert.Equal(100, position);
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
