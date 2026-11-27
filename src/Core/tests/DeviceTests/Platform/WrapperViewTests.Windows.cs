#nullable enable
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.View)]
	public class WrapperViewTests : HandlerTestBasement
	{
		[Theory]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, false)]
		[InlineData(false, true)]
		public Task PendingShadowMaskDoesNotAffectRemovedOrReplacedShadow(bool creating, bool replace)
			=> InvokeOnMainThreadAsync(async () =>
			{
				var shadow = new ShadowStub { Paint = new SolidPaint(Colors.Black), Opacity = 1 };
				// Set the shadow before assigning the child so the test can start and await creation itself.
				using var wrapper = new WrapperView { Shadow = shadow, Child = new TextBox() };
				var compositor = ElementCompositionPreview.GetElementVisual(wrapper).Compositor;
				if (!creating)
					await wrapper.CreateShadowAsync(Task.FromResult<CompositionBrush?>(null));

				var capture = new TaskCompletionSource<CompositionBrush?>();
				var pending = creating ? wrapper.CreateShadowAsync(capture.Task) : wrapper.UpdateShadowAsync(capture.Task);
				Assert.False(pending.IsCompleted);
				var oldHost = GetShadowHost(wrapper);

				wrapper.Shadow = null;
				UIElement? replacementHost = null;
				SpriteVisual? replacementVisual = null;
				using var currentMask = compositor.CreateColorBrush(Microsoft.UI.Colors.Black);
				var replacementSize = new Vector2(120, 45);
				if (replace)
				{
					// TextBlock mask generation is synchronous, so the setter finishes creating the replacement.
					wrapper.Child = new TextBlock();
					wrapper.Shadow = shadow;
					await wrapper.UpdateShadowAsync(Task.FromResult<CompositionBrush?>(currentMask));
					replacementHost = GetShadowHost(wrapper);
					replacementVisual = GetShadowVisual(replacementHost);
					Assert.Same(currentMask, Assert.IsAssignableFrom<DropShadow>(replacementVisual.Shadow).Mask);
					// A stale update must not run UpdateShadowSize against the replacement.
					replacementVisual.Size = replacementSize;
				}

				capture.SetResult(compositor.CreateColorBrush(Microsoft.UI.Colors.White));
				await pending;

				Assert.Null(ElementCompositionPreview.GetElementChildVisual(oldHost));
				if (replace)
				{
					Assert.Same(replacementHost, GetShadowHost(wrapper));
					var visual = GetShadowVisual(replacementHost!);
					Assert.Same(replacementVisual, visual);
					Assert.Same(currentMask, Assert.IsAssignableFrom<DropShadow>(visual.Shadow).Mask);
					Assert.Equal(replacementSize, visual.Size);
				}
				else
				{
					Assert.False(wrapper.HasShadow);
					Assert.IsType<TextBox>(Assert.Single(wrapper.CachedChildren));
				}
			});

		[Fact]
		public Task PendingShadowMaskIsAppliedWhenReconnectRetainsShadow()
			=> InvokeOnMainThreadAsync(async () =>
			{
				var label = new LabelStub
				{
					Text = "Shadow",
					Shadow = new ShadowStub { Paint = new SolidPaint(Colors.Black), Opacity = 1 }
				};
				var handler = CreateHandler<LabelHandler>(label);
				using var wrapper = Assert.IsType<WrapperView>(handler.ContainerView);
				var oldChild = wrapper.Child;
				var host = GetShadowHost(wrapper);
				var visual = GetShadowVisual(host);
				var dropShadow = Assert.IsAssignableFrom<DropShadow>(visual.Shadow);
				var capture = new TaskCompletionSource<CompositionBrush?>();
				var pending = wrapper.UpdateShadowAsync(capture.Task);
				Assert.False(pending.IsCompleted);

				((IElementHandler)handler).DisconnectHandler();
				handler.SetVirtualView(label);
				Assert.Same(wrapper, handler.ContainerView);
				Assert.NotSame(oldChild, wrapper.Child);

				using var mask = visual.Compositor.CreateColorBrush(Microsoft.UI.Colors.Black);
				capture.SetResult(mask);
				await pending;

				Assert.Same(host, GetShadowHost(wrapper));
				Assert.Same(visual, GetShadowVisual(host));
				Assert.Same(dropShadow, visual.Shadow);
				Assert.Same(mask, dropShadow.Mask);
				((IElementHandler)handler).DisconnectHandler();
			});

		static UIElement GetShadowHost(WrapperView wrapper)
		{
			var canvas = Assert.IsType<Canvas>(wrapper.CachedChildren[0]);
			return Assert.Single(canvas.Children);
		}

		static SpriteVisual GetShadowVisual(UIElement host)
			=> Assert.IsAssignableFrom<SpriteVisual>(ElementCompositionPreview.GetElementChildVisual(host));
	}
}
