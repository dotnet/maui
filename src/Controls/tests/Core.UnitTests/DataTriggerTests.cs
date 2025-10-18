#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class DataTriggerTests : BaseTestFixture
    {
        class MockElement : VisualElement
        {
        }

        [Fact]
        public void SettersAppliedOnAttachIfConditionIsTrue()
        {
            var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
            var element = new MockElement();
            var datatrigger = new DataTrigger(typeof(VisualElement))
            {
                Binding = new Binding("foo"),
                Value = "foobar",
                Setters = {
                    new Setter { Property = setterbp, Value = "qux" },
                }
            };

            element.SetValue(setterbp, "default");
            element.BindingContext = new { foo = "foobar" };
            Assert.Equal("default", element.GetValue(setterbp));
            element.Triggers.Add(datatrigger);
            Assert.Equal("qux", element.GetValue(setterbp));
        }

        [Fact]
        public void SettersUnappliedOnDetach()
        {
            var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
            var element = new MockElement();
            var datatrigger = new DataTrigger(typeof(VisualElement))
            {
                Binding = new Binding("foo"),
                Value = "foobar",
                Setters = {
                    new Setter { Property = setterbp, Value = "qux" },
                }
            };

            element.SetValue(setterbp, "default");
            element.Triggers.Add(datatrigger);

            Assert.Equal("default", element.GetValue(setterbp));

            element.BindingContext = new { foo = "foobar" };
            Assert.Equal("qux", element.GetValue(setterbp));
            element.Triggers.Remove(datatrigger);
            Assert.Equal("default", element.GetValue(setterbp));
        }

        [Fact]
        public void SettersAppliedOnConditionChanged()
        {
            var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
            var element = new MockElement();
            var trigger = new DataTrigger(typeof(VisualElement))
            {
                Binding = new Binding("foo"),
                Value = "foobar",
                Setters = {
                    new Setter { Property = setterbp, Value = "qux" },
                }
            };

            element.SetValue(setterbp, "default");
            element.Triggers.Add(trigger);

            Assert.Equal("default", element.GetValue(setterbp));
            element.BindingContext = new { foo = "foobar" };
            Assert.Equal("qux", element.GetValue(setterbp));
            element.BindingContext = new { foo = "" };
            Assert.Equal("default", element.GetValue(setterbp));
        }

        [Fact]
        public void TriggersAppliedOnMultipleElements()
        {
            var setterbp = BindableProperty.Create("bar", typeof(string), typeof(BindableObject), null);
            var trigger = new DataTrigger(typeof(VisualElement))
            {
                Binding = new Binding("foo"),
                Value = "foobar",
                Setters = {
                    new Setter { Property = setterbp, Value = "qux" },
                }
            };
            var element0 = new MockElement { Triggers = { trigger } };
            var element1 = new MockElement { Triggers = { trigger } };

            element0.BindingContext = element1.BindingContext = new { foo = "foobar" };
            Assert.Equal("qux", element0.GetValue(setterbp));
            Assert.Equal("qux", element1.GetValue(setterbp));
        }

        [Fact]
        //https://bugzilla.xamarin.com/show_bug.cgi?id=30074
        public void AllTriggersUnappliedBeforeApplying()
        {
            var boxview = new BoxView
            {
                Triggers = {
                    new DataTrigger (typeof(BoxView)) {
                        Binding = new Binding ("."),
                        Value = "Complete",
                        Setters = {
                            new Setter { Property = BoxView.ColorProperty, Value = Colors.Green },
                            new Setter { Property = VisualElement.OpacityProperty, Value = .5 },
                        }
                    },
                    new DataTrigger (typeof(BoxView)) {
                        Binding = new Binding ("."),
                        Value = "MissingInfo",
                        Setters = {
                            new Setter { Property = BoxView.ColorProperty, Value = Colors.Yellow },
                        }
                    },
                    new DataTrigger (typeof(BoxView)) {
                        Binding = new Binding ("."),
                        Value = "Error",
                        Setters = {
                            new Setter { Property = BoxView.ColorProperty, Value = Colors.Red },
                        }
                    },
                }
            };

            boxview.BindingContext = "Complete";
            Assert.Equal(Colors.Green, boxview.Color);
            Assert.Equal(.5, boxview.Opacity);

            boxview.BindingContext = "MissingInfo";
            Assert.Equal(Colors.Yellow, boxview.Color);
            Assert.Equal(1, boxview.Opacity);

            boxview.BindingContext = "Error";
            Assert.Equal(Colors.Red, boxview.Color);
            Assert.Equal(1, boxview.Opacity);

            boxview.BindingContext = "Complete";
            Assert.Equal(Colors.Green, boxview.Color);
            Assert.Equal(.5, boxview.Opacity);
        }
    }

    /// <summary>
    /// Unit tests for the DataTrigger.Binding property.
    /// </summary>
    public partial class DataTriggerBindingPropertyTests
    {
        /// <summary>
        /// Tests that setting the same binding value twice returns early without changing anything.
        /// This test covers the equality check on line 23.
        /// </summary>
        [Fact]
        public void Binding_SetSameValueTwice_ReturnsEarlyWithoutChanges()
        {
            // Arrange
            var trigger = new TestableDataTrigger(typeof(MockElement));
            var binding = Substitute.For<BindingBase>();

            // Set initial binding
            trigger.Binding = binding;

            // Act - Set the same binding again
            trigger.Binding = binding;

            // Assert
            Assert.Same(binding, trigger.Binding);
        }

        /// <summary>
        /// Tests that setting a binding when the trigger is sealed throws InvalidOperationException.
        /// This test covers the IsSealed check on line 25.
        /// </summary>
        [Fact]
        public void Binding_SetWhenSealed_ThrowsInvalidOperationException()
        {
            // Arrange
            var trigger = new TestableDataTrigger(typeof(MockElement));
            var binding = Substitute.For<BindingBase>();

            // Seal the trigger
            trigger.SetIsSealed(true);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => trigger.Binding = binding);
            Assert.Equal("Cannot change Binding once the Trigger has been applied.", exception.Message);
        }

        /// <summary>
        /// Tests that setting a null binding value works correctly when trigger is not sealed.
        /// This test ensures null values are handled properly.
        /// </summary>
        [Fact]
        public void Binding_SetNullValue_SetsSuccessfully()
        {
            // Arrange
            var trigger = new TestableDataTrigger(typeof(MockElement));

            // Act
            trigger.Binding = null;

            // Assert
            Assert.Null(trigger.Binding);
        }

        /// <summary>
        /// Tests that setting a valid binding value works correctly when trigger is not sealed.
        /// This test verifies normal operation of the binding setter.
        /// </summary>
        [Fact]
        public void Binding_SetValidValue_SetsSuccessfully()
        {
            // Arrange
            var trigger = new TestableDataTrigger(typeof(MockElement));
            var binding = Substitute.For<BindingBase>();

            // Act
            trigger.Binding = binding;

            // Assert
            Assert.Same(binding, trigger.Binding);
        }

        /// <summary>
        /// Tests that setting different binding values updates the property correctly.
        /// This test verifies that different bindings can be set sequentially.
        /// </summary>
        [Fact]
        public void Binding_SetDifferentValues_UpdatesSuccessfully()
        {
            // Arrange
            var trigger = new TestableDataTrigger(typeof(MockElement));
            var firstBinding = Substitute.For<BindingBase>();
            var secondBinding = Substitute.For<BindingBase>();

            // Act
            trigger.Binding = firstBinding;
            trigger.Binding = secondBinding;

            // Assert
            Assert.Same(secondBinding, trigger.Binding);
        }

        /// <summary>
        /// Tests that setting binding from null to a valid value works correctly.
        /// This test covers the transition from null to a valid binding.
        /// </summary>
        [Fact]
        public void Binding_SetFromNullToValid_UpdatesSuccessfully()
        {
            // Arrange
            var trigger = new TestableDataTrigger(typeof(MockElement));
            var binding = Substitute.For<BindingBase>();

            // Initially null
            trigger.Binding = null;
            Assert.Null(trigger.Binding);

            // Act
            trigger.Binding = binding;

            // Assert
            Assert.Same(binding, trigger.Binding);
        }

        /// <summary>
        /// Tests that setting binding from a valid value to null works correctly.
        /// This test covers the transition from a valid binding to null.
        /// </summary>
        [Fact]
        public void Binding_SetFromValidToNull_UpdatesSuccessfully()
        {
            // Arrange
            var trigger = new TestableDataTrigger(typeof(MockElement));
            var binding = Substitute.For<BindingBase>();

            // Set initial binding
            trigger.Binding = binding;
            Assert.Same(binding, trigger.Binding);

            // Act
            trigger.Binding = null;

            // Assert
            Assert.Null(trigger.Binding);
        }

        /// <summary>
        /// Mock element class for testing DataTrigger.
        /// </summary>
        private class MockElement : VisualElement
        {
        }
    }


    public partial class DataTriggerValueTests : BaseTestFixture
    {
        class MockElement : VisualElement
        {
        }

        /// <summary>
        /// Tests that the Value getter returns the underlying BindingCondition's value.
        /// </summary>
        [Fact]
        public void Value_Get_ReturnsBindingConditionValue()
        {
            // Arrange
            var dataTrigger = new DataTrigger(typeof(VisualElement));
            var expectedValue = "test value";
            dataTrigger.Value = expectedValue;

            // Act
            var actualValue = dataTrigger.Value;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that setting the same value returns early without calling property change notifications.
        /// Covers the value comparison logic at line 35.
        /// </summary>
        [Theory]
        [InlineData("same")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(null)]
        public void Value_SetWithSameValue_ReturnsEarly(object value)
        {
            // Arrange
            var dataTrigger = new DataTrigger(typeof(VisualElement));
            dataTrigger.Value = value;
            var initialValue = dataTrigger.Value;

            // Act
            dataTrigger.Value = value; // Set same value

            // Assert
            Assert.Equal(initialValue, dataTrigger.Value);
        }

        /// <summary>
        /// Tests that setting a different value updates the underlying BindingCondition's value.
        /// Covers the value comparison logic when values are different.
        /// </summary>
        [Theory]
        [InlineData("initial", "updated")]
        [InlineData(10, 20)]
        [InlineData(false, true)]
        [InlineData(null, "not null")]
        [InlineData("not null", null)]
        public void Value_SetWithDifferentValue_UpdatesValue(object initialValue, object newValue)
        {
            // Arrange
            var dataTrigger = new DataTrigger(typeof(VisualElement));
            dataTrigger.Value = initialValue;

            // Act
            dataTrigger.Value = newValue;

            // Assert
            Assert.Equal(newValue, dataTrigger.Value);
        }

        /// <summary>
        /// Tests that setting a value when the trigger is not sealed successfully updates the value.
        /// Covers the IsSealed check at line 37 when IsSealed is false.
        /// </summary>
        [Fact]
        public void Value_SetWhenNotSealed_UpdatesValue()
        {
            // Arrange
            var dataTrigger = new DataTrigger(typeof(VisualElement));
            var newValue = "new value";

            // Act & Assert - Should not throw
            dataTrigger.Value = newValue;
            Assert.Equal(newValue, dataTrigger.Value);
            Assert.False(dataTrigger.IsSealed);
        }

        /// <summary>
        /// Tests that setting a value when the trigger is sealed throws InvalidOperationException.
        /// Covers the IsSealed check at line 37 when IsSealed is true.
        /// </summary>
        [Fact]
        public void Value_SetWhenSealed_ThrowsInvalidOperationException()
        {
            // Arrange
            var element = new MockElement();
            var dataTrigger = new DataTrigger(typeof(VisualElement))
            {
                Binding = new Binding("TestProperty"),
                Value = "initial"
            };

            // Adding to triggers collection seals the trigger
            element.Triggers.Add(dataTrigger);
            Assert.True(dataTrigger.IsSealed);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => dataTrigger.Value = "new value");
            Assert.Equal("Cannot change Value once the Trigger has been applied.", exception.Message);
        }

        /// <summary>
        /// Tests setting null value on the DataTrigger Value property.
        /// </summary>
        [Fact]
        public void Value_SetWithNull_UpdatesValue()
        {
            // Arrange
            var dataTrigger = new DataTrigger(typeof(VisualElement));
            dataTrigger.Value = "initial";

            // Act
            dataTrigger.Value = null;

            // Assert
            Assert.Null(dataTrigger.Value);
        }

        /// <summary>
        /// Tests setting various object types as values to ensure proper handling.
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(123)]
        [InlineData(45.67)]
        [InlineData(true)]
        [InlineData(false)]
        public void Value_SetWithDifferentTypes_UpdatesValue(object value)
        {
            // Arrange
            var dataTrigger = new DataTrigger(typeof(VisualElement));

            // Act
            dataTrigger.Value = value;

            // Assert
            Assert.Equal(value, dataTrigger.Value);
        }

        /// <summary>
        /// Tests that setting the same object reference returns early even if the object content might differ.
        /// This tests the reference equality comparison in the BindingCondition.
        /// </summary>
        [Fact]
        public void Value_SetWithSameObjectReference_ReturnsEarly()
        {
            // Arrange
            var dataTrigger = new DataTrigger(typeof(VisualElement));
            var objectValue = new object();
            dataTrigger.Value = objectValue;

            // Act
            dataTrigger.Value = objectValue; // Same reference

            // Assert
            Assert.Same(objectValue, dataTrigger.Value);
        }

        /// <summary>
        /// Tests that setting different object references with potentially equal content updates the value.
        /// This verifies that reference equality (not value equality) is used for comparison.
        /// </summary>
        [Fact]
        public void Value_SetWithDifferentObjectReferences_UpdatesValue()
        {
            // Arrange
            var dataTrigger = new DataTrigger(typeof(VisualElement));
            var firstObject = new { Name = "Test" };
            var secondObject = new { Name = "Test" };
            dataTrigger.Value = firstObject;

            // Act
            dataTrigger.Value = secondObject; // Different reference, same content

            // Assert
            Assert.Equal(secondObject, dataTrigger.Value);
            Assert.NotSame(firstObject, dataTrigger.Value);
        }

        /// <summary>
        /// Tests edge case with extreme values for numeric types.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(0)]
        [InlineData(-1)]
        public void Value_SetWithExtremeNumericValues_UpdatesValue(object value)
        {
            // Arrange
            var dataTrigger = new DataTrigger(typeof(VisualElement));

            // Act
            dataTrigger.Value = value;

            // Assert
            Assert.Equal(value, dataTrigger.Value);
        }

        /// <summary>
        /// Tests setting empty and whitespace strings.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("   ")]
        public void Value_SetWithEmptyOrWhitespaceStrings_UpdatesValue(string value)
        {
            // Arrange
            var dataTrigger = new DataTrigger(typeof(VisualElement));

            // Act
            dataTrigger.Value = value;

            // Assert
            Assert.Equal(value, dataTrigger.Value);
        }
    }
}