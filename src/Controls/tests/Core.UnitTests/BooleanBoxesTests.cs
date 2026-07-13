using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class BooleanBoxesTests : BaseTestFixture
{
	[Fact]
	public void TrueBoxIsBoxedTrue()
	{
		Assert.Equal(true, BooleanBoxes.TrueBox);
		Assert.IsType<bool>(BooleanBoxes.TrueBox);
	}

	[Fact]
	public void FalseBoxIsBoxedFalse()
	{
		Assert.Equal(false, BooleanBoxes.FalseBox);
		Assert.IsType<bool>(BooleanBoxes.FalseBox);
	}

	[Fact]
	public void TrueBoxAndFalseBoxAreDifferentInstances()
	{
		Assert.NotSame(BooleanBoxes.TrueBox, BooleanBoxes.FalseBox);
	}

	[Fact]
	public void BoxTrueReturnsTrueBox()
	{
		var result = BooleanBoxes.Box(true);
		Assert.Same(BooleanBoxes.TrueBox, result);
	}

	[Fact]
	public void BoxFalseReturnsFalseBox()
	{
		var result = BooleanBoxes.Box(false);
		Assert.Same(BooleanBoxes.FalseBox, result);
	}

	[Fact]
	public void BoxNullableTrueReturnsTrueBox()
	{
		bool? value = true;
		var result = BooleanBoxes.Box(value);
		Assert.Same(BooleanBoxes.TrueBox, result);
	}

	[Fact]
	public void BoxNullableFalseReturnsFalseBox()
	{
		bool? value = false;
		var result = BooleanBoxes.Box(value);
		Assert.Same(BooleanBoxes.FalseBox, result);
	}

	[Fact]
	public void BoxNullableNullReturnsNull()
	{
		bool? value = null;
		var result = BooleanBoxes.Box(value);
		Assert.Null(result);
	}

	[Fact]
	public void BoxReturnsSameReferenceOnRepeatedCalls()
	{
		Assert.Same(BooleanBoxes.Box(true), BooleanBoxes.Box(true));
		Assert.Same(BooleanBoxes.Box(false), BooleanBoxes.Box(false));
	}
}

/// <summary>
/// Regression tests that verify production BindableProperty default values and setters
/// store the shared cached boxes rather than freshly-allocated boxed booleans.
/// If any of these fail, a SetValue call site or BindableProperty default has regressed
/// to boxing instead of using BooleanBoxes.
/// </summary>
public class BooleanBoxesProductionTests : BaseTestFixture
{
	// --- DefaultValue regression tests ---

	[Fact]
	public void ActivityIndicator_IsRunningProperty_DefaultValueIsFalseBox()
		=> Assert.Same(BooleanBoxes.FalseBox, ActivityIndicator.IsRunningProperty.DefaultValue);

	[Fact]
	public void VisualElement_IsVisibleProperty_DefaultValueIsTrueBox()
		=> Assert.Same(BooleanBoxes.TrueBox, VisualElement.IsVisibleProperty.DefaultValue);

	[Fact]
	public void VisualElement_IsEnabledProperty_DefaultValueIsTrueBox()
		=> Assert.Same(BooleanBoxes.TrueBox, VisualElement.IsEnabledProperty.DefaultValue);

	[Fact]
	public void VisualElement_InputTransparentProperty_DefaultValueIsFalseBox()
		=> Assert.Same(BooleanBoxes.FalseBox, VisualElement.InputTransparentProperty.DefaultValue);

	[Fact]
	public void CheckBox_IsCheckedProperty_DefaultValueIsFalseBox()
		=> Assert.Same(BooleanBoxes.FalseBox, CheckBox.IsCheckedProperty.DefaultValue);

	[Fact]
	public void Switch_IsToggledProperty_DefaultValueIsFalseBox()
		=> Assert.Same(BooleanBoxes.FalseBox, Switch.IsToggledProperty.DefaultValue);

	// --- Setter regression tests: GetValue returns the cached box, not a new allocation ---

	[Fact]
	public void ActivityIndicator_IsRunning_SetterStoresCachedBox()
	{
		var indicator = new ActivityIndicator();

		indicator.IsRunning = true;
		Assert.Same(BooleanBoxes.TrueBox, indicator.GetValue(ActivityIndicator.IsRunningProperty));

		indicator.IsRunning = false;
		Assert.Same(BooleanBoxes.FalseBox, indicator.GetValue(ActivityIndicator.IsRunningProperty));
	}

	[Fact]
	public void VisualElement_IsVisible_SetterStoresCachedBox()
	{
		var view = new ContentView();

		view.IsVisible = false;
		Assert.Same(BooleanBoxes.FalseBox, view.GetValue(VisualElement.IsVisibleProperty));

		view.IsVisible = true;
		Assert.Same(BooleanBoxes.TrueBox, view.GetValue(VisualElement.IsVisibleProperty));
	}

	[Fact]
	public void VisualElement_IsEnabled_SetterStoresCachedBox()
	{
		var view = new ContentView();

		view.IsEnabled = false;
		Assert.Same(BooleanBoxes.FalseBox, view.GetValue(VisualElement.IsEnabledProperty));

		view.IsEnabled = true;
		Assert.Same(BooleanBoxes.TrueBox, view.GetValue(VisualElement.IsEnabledProperty));
	}

	[Fact]
	public void VisualElement_InputTransparent_SetterStoresCachedBox()
	{
		var view = new ContentView();

		view.InputTransparent = true;
		Assert.Same(BooleanBoxes.TrueBox, view.GetValue(VisualElement.InputTransparentProperty));

		view.InputTransparent = false;
		Assert.Same(BooleanBoxes.FalseBox, view.GetValue(VisualElement.InputTransparentProperty));
	}

	[Fact]
	public void CheckBox_IsChecked_SetterStoresCachedBox()
	{
		var checkBox = new CheckBox();

		checkBox.IsChecked = true;
		Assert.Same(BooleanBoxes.TrueBox, checkBox.GetValue(CheckBox.IsCheckedProperty));

		checkBox.IsChecked = false;
		Assert.Same(BooleanBoxes.FalseBox, checkBox.GetValue(CheckBox.IsCheckedProperty));
	}

	[Fact]
	public void Switch_IsToggled_SetterStoresCachedBox()
	{
		var sw = new Switch();

		sw.IsToggled = true;
		Assert.Same(BooleanBoxes.TrueBox, sw.GetValue(Switch.IsToggledProperty));

		sw.IsToggled = false;
		Assert.Same(BooleanBoxes.FalseBox, sw.GetValue(Switch.IsToggledProperty));
	}

	[Fact]
	public void RefreshView_IsRefreshing_SetterStoresCachedBox()
	{
		var refreshView = new RefreshView();

		refreshView.IsRefreshing = true;
		Assert.Same(BooleanBoxes.TrueBox, refreshView.GetValue(RefreshView.IsRefreshingProperty));

		refreshView.IsRefreshing = false;
		Assert.Same(BooleanBoxes.FalseBox, refreshView.GetValue(RefreshView.IsRefreshingProperty));
	}

	[Fact]
	public void Entry_IsPassword_SetterStoresCachedBox()
	{
		var entry = new Entry();

		entry.IsPassword = true;
		Assert.Same(BooleanBoxes.TrueBox, entry.GetValue(Entry.IsPasswordProperty));

		entry.IsPassword = false;
		Assert.Same(BooleanBoxes.FalseBox, entry.GetValue(Entry.IsPasswordProperty));
	}
}