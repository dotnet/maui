using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ViewHandlerTests
	{
		[Fact]
		public async Task ChildIsVisibleIfWrapperIsVisible()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var child = new Button(MauiContext.Context);
				child.Visibility = ViewStates.Gone;

				var wrapper = new WrapperView(MauiContext.Context);
				wrapper.Visibility = ViewStates.Gone;
				wrapper.AddView(child);

				Assert.Equal(ViewStates.Gone, wrapper.Visibility);
				Assert.Equal(ViewStates.Gone, child.Visibility);

				wrapper.Visibility = ViewStates.Visible;

				Assert.Equal(ViewStates.Visible, wrapper.Visibility);
				Assert.Equal(ViewStates.Visible, child.Visibility);
			});
		}

		[Fact]
		public async Task ShadowInvalidationOnlyFollowsImmediateChild()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var wrapper = new WrapperView(MauiContext.Context);
				var child = new AView(MauiContext.Context);
				var descendant = new AView(MauiContext.Context);

				wrapper.SetSolidShadow(10, 0, 0, AColor.Black.ToArgb());

				Assert.True(wrapper.ShouldInvalidateShadow(child, child));
				Assert.False(wrapper.ShouldInvalidateShadow(child, descendant));

				wrapper.SetNoShadow();

				Assert.False(wrapper.ShouldInvalidateShadow(child, child));
			});
		}

		[Fact]
		public async Task InitialOpacityIsAppliedToNonWebViewContainer()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var view = new StubBase
				{
					Opacity = 0.5,
					Clip = new EllipseGeometryStub(new Point(50, 50), 50, 50),
				};
				var handler = CreateHandler(view);
				var container = Assert.IsType<WrapperView>(handler.ContainerView);

				Assert.Equal(0.5f, container.Alpha, 3);
				Assert.Equal(1f, handler.PlatformView.Alpha);

				view.Opacity = 0.25;
				handler.UpdateValue(nameof(IView.Opacity));

				Assert.Equal(0.25f, container.Alpha, 3);
				Assert.Equal(1f, handler.PlatformView.Alpha);
			});
		}
	}
}