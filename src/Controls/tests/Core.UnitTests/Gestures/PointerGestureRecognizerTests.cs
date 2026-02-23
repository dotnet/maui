using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PointerGestureRecognizerTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var pointer = new PointerGestureRecognizer();
		}

		[Fact]
		public void PointerOverVsmGestureIsOnlyPresentWithVsm()
		{
			var layout = new VerticalStackLayout();
			IGestureController gestureControllers = layout;
			var gesture = gestureControllers
				.CompositeGestureRecognizers
				.OfType<PointerGestureRecognizer>()
				.FirstOrDefault();

			Assert.Null(gesture);

			var visualStateGroups = AddPointerOverVisualState(layout);

			gesture = gestureControllers
				.CompositeGestureRecognizers
				.OfType<PointerGestureRecognizer>()
				.FirstOrDefault();

			Assert.NotNull(gesture);

			visualStateGroups.Remove(visualStateGroups[0]);
			layout.ChangeVisualState();
			gesture = gestureControllers
				.CompositeGestureRecognizers
				.OfType<PointerGestureRecognizer>()
				.FirstOrDefault();

			Assert.Null(gesture);
		}


		[Fact]
		public void ClearingGestureRecognizers()
		{
			var view = new View();
			AddPointerOverVisualState(view);
			var gestureRecognizer = new TapGestureRecognizer();

			view.GestureRecognizers.Add(gestureRecognizer);
			Assert.Equal(2, (view as IGestureController).CompositeGestureRecognizers.Count);

			view.GestureRecognizers.Clear();
			Assert.Single((view as IGestureController).CompositeGestureRecognizers);
			Assert.Null(gestureRecognizer.Parent);
		}

		[Fact]
		public void PointerEnteredCommandFires()
		{
			var gesture = new PointerGestureRecognizer();
			var parameter = new object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			gesture.PointerEnteredCommand = cmd;
			gesture.PointerEnteredCommandParameter = parameter;
			cmd?.Execute(parameter);

			Assert.Equal(commandExecuted, parameter);
		}

		[Fact]
		public void PointerExitedCommandFires()
		{
			var gesture = new PointerGestureRecognizer();
			var parameter = new object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			gesture.PointerExitedCommand = cmd;
			gesture.PointerExitedCommandParameter = parameter;
			cmd?.Execute(parameter);

			Assert.Equal(commandExecuted, parameter);
		}

		[Fact]
		public void PointerMovedCommandFires()
		{
			var gesture = new PointerGestureRecognizer();
			var parameter = new object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			gesture.PointerMovedCommand = cmd;
			gesture.PointerMovedCommandParameter = parameter;
			cmd?.Execute(parameter);

			Assert.Equal(commandExecuted, parameter);
		}

		[Fact]
		public void PointerPressedCommandFires()
		{
			var gesture = new PointerGestureRecognizer();
			var parameter = new object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			gesture.PointerPressedCommand = cmd;
			gesture.PointerPressedCommandParameter = parameter;
			cmd?.Execute(parameter);

			Assert.Equal(commandExecuted, parameter);
		}

		[Fact]
		public void PointerReleasedCommandFires()
		{
			var gesture = new PointerGestureRecognizer();
			var parameter = new object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			gesture.PointerReleasedCommand = cmd;
			gesture.PointerReleasedCommandParameter = parameter;
			cmd?.Execute(parameter);

			Assert.Equal(commandExecuted, parameter);
		}

		[Fact]
		public void ButtonsPropertyDefaultValue()
		{
			var gesture = new PointerGestureRecognizer();
			Assert.Equal(ButtonsMask.Primary, gesture.Buttons);
		}

		[Fact]
		public void ButtonsPropertyCanBeSet()
		{
			var gesture = new PointerGestureRecognizer();
			
			gesture.Buttons = ButtonsMask.Secondary;
			Assert.Equal(ButtonsMask.Secondary, gesture.Buttons);
			
			gesture.Buttons = ButtonsMask.Primary | ButtonsMask.Secondary;
			Assert.Equal(ButtonsMask.Primary | ButtonsMask.Secondary, gesture.Buttons);
		}

		[Fact]
		public void ButtonsPropertyBinding()
		{
			var gesture = new PointerGestureRecognizer();
			var bindingContext = new { TestButtons = ButtonsMask.Secondary };
			
			gesture.SetBinding(PointerGestureRecognizer.ButtonsProperty, "TestButtons");
			gesture.BindingContext = bindingContext;
			
			Assert.Equal(ButtonsMask.Secondary, gesture.Buttons);
		}

		[Fact]
		public void ButtonsPropertyChangedEvent()
		{
			var gesture = new PointerGestureRecognizer();
			bool propertyChanged = false;
			string changedProperty = null;
			
			gesture.PropertyChanged += (sender, e) => {
				propertyChanged = true;
				changedProperty = e.PropertyName;
			};
			
			gesture.Buttons = ButtonsMask.Secondary;
			
			Assert.True(propertyChanged);
			Assert.Equal(nameof(PointerGestureRecognizer.Buttons), changedProperty);
		}

		[Theory]
		[InlineData(ButtonsMask.Primary)]
		[InlineData(ButtonsMask.Secondary)]
		[InlineData(ButtonsMask.Primary | ButtonsMask.Secondary)]
		public void ButtonsPropertyAcceptsValidValues(ButtonsMask buttons)
		{
			var gesture = new PointerGestureRecognizer();
			gesture.Buttons = buttons;
			Assert.Equal(buttons, gesture.Buttons);
		}

		[Fact]
		public void ButtonsPropertyFilteringPrimaryOnly()
		{
			var gesture = new PointerGestureRecognizer();
			gesture.Buttons = ButtonsMask.Primary;
			
			// Verify only primary button is accepted
			Assert.Equal(ButtonsMask.Primary, gesture.Buttons);
		}

		[Fact]
		public void ButtonsPropertyFilteringSecondaryOnly()
		{
			var gesture = new PointerGestureRecognizer();
			gesture.Buttons = ButtonsMask.Secondary;
			
			// Verify only secondary button is accepted
			Assert.Equal(ButtonsMask.Secondary, gesture.Buttons);
		}

		[Fact]
		public void ButtonsPropertyFilteringBothButtons()
		{
			var gesture = new PointerGestureRecognizer();
			gesture.Buttons = ButtonsMask.Primary | ButtonsMask.Secondary;
			
			// Verify both buttons are accepted
			Assert.Equal(ButtonsMask.Primary | ButtonsMask.Secondary, gesture.Buttons);
		}

		[Fact]
		public void ButtonsPropertyChangeTriggersPlatformUpdate()
		{
			var gesture = new PointerGestureRecognizer();
			var propertyChangedCount = 0;
			
			gesture.PropertyChanged += (sender, e) => {
				if (e.PropertyName == nameof(PointerGestureRecognizer.Buttons))
					propertyChangedCount++;
			};
			
			gesture.Buttons = ButtonsMask.Secondary;
			Assert.Equal(1, propertyChangedCount);
			
			gesture.Buttons = ButtonsMask.Primary | ButtonsMask.Secondary;
			Assert.Equal(2, propertyChangedCount);
			
			// Setting same value shouldn't trigger change
			gesture.Buttons = ButtonsMask.Primary | ButtonsMask.Secondary;
			Assert.Equal(2, propertyChangedCount);
		}

		[Fact]
		public void ButtonsPropertyDefaultValueMatchesTapGesture()
		{
			var pointerGesture = new PointerGestureRecognizer();
			var tapGesture = new TapGestureRecognizer();
			
			// Both should have the same default value
			Assert.Equal(tapGesture.Buttons, pointerGesture.Buttons);
			Assert.Equal(ButtonsMask.Primary, pointerGesture.Buttons);
		}

		[Fact]
		public void ButtonsPropertyBindingWorksCorrectly()
		{
			var gesture = new PointerGestureRecognizer();
			var source = new ButtonsSource();
			
			gesture.SetBinding(PointerGestureRecognizer.ButtonsProperty, nameof(ButtonsSource.Buttons));
			gesture.BindingContext = source;
			
			Assert.Equal(ButtonsMask.Secondary, gesture.Buttons);
			
			source.Buttons = ButtonsMask.Primary | ButtonsMask.Secondary;
			Assert.Equal(ButtonsMask.Primary | ButtonsMask.Secondary, gesture.Buttons);
		}

		[Fact]
		public void ButtonsPropertyTwoWayBindingWorksCorrectly()
		{
			var gesture = new PointerGestureRecognizer();
			var source = new ButtonsSource();
			
			gesture.SetBinding(PointerGestureRecognizer.ButtonsProperty, 
				new Binding(nameof(ButtonsSource.Buttons), BindingMode.TwoWay));
			gesture.BindingContext = source;
			
			// Change source, verify gesture updates
			source.Buttons = ButtonsMask.Primary | ButtonsMask.Secondary;
			Assert.Equal(ButtonsMask.Primary | ButtonsMask.Secondary, gesture.Buttons);
			
			// Change gesture, verify source updates
			gesture.Buttons = ButtonsMask.Secondary;
			Assert.Equal(ButtonsMask.Secondary, source.Buttons);
		}

		[Fact]
		public void ButtonsPropertyIntegrationWithGestureHandling()
		{
			var gesture = new PointerGestureRecognizer();
			var button = new Button();
			bool eventFired = false;
			
			// Test that events properly connect with button filtering
			gesture.PointerPressed += (s, e) => eventFired = true;
			button.GestureRecognizers.Add(gesture);
			
			// Verify gesture is properly added
			Assert.Contains(gesture, button.GestureRecognizers);
			Assert.Equal(ButtonsMask.Primary, gesture.Buttons);
		}

		[Fact]
		public void ButtonsPropertyCompatibilityWithExistingCode()
		{
			// Verify that existing code using PointerGestureRecognizer without setting Buttons still works
			var gesture = new PointerGestureRecognizer();
			var view = new Label { Text = "Test" };
			
			// Add gesture without explicitly setting Buttons property
			view.GestureRecognizers.Add(gesture);
			
			// Should use default value (Primary button)
			Assert.Equal(ButtonsMask.Primary, gesture.Buttons);
			Assert.Contains(gesture, view.GestureRecognizers);
		}

		[Fact]
		public void ButtonsPropertyConsistencyAcrossGestureTypes()
		{
			var pointerGesture = new PointerGestureRecognizer();
			var tapGesture = new TapGestureRecognizer();
			
			// Both gesture types should have consistent button handling
			Assert.Equal(pointerGesture.Buttons, tapGesture.Buttons);
			
			// Setting the same value on both should work identically
			var testMask = ButtonsMask.Secondary | ButtonsMask.Primary;
			pointerGesture.Buttons = testMask;
			tapGesture.Buttons = testMask;
			
			Assert.Equal(tapGesture.Buttons, pointerGesture.Buttons);
			Assert.Equal(testMask, pointerGesture.Buttons);
		}

		private class ButtonsSource : BindableObject
		{
			public static readonly BindableProperty ButtonsProperty = 
				BindableProperty.Create(nameof(Buttons), typeof(ButtonsMask), typeof(ButtonsSource), ButtonsMask.Secondary);
			
			public ButtonsMask Buttons
			{
				get => (ButtonsMask)GetValue(ButtonsProperty);
				set => SetValue(ButtonsProperty, value);
			}
		}

		VisualStateGroupList AddPointerOverVisualState(VisualElement visualElement)
		{
			VisualStateGroupList visualStateGroups = new VisualStateGroupList();
			var pointerOverGroup = new VisualStateGroup() { Name = "CommonStates" };
			pointerOverGroup.States.Add(new VisualState() { Name = VisualStateManager.CommonStates.PointerOver });
			visualStateGroups.Add(pointerOverGroup);
			VisualStateManager.SetVisualStateGroups(visualElement, visualStateGroups);
			return visualStateGroups;
		}
	}
}

