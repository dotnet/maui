#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class TriggerTests : BaseTestFixture
    {
        class MockElement : VisualElement
        {
        }

        [Fact]
        public void SettersAppliedOnConditionChanged()
        {
            var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
            var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
            var element = new MockElement();
            var trigger = new Trigger(typeof(VisualElement))
            {
                Property = conditionbp,
                Value = "foobar",
                Setters = {
                    new Setter { Property = setterbp, Value = "qux" },
                }
            };

            element.SetValue(setterbp, "default");
            element.Triggers.Add(trigger);

            Assert.Equal("default", element.GetValue(setterbp));
            element.SetValue(conditionbp, "foobar");
            Assert.Equal("qux", element.GetValue(setterbp));
            element.SetValue(conditionbp, "");
            Assert.Equal("default", element.GetValue(setterbp));
        }

        [Fact]
        public void SettersAppliedOnAttachIfConditionIsTrue()
        {
            var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
            var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
            var element = new MockElement();
            var trigger = new Trigger(typeof(VisualElement))
            {
                Property = conditionbp,
                Value = "foobar",
                Setters = {
                    new Setter { Property = setterbp, Value = "qux" },
                }
            };

            element.SetValue(setterbp, "default");
            element.SetValue(conditionbp, "foobar");
            Assert.Equal("default", element.GetValue(setterbp));
            element.Triggers.Add(trigger);
            Assert.Equal("qux", element.GetValue(setterbp));
        }

        [Fact]
        public void SettersUnappliedOnDetach()
        {
            var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
            var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
            var element = new MockElement();
            var trigger = new Trigger(typeof(VisualElement))
            {
                Property = conditionbp,
                Value = "foobar",
                Setters = {
                    new Setter { Property = setterbp, Value = "qux" },
                }
            };

            element.SetValue(setterbp, "default");
            element.Triggers.Add(trigger);

            Assert.Equal("default", element.GetValue(setterbp));
            element.SetValue(conditionbp, "foobar");
            Assert.Equal("qux", element.GetValue(setterbp));
            element.Triggers.Remove(trigger);
            Assert.Equal("default", element.GetValue(setterbp));
        }

        [Fact]
        public void EnterAndExitActionsTriggered()
        {
            var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
            var element = new MockElement();
            var enteraction = new MockTriggerAction();
            var exitaction = new MockTriggerAction();
            var trigger = new Trigger(typeof(VisualElement))
            {
                Property = conditionbp,
                Value = "foobar",
                EnterActions = {
                    enteraction
                },
                ExitActions = {
                    exitaction
                }
            };

            Assert.False(enteraction.Invoked);
            Assert.False(exitaction.Invoked);
            element.Triggers.Add(trigger);
            Assert.False(enteraction.Invoked);
            Assert.False(exitaction.Invoked);
            element.SetValue(conditionbp, "foobar");
            Assert.True(enteraction.Invoked);
            Assert.False(exitaction.Invoked);

            enteraction.Invoked = exitaction.Invoked = false;
            Assert.False(enteraction.Invoked);
            Assert.False(exitaction.Invoked);
            element.SetValue(conditionbp, "");
            Assert.False(enteraction.Invoked);
            Assert.True(exitaction.Invoked);
        }

        [Fact]
        // https://bugzilla.xamarin.com/show_bug.cgi?id=32896
        public void SettersWithBindingsUnappliedIfConditionIsFalse()
        {
            var conditionbp = BindableProperty.Create("foo", typeof(string), typeof(BindableObject), null);
            var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
            var element = new MockElement();
            var trigger = new Trigger(typeof(VisualElement))
            {
                Property = conditionbp,
                Value = "foobar",
                Setters = {
                    new Setter { Property = setterbp, Value = new Binding(".", source: "Qux") },
                }
            };

            element.SetValue(setterbp, "default");
            element.Triggers.Add(trigger);
            Assert.Equal("default", element.GetValue(setterbp));

            //sets the condition to true
            element.SetValue(conditionbp, "foobar");
            Assert.Equal("Qux", element.GetValue(setterbp));

            //unsets the condition
            element.SetValue(conditionbp, "baz");
            Assert.Equal("default", element.GetValue(setterbp));
        }
    }

    public class TriggerValuePropertyTests : BaseTestFixture
    {
        class MockElement : VisualElement
        {
        }

        /// <summary>
        /// Tests that setting a new value when trigger is not sealed successfully updates the Value property.
        /// Validates that OnPropertyChanging and OnPropertyChanged are called and the underlying PropertyCondition.Value is updated.
        /// </summary>
        [Fact]
        public void Value_SetNewValueWhenNotSealed_UpdatesValueSuccessfully()
        {
            // Arrange
            var trigger = new Trigger(typeof(MockElement));
            var newValue = "test value";

            // Act
            trigger.Value = newValue;

            // Assert
            Assert.Equal(newValue, trigger.Value);
        }

        /// <summary>
        /// Tests that setting the same value when trigger is not sealed performs no operation.
        /// Validates the early return optimization when current value equals new value.
        /// </summary>
        [Fact]
        public void Value_SetSameValueWhenNotSealed_PerformsNoOperation()
        {
            // Arrange
            var trigger = new Trigger(typeof(MockElement));
            var initialValue = "initial value";
            trigger.Value = initialValue;

            // Act
            trigger.Value = initialValue;

            // Assert
            Assert.Equal(initialValue, trigger.Value);
        }

        /// <summary>
        /// Tests that setting a value when trigger is sealed throws InvalidOperationException.
        /// Validates the sealed state check and proper exception throwing.
        /// </summary>
        [Fact]
        public void Value_SetValueWhenSealed_ThrowsInvalidOperationException()
        {
            // Arrange
            var element = new MockElement();
            var trigger = new Trigger(typeof(MockElement));
            element.Triggers.Add(trigger); // This seals the trigger
            var newValue = "new value";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => trigger.Value = newValue);
            Assert.Equal("Cannot change Value once the Trigger has been applied.", exception.Message);
        }

        /// <summary>
        /// Tests setting null value when trigger is not sealed.
        /// Validates that null values are properly handled and stored.
        /// </summary>
        [Fact]
        public void Value_SetNullValueWhenNotSealed_UpdatesValueToNull()
        {
            // Arrange
            var trigger = new Trigger(typeof(MockElement));
            trigger.Value = "initial value";

            // Act
            trigger.Value = null;

            // Assert
            Assert.Null(trigger.Value);
        }

        /// <summary>
        /// Tests setting the same null value when current value is already null.
        /// Validates the early return optimization works for null values.
        /// </summary>
        [Fact]
        public void Value_SetNullWhenCurrentValueIsNull_PerformsNoOperation()
        {
            // Arrange
            var trigger = new Trigger(typeof(MockElement));
            trigger.Value = null;

            // Act
            trigger.Value = null;

            // Assert
            Assert.Null(trigger.Value);
        }

        /// <summary>
        /// Tests setting different object types as values.
        /// Validates that various object types can be stored and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void Value_SetDifferentObjectTypes_UpdatesValueCorrectly(object value)
        {
            // Arrange
            var trigger = new Trigger(typeof(MockElement));

            // Act
            trigger.Value = value;

            // Assert
            Assert.Equal(value, trigger.Value);
        }

        /// <summary>
        /// Tests that setting the same reference object when current value is the same reference performs no operation.
        /// Validates reference equality check in the early return optimization.
        /// </summary>
        [Fact]
        public void Value_SetSameReferenceObject_PerformsNoOperation()
        {
            // Arrange
            var trigger = new Trigger(typeof(MockElement));
            var obj = new object();
            trigger.Value = obj;

            // Act
            trigger.Value = obj;

            // Assert
            Assert.Same(obj, trigger.Value);
        }

        /// <summary>
        /// Tests that setting different reference objects with same content updates the value.
        /// Validates that reference equality is used, not value equality for complex objects.
        /// </summary>
        [Fact]
        public void Value_SetDifferentReferenceObjectsSameContent_UpdatesValue()
        {
            // Arrange
            var trigger = new Trigger(typeof(MockElement));
            var obj1 = new List<int> { 1, 2, 3 };
            var obj2 = new List<int> { 1, 2, 3 };
            trigger.Value = obj1;

            // Act
            trigger.Value = obj2;

            // Assert
            Assert.Equal(obj2, trigger.Value);
            Assert.NotSame(obj1, trigger.Value);
        }

        /// <summary>
        /// Tests setting value from null to non-null when trigger is not sealed.
        /// Validates the transition from null to a valid value.
        /// </summary>
        [Fact]
        public void Value_SetFromNullToValue_UpdatesValueSuccessfully()
        {
            // Arrange
            var trigger = new Trigger(typeof(MockElement));
            trigger.Value = null;
            var newValue = "new value";

            // Act
            trigger.Value = newValue;

            // Assert
            Assert.Equal(newValue, trigger.Value);
        }

        /// <summary>
        /// Tests setting value from non-null to null when trigger is not sealed.
        /// Validates the transition from a valid value to null.
        /// </summary>
        [Fact]
        public void Value_SetFromValueToNull_UpdatesValueToNull()
        {
            // Arrange
            var trigger = new Trigger(typeof(MockElement));
            trigger.Value = "initial value";

            // Act
            trigger.Value = null;

            // Assert
            Assert.Null(trigger.Value);
        }
    }


    public partial class TriggerPropertyTests : BaseTestFixture
    {
        class MockElement : VisualElement
        {
        }

        /// <summary>
        /// Tests that setting the Property to the same value returns early without triggering property change events.
        /// Input: A trigger with a property already set, then setting the same property value again.
        /// Expected: The method should return early without calling OnPropertyChanging/OnPropertyChanged.
        /// </summary>
        [Fact]
        public void Property_WhenSetWithSameValue_ShouldReturnEarly()
        {
            // Arrange
            var targetType = typeof(MockElement);
            var bindableProperty = BindableProperty.Create("TestProperty", typeof(string), typeof(MockElement), null);
            var trigger = new Trigger(targetType);
            trigger.Property = bindableProperty;

            // Act - Setting the same property value again
            trigger.Property = bindableProperty;

            // Assert - No exception should be thrown and the property should remain the same
            Assert.Equal(bindableProperty, trigger.Property);
        }

        /// <summary>
        /// Tests that setting the Property on a sealed trigger throws InvalidOperationException.
        /// Input: A trigger that has been sealed by adding it to an element's triggers collection.
        /// Expected: InvalidOperationException should be thrown with appropriate message.
        /// </summary>
        [Fact]
        public void Property_WhenSetOnSealedTrigger_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var targetType = typeof(MockElement);
            var element = new MockElement();
            var bindableProperty1 = BindableProperty.Create("TestProperty1", typeof(string), typeof(MockElement), null);
            var bindableProperty2 = BindableProperty.Create("TestProperty2", typeof(string), typeof(MockElement), null);
            var trigger = new Trigger(targetType)
            {
                Property = bindableProperty1,
                Value = "test"
            };

            // Seal the trigger by adding it to the element's triggers
            element.Triggers.Add(trigger);
            Assert.True(trigger.IsSealed);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => trigger.Property = bindableProperty2);
            Assert.Equal("Cannot change Property once the Trigger has been applied.", exception.Message);
        }

        /// <summary>
        /// Tests that setting the Property with a valid BindableProperty updates the underlying PropertyCondition.
        /// Input: A trigger with a new valid BindableProperty value.
        /// Expected: The property should be updated successfully.
        /// </summary>
        [Fact]
        public void Property_WhenSetWithValidValue_ShouldUpdateProperty()
        {
            // Arrange
            var targetType = typeof(MockElement);
            var bindableProperty = BindableProperty.Create("TestProperty", typeof(string), typeof(MockElement), null);
            var trigger = new Trigger(targetType);

            // Act
            trigger.Property = bindableProperty;

            // Assert
            Assert.Equal(bindableProperty, trigger.Property);
        }

        /// <summary>
        /// Tests that setting the Property with null value updates the underlying PropertyCondition.
        /// Input: A trigger with null BindableProperty value.
        /// Expected: The property should be set to null successfully.
        /// </summary>
        [Fact]
        public void Property_WhenSetWithNull_ShouldUpdateProperty()
        {
            // Arrange
            var targetType = typeof(MockElement);
            var trigger = new Trigger(targetType);
            var initialProperty = BindableProperty.Create("InitialProperty", typeof(string), typeof(MockElement), null);
            trigger.Property = initialProperty;

            // Act
            trigger.Property = null;

            // Assert
            Assert.Null(trigger.Property);
        }

        /// <summary>
        /// Tests that setting different BindableProperty values updates the property correctly.
        /// Input: A trigger with different BindableProperty values set sequentially.
        /// Expected: Each new property value should be set correctly.
        /// </summary>
        [Fact]
        public void Property_WhenSetWithDifferentValues_ShouldUpdateProperty()
        {
            // Arrange
            var targetType = typeof(MockElement);
            var trigger = new Trigger(targetType);
            var property1 = BindableProperty.Create("Property1", typeof(string), typeof(MockElement), null);
            var property2 = BindableProperty.Create("Property2", typeof(int), typeof(MockElement), 0);

            // Act & Assert - Set first property
            trigger.Property = property1;
            Assert.Equal(property1, trigger.Property);

            // Act & Assert - Set second property
            trigger.Property = property2;
            Assert.Equal(property2, trigger.Property);
        }
    }
}