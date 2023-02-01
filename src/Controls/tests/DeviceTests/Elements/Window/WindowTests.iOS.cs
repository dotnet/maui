using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowTests
	{
		[Theory]
		[InlineData(typeof(Editor))]
		[InlineData(typeof(Entry))]
		[InlineData(typeof(SearchBar))]
		public async Task FocusedTextInputAddsResignFirstResponderGesture(Type controlType)
		{
			SetupBuilder();
			var layout = new VerticalStackLayout();
			var view = (View)Activator.CreateInstance(controlType);
			layout.Children.Add(view);

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await OnLoadedAsync(view);
				view.Focus();
				await AssertionExtensions.WaitForFocused(view);
				Assert.Contains(typeof(ResignFirstResponderTouchGestureRecognizer),
					handler.PlatformView.Window.GestureRecognizers.Select(x => x.GetType()));

				// Work around bug where iOS elements aren't toggling the "IsFocused" property
				(view as IView).IsFocused = true;
				view.Unfocus();
				await AssertionExtensions.WaitForUnFocused(view);

				Assert.DoesNotContain(typeof(ResignFirstResponderTouchGestureRecognizer),
					handler.PlatformView.Window.GestureRecognizers.Select(x => x.GetType()));
			});
		}

		[Theory]
		[InlineData(typeof(Editor))]
		[InlineData(typeof(Entry))]
		[InlineData(typeof(SearchBar))]
		public async Task RemovingControlFromWindowRemovesGesture(Type controlType)
		{
			SetupBuilder();
			var layout = new VerticalStackLayout();
			var view = (View)Activator.CreateInstance(controlType);

			layout.Children.Add(view);

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await OnLoadedAsync(view);
				view.Focus();
				await AssertionExtensions.WaitForFocused(view);
				Assert.Contains(typeof(ResignFirstResponderTouchGestureRecognizer),
					handler.PlatformView.Window.GestureRecognizers.Select(x => x.GetType()));

				layout.Remove(view);
				await OnUnloadedAsync(view);

				Assert.DoesNotContain(typeof(ResignFirstResponderTouchGestureRecognizer),
					handler.PlatformView.Window.GestureRecognizers.Select(x => x.GetType()));
			});
		}
	}
}
