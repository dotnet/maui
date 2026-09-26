using System.Reflection;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ViewHandlerTests
	{
		[Fact]
		public void SafeAreaPixelComparisonUsesProvidedDisplayScale()
		{
			var subPixelInset = new SafeAreaPadding(0.24, 0, 0, 0);

			Assert.True(SafeAreaPadding.Empty.EqualsAtPixelLevel(subPixelInset, 2));
			Assert.False(SafeAreaPadding.Empty.EqualsAtPixelLevel(subPixelInset, 3));
		}

		[Theory]
		[InlineData(0.24, 2, false)]
		[InlineData(0.24, 3, true)]
		public void SafeAreaNonZeroComparisonUsesProvidedDisplayScale(double value, double scale, bool expected)
		{
			Assert.Equal(expected, SafeAreaPadding.IsNonZeroAtPixelLevel(value, scale));
		}

		[Fact]
		public async Task KeyboardTransitionsInvalidateDescendantSafeAreaCache()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var parent = new Microsoft.Maui.Platform.ContentView();
				using var child = new Microsoft.Maui.Platform.ContentView();
				parent.AddSubview(child);

				var cacheValidField = typeof(MauiView).GetField(
					"_blockedEdgesCacheValid",
					BindingFlags.Instance | BindingFlags.NonPublic);
				var keyboardWillShowMethod = typeof(MauiView).GetMethod(
					"OnKeyboardWillShow",
					BindingFlags.Instance | BindingFlags.NonPublic);
				var clearKeyboardStateMethod = typeof(MauiView).GetMethod(
					"ClearKeyboardState",
					BindingFlags.Instance | BindingFlags.NonPublic);

				Assert.NotNull(cacheValidField);
				Assert.NotNull(keyboardWillShowMethod);
				Assert.NotNull(clearKeyboardStateMethod);

				cacheValidField!.SetValue(child, true);
				using var keyboardFrame = NSValue.FromCGRect(new CGRect(0, 400, 400, 400));
				using var userInfo = NSDictionary.FromObjectAndKey(keyboardFrame, UIKeyboard.FrameEndUserInfoKey);
				using var notification = NSNotification.FromName(UIKeyboard.WillShowNotification, parent, userInfo);

				keyboardWillShowMethod!.Invoke(parent, [notification]);
				Assert.False((bool)cacheValidField.GetValue(child)!);

				cacheValidField.SetValue(child, true);
				clearKeyboardStateMethod!.Invoke(parent, null);
				Assert.False((bool)cacheValidField.GetValue(child)!);
			});
		}

		[Fact]
		public async Task NonUIControlDisablesUserInteractionWhenIsEnabledFalse()
		{
			var view = new StubBase();
			var handler = await CreateHandlerAsync(view);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.True(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.False(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.True(handler.PlatformView.UserInteractionEnabled);
			});
		}

		[Fact]
		public async Task NonUIControlKeepsInputTransparentAfterIsEnabledToggles()
		{
			var view = new StubBase { InputTransparent = true };
			var handler = await CreateHandlerAsync(view);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.False(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.False(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.False(handler.PlatformView.UserInteractionEnabled);
			});
		}

		[Theory]
		[InlineData(false, true)]
		[InlineData(true, false)]
		public async Task ContainerUserInteractionTracksIsEnabledAndInputTransparent(bool inputTransparent, bool expectedWhenEnabled)
		{
			var view = new StubBase
			{
				Clip = new PathShapeStub(),
				InputTransparent = inputTransparent
			};
			var handler = await CreateHandlerAsync(view);

			await InvokeOnMainThreadAsync(() =>
			{
				var containerView = Assert.IsType<WrapperView>(handler.ContainerView);

				Assert.Equal(expectedWhenEnabled, containerView.UserInteractionEnabled);

				view.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.False(containerView.UserInteractionEnabled);

				view.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.Equal(expectedWhenEnabled, containerView.UserInteractionEnabled);
			});
		}
	}
}