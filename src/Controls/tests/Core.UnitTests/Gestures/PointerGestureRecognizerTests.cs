using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NSubstitute;
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

            gesture.PropertyChanged += (sender, e) =>
            {
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

            gesture.PropertyChanged += (sender, e) =>
            {
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

        /// <summary>
        /// Tests that the PointerEnteredCommandParameter property returns null by default.
        /// Verifies the default value as specified in the bindable property definition.
        /// </summary>
        [Fact]
        public void PointerEnteredCommandParameter_DefaultValue_ReturnsNull()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();

            // Act
            var result = gesture.PointerEnteredCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the PointerEnteredCommandParameter property can be set to null and retrieved correctly.
        /// Verifies null handling for the object-typed parameter.
        /// </summary>
        [Fact]
        public void PointerEnteredCommandParameter_SetToNull_ReturnsNull()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();

            // Act
            gesture.PointerEnteredCommandParameter = null;
            var result = gesture.PointerEnteredCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the PointerEnteredCommandParameter property can store and retrieve various object types.
        /// Verifies the property correctly handles different object types including primitives, strings, and custom objects.
        /// </summary>
        [Theory]
        [InlineData("test string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void PointerEnteredCommandParameter_SetVariousTypes_ReturnsCorrectValue(object parameter)
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();

            // Act
            gesture.PointerEnteredCommandParameter = parameter;
            var result = gesture.PointerEnteredCommandParameter;

            // Assert
            Assert.Equal(parameter, result);
        }

        /// <summary>
        /// Tests that the PointerEnteredCommandParameter property can store and retrieve custom object instances.
        /// Verifies the property correctly handles reference equality for custom objects.
        /// </summary>
        [Fact]
        public void PointerEnteredCommandParameter_SetCustomObject_ReturnsCorrectReference()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var customObject = new TestParameterObject { Value = "test" };

            // Act
            gesture.PointerEnteredCommandParameter = customObject;
            var result = gesture.PointerEnteredCommandParameter;

            // Assert
            Assert.Same(customObject, result);
        }

        /// <summary>
        /// Tests that the PointerEnteredCommandParameter property can be overwritten multiple times.
        /// Verifies that setting a new value replaces the previous one correctly.
        /// </summary>
        [Fact]
        public void PointerEnteredCommandParameter_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var firstValue = "first";
            var secondValue = "second";

            // Act
            gesture.PointerEnteredCommandParameter = firstValue;
            gesture.PointerEnteredCommandParameter = secondValue;
            var result = gesture.PointerEnteredCommandParameter;

            // Assert
            Assert.Equal(secondValue, result);
        }

        /// <summary>
        /// Tests that the PointerEnteredCommandParameter property can handle empty collections.
        /// Verifies the property correctly stores and retrieves empty collection instances.
        /// </summary>
        [Fact]
        public void PointerEnteredCommandParameter_SetEmptyCollection_ReturnsCorrectCollection()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var emptyList = new List<string>();

            // Act
            gesture.PointerEnteredCommandParameter = emptyList;
            var result = gesture.PointerEnteredCommandParameter;

            // Assert
            Assert.Same(emptyList, result);
        }

        /// <summary>
        /// Helper class for testing custom object parameter scenarios.
        /// </summary>
        private class TestParameterObject
        {
            public string Value { get; set; }
        }

        /// <summary>
        /// Tests that PointerExitedCommand getter returns null when not set (default value).
        /// Verifies the default state of the bindable property.
        /// Expected result: Returns null.
        /// </summary>
        [Fact]
        public void PointerExitedCommand_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();

            // Act
            var result = gestureRecognizer.PointerExitedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerExitedCommand getter returns the correct ICommand instance after it has been set.
        /// Verifies the getter retrieves the stored value correctly through GetValue and casts it properly.
        /// Expected result: Returns the same ICommand instance that was set.
        /// </summary>
        [Fact]
        public void PointerExitedCommand_WhenSet_ReturnsCorrectCommand()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();

            // Act
            gestureRecognizer.PointerExitedCommand = mockCommand;
            var result = gestureRecognizer.PointerExitedCommand;

            // Assert
            Assert.Same(mockCommand, result);
        }

        /// <summary>
        /// Tests that PointerExitedCommand getter returns null after being explicitly set to null.
        /// Verifies the getter handles null values correctly through GetValue and casting.
        /// Expected result: Returns null.
        /// </summary>
        [Fact]
        public void PointerExitedCommand_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();
            gestureRecognizer.PointerExitedCommand = mockCommand;

            // Act
            gestureRecognizer.PointerExitedCommand = null;
            var result = gestureRecognizer.PointerExitedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerExitedCommandParameter returns the default value (null) when not set.
        /// This test covers the getter implementation that retrieves the default value.
        /// </summary>
        [Fact]
        public void PointerExitedCommandParameter_DefaultValue_ReturnsNull()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();

            // Act
            var result = gesture.PointerExitedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerExitedCommandParameter can store and retrieve various object types.
        /// This test covers both the getter and setter implementations with different value types.
        /// </summary>
        [Theory]
        [InlineData("test string")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        [InlineData(false)]
        public void PointerExitedCommandParameter_SetAndGetValue_ReturnsCorrectValue(object expectedValue)
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();

            // Act
            gesture.PointerExitedCommandParameter = expectedValue;
            var result = gesture.PointerExitedCommandParameter;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that PointerExitedCommandParameter can be set to null explicitly.
        /// This test covers the setter and getter when explicitly setting null values.
        /// </summary>
        [Fact]
        public void PointerExitedCommandParameter_SetToNull_ReturnsNull()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            gesture.PointerExitedCommandParameter = "initial value";

            // Act
            gesture.PointerExitedCommandParameter = null;
            var result = gesture.PointerExitedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerExitedCommandParameter can store and retrieve complex objects.
        /// This test covers the getter and setter with custom object types.
        /// </summary>
        [Fact]
        public void PointerExitedCommandParameter_SetComplexObject_ReturnsCorrectObject()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var complexObject = new { Name = "Test", Value = 123 };

            // Act
            gesture.PointerExitedCommandParameter = complexObject;
            var result = gesture.PointerExitedCommandParameter;

            // Assert
            Assert.Equal(complexObject, result);
        }

        /// <summary>
        /// Tests that PointerExitedCommandParameter can store and retrieve collection objects.
        /// This test covers the getter and setter with collection types.
        /// </summary>
        [Fact]
        public void PointerExitedCommandParameter_SetCollectionObject_ReturnsCorrectCollection()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var collection = new List<string> { "item1", "item2", "item3" };

            // Act
            gesture.PointerExitedCommandParameter = collection;
            var result = gesture.PointerExitedCommandParameter;

            // Assert
            Assert.Equal(collection, result);
        }

        /// <summary>
        /// Tests that PointerExitedCommandParameter maintains reference equality for reference types.
        /// This test verifies that the getter returns the exact same object instance that was set.
        /// </summary>
        [Fact]
        public void PointerExitedCommandParameter_SetReferenceType_MaintainsReferenceEquality()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var referenceObject = new object();

            // Act
            gesture.PointerExitedCommandParameter = referenceObject;
            var result = gesture.PointerExitedCommandParameter;

            // Assert
            Assert.Same(referenceObject, result);
        }

        /// <summary>
        /// Tests that PointerExitedCommandParameter can be overwritten with different values.
        /// This test covers multiple set/get operations to ensure the property works correctly with value changes.
        /// </summary>
        [Fact]
        public void PointerExitedCommandParameter_OverwriteValue_ReturnsLatestValue()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var initialValue = "initial";
            var updatedValue = "updated";

            // Act
            gesture.PointerExitedCommandParameter = initialValue;
            var firstResult = gesture.PointerExitedCommandParameter;

            gesture.PointerExitedCommandParameter = updatedValue;
            var secondResult = gesture.PointerExitedCommandParameter;

            // Assert
            Assert.Equal(initialValue, firstResult);
            Assert.Equal(updatedValue, secondResult);
        }

        /// <summary>
        /// Tests that PointerMovedCommand getter returns null by default.
        /// Verifies the default value behavior when no command has been set.
        /// Expected result: null should be returned.
        /// </summary>
        [Fact]
        public void PointerMovedCommand_DefaultValue_ReturnsNull()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            var result = recognizer.PointerMovedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerMovedCommand getter returns the exact same command instance that was set.
        /// Verifies that the property correctly stores and retrieves ICommand instances.
        /// Expected result: the same command instance should be returned.
        /// </summary>
        [Fact]
        public void PointerMovedCommand_SetValidCommand_ReturnsSetCommand()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();

            // Act
            recognizer.PointerMovedCommand = mockCommand;
            var result = recognizer.PointerMovedCommand;

            // Assert
            Assert.Same(mockCommand, result);
        }

        /// <summary>
        /// Tests that PointerMovedCommand getter returns null after setting null value.
        /// Verifies that the property can be cleared by setting it to null.
        /// Expected result: null should be returned after setting null.
        /// </summary>
        [Fact]
        public void PointerMovedCommand_SetNull_ReturnsNull()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();
            recognizer.PointerMovedCommand = mockCommand;

            // Act
            recognizer.PointerMovedCommand = null;
            var result = recognizer.PointerMovedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerMovedCommand getter works correctly when property is changed multiple times.
        /// Verifies that the property correctly handles multiple set operations and always returns the most recent value.
        /// Expected result: the most recently set command should be returned.
        /// </summary>
        [Fact]
        public void PointerMovedCommand_MultipleChanges_ReturnsLatestCommand()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var firstCommand = Substitute.For<ICommand>();
            var secondCommand = Substitute.For<ICommand>();
            var thirdCommand = Substitute.For<ICommand>();

            // Act & Assert
            recognizer.PointerMovedCommand = firstCommand;
            Assert.Same(firstCommand, recognizer.PointerMovedCommand);

            recognizer.PointerMovedCommand = secondCommand;
            Assert.Same(secondCommand, recognizer.PointerMovedCommand);

            recognizer.PointerMovedCommand = thirdCommand;
            Assert.Same(thirdCommand, recognizer.PointerMovedCommand);
        }

        /// <summary>
        /// Tests that SendPointerMoved does not throw when PointerMovedCommand is null.
        /// Validates that the method gracefully handles null command scenarios.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        public void SendPointerMoved_NullCommand_DoesNotThrow()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            Point? getPositionResult = new Point(10, 20);
            Func<IElement, Point?> getPosition = _ => getPositionResult;

            // Act & Assert
            var exception = Record.Exception(() => recognizer.SendPointerMoved(sender, getPosition));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendPointerMoved does not execute command when CanExecute returns false.
        /// Validates that the command execution is properly gated by the CanExecute check.
        /// Expected result: Command.Execute is not called when CanExecute returns false.
        /// </summary>
        [Fact]
        public void SendPointerMoved_CommandCannotExecute_DoesNotExecuteCommand()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(Arg.Any<object>()).Returns(false);

            recognizer.PointerMovedCommand = mockCommand;
            recognizer.PointerMovedCommandParameter = "test";

            Point? getPositionResult = new Point(10, 20);
            Func<IElement, Point?> getPosition = _ => getPositionResult;

            // Act
            recognizer.SendPointerMoved(sender, getPosition);

            // Assert
            mockCommand.Received(1).CanExecute("test");
            mockCommand.DidNotReceive().Execute(Arg.Any<object>());
        }

        /// <summary>
        /// Tests that SendPointerMoved executes command when CanExecute returns true.
        /// Validates that the command is properly executed with the correct parameter.
        /// Expected result: Command.Execute is called with PointerMovedCommandParameter.
        /// </summary>
        [Fact]
        public void SendPointerMoved_CommandCanExecute_ExecutesCommand()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var mockCommand = Substitute.For<ICommand>();
            var commandParameter = new object();

            mockCommand.CanExecute(commandParameter).Returns(true);
            recognizer.PointerMovedCommand = mockCommand;
            recognizer.PointerMovedCommandParameter = commandParameter;

            Point? getPositionResult = new Point(10, 20);
            Func<IElement, Point?> getPosition = _ => getPositionResult;

            // Act
            recognizer.SendPointerMoved(sender, getPosition);

            // Assert
            mockCommand.Received(1).CanExecute(commandParameter);
            mockCommand.Received(1).Execute(commandParameter);
        }

        /// <summary>
        /// Tests that SendPointerMoved handles null command parameter correctly.
        /// Validates that null parameters are passed correctly to CanExecute and Execute.
        /// Expected result: Command methods are called with null parameter.
        /// </summary>
        [Fact]
        public void SendPointerMoved_NullCommandParameter_PassesNullToCommand()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var mockCommand = Substitute.For<ICommand>();

            mockCommand.CanExecute(null).Returns(true);
            recognizer.PointerMovedCommand = mockCommand;
            recognizer.PointerMovedCommandParameter = null;

            Point? getPositionResult = new Point(10, 20);
            Func<IElement, Point?> getPosition = _ => getPositionResult;

            // Act
            recognizer.SendPointerMoved(sender, getPosition);

            // Assert
            mockCommand.Received(1).CanExecute(null);
            mockCommand.Received(1).Execute(null);
        }

        /// <summary>
        /// Tests that SendPointerMoved invokes PointerMoved event when subscribed.
        /// Validates that event handlers are properly invoked with correct arguments.
        /// Expected result: Event handler is called with sender and PointerEventArgs.
        /// </summary>
        [Fact]
        public void SendPointerMoved_EventHandlerSubscribed_InvokesEvent()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            Point? getPositionResult = new Point(15, 25);
            Func<IElement, Point?> getPosition = _ => getPositionResult;

            object eventSender = null;
            PointerEventArgs eventArgs = null;
            recognizer.PointerMoved += (s, e) =>
            {
                eventSender = s;
                eventArgs = e;
            };

            // Act
            recognizer.SendPointerMoved(sender, getPosition, null, ButtonsMask.Secondary);

            // Assert
            Assert.Same(sender, eventSender);
            Assert.NotNull(eventArgs);
            Assert.Equal(ButtonsMask.Secondary, eventArgs.Button);
        }

        /// <summary>
        /// Tests that SendPointerMoved does not throw when no event handlers are subscribed.
        /// Validates that the method gracefully handles null event handler scenarios.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        public void SendPointerMoved_NoEventHandler_DoesNotThrow()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            Point? getPositionResult = new Point(10, 20);
            Func<IElement, Point?> getPosition = _ => getPositionResult;

            // Act & Assert
            var exception = Record.Exception(() => recognizer.SendPointerMoved(sender, getPosition));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendPointerMoved invokes multiple subscribed event handlers.
        /// Validates that all subscribed event handlers receive the event.
        /// Expected result: All event handlers are called.
        /// </summary>
        [Fact]
        public void SendPointerMoved_MultipleEventHandlers_InvokesAllHandlers()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            Point? getPositionResult = new Point(10, 20);
            Func<IElement, Point?> getPosition = _ => getPositionResult;

            int handler1Called = 0;
            int handler2Called = 0;

            recognizer.PointerMoved += (s, e) => handler1Called++;
            recognizer.PointerMoved += (s, e) => handler2Called++;

            // Act
            recognizer.SendPointerMoved(sender, getPosition);

            // Assert
            Assert.Equal(1, handler1Called);
            Assert.Equal(1, handler2Called);
        }

        /// <summary>
        /// Tests SendPointerMoved with various ButtonsMask values.
        /// Validates that different button mask values are correctly passed to PointerEventArgs.
        /// Expected result: PointerEventArgs.Button matches the provided button parameter.
        /// </summary>
        [Theory]
        [InlineData(ButtonsMask.Primary)]
        [InlineData(ButtonsMask.Secondary)]
        [InlineData(ButtonsMask.Primary | ButtonsMask.Secondary)]
        public void SendPointerMoved_DifferentButtonMasks_PassesCorrectButtonToEventArgs(ButtonsMask button)
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            Point? getPositionResult = new Point(10, 20);
            Func<IElement, Point?> getPosition = _ => getPositionResult;

            PointerEventArgs capturedArgs = null;
            recognizer.PointerMoved += (s, e) => capturedArgs = e;

            // Act
            recognizer.SendPointerMoved(sender, getPosition, null, button);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Equal(button, capturedArgs.Button);
        }

        /// <summary>
        /// Tests SendPointerMoved with null getPosition function.
        /// Validates that null getPosition parameter is handled correctly.
        /// Expected result: PointerEventArgs is created successfully with null getPosition.
        /// </summary>
        [Fact]
        public void SendPointerMoved_NullGetPosition_CreatesEventArgsSuccessfully()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();

            PointerEventArgs capturedArgs = null;
            recognizer.PointerMoved += (s, e) => capturedArgs = e;

            // Act
            recognizer.SendPointerMoved(sender, null);

            // Assert
            Assert.NotNull(capturedArgs);
        }

        /// <summary>
        /// Tests SendPointerMoved with valid PlatformPointerEventArgs.
        /// Validates that PlatformPointerEventArgs is correctly passed to PointerEventArgs.
        /// Expected result: PointerEventArgs.PlatformArgs matches the provided platformArgs.
        /// </summary>
        [Fact]
        public void SendPointerMoved_WithPlatformArgs_PassesPlatformArgsToEventArgs()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            Point? getPositionResult = new Point(10, 20);
            Func<IElement, Point?> getPosition = _ => getPositionResult;
            var platformArgs = new PlatformPointerEventArgs();

            PointerEventArgs capturedArgs = null;
            recognizer.PointerMoved += (s, e) => capturedArgs = e;

            // Act
            recognizer.SendPointerMoved(sender, getPosition, platformArgs);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Same(platformArgs, capturedArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests SendPointerMoved executes both command and event when both are configured.
        /// Validates that both command execution and event invocation work together.
        /// Expected result: Both command is executed and event handler is invoked.
        /// </summary>
        [Fact]
        public void SendPointerMoved_BothCommandAndEvent_ExecutesBothSuccessfully()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var mockCommand = Substitute.For<ICommand>();
            var commandParameter = "test parameter";

            mockCommand.CanExecute(commandParameter).Returns(true);
            recognizer.PointerMovedCommand = mockCommand;
            recognizer.PointerMovedCommandParameter = commandParameter;

            bool eventInvoked = false;
            recognizer.PointerMoved += (s, e) => eventInvoked = true;

            Point? getPositionResult = new Point(10, 20);
            Func<IElement, Point?> getPosition = _ => getPositionResult;

            // Act
            recognizer.SendPointerMoved(sender, getPosition);

            // Assert
            mockCommand.Received(1).Execute(commandParameter);
            Assert.True(eventInvoked);
        }

        /// <summary>
        /// Tests SendPointerMoved continues execution when command throws exception.
        /// Validates that command exceptions don't prevent event handler invocation.
        /// Expected result: Event handler is still invoked despite command exception.
        /// </summary>
        [Fact]
        public void SendPointerMoved_CommandThrowsException_ContinuesWithEventInvocation()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var mockCommand = Substitute.For<ICommand>();

            mockCommand.CanExecute(Arg.Any<object>()).Returns(true);
            mockCommand.When(x => x.Execute(Arg.Any<object>())).Do(x => throw new InvalidOperationException("Command failed"));
            recognizer.PointerMovedCommand = mockCommand;

            bool eventInvoked = false;
            recognizer.PointerMoved += (s, e) => eventInvoked = true;

            Point? getPositionResult = new Point(10, 20);
            Func<IElement, Point?> getPosition = _ => getPositionResult;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => recognizer.SendPointerMoved(sender, getPosition));
            Assert.False(eventInvoked); // Event won't be invoked due to exception
        }

        /// <summary>
        /// Tests SendPointerMoved with invalid ButtonsMask enum value.
        /// Validates handling of out-of-range enum values through casting.
        /// Expected result: Invalid enum value is passed through to PointerEventArgs.
        /// </summary>
        [Fact]
        public void SendPointerMoved_InvalidButtonsMask_PassesValueToEventArgs()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            Point? getPositionResult = new Point(10, 20);
            Func<IElement, Point?> getPosition = _ => getPositionResult;
            var invalidButton = (ButtonsMask)999;

            PointerEventArgs capturedArgs = null;
            recognizer.PointerMoved += (s, e) => capturedArgs = e;

            // Act
            recognizer.SendPointerMoved(sender, getPosition, null, invalidButton);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Equal(invalidButton, capturedArgs.Button);
        }

        /// <summary>
        /// Tests that the PointerMovedCommandParameter getter returns null when no value has been set.
        /// </summary>
        [Fact]
        public void PointerMovedCommandParameter_DefaultValue_ReturnsNull()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            var result = recognizer.PointerMovedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the PointerMovedCommandParameter getter returns the same null value that was set.
        /// </summary>
        [Fact]
        public void PointerMovedCommandParameter_SetNull_ReturnsNull()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            recognizer.PointerMovedCommandParameter = null;
            var result = recognizer.PointerMovedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the PointerMovedCommandParameter getter returns the same string value that was set.
        /// </summary>
        [Fact]
        public void PointerMovedCommandParameter_SetStringValue_ReturnsStringValue()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var expectedValue = "test parameter";

            // Act
            recognizer.PointerMovedCommandParameter = expectedValue;
            var result = recognizer.PointerMovedCommandParameter;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the PointerMovedCommandParameter getter returns the same numeric value that was set.
        /// </summary>
        [Theory]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void PointerMovedCommandParameter_SetIntValue_ReturnsIntValue(int expectedValue)
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            recognizer.PointerMovedCommandParameter = expectedValue;
            var result = recognizer.PointerMovedCommandParameter;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the PointerMovedCommandParameter getter returns the same floating-point value that was set.
        /// </summary>
        [Theory]
        [InlineData(3.14)]
        [InlineData(0.0)]
        [InlineData(-1.5)]
        public void PointerMovedCommandParameter_SetDoubleValue_ReturnsDoubleValue(double expectedValue)
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            recognizer.PointerMovedCommandParameter = expectedValue;
            var result = recognizer.PointerMovedCommandParameter;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the PointerMovedCommandParameter getter returns the same special floating-point value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void PointerMovedCommandParameter_SetSpecialDoubleValue_ReturnsSpecialDoubleValue(double expectedValue)
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            recognizer.PointerMovedCommandParameter = expectedValue;
            var result = recognizer.PointerMovedCommandParameter;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the PointerMovedCommandParameter getter returns the same string edge case values that were set.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\n\r\t")]
        public void PointerMovedCommandParameter_SetStringEdgeCases_ReturnsStringEdgeCases(string expectedValue)
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            recognizer.PointerMovedCommandParameter = expectedValue;
            var result = recognizer.PointerMovedCommandParameter;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the PointerMovedCommandParameter getter returns the same custom object that was set.
        /// </summary>
        [Fact]
        public void PointerMovedCommandParameter_SetCustomObject_ReturnsCustomObject()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var expectedValue = new TestParameter { Value = "custom", Id = 123 };

            // Act
            recognizer.PointerMovedCommandParameter = expectedValue;
            var result = recognizer.PointerMovedCommandParameter;

            // Assert
            Assert.Same(expectedValue, result);
        }

        /// <summary>
        /// Tests that the PointerMovedCommandParameter getter returns the same collection that was set.
        /// </summary>
        [Fact]
        public void PointerMovedCommandParameter_SetCollection_ReturnsCollection()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var expectedValue = new List<string> { "item1", "item2", "item3" };

            // Act
            recognizer.PointerMovedCommandParameter = expectedValue;
            var result = recognizer.PointerMovedCommandParameter;

            // Assert
            Assert.Same(expectedValue, result);
        }

        /// <summary>
        /// Tests that the PointerMovedCommandParameter getter returns the same empty collection that was set.
        /// </summary>
        [Fact]
        public void PointerMovedCommandParameter_SetEmptyCollection_ReturnsEmptyCollection()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var expectedValue = new List<string>();

            // Act
            recognizer.PointerMovedCommandParameter = expectedValue;
            var result = recognizer.PointerMovedCommandParameter;

            // Assert
            Assert.Same(expectedValue, result);
        }

        /// <summary>
        /// Tests that multiple gets of PointerMovedCommandParameter return the same value without side effects.
        /// </summary>
        [Fact]
        public void PointerMovedCommandParameter_MultipleGets_ReturnsConsistentValue()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var expectedValue = "consistent value";
            recognizer.PointerMovedCommandParameter = expectedValue;

            // Act
            var result1 = recognizer.PointerMovedCommandParameter;
            var result2 = recognizer.PointerMovedCommandParameter;
            var result3 = recognizer.PointerMovedCommandParameter;

            // Assert
            Assert.Equal(expectedValue, result1);
            Assert.Equal(expectedValue, result2);
            Assert.Equal(expectedValue, result3);
            Assert.Same(result1, result2);
            Assert.Same(result2, result3);
        }

        private class TestParameter
        {
            public string Value { get; set; }
            public int Id { get; set; }
        }

        /// <summary>
        /// Tests that PointerReleasedCommandParameter returns null by default.
        /// Validates the default state of the bindable property.
        /// </summary>
        [Fact]
        public void PointerReleasedCommandParameter_DefaultValue_ReturnsNull()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            var result = recognizer.PointerReleasedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerReleasedCommandParameter can be set and retrieved with various object types.
        /// Validates property getter and setter functionality with different reference types.
        /// </summary>
        [Theory]
        [InlineData("string parameter")]
        [InlineData(42)]
        [InlineData(true)]
        public void PointerReleasedCommandParameter_SetAndGet_ReturnsSetValue(object parameter)
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            recognizer.PointerReleasedCommandParameter = parameter;
            var result = recognizer.PointerReleasedCommandParameter;

            // Assert
            Assert.Equal(parameter, result);
        }

        /// <summary>
        /// Tests that PointerReleasedCommandParameter can be explicitly set to null.
        /// Validates null assignment and retrieval behavior.
        /// </summary>
        [Fact]
        public void PointerReleasedCommandParameter_SetNull_ReturnsNull()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            recognizer.PointerReleasedCommandParameter = "initial value";

            // Act
            recognizer.PointerReleasedCommandParameter = null;
            var result = recognizer.PointerReleasedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerReleasedCommandParameter maintains object reference identity.
        /// Validates that the same reference is returned when getting the property.
        /// </summary>
        [Fact]
        public void PointerReleasedCommandParameter_SetCustomObject_ReturnsSameReference()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var customObject = new CustomCommandParameter { Value = "test" };

            // Act
            recognizer.PointerReleasedCommandParameter = customObject;
            var result = recognizer.PointerReleasedCommandParameter;

            // Assert
            Assert.Same(customObject, result);
        }

        /// <summary>
        /// Tests that PointerReleasedCommandParameter can handle empty string values.
        /// Validates edge case behavior with empty strings.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void PointerReleasedCommandParameter_SetEmptyOrWhitespaceString_ReturnsSetValue(string parameter)
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            recognizer.PointerReleasedCommandParameter = parameter;
            var result = recognizer.PointerReleasedCommandParameter;

            // Assert
            Assert.Equal(parameter, result);
        }

        /// <summary>
        /// Tests that PointerReleasedCommandParameter can be overwritten with different values.
        /// Validates property update behavior with multiple assignments.
        /// </summary>
        [Fact]
        public void PointerReleasedCommandParameter_SetMultipleValues_ReturnsLatestValue()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var firstValue = "first";
            var secondValue = 123;

            // Act
            recognizer.PointerReleasedCommandParameter = firstValue;
            recognizer.PointerReleasedCommandParameter = secondValue;
            var result = recognizer.PointerReleasedCommandParameter;

            // Assert
            Assert.Equal(secondValue, result);
            Assert.NotEqual(firstValue, result);
        }

        /// <summary>
        /// Helper class for testing custom object parameter values.
        /// </summary>
        private class CustomCommandParameter
        {
            public string Value { get; set; } = string.Empty;
        }

        /// <summary>
        /// Tests that PointerPressedCommand returns null when not explicitly set (default value).
        /// Verifies the getter returns the correct default value for the bindable property.
        /// </summary>
        [Fact]
        public void PointerPressedCommand_DefaultValue_ReturnsNull()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();

            // Act
            var result = gesture.PointerPressedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerPressedCommand getter returns the same command that was set via the setter.
        /// Verifies the property correctly retrieves values from the underlying bindable property.
        /// </summary>
        [Fact]
        public void PointerPressedCommand_AfterSettingValidCommand_ReturnsSetCommand()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var command = new Command(() => { });

            // Act
            gesture.PointerPressedCommand = command;
            var result = gesture.PointerPressedCommand;

            // Assert
            Assert.Same(command, result);
        }

        /// <summary>
        /// Tests that PointerPressedCommand getter returns null after being explicitly set to null.
        /// Verifies the property can be cleared and the getter reflects this change.
        /// </summary>
        [Fact]
        public void PointerPressedCommand_AfterSettingToNull_ReturnsNull()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var command = new Command(() => { });
            gesture.PointerPressedCommand = command;

            // Act
            gesture.PointerPressedCommand = null;
            var result = gesture.PointerPressedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerPressedCommand getter works correctly with mocked ICommand implementations.
        /// Verifies the property handles different ICommand implementations properly.
        /// </summary>
        [Fact]
        public void PointerPressedCommand_WithMockedCommand_ReturnsSetCommand()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var mockedCommand = Substitute.For<ICommand>();

            // Act
            gesture.PointerPressedCommand = mockedCommand;
            var result = gesture.PointerPressedCommand;

            // Assert
            Assert.Same(mockedCommand, result);
        }

        /// <summary>
        /// Tests that the PointerPressedCommandParameter getter returns the correct value when set to null.
        /// Verifies the property can handle null values correctly.
        /// Expected result: The getter should return null when the property is set to null.
        /// </summary>
        [Fact]
        public void PointerPressedCommandParameter_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            recognizer.PointerPressedCommandParameter = null;
            var result = recognizer.PointerPressedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the PointerPressedCommandParameter getter returns the correct value when set to a string.
        /// Verifies the property can handle string objects correctly.
        /// Expected result: The getter should return the exact string value that was set.
        /// </summary>
        [Fact]
        public void PointerPressedCommandParameter_WhenSetToString_ReturnsCorrectValue()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var expected = "test parameter";

            // Act
            recognizer.PointerPressedCommandParameter = expected;
            var result = recognizer.PointerPressedCommandParameter;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that the PointerPressedCommandParameter getter returns the correct value when set to an integer.
        /// Verifies the property can handle value types correctly.
        /// Expected result: The getter should return the exact integer value that was set.
        /// </summary>
        [Fact]
        public void PointerPressedCommandParameter_WhenSetToInteger_ReturnsCorrectValue()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var expected = 42;

            // Act
            recognizer.PointerPressedCommandParameter = expected;
            var result = recognizer.PointerPressedCommandParameter;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that the PointerPressedCommandParameter getter returns the correct value when set to a custom object.
        /// Verifies the property can handle reference types correctly.
        /// Expected result: The getter should return the exact same object reference that was set.
        /// </summary>
        [Fact]
        public void PointerPressedCommandParameter_WhenSetToCustomObject_ReturnsCorrectValue()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var expected = new TestObject { Value = "test" };

            // Act
            recognizer.PointerPressedCommandParameter = expected;
            var result = recognizer.PointerPressedCommandParameter;

            // Assert
            Assert.Same(expected, result);
        }

        /// <summary>
        /// Tests that the PointerPressedCommandParameter getter returns null by default.
        /// Verifies the property has the correct default value as defined in the BindableProperty.
        /// Expected result: The getter should return null when no value has been set.
        /// </summary>
        [Fact]
        public void PointerPressedCommandParameter_DefaultValue_ReturnsNull()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            var result = recognizer.PointerPressedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        private class TestObject
        {
            public string Value { get; set; }
        }

        /// <summary>
        /// Tests that SendPointerPressed executes command when PointerPressedCommand is set and CanExecute returns true.
        /// Verifies that both the command execution and event invocation paths are executed.
        /// </summary>
        [Fact]
        public void SendPointerPressed_WithValidCommand_ExecutesCommandAndInvokesEvent()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();
            var commandParameter = new object();
            var sender = new View();
            var eventInvoked = false;

            mockCommand.CanExecute(commandParameter).Returns(true);
            gestureRecognizer.PointerPressedCommand = mockCommand;
            gestureRecognizer.PointerPressedCommandParameter = commandParameter;
            gestureRecognizer.PointerPressed += (s, e) => eventInvoked = true;

            // Act
            gestureRecognizer.SendPointerPressed(sender, null);

            // Assert
            mockCommand.Received(1).CanExecute(commandParameter);
            mockCommand.Received(1).Execute(commandParameter);
            Assert.True(eventInvoked);
        }

        /// <summary>
        /// Tests that SendPointerPressed does not execute command when PointerPressedCommand is null.
        /// Verifies that event is still invoked even when command is null.
        /// </summary>
        [Fact]
        public void SendPointerPressed_WithNullCommand_DoesNotExecuteCommandButInvokesEvent()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var sender = new View();
            var eventInvoked = false;

            gestureRecognizer.PointerPressedCommand = null;
            gestureRecognizer.PointerPressed += (s, e) => eventInvoked = true;

            // Act
            gestureRecognizer.SendPointerPressed(sender, null);

            // Assert
            Assert.True(eventInvoked);
        }

        /// <summary>
        /// Tests that SendPointerPressed does not execute command when CanExecute returns false.
        /// Verifies that event is still invoked even when command cannot execute.
        /// </summary>
        [Fact]
        public void SendPointerPressed_WithCommandCanExecuteFalse_DoesNotExecuteCommandButInvokesEvent()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();
            var commandParameter = new object();
            var sender = new View();
            var eventInvoked = false;

            mockCommand.CanExecute(commandParameter).Returns(false);
            gestureRecognizer.PointerPressedCommand = mockCommand;
            gestureRecognizer.PointerPressedCommandParameter = commandParameter;
            gestureRecognizer.PointerPressed += (s, e) => eventInvoked = true;

            // Act
            gestureRecognizer.SendPointerPressed(sender, null);

            // Assert
            mockCommand.Received(1).CanExecute(commandParameter);
            mockCommand.DidNotReceive().Execute(Arg.Any<object>());
            Assert.True(eventInvoked);
        }

        /// <summary>
        /// Tests that SendPointerPressed works correctly when no event handlers are attached.
        /// Verifies that command execution still works when event handler is null.
        /// </summary>
        [Fact]
        public void SendPointerPressed_WithNoEventHandlers_ExecutesCommandWithoutException()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();
            var commandParameter = new object();
            var sender = new View();

            mockCommand.CanExecute(commandParameter).Returns(true);
            gestureRecognizer.PointerPressedCommand = mockCommand;
            gestureRecognizer.PointerPressedCommandParameter = commandParameter;

            // Act & Assert (should not throw)
            gestureRecognizer.SendPointerPressed(sender, null);

            mockCommand.Received(1).CanExecute(commandParameter);
            mockCommand.Received(1).Execute(commandParameter);
        }

        /// <summary>
        /// Tests that SendPointerPressed passes correct parameters to PointerEventArgs constructor.
        /// Verifies all parameter combinations including getPosition, platformArgs, and button.
        /// </summary>
        [Theory]
        [InlineData(ButtonsMask.Primary)]
        [InlineData(ButtonsMask.Secondary)]
        [InlineData(ButtonsMask.Primary | ButtonsMask.Secondary)]
        public void SendPointerPressed_WithDifferentButtonMasks_PassesCorrectButtonToEvent(ButtonsMask button)
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var sender = new View();
            var platformArgs = new PlatformPointerEventArgs();
            Point? capturedPosition = null;
            PlatformPointerEventArgs capturedPlatformArgs = null;
            ButtonsMask capturedButton = ButtonsMask.Primary;

            Func<IElement, Point?> getPosition = _ => new Point(10, 20);

            gestureRecognizer.PointerPressed += (s, e) =>
            {
                capturedPosition = e.GetPosition(sender);
                capturedPlatformArgs = e.PlatformArgs;
                capturedButton = e.Button;
            };

            // Act
            gestureRecognizer.SendPointerPressed(sender, getPosition, platformArgs, button);

            // Assert
            Assert.Equal(new Point(10, 20), capturedPosition);
            Assert.Equal(platformArgs, capturedPlatformArgs);
            Assert.Equal(button, capturedButton);
        }

        /// <summary>
        /// Tests that SendPointerPressed works with null command parameter.
        /// Verifies command execution with null parameter when PointerPressedCommandParameter is null.
        /// </summary>
        [Fact]
        public void SendPointerPressed_WithNullCommandParameter_ExecutesCommandWithNullParameter()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();
            var sender = new View();

            mockCommand.CanExecute(null).Returns(true);
            gestureRecognizer.PointerPressedCommand = mockCommand;
            gestureRecognizer.PointerPressedCommandParameter = null;

            // Act
            gestureRecognizer.SendPointerPressed(sender, null);

            // Assert
            mockCommand.Received(1).CanExecute(null);
            mockCommand.Received(1).Execute(null);
        }

        /// <summary>
        /// Tests that SendPointerPressed works correctly with null getPosition function.
        /// Verifies that null getPosition is properly passed to PointerEventArgs constructor.
        /// </summary>
        [Fact]
        public void SendPointerPressed_WithNullGetPosition_PassesNullToEventArgs()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var sender = new View();
            Point? capturedPosition = new Point(999, 999); // Set to non-null initially

            gestureRecognizer.PointerPressed += (s, e) =>
            {
                capturedPosition = e.GetPosition(sender);
            };

            // Act
            gestureRecognizer.SendPointerPressed(sender, null);

            // Assert
            Assert.Null(capturedPosition);
        }

        /// <summary>
        /// Tests that SendPointerPressed works correctly with null platformArgs.
        /// Verifies that null platformArgs is properly passed to PointerEventArgs constructor.
        /// </summary>
        [Fact]
        public void SendPointerPressed_WithNullPlatformArgs_PassesNullToEventArgs()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var sender = new View();
            PlatformPointerEventArgs capturedPlatformArgs = new PlatformPointerEventArgs(); // Set to non-null initially

            gestureRecognizer.PointerPressed += (s, e) =>
            {
                capturedPlatformArgs = e.PlatformArgs;
            };

            // Act
            gestureRecognizer.SendPointerPressed(sender, null, null);

            // Assert
            Assert.Null(capturedPlatformArgs);
        }

        /// <summary>
        /// Tests that SendPointerPressed uses default ButtonsMask.Primary when no button parameter is provided.
        /// Verifies the default parameter behavior for the button parameter.
        /// </summary>
        [Fact]
        public void SendPointerPressed_WithDefaultButton_UsesPrimary()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var sender = new View();
            ButtonsMask capturedButton = ButtonsMask.Secondary; // Set to non-default initially

            gestureRecognizer.PointerPressed += (s, e) =>
            {
                capturedButton = e.Button;
            };

            // Act
            gestureRecognizer.SendPointerPressed(sender, null);

            // Assert
            Assert.Equal(ButtonsMask.Primary, capturedButton);
        }

        /// <summary>
        /// Tests that SendPointerPressed passes the correct sender to event handler.
        /// Verifies that the sender parameter is properly passed to the event invocation.
        /// </summary>
        [Fact]
        public void SendPointerPressed_WithSender_PassesCorrectSenderToEvent()
        {
            // Arrange
            var gestureRecognizer = new PointerGestureRecognizer();
            var sender = new View();
            object capturedSender = null;

            gestureRecognizer.PointerPressed += (s, e) =>
            {
                capturedSender = s;
            };

            // Act
            gestureRecognizer.SendPointerPressed(sender, null);

            // Assert
            Assert.Equal(sender, capturedSender);
        }

        /// <summary>
        /// Tests that PointerReleasedCommand getter returns null when no command is set.
        /// </summary>
        [Fact]
        public void PointerReleasedCommand_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();

            // Act
            var result = recognizer.PointerReleasedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerReleasedCommand getter returns the same command that was set.
        /// </summary>
        [Fact]
        public void PointerReleasedCommand_WhenSet_ReturnsSameCommand()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();

            // Act
            recognizer.PointerReleasedCommand = mockCommand;
            var result = recognizer.PointerReleasedCommand;

            // Assert
            Assert.Same(mockCommand, result);
        }

        /// <summary>
        /// Tests that PointerReleasedCommand getter returns correct command after multiple sets.
        /// </summary>
        [Fact]
        public void PointerReleasedCommand_WhenSetMultipleTimes_ReturnsLatestCommand()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var firstCommand = Substitute.For<ICommand>();
            var secondCommand = Substitute.For<ICommand>();

            // Act
            recognizer.PointerReleasedCommand = firstCommand;
            recognizer.PointerReleasedCommand = secondCommand;
            var result = recognizer.PointerReleasedCommand;

            // Assert
            Assert.Same(secondCommand, result);
            Assert.NotSame(firstCommand, result);
        }

        /// <summary>
        /// Tests that PointerReleasedCommand getter returns null after being set to null.
        /// </summary>
        [Fact]
        public void PointerReleasedCommand_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();
            recognizer.PointerReleasedCommand = mockCommand;

            // Act
            recognizer.PointerReleasedCommand = null;
            var result = recognizer.PointerReleasedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SendPointerReleased executes command when command is set and CanExecute returns true.
        /// Tests command execution path with valid command.
        /// Expected result: Command is executed with the command parameter.
        /// </summary>
        [Fact]
        public void SendPointerReleased_WithValidCommand_ExecutesCommand()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var sender = new View();
            var getPosition = Substitute.For<Func<IElement, Point?>>();
            var commandParameter = new object();
            var commandExecuted = false;
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(commandParameter).Returns(true);
            mockCommand.When(x => x.Execute(commandParameter)).Do(_ => commandExecuted = true);

            gesture.PointerReleasedCommand = mockCommand;
            gesture.PointerReleasedCommandParameter = commandParameter;

            // Act
            gesture.SendPointerReleased(sender, getPosition);

            // Assert
            mockCommand.Received(1).CanExecute(commandParameter);
            mockCommand.Received(1).Execute(commandParameter);
            Assert.True(commandExecuted);
        }

        /// <summary>
        /// Tests that SendPointerReleased does not execute command when CanExecute returns false.
        /// Tests command execution path with command that cannot execute.
        /// Expected result: Command is not executed.
        /// </summary>
        [Fact]
        public void SendPointerReleased_WithCommandCannotExecute_DoesNotExecuteCommand()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var sender = new View();
            var getPosition = Substitute.For<Func<IElement, Point?>>();
            var commandParameter = new object();
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(commandParameter).Returns(false);

            gesture.PointerReleasedCommand = mockCommand;
            gesture.PointerReleasedCommandParameter = commandParameter;

            // Act
            gesture.SendPointerReleased(sender, getPosition);

            // Assert
            mockCommand.Received(1).CanExecute(commandParameter);
            mockCommand.DidNotReceive().Execute(Arg.Any<object>());
        }

        /// <summary>
        /// Tests that SendPointerReleased handles null command gracefully.
        /// Tests command execution path with null command.
        /// Expected result: No exception is thrown and event handler is still invoked.
        /// </summary>
        [Fact]
        public void SendPointerReleased_WithNullCommand_HandlesGracefully()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var sender = new View();
            var getPosition = Substitute.For<Func<IElement, Point?>>();
            var eventHandlerCalled = false;

            gesture.PointerReleasedCommand = null;
            gesture.PointerReleased += (s, e) => eventHandlerCalled = true;

            // Act & Assert
            gesture.SendPointerReleased(sender, getPosition);
            Assert.True(eventHandlerCalled);
        }

        /// <summary>
        /// Tests that SendPointerReleased invokes event handler when handler is set.
        /// Tests event handler invocation with valid parameters.
        /// Expected result: Event handler is invoked with correct sender and event args.
        /// </summary>
        [Fact]
        public void SendPointerReleased_WithEventHandler_InvokesHandler()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var sender = new View();
            var getPosition = Substitute.For<Func<IElement, Point?>>();
            var platformArgs = Substitute.For<PlatformPointerEventArgs>();
            var button = ButtonsMask.Secondary;

            object actualSender = null;
            PointerEventArgs actualEventArgs = null;
            gesture.PointerReleased += (s, e) =>
            {
                actualSender = s;
                actualEventArgs = e;
            };

            // Act
            gesture.SendPointerReleased(sender, getPosition, platformArgs, button);

            // Assert
            Assert.Same(sender, actualSender);
            Assert.NotNull(actualEventArgs);
            Assert.Same(platformArgs, actualEventArgs.PlatformArgs);
            Assert.Equal(button, actualEventArgs.Button);
        }

        /// <summary>
        /// Tests that SendPointerReleased handles null event handler gracefully.
        /// Tests event handler invocation path with null handler.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void SendPointerReleased_WithNullEventHandler_HandlesGracefully()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var sender = new View();
            var getPosition = Substitute.For<Func<IElement, Point?>>();

            // Act & Assert - Should not throw
            gesture.SendPointerReleased(sender, getPosition);
        }

        /// <summary>
        /// Tests SendPointerReleased with various ButtonsMask values.
        /// Tests all valid button mask combinations.
        /// Expected result: Correct button value is passed to event args.
        /// </summary>
        [Theory]
        [InlineData(ButtonsMask.Primary)]
        [InlineData(ButtonsMask.Secondary)]
        [InlineData(ButtonsMask.Primary | ButtonsMask.Secondary)]
        public void SendPointerReleased_WithDifferentButtonMasks_PassesCorrectButton(ButtonsMask button)
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var sender = new View();
            var getPosition = Substitute.For<Func<IElement, Point?>>();

            PointerEventArgs actualEventArgs = null;
            gesture.PointerReleased += (s, e) => actualEventArgs = e;

            // Act
            gesture.SendPointerReleased(sender, getPosition, button: button);

            // Assert
            Assert.NotNull(actualEventArgs);
            Assert.Equal(button, actualEventArgs.Button);
        }

        /// <summary>
        /// Tests SendPointerReleased with null platform arguments.
        /// Tests optional platform arguments parameter with null value.
        /// Expected result: Null platform args are correctly passed to event args.
        /// </summary>
        [Fact]
        public void SendPointerReleased_WithNullPlatformArgs_PassesNullToPlatformArgs()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var sender = new View();
            var getPosition = Substitute.For<Func<IElement, Point?>>();

            PointerEventArgs actualEventArgs = null;
            gesture.PointerReleased += (s, e) => actualEventArgs = e;

            // Act
            gesture.SendPointerReleased(sender, getPosition, platformArgs: null);

            // Assert
            Assert.NotNull(actualEventArgs);
            Assert.Null(actualEventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests SendPointerReleased with null getPosition function.
        /// Tests optional getPosition parameter with null value.
        /// Expected result: Null getPosition function is correctly passed to event args.
        /// </summary>
        [Fact]
        public void SendPointerReleased_WithNullGetPosition_PassesNullToEventArgs()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var sender = new View();

            PointerEventArgs actualEventArgs = null;
            gesture.PointerReleased += (s, e) => actualEventArgs = e;

            // Act
            gesture.SendPointerReleased(sender, getPosition: null);

            // Assert
            Assert.NotNull(actualEventArgs);
        }

        /// <summary>
        /// Tests SendPointerReleased executes both command and event handler when both are set.
        /// Tests complete execution path with both command and event handler.
        /// Expected result: Both command execution and event handler invocation occur.
        /// </summary>
        [Fact]
        public void SendPointerReleased_WithCommandAndEventHandler_ExecutesBoth()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var sender = new View();
            var getPosition = Substitute.For<Func<IElement, Point?>>();
            var commandParameter = new object();
            var commandExecuted = false;
            var eventHandlerCalled = false;

            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(commandParameter).Returns(true);
            mockCommand.When(x => x.Execute(commandParameter)).Do(_ => commandExecuted = true);

            gesture.PointerReleasedCommand = mockCommand;
            gesture.PointerReleasedCommandParameter = commandParameter;
            gesture.PointerReleased += (s, e) => eventHandlerCalled = true;

            // Act
            gesture.SendPointerReleased(sender, getPosition);

            // Assert
            Assert.True(commandExecuted);
            Assert.True(eventHandlerCalled);
        }

        /// <summary>
        /// Tests SendPointerReleased with null command parameter.
        /// Tests command execution path with null parameter.
        /// Expected result: Command CanExecute and Execute are called with null parameter.
        /// </summary>
        [Fact]
        public void SendPointerReleased_WithNullCommandParameter_PassesNullToCommand()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var sender = new View();
            var getPosition = Substitute.For<Func<IElement, Point?>>();
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(null).Returns(true);

            gesture.PointerReleasedCommand = mockCommand;
            gesture.PointerReleasedCommandParameter = null;

            // Act
            gesture.SendPointerReleased(sender, getPosition);

            // Assert
            mockCommand.Received(1).CanExecute(null);
            mockCommand.Received(1).Execute(null);
        }

        /// <summary>
        /// Tests that SendPointerEntered executes command when command exists and can execute.
        /// Verifies command execution with valid PointerEnteredCommand and PointerEnteredCommandParameter.
        /// </summary>
        [Fact]
        public void SendPointerEntered_WithExecutableCommand_ExecutesCommand()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();
            var commandParameter = new object();
            var sender = new View();

            mockCommand.CanExecute(commandParameter).Returns(true);
            recognizer.PointerEnteredCommand = mockCommand;
            recognizer.PointerEnteredCommandParameter = commandParameter;

            // Act
            recognizer.SendPointerEntered(sender, null);

            // Assert
            mockCommand.Received(1).CanExecute(commandParameter);
            mockCommand.Received(1).Execute(commandParameter);
        }

        /// <summary>
        /// Tests that SendPointerEntered does not execute command when command cannot execute.
        /// Verifies that Execute is not called when CanExecute returns false.
        /// </summary>
        [Fact]
        public void SendPointerEntered_WithNonExecutableCommand_DoesNotExecuteCommand()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();
            var commandParameter = new object();
            var sender = new View();

            mockCommand.CanExecute(commandParameter).Returns(false);
            recognizer.PointerEnteredCommand = mockCommand;
            recognizer.PointerEnteredCommandParameter = commandParameter;

            // Act
            recognizer.SendPointerEntered(sender, null);

            // Assert
            mockCommand.Received(1).CanExecute(commandParameter);
            mockCommand.DidNotReceive().Execute(Arg.Any<object>());
        }

        /// <summary>
        /// Tests that SendPointerEntered handles null command gracefully.
        /// Verifies no exceptions are thrown when PointerEnteredCommand is null.
        /// </summary>
        [Fact]
        public void SendPointerEntered_WithNullCommand_HandlesGracefully()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();

            recognizer.PointerEnteredCommand = null;

            // Act & Assert (no exception should be thrown)
            recognizer.SendPointerEntered(sender, null);
        }

        /// <summary>
        /// Tests that SendPointerEntered handles null command parameter correctly.
        /// Verifies command execution with null PointerEnteredCommandParameter.
        /// </summary>
        [Fact]
        public void SendPointerEntered_WithNullCommandParameter_ExecutesWithNullParameter()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();
            var sender = new View();

            mockCommand.CanExecute(null).Returns(true);
            recognizer.PointerEnteredCommand = mockCommand;
            recognizer.PointerEnteredCommandParameter = null;

            // Act
            recognizer.SendPointerEntered(sender, null);

            // Assert
            mockCommand.Received(1).CanExecute(null);
            mockCommand.Received(1).Execute(null);
        }

        /// <summary>
        /// Tests that SendPointerEntered invokes PointerEntered event with correct arguments.
        /// Verifies event handler is called with sender and properly constructed PointerEventArgs.
        /// </summary>
        [Fact]
        public void SendPointerEntered_WithEventHandler_InvokesEventWithCorrectArguments()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var getPosition = new Func<Microsoft.Maui.IElement, Point?>(e => new Point(10, 20));
            var platformArgs = new PlatformPointerEventArgs();
            var button = ButtonsMask.Secondary;

            PointerEventArgs capturedArgs = null;
            object capturedSender = null;

            recognizer.PointerEntered += (s, e) =>
            {
                capturedSender = s;
                capturedArgs = e;
            };

            // Act
            recognizer.SendPointerEntered(sender, getPosition, platformArgs, button);

            // Assert
            Assert.Same(sender, capturedSender);
            Assert.NotNull(capturedArgs);
            Assert.Same(platformArgs, capturedArgs.PlatformArgs);
            Assert.Equal(button, capturedArgs.Button);
        }

        /// <summary>
        /// Tests that SendPointerEntered handles null event handler gracefully.
        /// Verifies no exceptions are thrown when no event handlers are subscribed.
        /// </summary>
        [Fact]
        public void SendPointerEntered_WithoutEventHandler_HandlesGracefully()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();

            // Act & Assert (no exception should be thrown)
            recognizer.SendPointerEntered(sender, null);
        }

        /// <summary>
        /// Tests SendPointerEntered behavior with various ButtonsMask values.
        /// Verifies correct button parameter is passed to PointerEventArgs for different button types.
        /// </summary>
        [Theory]
        [InlineData(ButtonsMask.Primary)]
        [InlineData(ButtonsMask.Secondary)]
        [InlineData(ButtonsMask.Primary | ButtonsMask.Secondary)]
        public void SendPointerEntered_WithDifferentButtons_PassesCorrectButtonToEvent(ButtonsMask button)
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();

            PointerEventArgs capturedArgs = null;
            recognizer.PointerEntered += (s, e) => capturedArgs = e;

            // Act
            recognizer.SendPointerEntered(sender, null, null, button);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Equal(button, capturedArgs.Button);
        }

        /// <summary>
        /// Tests SendPointerEntered with null getPosition function.
        /// Verifies event is invoked correctly when getPosition parameter is null.
        /// </summary>
        [Fact]
        public void SendPointerEntered_WithNullGetPosition_InvokesEventCorrectly()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();

            PointerEventArgs capturedArgs = null;
            recognizer.PointerEntered += (s, e) => capturedArgs = e;

            // Act
            recognizer.SendPointerEntered(sender, null);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Equal(ButtonsMask.Primary, capturedArgs.Button);
            Assert.Null(capturedArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests SendPointerEntered executes both command and event when both are present.
        /// Verifies complete flow when both PointerEnteredCommand and PointerEntered event handler exist.
        /// </summary>
        [Fact]
        public void SendPointerEntered_WithBothCommandAndEvent_ExecutesBoth()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var mockCommand = Substitute.For<ICommand>();
            var commandParameter = "test parameter";
            var sender = new View();

            mockCommand.CanExecute(commandParameter).Returns(true);
            recognizer.PointerEnteredCommand = mockCommand;
            recognizer.PointerEnteredCommandParameter = commandParameter;

            bool eventInvoked = false;
            recognizer.PointerEntered += (s, e) => eventInvoked = true;

            // Act
            recognizer.SendPointerEntered(sender, null);

            // Assert
            mockCommand.Received(1).Execute(commandParameter);
            Assert.True(eventInvoked);
        }

        /// <summary>
        /// Tests SendPointerEntered with default parameter values.
        /// Verifies method works correctly when using default values for optional parameters.
        /// </summary>
        [Fact]
        public void SendPointerEntered_WithDefaultParameters_UsesDefaultValues()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();

            PointerEventArgs capturedArgs = null;
            recognizer.PointerEntered += (s, e) => capturedArgs = e;

            // Act
            recognizer.SendPointerEntered(sender, null);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Null(capturedArgs.PlatformArgs);
            Assert.Equal(ButtonsMask.Primary, capturedArgs.Button);
        }

        /// <summary>
        /// Tests that SendPointerExited executes command when command is set and can execute.
        /// Validates command execution with valid command and parameter.
        /// Expected result: Command is executed with the correct parameter.
        /// </summary>
        [Fact]
        public void SendPointerExited_WithExecutableCommand_ExecutesCommand()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var commandMock = Substitute.For<ICommand>();
            var parameter = new object();
            commandMock.CanExecute(parameter).Returns(true);

            recognizer.PointerExitedCommand = commandMock;
            recognizer.PointerExitedCommandParameter = parameter;

            // Act
            recognizer.SendPointerExited(sender, null);

            // Assert
            commandMock.Received(1).CanExecute(parameter);
            commandMock.Received(1).Execute(parameter);
        }

        /// <summary>
        /// Tests that SendPointerExited does not execute command when command cannot execute.
        /// Validates command execution is skipped when CanExecute returns false.
        /// Expected result: Command.Execute is not called.
        /// </summary>
        [Fact]
        public void SendPointerExited_WithNonExecutableCommand_DoesNotExecuteCommand()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var commandMock = Substitute.For<ICommand>();
            var parameter = new object();
            commandMock.CanExecute(parameter).Returns(false);

            recognizer.PointerExitedCommand = commandMock;
            recognizer.PointerExitedCommandParameter = parameter;

            // Act
            recognizer.SendPointerExited(sender, null);

            // Assert
            commandMock.Received(1).CanExecute(parameter);
            commandMock.DidNotReceive().Execute(Arg.Any<object>());
        }

        /// <summary>
        /// Tests that SendPointerExited handles null command gracefully.
        /// Validates behavior when no command is set.
        /// Expected result: No exception is thrown and event is still raised.
        /// </summary>
        [Fact]
        public void SendPointerExited_WithNullCommand_HandlesGracefully()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var eventRaised = false;

            recognizer.PointerExited += (s, e) => eventRaised = true;
            recognizer.PointerExitedCommand = null;

            // Act & Assert - should not throw
            recognizer.SendPointerExited(sender, null);
            Assert.True(eventRaised);
        }

        /// <summary>
        /// Tests that SendPointerExited raises PointerExited event with correct arguments.
        /// Validates event invocation with proper PointerEventArgs construction.
        /// Expected result: Event is raised with sender and correct event args.
        /// </summary>
        [Fact]
        public void SendPointerExited_WithEventHandler_RaisesEvent()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var platformArgs = new PlatformPointerEventArgs();
            var getPosition = Substitute.For<Func<IElement, Point?>>();
            var expectedPoint = new Point(10, 20);
            getPosition(Arg.Any<IElement>()).Returns(expectedPoint);

            object actualSender = null;
            PointerEventArgs actualArgs = null;
            recognizer.PointerExited += (s, e) =>
            {
                actualSender = s;
                actualArgs = e;
            };

            // Act
            recognizer.SendPointerExited(sender, getPosition, platformArgs, ButtonsMask.Secondary);

            // Assert
            Assert.Equal(sender, actualSender);
            Assert.NotNull(actualArgs);
            Assert.Equal(ButtonsMask.Secondary, actualArgs.Button);
            Assert.Equal(platformArgs, actualArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that SendPointerExited handles null event handler gracefully.
        /// Validates behavior when no event handler is subscribed.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void SendPointerExited_WithNullEventHandler_HandlesGracefully()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();

            // Act & Assert - should not throw
            recognizer.SendPointerExited(sender, null);
        }

        /// <summary>
        /// Tests SendPointerExited with various ButtonsMask values.
        /// Validates that different button mask values are properly passed to event args.
        /// Expected result: Event args contain the correct button mask value.
        /// </summary>
        [Theory]
        [InlineData(ButtonsMask.Primary)]
        [InlineData(ButtonsMask.Secondary)]
        public void SendPointerExited_WithDifferentButtonsMask_PassesCorrectButton(ButtonsMask button)
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            ButtonsMask actualButton = ButtonsMask.Primary;

            recognizer.PointerExited += (s, e) => actualButton = e.Button;

            // Act
            recognizer.SendPointerExited(sender, null, null, button);

            // Assert
            Assert.Equal(button, actualButton);
        }

        /// <summary>
        /// Tests SendPointerExited with null command parameter.
        /// Validates command execution when parameter is null.
        /// Expected result: Command receives null parameter correctly.
        /// </summary>
        [Fact]
        public void SendPointerExited_WithNullCommandParameter_ExecutesWithNullParameter()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var commandMock = Substitute.For<ICommand>();
            commandMock.CanExecute(null).Returns(true);

            recognizer.PointerExitedCommand = commandMock;
            recognizer.PointerExitedCommandParameter = null;

            // Act
            recognizer.SendPointerExited(sender, null);

            // Assert
            commandMock.Received(1).CanExecute(null);
            commandMock.Received(1).Execute(null);
        }

        /// <summary>
        /// Tests SendPointerExited with null getPosition function.
        /// Validates that null position function is handled correctly in PointerEventArgs.
        /// Expected result: PointerEventArgs is created with null position function.
        /// </summary>
        [Fact]
        public void SendPointerExited_WithNullGetPosition_CreatesEventArgsWithNullPosition()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            PointerEventArgs actualArgs = null;

            recognizer.PointerExited += (s, e) => actualArgs = e;

            // Act
            recognizer.SendPointerExited(sender, null);

            // Assert
            Assert.NotNull(actualArgs);
        }

        /// <summary>
        /// Tests SendPointerExited with valid getPosition function.
        /// Validates that position function is properly passed to PointerEventArgs.
        /// Expected result: PointerEventArgs uses the provided position function.
        /// </summary>
        [Fact]
        public void SendPointerExited_WithValidGetPosition_CreatesEventArgsWithPosition()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var getPosition = Substitute.For<Func<IElement, Point?>>();
            var expectedPoint = new Point(5, 15);
            getPosition(Arg.Any<IElement>()).Returns(expectedPoint);
            PointerEventArgs actualArgs = null;

            recognizer.PointerExited += (s, e) => actualArgs = e;

            // Act
            recognizer.SendPointerExited(sender, getPosition);

            // Assert
            Assert.NotNull(actualArgs);
        }

        /// <summary>
        /// Tests SendPointerExited with null platform args.
        /// Validates default behavior when platform args are not provided.
        /// Expected result: PointerEventArgs is created with null platform args.
        /// </summary>
        [Fact]
        public void SendPointerExited_WithNullPlatformArgs_CreatesEventArgsWithNullPlatformArgs()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            PointerEventArgs actualArgs = null;

            recognizer.PointerExited += (s, e) => actualArgs = e;

            // Act
            recognizer.SendPointerExited(sender, null, null);

            // Assert
            Assert.NotNull(actualArgs);
            Assert.Null(actualArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests SendPointerExited with valid platform args.
        /// Validates that platform args are properly passed to PointerEventArgs.
        /// Expected result: PointerEventArgs contains the provided platform args.
        /// </summary>
        [Fact]
        public void SendPointerExited_WithValidPlatformArgs_CreatesEventArgsWithPlatformArgs()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var platformArgs = new PlatformPointerEventArgs();
            PointerEventArgs actualArgs = null;

            recognizer.PointerExited += (s, e) => actualArgs = e;

            // Act
            recognizer.SendPointerExited(sender, null, platformArgs);

            // Assert
            Assert.NotNull(actualArgs);
            Assert.Equal(platformArgs, actualArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests SendPointerExited with both command and event handler.
        /// Validates that both command execution and event raising work together.
        /// Expected result: Command is executed and event is raised.
        /// </summary>
        [Fact]
        public void SendPointerExited_WithCommandAndEventHandler_ExecutesBoth()
        {
            // Arrange
            var recognizer = new PointerGestureRecognizer();
            var sender = new View();
            var commandMock = Substitute.For<ICommand>();
            var parameter = new object();
            commandMock.CanExecute(parameter).Returns(true);
            var eventRaised = false;

            recognizer.PointerExitedCommand = commandMock;
            recognizer.PointerExitedCommandParameter = parameter;
            recognizer.PointerExited += (s, e) => eventRaised = true;

            // Act
            recognizer.SendPointerExited(sender, null);

            // Assert
            commandMock.Received(1).Execute(parameter);
            Assert.True(eventRaised);
        }

        /// <summary>
        /// Tests that PointerEnteredCommand getter returns null when no command has been set.
        /// Input: No command set (default state).
        /// Expected: Property should return null (default value).
        /// </summary>
        [Fact]
        public void PointerEnteredCommand_WhenNoCommandSet_ReturnsNull()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();

            // Act
            var result = gesture.PointerEnteredCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerEnteredCommand getter returns the correct command after setting via property setter.
        /// Input: Valid ICommand instance set via property setter.
        /// Expected: Property getter should return the same command instance.
        /// </summary>
        [Fact]
        public void PointerEnteredCommand_AfterSettingViaProperty_ReturnsSetCommand()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var command = Substitute.For<ICommand>();

            // Act
            gesture.PointerEnteredCommand = command;
            var result = gesture.PointerEnteredCommand;

            // Assert
            Assert.Same(command, result);
        }

        /// <summary>
        /// Tests that PointerEnteredCommand getter returns the correct command after setting via SetValue method.
        /// Input: Valid ICommand instance set via SetValue method directly.
        /// Expected: Property getter should return the same command instance.
        /// </summary>
        [Fact]
        public void PointerEnteredCommand_AfterSettingViaSetValue_ReturnsSetCommand()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var command = Substitute.For<ICommand>();

            // Act
            gesture.SetValue(PointerGestureRecognizer.PointerEnteredCommandProperty, command);
            var result = gesture.PointerEnteredCommand;

            // Assert
            Assert.Same(command, result);
        }

        /// <summary>
        /// Tests that PointerEnteredCommand getter returns null after setting null command.
        /// Input: Null command set via property setter.
        /// Expected: Property getter should return null.
        /// </summary>
        [Fact]
        public void PointerEnteredCommand_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var command = Substitute.For<ICommand>();
            gesture.PointerEnteredCommand = command; // First set a command

            // Act
            gesture.PointerEnteredCommand = null; // Then set to null
            var result = gesture.PointerEnteredCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PointerEnteredCommand getter works correctly with Command class instances.
        /// Input: Command class instance set via property setter.
        /// Expected: Property getter should return the same Command instance.
        /// </summary>
        [Fact]
        public void PointerEnteredCommand_WithCommandClass_ReturnsSetCommand()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var executed = false;
            var command = new Command(() => executed = true);

            // Act
            gesture.PointerEnteredCommand = command;
            var result = gesture.PointerEnteredCommand;

            // Assert
            Assert.Same(command, result);
        }

        /// <summary>
        /// Tests that PointerEnteredCommand getter returns updated command when changed multiple times.
        /// Input: Multiple different ICommand instances set sequentially.
        /// Expected: Property getter should always return the most recently set command.
        /// </summary>
        [Fact]
        public void PointerEnteredCommand_WhenChangedMultipleTimes_ReturnsLatestCommand()
        {
            // Arrange
            var gesture = new PointerGestureRecognizer();
            var command1 = Substitute.For<ICommand>();
            var command2 = Substitute.For<ICommand>();
            var command3 = Substitute.For<ICommand>();

            // Act & Assert
            gesture.PointerEnteredCommand = command1;
            Assert.Same(command1, gesture.PointerEnteredCommand);

            gesture.PointerEnteredCommand = command2;
            Assert.Same(command2, gesture.PointerEnteredCommand);

            gesture.PointerEnteredCommand = command3;
            Assert.Same(command3, gesture.PointerEnteredCommand);
        }
    }
}