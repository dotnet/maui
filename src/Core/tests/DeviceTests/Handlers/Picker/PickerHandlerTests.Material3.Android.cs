using System;
using System.Reflection;
using System.Threading.Tasks;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Google.Android.Material.Shape;
using Google.Android.Material.TextField;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AppCompatAlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace Microsoft.Maui.DeviceTests;

public partial class PickerHandlerTests
{
	[Fact]
	public Task MaterialPickerSemanticsTargetChild() =>
		RunMaterialPickerTest((handler, picker) =>
		{
			var child = handler.PlatformView.EditText;
			var nativeDelegate = ViewCompat.GetAccessibilityDelegate(child);
			picker.AutomationId = "destination";
			picker.Semantics.Description = "Destination";
			picker.Semantics.Hint = "Choose a destination";
			picker.Semantics.HeadingLevel = SemanticHeadingLevel.Level1;
			handler.UpdateValue(nameof(IView.AutomationId));
			handler.UpdateValue(nameof(IView.Semantics));
			using (var node = child.CreateAccessibilityNodeInfo())
			{
				Assert.Equal("Destination, One", node.Text);
				Assert.Equal("Destination", node.ContentDescription);
				Assert.Equal("Choose a destination", node.HintText);
				Assert.Equal($"{child.Context.PackageName}:id/destination", node.ViewIdResourceName);
			}
			Assert.True(ViewCompat.IsAccessibilityHeading(child));
			Assert.False(ViewCompat.IsAccessibilityHeading(handler.PlatformView));
			Assert.Equal(ImportantForAccessibility.No, handler.PlatformView.ImportantForAccessibility);
			Assert.True(string.IsNullOrEmpty(handler.PlatformView.ContentDescription));
			Assert.False(string.IsNullOrEmpty(GetMaterialPickerIcon(handler).ContentDescription));
			picker.SelectedIndex = -1;
			handler.UpdateValue(nameof(IPicker.SelectedIndex));
			using (var node = child.CreateAccessibilityNodeInfo())
				Assert.Equal("Destination", node.Text);
			picker.SelectedIndex = 1;
			handler.UpdateValue(nameof(IPicker.SelectedIndex));
			using (var node = child.CreateAccessibilityNodeInfo())
				Assert.Equal("Destination, Two", node.Text);
			Assert.Same(nativeDelegate, ViewCompat.GetAccessibilityDelegate(child));
			return Task.CompletedTask;
		});

	[Fact]
	public Task MaterialPickerSemanticsResetPreservesNativeHint() =>
		RunMaterialPickerTest((handler, picker) =>
		{
			var child = handler.PlatformView.EditText;
			var original = Assert.IsType<PickerHandler2.PickerAccessibilityDelegate>(ViewCompat.GetAccessibilityDelegate(child));
			Assert.NotNull(original.WrappedDelegate);
			for (var i = 0; i < 3; i++)
			{
				picker.Semantics.Description = "Destination";
				picker.Semantics.Hint = "Custom hint";
				picker.AutomationId = "destination";
				handler.UpdateValue(nameof(IView.Semantics));
				handler.UpdateValue(nameof(IView.AutomationId));
				picker.Semantics = new Semantics();
				picker.AutomationId = null;
				handler.UpdateValue(nameof(IView.Semantics));
				handler.UpdateValue(nameof(IView.AutomationId));
				using var node = child.CreateAccessibilityNodeInfo();
				Assert.Equal("One", node.Text);
				Assert.Equal(picker.Title, node.HintText);
				Assert.True(string.IsNullOrEmpty(node.ContentDescription));
				Assert.NotEqual($"{child.Context.PackageName}:id/destination", node.ViewIdResourceName);
				Assert.Same(original, ViewCompat.GetAccessibilityDelegate(child));
				Assert.False(ViewCompat.IsAccessibilityHeading(child));
			}
			return Task.CompletedTask;
		});

	[Fact]
	public Task MaterialPickerSemanticsDisconnectAndReconnect() =>
		RunMaterialPickerTest((handler, picker) =>
		{
			var layout = handler.PlatformView;
			var child = layout.EditText;
			picker.AutomationId = "destination";
			picker.Semantics.Description = "Destination";
			handler.UpdateValue(nameof(IView.AutomationId));
			handler.UpdateValue(nameof(IView.Semantics));
			for (var i = 0; i < 3; i++)
			{
				var previous = Assert.IsType<PickerHandler2.PickerAccessibilityDelegate>(ViewCompat.GetAccessibilityDelegate(child));
				var nativeDelegate = previous.WrappedDelegate;
				Assert.Same(handler, previous.Handler);
				Assert.IsNotType<PickerHandler2.PickerAccessibilityDelegate>(nativeDelegate);
				((IElementHandler)handler).DisconnectHandler();
				Assert.Null(previous.Handler);
				Assert.Same(nativeDelegate, ViewCompat.GetAccessibilityDelegate(child));
				// Reuse the native field to detect stacked delegates or lost native behavior.
				var factory = PickerHandler2.PlatformViewFactory;
				try
				{
					PickerHandler2.PlatformViewFactory = _ => layout;
					handler.SetVirtualView(picker);
				}
				finally
				{
					PickerHandler2.PlatformViewFactory = factory;
				}
				var current = Assert.IsType<PickerHandler2.PickerAccessibilityDelegate>(ViewCompat.GetAccessibilityDelegate(child));
				Assert.NotSame(previous, current);
				Assert.Same(handler, current.Handler);
				Assert.Same(nativeDelegate, current.WrappedDelegate);
				using var node = child.CreateAccessibilityNodeInfo();
				Assert.Equal("Destination, One", node.Text);
				Assert.Equal(picker.Title, node.HintText);
				Assert.Equal($"{child.Context.PackageName}:id/destination", node.ViewIdResourceName);
			}
			return Task.CompletedTask;
		});

	[Fact]
	public Task MaterialPickerCreatesOutlinedReadOnlyFieldWithArrow() =>
		RunMaterialPickerTest((handler, picker) =>
		{
			var layout = handler.PlatformView;
			var child = Assert.IsType<MauiMaterialPicker>(layout.EditText);
			Assert.Equal(TextInputLayout.BoxBackgroundOutline, layout.BoxBackgroundMode);
			Assert.Equal(TextInputLayout.EndIconCustom, layout.EndIconMode);
			Assert.True(layout.EndIconVisible);
			Assert.NotNull(layout.EndIconDrawable);
			Assert.IsType<MaterialShapeDrawable>(child.Background);
			AssertReadOnlyPicker(child);
			Assert.Equal(picker.Title, layout.Hint);
			Assert.Equal(picker.Items[0], child.Text);
			return Task.CompletedTask;
		});

	[Theory]
	[InlineData("field")]
	[InlineData("icon")]
	[InlineData("container")]
	public Task MaterialPickerClickOpensOneDialogAndSelectsItem(string target) =>
		RunMaterialPickerTest(async (handler, picker) =>
		{
			var layout = handler.PlatformView;
			var view = target == "field" ? layout.EditText : target == "icon" ? GetMaterialPickerIcon(handler) : (View)layout;
			view.PerformClick();
			await WaitForPostedCallbacks(layout);
			var dialog = Assert.IsType<AppCompatAlertDialog>(GetMaterialPickerDialog(handler));
			Assert.True(dialog.IsShowing);
			Assert.True(picker.IsOpen);
			Assert.True(picker.IsFocused);
			view.PerformClick();
			Assert.Same(dialog, GetMaterialPickerDialog(handler));
			Assert.Equal(0, dialog.ListView.CheckedItemPosition);
			dialog.ListView.PerformItemClick(dialog.ListView.GetChildAt(1), 1, dialog.ListView.Adapter.GetItemId(1));
			await WaitForPostedCallbacks(layout);
			Assert.Equal(1, picker.SelectedIndex);
			Assert.Equal(picker.Items[1], layout.EditText.Text);
			Assert.False(picker.IsOpen);
			Assert.False(picker.IsFocused);
			Assert.Null(GetMaterialPickerDialog(handler));
		});

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public Task MaterialPickerPreservesDialogLifecycle(bool cancel) =>
		RunMaterialPickerTest(async (handler, picker) =>
		{
			picker.IsOpen = true;
			handler.UpdateValue(nameof(IPicker.IsOpen));
			await WaitForPostedCallbacks(handler.PlatformView);
			var dialog = GetMaterialPickerDialog(handler);
			Assert.NotNull(dialog);
			var dismissed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			EventHandler onDismiss = (_, _) => dismissed.TrySetResult(true);
			dialog.DismissEvent += onDismiss;
			try
			{
				if (cancel)
					dialog.Cancel();
				else
					dialog.GetButton((int)global::Android.Content.DialogButtonType.Negative).PerformClick();
				// The native button queues dismissal, which then queues DismissEvent.
				// One posted view callback does not guarantee the handler has received it.
				await dismissed.Task.WaitAsync(TimeSpan.FromSeconds(5));
			}
			finally
			{
				dialog.DismissEvent -= onDismiss;
			}
			Assert.False(picker.IsOpen);
			Assert.False(picker.IsFocused);
			Assert.Equal(0, picker.SelectedIndex);
			Assert.False(dialog.IsShowing);
			Assert.Null(GetMaterialPickerDialog(handler));

			PickerHandler2.MapFocus(handler, picker, new FocusRequest());
			Assert.NotNull(GetMaterialPickerDialog(handler));
			PickerHandler2.MapUnfocus(handler, picker, null);
			await WaitForPostedCallbacks(handler.PlatformView);
			Assert.False(picker.IsOpen);
			Assert.Null(GetMaterialPickerDialog(handler));

			picker.IsOpen = true;
			handler.UpdateValue(nameof(IPicker.IsOpen));
			picker.IsOpen = false;
			handler.UpdateValue(nameof(IPicker.IsOpen));
			await WaitForPostedCallbacks(handler.PlatformView);
			Assert.Null(GetMaterialPickerDialog(handler));

			handler.PlatformView.EditText.PerformClick();
			Assert.NotNull(GetMaterialPickerDialog(handler));
			((IElementHandler)handler).DisconnectHandler();
			Assert.False(picker.IsOpen);
			Assert.False(picker.IsFocused);
			Assert.Null(GetMaterialPickerDialog(handler));
		});

	[Fact]
	public Task MaterialPickerDisabledFieldAndIconCannotOpenDialog() =>
		RunMaterialPickerTest(async (handler, picker) =>
		{
			picker.IsEnabled = false;
			handler.UpdateValue(nameof(IView.IsEnabled));
			Assert.False(handler.PlatformView.Enabled);
			Assert.False(handler.PlatformView.EditText.Enabled);
			Assert.False(GetMaterialPickerIcon(handler).Enabled);
			// PerformClick deliberately bypasses Android's touch gating: the handler must guard too.
			handler.PlatformView.PerformClick();
			handler.PlatformView.EditText.PerformClick();
			GetMaterialPickerIcon(handler).PerformClick();
			Assert.Null(GetMaterialPickerDialog(handler));
			picker.IsOpen = true;
			handler.UpdateValue(nameof(IPicker.IsOpen));
			Assert.False(picker.IsOpen);
			Assert.Null(GetMaterialPickerDialog(handler));
			picker.IsEnabled = true;
			handler.UpdateValue(nameof(IView.IsEnabled));
			GetMaterialPickerIcon(handler).PerformClick();
			Assert.NotNull(GetMaterialPickerDialog(handler));
			picker.IsEnabled = false;
			handler.UpdateValue(nameof(IView.IsEnabled));
			await WaitForPostedCallbacks(handler.PlatformView);
			Assert.False(picker.IsOpen);
		});

	[Fact]
	public Task MaterialPickerMappingsPreserveOutlineAndTitle() =>
		RunMaterialPickerTest((handler, picker) =>
		{
			var layout = handler.PlatformView;
			var background = layout.EditText.Background;
			var hintColors = layout.DefaultHintTextColor;
			picker.Title = "Updated title";
			handler.UpdateValue(nameof(IPicker.Title));
			picker.TitleColor = Colors.Red;
			handler.UpdateValue(nameof(IPicker.TitleColor));
			Assert.Equal(Colors.Red.ToPlatform().ToArgb(), layout.DefaultHintTextColor.DefaultColor);
			picker.TitleColor = null;
			handler.UpdateValue(nameof(IPicker.TitleColor));
			Assert.Equal(hintColors, layout.DefaultHintTextColor);
			picker.SelectedIndex = -1;
			handler.UpdateValue(nameof(IPicker.SelectedIndex));
			Assert.True(string.IsNullOrEmpty(layout.EditText.Text));
			picker.SelectedIndex = 1;
			handler.UpdateValue(nameof(IPicker.SelectedIndex));
			Assert.Equal("Updated title", layout.Hint);
			Assert.Equal(picker.Items[1], layout.EditText.Text);
			picker.FlowDirection = FlowDirection.RightToLeft;
			handler.UpdateValue(nameof(IView.FlowDirection));
			Assert.Equal(LayoutDirection.Rtl, layout.LayoutDirection);
			Assert.Equal(LayoutDirection.Rtl, layout.EditText.LayoutDirection);
			picker.Font = Font.OfSize(null, 24);
			handler.UpdateValue(nameof(IPicker.Font));
			picker.CharacterSpacing = 3;
			handler.UpdateValue(nameof(IPicker.CharacterSpacing));
			Assert.Equal(picker.CharacterSpacing.ToEm(), layout.EditText.LetterSpacing, EmCoefficientPrecision);
			foreach (var color in new[] { Colors.Red, Colors.Transparent, null })
			{
				picker.Background = color is null ? null : new SolidPaint(color);
				handler.UpdateValue(nameof(IView.Background));
				Assert.Same(background, layout.EditText.Background);
				Assert.Equal(TextInputLayout.BoxBackgroundOutline, layout.BoxBackgroundMode);
			}
			Assert.Null(layout.Background);
			return Task.CompletedTask;
		});

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public Task MaterialPickerCompositionPreservesGestures(bool delayPressedState) =>
		RunMaterialPickerTest(async (handler, picker) =>
		{
			var child = handler.PlatformView.EditText;
			picker.Items = new[] { new string('W', 200) };
			handler.UpdateValue(nameof(IPicker.Items));
			await WaitForPostedCallbacks(child);
			DispatchHorizontalDrag(child);
			await WaitForPostedCallbacks(child);
			Assert.True(child.ScrollX > 0);
			Assert.Null(GetMaterialPickerDialog(handler));
			DispatchExactTouchSlopDrag(child);
			DispatchVerticalDrag(child);
			DispatchCanceledDrag(child);
			DispatchSwipeWithoutMove(child);
			await WaitForPostedCallbacks(child);
			Assert.Null(GetMaterialPickerDialog(handler));
			DispatchJitteredTap(child);
			await WaitForPostedCallbacks(child);
			Assert.NotNull(GetMaterialPickerDialog(handler));
			AssertReadOnlyPicker(child);
		}, delayPressedState);

	Task RunMaterialPickerTest(Func<PickerHandler2, PickerStub, Task> test, bool delayPressedState = false) =>
		InvokeOnMainThreadAsync(async () =>
		{
			var picker = new PickerStub { Title = "Choose an item", Items = new[] { "One", "Two" }, SelectedIndex = 0 };
			var handler = CreateHandler<PickerHandler2>(picker);
			var layout = handler.PlatformView;
			using var host = delayPressedState
				? new DelayedPressedFrameLayout(MauiContext.Context)
				: new global::Android.Widget.FrameLayout(MauiContext.Context);
			var width = (int)MauiContext.Context.ToPixels(240);
			var height = (int)MauiContext.Context.ToPixels(100);
			host.AddView(handler.PlatformView, new global::Android.Widget.FrameLayout.LayoutParams(width, height));
			try
			{
				await host.AttachAndRun(async () =>
				{
					handler.PlatformView.Measure(View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
						View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
					handler.PlatformView.Layout(0, 0, width, height);
					await test(handler, picker);
				});
			}
			finally
			{
				((IElementHandler)handler).DisconnectHandler();
				host.RemoveAllViews();
				layout.Dispose();
			}
		});

	static View GetMaterialPickerIcon(PickerHandler2 handler) =>
		handler.PlatformView.FindViewById<View>(Resource.Id.text_input_end_icon);

	static AppCompatAlertDialog GetMaterialPickerDialog(PickerHandler2 handler) =>
		(AppCompatAlertDialog)typeof(PickerHandler2).GetField("_dialog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(handler);
}
