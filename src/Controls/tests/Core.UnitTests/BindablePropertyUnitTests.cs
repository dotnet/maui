#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Converters;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Converters;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class BindablePropertyUnitTests : BaseTestFixture
    {
        [Fact]
        public void Create()
        {
            const BindingMode mode = BindingMode.OneWayToSource;
            const string dvalue = "default";
            BindableProperty.CoerceValueDelegate coerce = (bindable, value) => value;
            BindableProperty.ValidateValueDelegate validate = (b, v) => true;
            BindableProperty.BindingPropertyChangedDelegate changed = (b, ov, nv) => { };
            BindableProperty.BindingPropertyChangingDelegate changing = (b, ov, nv) => { };

            var prop = BindableProperty.Create(nameof(Button.Text), typeof(string), typeof(Button), dvalue, mode, validate, changed, changing, coerce);
            Assert.Equal("Text", prop.PropertyName);
            Assert.Equal(typeof(Button), prop.DeclaringType);
            Assert.Equal(typeof(string), prop.ReturnType);
            Assert.Equal(dvalue, prop.DefaultValue);
            Assert.Equal(mode, prop.DefaultBindingMode);
        }

        [Fact]
        public void CreateWithDefaultMode()
        {
            const BindingMode mode = BindingMode.Default;
            var prop = BindableProperty.Create(nameof(Button.Text), typeof(string), typeof(Button), null, defaultBindingMode: mode);
            Assert.Equal(BindingMode.OneWay, prop.DefaultBindingMode);
        }

        [Fact]
        public void CreateCasted()
        {
            var prop = BindableProperty.Create(nameof(Cell.IsEnabled), typeof(bool), typeof(Cell), true);

            Assert.Equal("IsEnabled", prop.PropertyName);
            Assert.Equal(typeof(Cell), prop.DeclaringType);
            Assert.Equal(typeof(bool), prop.ReturnType);
        }

        [Fact]
        public void CreateNonGeneric()
        {
            const BindingMode mode = BindingMode.OneWayToSource;
            const string dvalue = "default";
            BindableProperty.CoerceValueDelegate coerce = (bindable, value) => value;
            BindableProperty.ValidateValueDelegate validate = (b, v) => true;
            BindableProperty.BindingPropertyChangedDelegate changed = (b, ov, nv) => { };
            BindableProperty.BindingPropertyChangingDelegate changing = (b, ov, nv) => { };

            var prop = BindableProperty.Create("Text", typeof(string), typeof(Button), dvalue, mode, validate, changed, changing, coerce);
            Assert.Equal("Text", prop.PropertyName);
            Assert.Equal(typeof(Button), prop.DeclaringType);
            Assert.Equal(typeof(string), prop.ReturnType);
            Assert.Equal(dvalue, prop.DefaultValue);
            Assert.Equal(mode, prop.DefaultBindingMode);
        }

        class GenericView<T> : View
        {
            public string Text
            {
                get;
                set;
            }
        }

        [Fact]
        public void CreateForGeneric()
        {
            const BindingMode mode = BindingMode.OneWayToSource;
            const string dvalue = "default";
            BindableProperty.CoerceValueDelegate coerce = (bindable, value) => value;
            BindableProperty.ValidateValueDelegate validate = (b, v) => true;
            BindableProperty.BindingPropertyChangedDelegate changed = (b, ov, nv) => { };
            BindableProperty.BindingPropertyChangingDelegate changing = (b, ov, nv) => { };

            var prop = BindableProperty.Create("Text", typeof(string), typeof(GenericView<>), dvalue, mode, validate, changed, changing, coerce);
            Assert.Equal("Text", prop.PropertyName);
            Assert.Equal(typeof(GenericView<>), prop.DeclaringType);
            Assert.Equal(typeof(string), prop.ReturnType);
            Assert.Equal(dvalue, prop.DefaultValue);
            Assert.Equal(mode, prop.DefaultBindingMode);
        }

        [Fact]
        public void ChangingBeforeChanged()
        {
            bool changingfired = false;
            bool changedfired = false;
            BindableProperty.BindingPropertyChangedDelegate changed = (b, ov, nv) =>
            {
                Assert.True(changingfired);
                changedfired = true;
            };
            BindableProperty.BindingPropertyChangingDelegate changing = (b, ov, nv) =>
            {
                Assert.False(changedfired);
                changingfired = true;
            };

            var prop = BindableProperty.Create(nameof(Button.Text), typeof(string), typeof(Button), "Foo", propertyChanging: changing, propertyChanged: changed);

            Assert.False(changingfired);
            Assert.False(changedfired);

            (new View()).SetValue(prop, "Bar");

            Assert.True(changingfired);
            Assert.True(changedfired);
        }

        [Fact]
        public void NullableProperty()
        {
            var prop = BindableProperty.Create("foo", typeof(DateTime?), typeof(MockBindable), null);
            Assert.Equal(typeof(DateTime?), prop.ReturnType);

            var bindable = new MockBindable();
            Assert.Null(bindable.GetValue(prop));

            var now = DateTime.Now;
            bindable.SetValue(prop, now);
            Assert.Equal(now, bindable.GetValue(prop));

            bindable.SetValue(prop, null);
            Assert.Null(bindable.GetValue(prop));
        }

        [Fact]
        public void ValueTypePropertyDefaultValue()
        {
            // Create BindableProperty without explicit default value
            var prop = BindableProperty.Create("foo", typeof(int), typeof(MockBindable));
            Assert.Equal(typeof(int), prop.ReturnType);

            Assert.Equal(0, prop.DefaultValue);

            var bindable = new MockBindable();
            Assert.Equal(0, bindable.GetValue(prop));

            bindable.SetValue(prop, 1);
            Assert.Equal(1, bindable.GetValue(prop));
        }

        enum TestEnum
        {
            One, Two, Three
        }

        [Fact]
        public void EnumPropertyDefaultValue()
        {
            // Create BindableProperty without explicit default value
            var prop = BindableProperty.Create("foo", typeof(TestEnum), typeof(MockBindable));
            Assert.Equal(typeof(TestEnum), prop.ReturnType);

            Assert.Equal(default(TestEnum), prop.DefaultValue);

            var bindable = new MockBindable();
            Assert.Equal(default(TestEnum), bindable.GetValue(prop));

            bindable.SetValue(prop, TestEnum.Two);
            Assert.Equal(TestEnum.Two, bindable.GetValue(prop));
        }

        struct TestStruct
        {
            public int IntValue;
        }

        [Fact]
        public void StructPropertyDefaultValue()
        {
            // Create BindableProperty without explicit default value
            var prop = BindableProperty.Create("foo", typeof(TestStruct), typeof(MockBindable));
            Assert.Equal(typeof(TestStruct), prop.ReturnType);

            Assert.Equal(default(int), ((TestStruct)prop.DefaultValue).IntValue);

            var bindable = new MockBindable();
            Assert.Equal(default(int), ((TestStruct)bindable.GetValue(prop)).IntValue);

            var propStruct = new TestStruct { IntValue = 1 };

            bindable.SetValue(prop, propStruct);
            Assert.Equal(1, ((TestStruct)bindable.GetValue(prop)).IntValue);
        }

    }

    public partial class BindablePropertyTests
    {
        /// <summary>
        /// Tests that GetDefaultValue returns the DefaultValue property when DefaultValueCreator is null.
        /// This tests the path where no custom default value creator is provided.
        /// </summary>
        [Fact]
        public void GetDefaultValue_DefaultValueCreatorNull_ReturnsDefaultValue()
        {
            // Arrange
            const string expectedDefaultValue = "TestDefaultValue";
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(View), expectedDefaultValue);
            var bindableObject = new View();

            // Act
            var result = property.GetDefaultValue(bindableObject);

            // Assert
            Assert.Equal(expectedDefaultValue, result);
        }

        /// <summary>
        /// Tests that GetDefaultValue calls DefaultValueCreator and returns its result when DefaultValueCreator is not null.
        /// This tests the uncovered branch where DefaultValueCreator != null.
        /// </summary>
        [Fact]
        public void GetDefaultValue_DefaultValueCreatorNotNull_ReturnsCreatorResult()
        {
            // Arrange
            const string expectedCreatedValue = "CreatedValue";
            var receivedBindable = (BindableObject)null;
            BindableProperty.CreateDefaultValueDelegate creator = (bindable) =>
            {
                receivedBindable = bindable;
                return expectedCreatedValue;
            };

            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(View), null, defaultValueCreator: creator);
            var bindableObject = new View();

            // Act
            var result = property.GetDefaultValue(bindableObject);

            // Assert
            Assert.Equal(expectedCreatedValue, result);
            Assert.Same(bindableObject, receivedBindable);
        }

        /// <summary>
        /// Tests that GetDefaultValue handles null return value from DefaultValueCreator.
        /// This ensures the method properly handles when the creator returns null.
        /// </summary>
        [Fact]
        public void GetDefaultValue_DefaultValueCreatorReturnsNull_ReturnsNull()
        {
            // Arrange
            BindableProperty.CreateDefaultValueDelegate creator = (bindable) => null;
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(View), "DefaultValue", defaultValueCreator: creator);
            var bindableObject = new View();

            // Act
            var result = property.GetDefaultValue(bindableObject);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetDefaultValue works with different types of BindableObject instances.
        /// This ensures the method works with various bindable object types.
        /// </summary>
        [Theory]
        [InlineData(typeof(View))]
        [InlineData(typeof(Button))]
        public void GetDefaultValue_DifferentBindableObjectTypes_CallsCreator(Type bindableType)
        {
            // Arrange
            var callCount = 0;
            BindableProperty.CreateDefaultValueDelegate creator = (bindable) =>
            {
                callCount++;
                return $"Value for {bindable.GetType().Name}";
            };

            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(View), null, defaultValueCreator: creator);
            var bindableObject = (BindableObject)Activator.CreateInstance(bindableType);

            // Act
            var result = property.GetDefaultValue(bindableObject);

            // Assert
            Assert.Equal(1, callCount);
            Assert.Equal($"Value for {bindableType.Name}", result);
        }

        /// <summary>
        /// Tests that GetDefaultValue handles complex object return values from DefaultValueCreator.
        /// This ensures the method properly handles when the creator returns complex objects.
        /// </summary>
        [Fact]
        public void GetDefaultValue_DefaultValueCreatorReturnsComplexObject_ReturnsObject()
        {
            // Arrange
            var expectedObject = new { Name = "Test", Value = 42 };
            BindableProperty.CreateDefaultValueDelegate creator = (bindable) => expectedObject;
            var property = BindableProperty.Create("TestProperty", typeof(object), typeof(View), null, defaultValueCreator: creator);
            var bindableObject = new View();

            // Act
            var result = property.GetDefaultValue(bindableObject);

            // Assert
            Assert.Same(expectedObject, result);
        }

        /// <summary>
        /// Tests that GetDefaultValue with null DefaultValueCreator returns the exact DefaultValue object reference.
        /// This ensures reference equality when no creator is used.
        /// </summary>
        [Fact]
        public void GetDefaultValue_NoCreator_ReturnsExactDefaultValueReference()
        {
            // Arrange
            var defaultValueObject = new object();
            var property = BindableProperty.Create("TestProperty", typeof(object), typeof(View), defaultValueObject);
            var bindableObject = new View();

            // Act
            var result = property.GetDefaultValue(bindableObject);

            // Assert
            Assert.Same(defaultValueObject, result);
        }
    }
}