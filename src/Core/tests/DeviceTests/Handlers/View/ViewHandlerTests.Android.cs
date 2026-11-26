using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Microsoft.Maui.DeviceTests.Stubs;
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
		public async Task OpacityChangesOnShadowedWrapperInvalidateWithoutCreatingGlobalReferences()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var wrapper = new InvalidationCountingWrapperView(MauiContext.Context);
				wrapper.AddView(new AView(MauiContext.Context));
				wrapper.Shadow = new ShadowStub { Paint = new SolidPaint(Colors.Black), Radius = 10, Opacity = 1 };

				await wrapper.AttachAndRun(async () =>
				{
					wrapper.InvalidateCount = 0;
					var globalRefsBefore = JniRuntime.CurrentRuntime.GlobalReferenceCount;

					// One synchronous burst, the way an opacity animation writes frame after frame.
					for (int i = 0; i < 200; i++)
					{
						wrapper.UpdateOpacity(i % 2 == 0 ? 0.5 : 1.0);
					}

					var newGlobalRefs = JniRuntime.CurrentRuntime.GlobalReferenceCount - globalRefsBefore;
					Assert.True(newGlobalRefs < 20, $"200 opacity changes created {newGlobalRefs} JNI global references.");

					// The shadow still has to be redrawn after the opacity changes.
					for (int i = 0; i < 60 && wrapper.InvalidateCount == 0; i++)
					{
						await Task.Delay(16);
					}

					Assert.True(wrapper.InvalidateCount > 0, "The shadowed wrapper was not invalidated after its opacity changed.");
				});
			});
		}

		class InvalidationCountingWrapperView : WrapperView
		{
			public InvalidationCountingWrapperView(Context context) : base(context)
			{
			}

			public int InvalidateCount { get; set; }

			public override void Invalidate()
			{
				InvalidateCount++;
				base.Invalidate();
			}
		}
	}
}