#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Layouts.Flex;
using NSubstitute;
using Xunit;

using FlexLayout = Microsoft.Maui.Controls.Compatibility.FlexLayout;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class FlexLayoutMarginTests : BaseTestFixture
    {
        [Fact]
        public void TestMarginLeft()
        {
            var view0 = MockPlatformSizeService.Sub<View>(width: 10, margin: new(10, 0, 0, 0));
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = {
                    view0,
                },
                Direction = FlexDirection.Row,
            };

            layout.Layout(new Rect(0, 0, 100, 100));
            Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
            Assert.Equal(view0.Bounds, new Rect(10, 0, 10, 100));
        }

        [Fact]
        public void TestMarginTop()
        {
            var view0 = MockPlatformSizeService.Sub<View>(height: 10, margin: new(0, 10, 0, 0));
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = {
                    view0,
                },

                Direction = FlexDirection.Column,
            };

            layout.Layout(new Rect(0, 0, 100, 100));
            Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
            Assert.Equal(view0.Bounds, new Rect(0, 10, 100, 10));
        }

        [Fact]
        public void TestMarginRight()
        {
            var view0 = MockPlatformSizeService.Sub<View>(width: 10, margin: new(0, 0, 10, 0));
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = {
                    view0,
                },

                Direction = FlexDirection.Row,
                JustifyContent = FlexJustify.End,
            };

            layout.Layout(new Rect(0, 0, 100, 100));
            Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
            Assert.Equal(view0.Bounds, new Rect(80, 0, 10, 100));
        }

        [Fact]
        public void TestMarginBottom()
        {
            var view0 = MockPlatformSizeService.Sub<View>(height: 10, margin: new(0, 0, 0, 10));
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = {
                    view0,
                },

                Direction = FlexDirection.Column,
                JustifyContent = FlexJustify.End,
            };

            layout.Layout(new Rect(0, 0, 100, 100));
            Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
            Assert.Equal(view0.Bounds, new Rect(0, 80, 100, 10));
        }

        [Fact]
        public void TestMarginAndFlexRow()
        {
            var view0 = MockPlatformSizeService.Sub<View>(margin: new(10, 0, 10, 0));
            FlexLayout.SetGrow(view0, 1);
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = {
                    view0,
                },

                Direction = FlexDirection.Row,
            };
            layout.Layout(new Rect(0, 0, 100, 100));
            Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
            Assert.Equal(view0.Bounds, new Rect(10, 0, 80, 100));
        }

        [Fact]
        public void TestMarginAndFlexColumn()
        {
            var view0 = MockPlatformSizeService.Sub<View>(margin: new(0, 10, 0, 10));
            FlexLayout.SetGrow(view0, 1);
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = {
                    view0,
                },

                Direction = FlexDirection.Column,
            };
            layout.Layout(new Rect(0, 0, 100, 100));
            Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
            Assert.Equal(view0.Bounds, new Rect(0, 10, 100, 80));
        }

        [Fact]
        public void TestMarginAndStretchRow()
        {
            var view0 = MockPlatformSizeService.Sub<View>(margin: new(0, 10, 0, 10));
            FlexLayout.SetGrow(view0, 1);
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = {
                    view0,
                },

                Direction = FlexDirection.Row,
            };

            layout.Layout(new Rect(0, 0, 100, 100));
            Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
            Assert.Equal(view0.Bounds, new Rect(0, 10, 100, 80));
        }

        [Fact]
        public void TestMarginAndStretchColumn()
        {
            var view0 = MockPlatformSizeService.Sub<View>(margin: new(10, 0, 10, 0));
            FlexLayout.SetGrow(view0, 1);
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = {
                    view0,
                },
                Direction = FlexDirection.Column,
            };

            layout.Layout(new Rect(0, 0, 100, 100));
            Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
            Assert.Equal(view0.Bounds, new Rect(10, 0, 80, 100));
        }

        [Fact]
        public void TestMarginWithSiblingRow()
        {
            static SizeRequest GetSize(VisualElement _, double w, double h) => new(new(0, 0));

            var view0 = MockPlatformSizeService.Sub<View>(GetSize, margin: new(0, 0, 10, 0));
            FlexLayout.SetGrow(view0, 1);
            var view1 = MockPlatformSizeService.Sub<View>(GetSize);
            FlexLayout.SetGrow(view1, 1);
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = {
                    view0,
                    view1,
                },

                Direction = FlexDirection.Row,
            };

            layout.Layout(new Rect(0, 0, 100, 100));
            Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
            Assert.Equal(view0.Bounds, new Rect(0, 0, 45, 100));
            Assert.Equal(view1.Bounds, new Rect(55, 0, 45, 100));
        }

        [Fact]
        public void TestMarginWithSiblingColumn()
        {
            var view0 = MockPlatformSizeService.Sub<View>(margin: new(0, 0, 0, 10));
            FlexLayout.SetGrow(view0, 1);
            var view1 = MockPlatformSizeService.Sub<View>();
            FlexLayout.SetGrow(view1, 1);

            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = {
                    view0,
                    view1,
                },

                Direction = FlexDirection.Column,
            };

            layout.Layout(new Rect(0, 0, 100, 100));
            Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
            Assert.Equal(view0.Bounds, new Rect(0, 0, 100, 45));
            Assert.Equal(view1.Bounds, new Rect(0, 55, 100, 45));
        }
    }


    public class FlexLayoutGetGrowTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that GetGrow returns the default value (0.0f) when no explicit value has been set.
        /// Input: BindableObject with no Grow value set.
        /// Expected: Returns 0.0f (default value).
        /// </summary>
        [Fact]
        public void GetGrow_WithDefaultValue_ReturnsZero()
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();

            // Act
            var result = FlexLayout.GetGrow(view);

            // Assert
            Assert.Equal(0.0f, result);
        }

        /// <summary>
        /// Tests that GetGrow returns the explicitly set positive value.
        /// Input: BindableObject with explicitly set positive Grow value.
        /// Expected: Returns the set value.
        /// </summary>
        [Theory]
        [InlineData(1.0f)]
        [InlineData(2.5f)]
        [InlineData(100.0f)]
        [InlineData(float.MaxValue)]
        public void GetGrow_WithExplicitPositiveValue_ReturnsSetValue(float expectedValue)
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();
            FlexLayout.SetGrow(view, expectedValue);

            // Act
            var result = FlexLayout.GetGrow(view);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetGrow returns zero when explicitly set to zero.
        /// Input: BindableObject with explicitly set zero Grow value.
        /// Expected: Returns 0.0f.
        /// </summary>
        [Fact]
        public void GetGrow_WithExplicitZeroValue_ReturnsZero()
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();
            FlexLayout.SetGrow(view, 0.0f);

            // Act
            var result = FlexLayout.GetGrow(view);

            // Assert
            Assert.Equal(0.0f, result);
        }

        /// <summary>
        /// Tests that GetGrow handles special float values correctly.
        /// Input: BindableObject with special float values that are valid (>= 0).
        /// Expected: Returns the set special value.
        /// </summary>
        [Theory]
        [InlineData(float.PositiveInfinity)]
        public void GetGrow_WithSpecialValidFloatValues_ReturnsSetValue(float expectedValue)
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();
            FlexLayout.SetGrow(view, expectedValue);

            // Act
            var result = FlexLayout.GetGrow(view);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetGrow throws ArgumentNullException when bindable parameter is null.
        /// Input: null BindableObject.
        /// Expected: Throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void GetGrow_WithNullBindable_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullBindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlexLayout.GetGrow(nullBindable));
        }

        /// <summary>
        /// Tests that GetGrow works with different types of BindableObject implementations.
        /// Input: Various BindableObject types with set Grow values.
        /// Expected: Returns the set value regardless of the specific BindableObject type.
        /// </summary>
        [Theory]
        [InlineData(1.5f)]
        [InlineData(3.0f)]
        public void GetGrow_WithDifferentBindableObjectTypes_ReturnsSetValue(float expectedValue)
        {
            // Arrange
            var label = MockPlatformSizeService.Sub<Label>();
            FlexLayout.SetGrow(label, expectedValue);

            // Act
            var result = FlexLayout.GetGrow(label);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetGrow maintains precision for small float values.
        /// Input: BindableObject with very small positive float value.
        /// Expected: Returns the exact value with proper precision.
        /// </summary>
        [Theory]
        [InlineData(0.001f)]
        [InlineData(0.0001f)]
        [InlineData(float.Epsilon)]
        public void GetGrow_WithSmallPositiveValues_ReturnsPreciseValue(float expectedValue)
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();
            FlexLayout.SetGrow(view, expectedValue);

            // Act
            var result = FlexLayout.GetGrow(view);

            // Assert
            Assert.Equal(expectedValue, result);
        }
    }


    public class FlexLayoutTests
    {
        /// <summary>
        /// Tests that GetShrink returns the default shrink value (1.0f) when no custom value is set.
        /// Input: Valid BindableObject with default shrink value.
        /// Expected: Returns 1.0f (the default shrink value).
        /// </summary>
        [Fact]
        public void GetShrink_ValidBindableObjectWithDefaultValue_ReturnsDefaultValue()
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();
            bindableObject.GetValue(FlexLayout.ShrinkProperty).Returns(1f);

            // Act
            var result = FlexLayout.GetShrink(bindableObject);

            // Assert
            Assert.Equal(1f, result);
        }

        /// <summary>
        /// Tests that GetShrink returns custom shrink values when set.
        /// Input: Valid BindableObject with various custom shrink values.
        /// Expected: Returns the corresponding custom shrink value.
        /// </summary>
        [Theory]
        [InlineData(0f)]
        [InlineData(0.5f)]
        [InlineData(2.0f)]
        [InlineData(10.5f)]
        [InlineData(float.MaxValue)]
        public void GetShrink_ValidBindableObjectWithCustomValue_ReturnsCustomValue(float customValue)
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();
            bindableObject.GetValue(FlexLayout.ShrinkProperty).Returns(customValue);

            // Act
            var result = FlexLayout.GetShrink(bindableObject);

            // Assert
            Assert.Equal(customValue, result);
        }

        /// <summary>
        /// Tests that GetShrink throws NullReferenceException when bindable parameter is null.
        /// Input: null BindableObject.
        /// Expected: Throws NullReferenceException.
        /// </summary>
        [Fact]
        public void GetShrink_NullBindableObject_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindableObject = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlexLayout.GetShrink(bindableObject));
        }

        /// <summary>
        /// Tests that GetShrink properly casts the returned value from GetValue to float.
        /// Input: Valid BindableObject with GetValue returning an object that can be cast to float.
        /// Expected: Returns the correctly cast float value.
        /// </summary>
        [Fact]
        public void GetShrink_ValidBindableObjectWithObjectValue_CastsToFloat()
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();
            object shrinkValue = 3.14f;
            bindableObject.GetValue(FlexLayout.ShrinkProperty).Returns(shrinkValue);

            // Act
            var result = FlexLayout.GetShrink(bindableObject);

            // Assert
            Assert.Equal(3.14f, result);
        }

        /// <summary>
        /// Tests that GetAlignSelf throws NullReferenceException when bindable parameter is null.
        /// Input: null bindable object
        /// Expected: NullReferenceException is thrown
        /// </summary>
        [Fact]
        public void GetAlignSelf_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlexLayout.GetAlignSelf(bindable));
        }

        /// <summary>
        /// Tests that GetAlignSelf returns the default value when no explicit value is set.
        /// Input: bindable object with no AlignSelf value set
        /// Expected: FlexAlignSelf.Auto (default value)
        /// </summary>
        [Fact]
        public void GetAlignSelf_NoValueSet_ReturnsDefaultValue()
        {
            // Arrange
            var bindable = new Label();

            // Act
            var result = FlexLayout.GetAlignSelf(bindable);

            // Assert
            Assert.Equal(FlexAlignSelf.Auto, result);
        }

        /// <summary>
        /// Tests that GetAlignSelf returns the correct value for all FlexAlignSelf enum values.
        /// Input: bindable object with specific FlexAlignSelf value set
        /// Expected: The same FlexAlignSelf value that was set
        /// </summary>
        [Theory]
        [InlineData(FlexAlignSelf.Auto)]
        [InlineData(FlexAlignSelf.Stretch)]
        [InlineData(FlexAlignSelf.Center)]
        [InlineData(FlexAlignSelf.Start)]
        [InlineData(FlexAlignSelf.End)]
        public void GetAlignSelf_ValidValues_ReturnsCorrectValue(FlexAlignSelf expectedValue)
        {
            // Arrange
            var bindable = new Label();
            FlexLayout.SetAlignSelf(bindable, expectedValue);

            // Act
            var result = FlexLayout.GetAlignSelf(bindable);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that SetAlignSelf method correctly sets the AlignSelf property value on a valid BindableObject.
        /// </summary>
        [Fact]
        public void SetAlignSelf_ValidBindableObjectAndValidValue_SetsValue()
        {
            // Arrange
            var view = new View();
            var alignSelfValue = FlexAlignSelf.Center;

            // Act
            FlexLayout.SetAlignSelf(view, alignSelfValue);

            // Assert
            Assert.Equal(alignSelfValue, FlexLayout.GetAlignSelf(view));
        }

        /// <summary>
        /// Tests that SetAlignSelf method throws ArgumentNullException when bindable parameter is null.
        /// </summary>
        [Fact]
        public void SetAlignSelf_NullBindableObject_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullBindable = null;
            var alignSelfValue = FlexAlignSelf.Auto;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => FlexLayout.SetAlignSelf(nullBindable, alignSelfValue));
        }

        /// <summary>
        /// Tests that SetAlignSelf method correctly sets all valid FlexAlignSelf enum values.
        /// Validates each enum value can be set and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData(FlexAlignSelf.Auto)]
        [InlineData(FlexAlignSelf.Stretch)]
        [InlineData(FlexAlignSelf.Center)]
        [InlineData(FlexAlignSelf.Start)]
        [InlineData(FlexAlignSelf.End)]
        public void SetAlignSelf_AllValidEnumValues_SetsValueCorrectly(FlexAlignSelf alignSelfValue)
        {
            // Arrange
            var view = new View();

            // Act
            FlexLayout.SetAlignSelf(view, alignSelfValue);

            // Assert
            Assert.Equal(alignSelfValue, FlexLayout.GetAlignSelf(view));
        }

        /// <summary>
        /// Tests that SetAlignSelf method handles invalid enum values (cast from integers outside defined range).
        /// The method should still set the value even if it's not a defined enum member.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetAlignSelf_InvalidEnumValue_SetsValue(int invalidEnumValue)
        {
            // Arrange
            var view = new View();
            var castValue = (FlexAlignSelf)invalidEnumValue;

            // Act
            FlexLayout.SetAlignSelf(view, castValue);

            // Assert
            Assert.Equal(castValue, FlexLayout.GetAlignSelf(view));
        }

        /// <summary>
        /// Tests that SetAlignSelf method works correctly with different types of BindableObject implementations.
        /// Validates the method works with various UI elements that inherit from BindableObject.
        /// </summary>
        [Fact]
        public void SetAlignSelf_DifferentBindableObjectTypes_SetsValue()
        {
            // Arrange
            var label = new Label();
            var button = new Button();
            var alignSelfValue = FlexAlignSelf.End;

            // Act
            FlexLayout.SetAlignSelf(label, alignSelfValue);
            FlexLayout.SetAlignSelf(button, alignSelfValue);

            // Assert
            Assert.Equal(alignSelfValue, FlexLayout.GetAlignSelf(label));
            Assert.Equal(alignSelfValue, FlexLayout.GetAlignSelf(button));
        }

        /// <summary>
        /// Tests GetOrder method with null BindableObject parameter.
        /// Should throw ArgumentNullException when bindable parameter is null.
        /// </summary>
        [Fact]
        public void GetOrder_NullBindableObject_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlexLayout.GetOrder(bindable));
        }

        /// <summary>
        /// Tests GetOrder method with valid BindableObject that has default order value.
        /// Should return the default order value of 0 when no explicit order is set.
        /// </summary>
        [Fact]
        public void GetOrder_DefaultValue_ReturnsZero()
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();

            // Act
            var result = FlexLayout.GetOrder(view);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests GetOrder method with various order values including edge cases.
        /// Should return the exact order value that was previously set.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void GetOrder_SetOrderValue_ReturnsCorrectValue(int expectedOrder)
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();
            FlexLayout.SetOrder(view, expectedOrder);

            // Act
            var result = FlexLayout.GetOrder(view);

            // Assert
            Assert.Equal(expectedOrder, result);
        }

        /// <summary>
        /// Tests GetOrder method with FlexLayout as BindableObject.
        /// Should return the correct order value when applied to the layout itself.
        /// </summary>
        [Fact]
        public void GetOrder_FlexLayoutBindableObject_ReturnsCorrectValue()
        {
            // Arrange
            var layout = new FlexLayout { IsPlatformEnabled = true };
            var expectedOrder = 42;
            FlexLayout.SetOrder(layout, expectedOrder);

            // Act
            var result = FlexLayout.GetOrder(layout);

            // Assert
            Assert.Equal(expectedOrder, result);
        }

        /// <summary>
        /// Tests GetOrder method with Label as BindableObject.
        /// Should return the correct order value when applied to different view types.
        /// </summary>
        [Fact]
        public void GetOrder_LabelBindableObject_ReturnsCorrectValue()
        {
            // Arrange
            var label = MockPlatformSizeService.Sub<Label>();
            var expectedOrder = -5;
            FlexLayout.SetOrder(label, expectedOrder);

            // Act
            var result = FlexLayout.GetOrder(label);

            // Assert
            Assert.Equal(expectedOrder, result);
        }

        /// <summary>
        /// Tests that the Wrap property returns the default value of NoWrap when not explicitly set
        /// </summary>
        [Fact]
        public void Wrap_DefaultValue_ReturnsNoWrap()
        {
            // Arrange
            var flexLayout = new Microsoft.Maui.Controls.Compatibility.FlexLayout();

            // Act
            var result = flexLayout.Wrap;

            // Assert
            Assert.Equal(FlexWrap.NoWrap, result);
        }

        /// <summary>
        /// Tests that the Wrap property can be set and retrieved for all valid FlexWrap enum values
        /// </summary>
        /// <param name="expectedWrap">The FlexWrap value to test</param>
        [Theory]
        [InlineData(FlexWrap.NoWrap)]
        [InlineData(FlexWrap.Wrap)]
        [InlineData(FlexWrap.Reverse)]
        public void Wrap_SetValidValue_ReturnsSetValue(FlexWrap expectedWrap)
        {
            // Arrange
            var flexLayout = new Microsoft.Maui.Controls.Compatibility.FlexLayout();

            // Act
            flexLayout.Wrap = expectedWrap;
            var result = flexLayout.Wrap;

            // Assert
            Assert.Equal(expectedWrap, result);
        }

        /// <summary>
        /// Tests that setting the Wrap property updates the underlying BindableProperty correctly
        /// </summary>
        /// <param name="wrapValue">The FlexWrap value to test</param>
        [Theory]
        [InlineData(FlexWrap.NoWrap)]
        [InlineData(FlexWrap.Wrap)]
        [InlineData(FlexWrap.Reverse)]
        public void Wrap_SetValue_UpdatesBindableProperty(FlexWrap wrapValue)
        {
            // Arrange
            var flexLayout = new Microsoft.Maui.Controls.Compatibility.FlexLayout();

            // Act
            flexLayout.Wrap = wrapValue;
            var bindableValue = (FlexWrap)flexLayout.GetValue(Microsoft.Maui.Controls.Compatibility.FlexLayout.WrapProperty);

            // Assert
            Assert.Equal(wrapValue, bindableValue);
        }

        /// <summary>
        /// Tests that the Wrap property getter correctly retrieves values from the underlying BindableProperty
        /// </summary>
        /// <param name="wrapValue">The FlexWrap value to test</param>
        [Theory]
        [InlineData(FlexWrap.NoWrap)]
        [InlineData(FlexWrap.Wrap)]
        [InlineData(FlexWrap.Reverse)]
        public void Wrap_GetterWithBindablePropertyValue_ReturnsCorrectValue(FlexWrap wrapValue)
        {
            // Arrange
            var flexLayout = new Microsoft.Maui.Controls.Compatibility.FlexLayout();
            flexLayout.SetValue(Microsoft.Maui.Controls.Compatibility.FlexLayout.WrapProperty, wrapValue);

            // Act
            var result = flexLayout.Wrap;

            // Assert
            Assert.Equal(wrapValue, result);
        }

        /// <summary>
        /// Tests that casting an invalid integer value to FlexWrap behaves consistently with the property
        /// </summary>
        [Fact]
        public void Wrap_SetInvalidEnumValue_StoresAndReturnsValue()
        {
            // Arrange
            var flexLayout = new Microsoft.Maui.Controls.Compatibility.FlexLayout();
            var invalidEnumValue = (FlexWrap)999;

            // Act
            flexLayout.Wrap = invalidEnumValue;
            var result = flexLayout.Wrap;

            // Assert
            Assert.Equal(invalidEnumValue, result);
        }

        /// <summary>
        /// Tests that SetBasis correctly sets the FlexBasis.Auto value on a valid BindableObject.
        /// Verifies that the basis property is set and can be retrieved using GetBasis.
        /// </summary>
        [Fact]
        public void SetBasis_ValidBindableAndAutoBasis_SetsCorrectValue()
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();
            var expectedBasis = FlexBasis.Auto;

            // Act
            FlexLayout.SetBasis(view, expectedBasis);

            // Assert
            var actualBasis = FlexLayout.GetBasis(view);
            Assert.Equal(expectedBasis, actualBasis);
        }

        /// <summary>
        /// Tests that SetBasis correctly sets FlexBasis values created from various float lengths.
        /// Verifies that the basis property is set and can be retrieved using GetBasis.
        /// </summary>
        /// <param name="length">The length value to create FlexBasis from</param>
        [Theory]
        [InlineData(0f)]
        [InlineData(1f)]
        [InlineData(10f)]
        [InlineData(100f)]
        [InlineData(1000f)]
        [InlineData(float.MaxValue)]
        public void SetBasis_ValidBindableAndLengthBasis_SetsCorrectValue(float length)
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();
            var expectedBasis = new FlexBasis(length);

            // Act
            FlexLayout.SetBasis(view, expectedBasis);

            // Assert
            var actualBasis = FlexLayout.GetBasis(view);
            Assert.Equal(expectedBasis, actualBasis);
        }

        /// <summary>
        /// Tests that SetBasis correctly sets FlexBasis values with relative lengths.
        /// Verifies that relative basis values are properly set and retrieved.
        /// </summary>
        /// <param name="relativeLength">The relative length value (0-1) for FlexBasis</param>
        [Theory]
        [InlineData(0f)]
        [InlineData(0.25f)]
        [InlineData(0.5f)]
        [InlineData(0.75f)]
        [InlineData(1f)]
        public void SetBasis_ValidBindableAndRelativeBasis_SetsCorrectValue(float relativeLength)
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();
            var expectedBasis = new FlexBasis(relativeLength, true);

            // Act
            FlexLayout.SetBasis(view, expectedBasis);

            // Assert
            var actualBasis = FlexLayout.GetBasis(view);
            Assert.Equal(expectedBasis, actualBasis);
        }

        /// <summary>
        /// Tests that SetBasis throws an exception when passed a null BindableObject.
        /// Verifies that the method properly validates the bindable parameter.
        /// </summary>
        [Fact]
        public void SetBasis_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullBindable = null;
            var basis = FlexBasis.Auto;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlexLayout.SetBasis(nullBindable, basis));
        }

        /// <summary>
        /// Tests that SetBasis works correctly with FlexBasis values created via implicit conversion from float.
        /// Verifies that implicit conversion and basis setting work together properly.
        /// </summary>
        [Fact]
        public void SetBasis_ValidBindableAndImplicitFloatBasis_SetsCorrectValue()
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();
            FlexBasis expectedBasis = 50f; // Using implicit conversion

            // Act
            FlexLayout.SetBasis(view, expectedBasis);

            // Assert
            var actualBasis = FlexLayout.GetBasis(view);
            Assert.Equal(expectedBasis, actualBasis);
        }
    }



    public class FlexLayoutSetShrinkTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that SetShrink successfully sets valid shrink values on a BindableObject.
        /// Verifies that positive values and zero are accepted and can be retrieved via GetShrink.
        /// </summary>
        /// <param name="shrinkValue">The shrink value to test</param>
        [Theory]
        [InlineData(0.0f)]
        [InlineData(1.0f)]
        [InlineData(2.5f)]
        [InlineData(100.0f)]
        public void SetShrink_ValidValues_SetsValueSuccessfully(float shrinkValue)
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();

            // Act
            FlexLayout.SetShrink(view, shrinkValue);

            // Assert
            Assert.Equal(shrinkValue, FlexLayout.GetShrink(view));
        }

        /// <summary>
        /// Tests that SetShrink handles boundary values correctly.
        /// Verifies that very small positive values and maximum values are accepted.
        /// </summary>
        /// <param name="shrinkValue">The boundary shrink value to test</param>
        [Theory]
        [InlineData(float.Epsilon)]
        [InlineData(float.MaxValue)]
        public void SetShrink_BoundaryValues_SetsValueSuccessfully(float shrinkValue)
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();

            // Act
            FlexLayout.SetShrink(view, shrinkValue);

            // Assert
            Assert.Equal(shrinkValue, FlexLayout.GetShrink(view));
        }

        /// <summary>
        /// Tests that SetShrink rejects negative values due to validation constraints.
        /// Verifies that the bindable property validation prevents setting negative shrink values.
        /// </summary>
        /// <param name="invalidValue">The invalid shrink value to test</param>
        [Theory]
        [InlineData(-1.0f)]
        [InlineData(-100.0f)]
        [InlineData(float.MinValue)]
        [InlineData(float.NegativeInfinity)]
        public void SetShrink_NegativeValues_ValidationPreventsSettingValue(float invalidValue)
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();
            var originalValue = FlexLayout.GetShrink(view); // Should be default value (1f)

            // Act
            FlexLayout.SetShrink(view, invalidValue);

            // Assert - Value should remain unchanged due to validation failure
            Assert.Equal(originalValue, FlexLayout.GetShrink(view));
        }

        /// <summary>
        /// Tests that SetShrink handles NaN value appropriately.
        /// Verifies that NaN values are handled by the validation system.
        /// </summary>
        [Fact]
        public void SetShrink_NaNValue_ValidationPreventsSettingValue()
        {
            // Arrange
            var view = MockPlatformSizeService.Sub<View>();
            var originalValue = FlexLayout.GetShrink(view); // Should be default value (1f)

            // Act
            FlexLayout.SetShrink(view, float.NaN);

            // Assert - Value should remain unchanged due to validation failure
            Assert.Equal(originalValue, FlexLayout.GetShrink(view));
        }

        /// <summary>
        /// Tests that SetShrink throws ArgumentNullException when bindable parameter is null.
        /// Verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void SetShrink_NullBindable_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullBindable = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => FlexLayout.SetShrink(nullBindable, 1.0f));
        }

        /// <summary>
        /// Tests that SetShrink works with different types of BindableObjects.
        /// Verifies that the method works with various BindableObject-derived types.
        /// </summary>
        [Fact]
        public void SetShrink_DifferentBindableObjectTypes_SetsValueSuccessfully()
        {
            // Arrange
            var label = MockPlatformSizeService.Sub<Label>();
            var view = MockPlatformSizeService.Sub<View>();
            var testValue = 3.5f;

            // Act
            FlexLayout.SetShrink(label, testValue);
            FlexLayout.SetShrink(view, testValue);

            // Assert
            Assert.Equal(testValue, FlexLayout.GetShrink(label));
            Assert.Equal(testValue, FlexLayout.GetShrink(view));
        }
    }



    public class FlexLayoutOnMeasureTests : BaseTestFixture
    {
        /// <summary>
        /// Helper class to expose protected OnMeasure method for testing
        /// </summary>
        private class TestableFlexLayout : FlexLayout
        {
            public new SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
            {
                return base.OnMeasure(widthConstraint, heightConstraint);
            }

            public void SetRoot(Item root)
            {
                _root = root;
            }

            public Item GetRoot() => _root;
        }

        /// <summary>
        /// Tests OnMeasure when _root is null.
        /// Should return a SizeRequest with the provided constraints as the size.
        /// </summary>
        [Fact]
        public void OnMeasure_RootIsNull_ReturnsSizeRequestWithConstraints()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            double widthConstraint = 100.0;
            double heightConstraint = 200.0;

            // Act
            var result = layout.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(new Size(widthConstraint, heightConstraint), result.Request);
        }

        /// <summary>
        /// Tests OnMeasure when both width and height constraints are finite.
        /// Should return a SizeRequest with the provided constraints as the size.
        /// </summary>
        [Theory]
        [InlineData(100.0, 200.0)]
        [InlineData(50.0, 75.0)]
        [InlineData(0.0, 0.0)]
        [InlineData(1.0, 1.0)]
        public void OnMeasure_BothConstraintsFinite_ReturnsSizeRequestWithConstraints(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var layout = new TestableFlexLayout
            {
                IsPlatformEnabled = true
            };
            layout.SetRoot(new Item());

            // Act
            var result = layout.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(new Size(widthConstraint, heightConstraint), result.Request);
        }

        /// <summary>
        /// Tests OnMeasure when width constraint is positive infinity and height is finite.
        /// Should calculate width from child items and use provided height constraint.
        /// </summary>
        [Fact]
        public void OnMeasure_WidthConstraintInfinite_CalculatesWidthFromChildren()
        {
            // Arrange
            var layout = new TestableFlexLayout
            {
                IsPlatformEnabled = true
            };

            var child1 = MockPlatformSizeService.Sub<View>();
            var child2 = MockPlatformSizeService.Sub<View>();

            layout.Children.Add(child1);
            layout.Children.Add(child2);

            // PopulateLayout to ensure _root is set up
            layout.OnIsPlatformEnabledChanged();

            double widthConstraint = double.PositiveInfinity;
            double heightConstraint = 100.0;

            // Act
            var result = layout.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(heightConstraint, result.Request.Height);
            Assert.True(result.Request.Width >= 0);
        }

        /// <summary>
        /// Tests OnMeasure when height constraint is positive infinity and width is finite.
        /// Should calculate height from child items and use provided width constraint.
        /// </summary>
        [Fact]
        public void OnMeasure_HeightConstraintInfinite_CalculatesHeightFromChildren()
        {
            // Arrange
            var layout = new TestableFlexLayout
            {
                IsPlatformEnabled = true
            };

            var child1 = MockPlatformSizeService.Sub<View>();
            var child2 = MockPlatformSizeService.Sub<View>();

            layout.Children.Add(child1);
            layout.Children.Add(child2);

            // PopulateLayout to ensure _root is set up
            layout.OnIsPlatformEnabledChanged();

            double widthConstraint = 100.0;
            double heightConstraint = double.PositiveInfinity;

            // Act
            var result = layout.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(widthConstraint, result.Request.Width);
            Assert.True(result.Request.Height >= 0);
        }

        /// <summary>
        /// Tests OnMeasure when both constraints are positive infinity.
        /// Should calculate both dimensions from child items.
        /// </summary>
        [Fact]
        public void OnMeasure_BothConstraintsInfinite_CalculatesBothDimensionsFromChildren()
        {
            // Arrange
            var layout = new TestableFlexLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();
            layout.Children.Add(child);

            // PopulateLayout to ensure _root is set up
            layout.OnIsPlatformEnabledChanged();

            double widthConstraint = double.PositiveInfinity;
            double heightConstraint = double.PositiveInfinity;

            // Act
            var result = layout.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Request.Width >= 0);
            Assert.True(result.Request.Height >= 0);
        }

        /// <summary>
        /// Tests OnMeasure with empty children collection and infinite constraints.
        /// Should return zero dimensions when no children are present.
        /// </summary>
        [Fact]
        public void OnMeasure_EmptyChildrenWithInfiniteConstraints_ReturnsZeroDimensions()
        {
            // Arrange
            var layout = new TestableFlexLayout
            {
                IsPlatformEnabled = true
            };

            // PopulateLayout to ensure _root is set up
            layout.OnIsPlatformEnabledChanged();

            double widthConstraint = double.PositiveInfinity;
            double heightConstraint = double.PositiveInfinity;

            // Act
            var result = layout.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.0, result.Request.Width);
            Assert.Equal(0.0, result.Request.Height);
        }

        /// <summary>
        /// Tests OnMeasure with special double values.
        /// Should handle edge cases like NaN, negative infinity appropriately.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 100.0)]
        [InlineData(100.0, double.NaN)]
        [InlineData(double.NegativeInfinity, 100.0)]
        [InlineData(100.0, double.NegativeInfinity)]
        [InlineData(-1.0, 100.0)]
        [InlineData(100.0, -1.0)]
        public void OnMeasure_SpecialDoubleValues_HandlesEdgeCases(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var layout = new TestableFlexLayout
            {
                IsPlatformEnabled = true
            };
            layout.SetRoot(new Item());

            // Act & Assert - Should not throw exceptions
            var result = layout.OnMeasure(widthConstraint, heightConstraint);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests OnMeasure modifies and restores flex item properties during measuring.
        /// Should temporarily set Shrink=0 and AlignSelf=Start, then restore original values.
        /// </summary>
        [Fact]
        public void OnMeasure_ModifiesAndRestoresFlexItemProperties()
        {
            // Arrange
            var layout = new TestableFlexLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();
            layout.Children.Add(child);

            // Set initial values
            FlexLayout.SetShrink(child, 2.0f);
            FlexLayout.SetAlignSelf(child, FlexAlignSelf.Center);

            // PopulateLayout to ensure _root is set up
            layout.OnIsPlatformEnabledChanged();

            double widthConstraint = double.PositiveInfinity;
            double heightConstraint = 100.0;

            // Act
            var result = layout.OnMeasure(widthConstraint, heightConstraint);

            // Assert - Properties should be restored to original values
            Assert.Equal(2.0f, FlexLayout.GetShrink(child));
            Assert.Equal(FlexAlignSelf.Center, FlexLayout.GetAlignSelf(child));
        }

        /// <summary>
        /// Tests OnMeasure with children that have different flex properties.
        /// Should handle children with various Order, Grow, Shrink, AlignSelf, and Basis values.
        /// </summary>
        [Fact]
        public void OnMeasure_ChildrenWithDifferentFlexProperties_HandlesVariousProperties()
        {
            // Arrange
            var layout = new TestableFlexLayout
            {
                IsPlatformEnabled = true,
                Direction = FlexDirection.Row
            };

            var child1 = MockPlatformSizeService.Sub<View>();
            var child2 = MockPlatformSizeService.Sub<View>();

            FlexLayout.SetOrder(child1, 1);
            FlexLayout.SetGrow(child1, 1.0f);
            FlexLayout.SetShrink(child1, 0.5f);
            FlexLayout.SetAlignSelf(child1, FlexAlignSelf.End);
            FlexLayout.SetBasis(child1, FlexBasis.Auto);

            FlexLayout.SetOrder(child2, 2);
            FlexLayout.SetGrow(child2, 2.0f);
            FlexLayout.SetShrink(child2, 1.5f);
            FlexLayout.SetAlignSelf(child2, FlexAlignSelf.Stretch);

            layout.Children.Add(child1);
            layout.Children.Add(child2);

            // PopulateLayout to ensure _root is set up
            layout.OnIsPlatformEnabledChanged();

            double widthConstraint = double.PositiveInfinity;
            double heightConstraint = double.PositiveInfinity;

            // Act
            var result = layout.OnMeasure(widthConstraint, heightConstraint);

            // Assert - Should complete without exceptions and restore properties
            Assert.NotNull(result);
            Assert.Equal(0.5f, FlexLayout.GetShrink(child1));
            Assert.Equal(FlexAlignSelf.End, FlexLayout.GetAlignSelf(child1));
            Assert.Equal(1.5f, FlexLayout.GetShrink(child2));
            Assert.Equal(FlexAlignSelf.Stretch, FlexLayout.GetAlignSelf(child2));
        }

        /// <summary>
        /// Tests OnMeasure with large constraint values.
        /// Should handle very large finite values without overflow issues.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue / 2, 100.0)]
        [InlineData(100.0, double.MaxValue / 2)]
        [InlineData(1e10, 1e10)]
        public void OnMeasure_LargeConstraintValues_HandlesLargeValues(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var layout = new TestableFlexLayout
            {
                IsPlatformEnabled = true
            };
            layout.SetRoot(new Item());

            // Act & Assert - Should not throw exceptions
            var result = layout.OnMeasure(widthConstraint, heightConstraint);
            Assert.NotNull(result);
            Assert.Equal(new Size(widthConstraint, heightConstraint), result.Request);
        }
    }



    public class FlexLayoutPositionTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the Position property returns the default value when not explicitly set.
        /// Input: FlexLayout instance with default Position value.
        /// Expected: Position should return FlexPosition.Relative (the default value).
        /// </summary>
        [Fact]
        public void Position_DefaultValue_ReturnsRelative()
        {
            // Arrange
            var flexLayout = new FlexLayout
            {
                IsPlatformEnabled = true
            };

            // Act
            var position = flexLayout.Position;

            // Assert
            Assert.Equal(FlexPosition.Relative, position);
        }

        /// <summary>
        /// Tests that the Position property can be set to and retrieved as FlexPosition.Relative.
        /// Input: FlexPosition.Relative value.
        /// Expected: Position getter returns FlexPosition.Relative.
        /// </summary>
        [Fact]
        public void Position_SetRelative_ReturnsRelative()
        {
            // Arrange
            var flexLayout = new FlexLayout
            {
                IsPlatformEnabled = true
            };

            // Act
            flexLayout.Position = FlexPosition.Relative;
            var position = flexLayout.Position;

            // Assert
            Assert.Equal(FlexPosition.Relative, position);
        }

        /// <summary>
        /// Tests that the Position property can be set to and retrieved as FlexPosition.Absolute.
        /// Input: FlexPosition.Absolute value.
        /// Expected: Position getter returns FlexPosition.Absolute.
        /// </summary>
        [Fact]
        public void Position_SetAbsolute_ReturnsAbsolute()
        {
            // Arrange
            var flexLayout = new FlexLayout
            {
                IsPlatformEnabled = true
            };

            // Act
            flexLayout.Position = FlexPosition.Absolute;
            var position = flexLayout.Position;

            // Assert
            Assert.Equal(FlexPosition.Absolute, position);
        }

        /// <summary>
        /// Tests Position property with all valid FlexPosition enum values using parameterized test.
        /// Input: Various FlexPosition enum values.
        /// Expected: Position getter returns the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(FlexPosition.Relative)]
        [InlineData(FlexPosition.Absolute)]
        public void Position_SetValidEnumValues_ReturnsExpectedValue(FlexPosition expectedPosition)
        {
            // Arrange
            var flexLayout = new FlexLayout
            {
                IsPlatformEnabled = true
            };

            // Act
            flexLayout.Position = expectedPosition;
            var actualPosition = flexLayout.Position;

            // Assert
            Assert.Equal(expectedPosition, actualPosition);
        }

        /// <summary>
        /// Tests Position property with invalid enum value to verify casting behavior.
        /// Input: Invalid FlexPosition enum value (cast from integer).
        /// Expected: Position getter returns the invalid value as cast FlexPosition.
        /// </summary>
        [Fact]
        public void Position_SetInvalidEnumValue_ReturnsInvalidValue()
        {
            // Arrange
            var flexLayout = new FlexLayout
            {
                IsPlatformEnabled = true
            };
            var invalidPosition = (FlexPosition)999;

            // Act
            flexLayout.Position = invalidPosition;
            var position = flexLayout.Position;

            // Assert
            Assert.Equal(invalidPosition, position);
        }
    }



    public class FlexLayoutOnRemovedTests : BaseTestFixture
    {
        /// <summary>
        /// Test that OnRemoved properly unsubscribes from PropertyChanged event, calls RemoveChild, and calls base.OnRemoved with a valid view.
        /// Tests the normal removal scenario where a view that was previously added is removed.
        /// Should execute all three lines of the OnRemoved method successfully.
        /// </summary>
        [Fact]
        public void OnRemoved_ValidView_UnsubscribesEventCallsRemoveChildAndBase()
        {
            // Arrange
            var testLayout = new TestableFlexLayout();
            testLayout.IsPlatformEnabled = true;

            var view = MockPlatformSizeService.Sub<View>();
            testLayout.Children.Add(view);

            bool propertyChangedWasCalled = false;
            view.PropertyChanged += (sender, e) => propertyChangedWasCalled = true;

            // Act
            testLayout.PublicOnRemoved(view);

            // Trigger property change to verify event was unsubscribed
            view.WidthRequest = 100;

            // Assert
            Assert.False(propertyChangedWasCalled); // Event should be unsubscribed
            Assert.True(testLayout.RemoveChildWasCalled);
            Assert.True(testLayout.BaseOnRemovedWasCalled);
            Assert.Equal(view, testLayout.LastRemovedView);
        }

        /// <summary>
        /// Test that OnRemoved handles null view parameter appropriately.
        /// Tests edge case where null is passed as the view parameter.
        /// Should handle null gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void OnRemoved_NullView_HandlesGracefully()
        {
            // Arrange
            var testLayout = new TestableFlexLayout();
            testLayout.IsPlatformEnabled = true;

            // Act & Assert - should not throw
            testLayout.PublicOnRemoved(null);

            Assert.True(testLayout.RemoveChildWasCalled);
            Assert.True(testLayout.BaseOnRemovedWasCalled);
            Assert.Null(testLayout.LastRemovedView);
        }

        /// <summary>
        /// Test that OnRemoved works correctly when layout is not initialized (root is null).
        /// Tests scenario where RemoveChild encounters null _root.
        /// Should still unsubscribe event and call base method even if RemoveChild returns early.
        /// </summary>
        [Fact]
        public void OnRemoved_UninitializedLayout_StillProcessesCorrectly()
        {
            // Arrange
            var testLayout = new TestableFlexLayout(); // Not setting IsPlatformEnabled, so _root remains null
            var view = MockPlatformSizeService.Sub<View>();

            bool propertyChangedWasCalled = false;
            view.PropertyChanged += (sender, e) => propertyChangedWasCalled = true;

            // Act
            testLayout.PublicOnRemoved(view);

            // Trigger property change to verify event was unsubscribed
            view.WidthRequest = 100;

            // Assert
            Assert.False(propertyChangedWasCalled); // Event should still be unsubscribed
            Assert.True(testLayout.RemoveChildWasCalled);
            Assert.True(testLayout.BaseOnRemovedWasCalled);
            Assert.Equal(view, testLayout.LastRemovedView);
        }

        /// <summary>
        /// Test that OnRemoved works with a view that has no FlexItemProperty set.
        /// Tests scenario where view was never properly added to flex layout.
        /// Should handle the case where GetFlexItem returns null gracefully.
        /// </summary>
        [Fact]
        public void OnRemoved_ViewWithNoFlexItem_HandlesCorrectly()
        {
            // Arrange
            var testLayout = new TestableFlexLayout();
            testLayout.IsPlatformEnabled = true;

            var view = MockPlatformSizeService.Sub<View>();
            // Not adding view to Children, so it has no FlexItemProperty

            bool propertyChangedWasCalled = false;
            view.PropertyChanged += (sender, e) => propertyChangedWasCalled = true;

            // Act
            testLayout.PublicOnRemoved(view);

            // Trigger property change to verify event was unsubscribed
            view.WidthRequest = 100;

            // Assert
            Assert.False(propertyChangedWasCalled); // Event should be unsubscribed
            Assert.True(testLayout.RemoveChildWasCalled);
            Assert.True(testLayout.BaseOnRemovedWasCalled);
            Assert.Equal(view, testLayout.LastRemovedView);
        }

        /// <summary>
        /// Test that OnRemoved works when PropertyChanged event handler was never subscribed.
        /// Tests edge case where the event unsubscription doesn't match any existing subscription.
        /// Should not throw exception when trying to unsubscribe from non-existent event.
        /// </summary>
        [Fact]
        public void OnRemoved_NoPropertyChangedSubscription_DoesNotThrow()
        {
            // Arrange
            var testLayout = new TestableFlexLayout();
            testLayout.IsPlatformEnabled = true;

            var view = MockPlatformSizeService.Sub<View>();
            // Not subscribing to PropertyChanged event

            // Act & Assert - should not throw
            testLayout.PublicOnRemoved(view);

            Assert.True(testLayout.RemoveChildWasCalled);
            Assert.True(testLayout.BaseOnRemovedWasCalled);
            Assert.Equal(view, testLayout.LastRemovedView);
        }

        /// <summary>
        /// Testable subclass of FlexLayout that exposes the protected OnRemoved method
        /// and tracks method calls for verification.
        /// </summary>
        private class TestableFlexLayout : FlexLayout
        {
            public bool RemoveChildWasCalled { get; private set; }
            public bool BaseOnRemovedWasCalled { get; private set; }
            public View LastRemovedView { get; private set; }

            public void PublicOnRemoved(View view)
            {
                OnRemoved(view);
            }

        }
    }



    /// <summary>
    /// Unit tests for the AlignItems property of FlexLayout.
    /// </summary>
    public class FlexLayoutAlignItemsTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the AlignItems property returns the default value of Stretch when first accessed.
        /// </summary>
        [Fact]
        public void AlignItems_DefaultValue_ReturnsStretch()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(FlexAlignItems.Stretch, result);
        }

        /// <summary>
        /// Tests that the AlignItems property can be set to and retrieved as Stretch.
        /// </summary>
        [Fact]
        public void AlignItems_SetToStretch_ReturnsStretch()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.AlignItems = FlexAlignItems.Stretch;
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(FlexAlignItems.Stretch, result);
        }

        /// <summary>
        /// Tests that the AlignItems property can be set to and retrieved as Center.
        /// </summary>
        [Fact]
        public void AlignItems_SetToCenter_ReturnsCenter()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.AlignItems = FlexAlignItems.Center;
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(FlexAlignItems.Center, result);
        }

        /// <summary>
        /// Tests that the AlignItems property can be set to and retrieved as Start.
        /// </summary>
        [Fact]
        public void AlignItems_SetToStart_ReturnsStart()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.AlignItems = FlexAlignItems.Start;
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(FlexAlignItems.Start, result);
        }

        /// <summary>
        /// Tests that the AlignItems property can be set to and retrieved as End.
        /// </summary>
        [Fact]
        public void AlignItems_SetToEnd_ReturnsEnd()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.AlignItems = FlexAlignItems.End;
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(FlexAlignItems.End, result);
        }

        /// <summary>
        /// Tests that the AlignItems property can handle invalid enum values by casting them appropriately.
        /// Tests setting an out-of-range integer value cast to FlexAlignItems enum.
        /// </summary>
        [Fact]
        public void AlignItems_SetToInvalidEnumValue_ReturnsSetValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var invalidValue = (FlexAlignItems)999;

            // Act
            flexLayout.AlignItems = invalidValue;
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(invalidValue, result);
        }

        /// <summary>
        /// Tests that the AlignItems property can handle negative enum values.
        /// Tests setting a negative integer value cast to FlexAlignItems enum.
        /// </summary>
        [Fact]
        public void AlignItems_SetToNegativeEnumValue_ReturnsSetValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var negativeValue = (FlexAlignItems)(-1);

            // Act
            flexLayout.AlignItems = negativeValue;
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(negativeValue, result);
        }

        /// <summary>
        /// Tests that the AlignItems property can handle maximum integer enum values.
        /// Tests setting the maximum integer value cast to FlexAlignItems enum.
        /// </summary>
        [Fact]
        public void AlignItems_SetToMaximumEnumValue_ReturnsSetValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var maxValue = (FlexAlignItems)int.MaxValue;

            // Act
            flexLayout.AlignItems = maxValue;
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(maxValue, result);
        }

        /// <summary>
        /// Tests that the AlignItems property can handle minimum integer enum values.
        /// Tests setting the minimum integer value cast to FlexAlignItems enum.
        /// </summary>
        [Fact]
        public void AlignItems_SetToMinimumEnumValue_ReturnsSetValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var minValue = (FlexAlignItems)int.MinValue;

            // Act
            flexLayout.AlignItems = minValue;
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(minValue, result);
        }

        /// <summary>
        /// Tests that setting the AlignItems property multiple times in succession works correctly.
        /// Verifies that the property can be changed from one valid value to another.
        /// </summary>
        [Fact]
        public void AlignItems_SetMultipleTimes_ReturnsLastSetValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert - Set to Center
            flexLayout.AlignItems = FlexAlignItems.Center;
            Assert.Equal(FlexAlignItems.Center, flexLayout.AlignItems);

            // Act & Assert - Set to Start
            flexLayout.AlignItems = FlexAlignItems.Start;
            Assert.Equal(FlexAlignItems.Start, flexLayout.AlignItems);

            // Act & Assert - Set to End
            flexLayout.AlignItems = FlexAlignItems.End;
            Assert.Equal(FlexAlignItems.End, flexLayout.AlignItems);

            // Act & Assert - Set back to default
            flexLayout.AlignItems = FlexAlignItems.Stretch;
            Assert.Equal(FlexAlignItems.Stretch, flexLayout.AlignItems);
        }
    }



    public class FlexLayoutLayoutChildrenTests : BaseTestFixture
    {
        /// <summary>
        /// Tests LayoutChildren when _root is null.
        /// Should return early without processing children.
        /// Expected: No exception thrown, method returns without error.
        /// </summary>
        [Fact]
        public void LayoutChildren_WhenRootIsNull_ReturnsEarly()
        {
            // Arrange
            var layout = new FlexLayout();
            // Don't set IsPlatformEnabled or Parent, so _root remains null

            // Act & Assert - Should not throw
            layout.Layout(new Rect(0, 0, 100, 100));
        }

        /// <summary>
        /// Tests LayoutChildren with valid parameters and children.
        /// Should process all children and apply proper layout.
        /// Expected: Children are positioned correctly with offset applied.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithValidChildren_LayoutsCorrectly()
        {
            // Arrange
            var child1 = MockPlatformSizeService.Sub<View>();
            var child2 = MockPlatformSizeService.Sub<View>();

            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Direction = FlexDirection.Column,
                Children = { child1, child2 }
            };

            // Act
            layout.Layout(new Rect(10, 20, 200, 300));

            // Assert
            Assert.Equal(new Rect(10, 20, 200, 300), layout.Bounds);
            // Children should be positioned with the offset applied
            Assert.True(child1.Bounds.X >= 10);
            Assert.True(child1.Bounds.Y >= 20);
            Assert.True(child2.Bounds.X >= 10);
            Assert.True(child2.Bounds.Y >= 20);
        }

        /// <summary>
        /// Tests LayoutChildren with empty children collection.
        /// Should complete without error when no children exist.
        /// Expected: No exception thrown, layout completes successfully.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithNoChildren_CompletesSuccessfully()
        {
            // Arrange
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true
            };

            // Act & Assert - Should not throw
            layout.Layout(new Rect(0, 0, 100, 100));
        }

        /// <summary>
        /// Tests LayoutChildren with extreme parameter values.
        /// Should handle boundary cases for position and size parameters.
        /// Expected: Layout completes without error for extreme values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MinValue, 100, 100)]
        [InlineData(double.MaxValue, double.MaxValue, 100, 100)]
        [InlineData(-1000, -1000, 100, 100)]
        [InlineData(0, 0, 0, 0)]
        [InlineData(0, 0, double.MaxValue, double.MaxValue)]
        public void LayoutChildren_WithExtremeParameters_HandlesGracefully(double x, double y, double width, double height)
        {
            // Arrange
            var child = MockPlatformSizeService.Sub<View>();
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = { child }
            };

            // Act & Assert - Should not throw
            layout.Layout(new Rect(x, y, width, height));
        }

        /// <summary>
        /// Tests LayoutChildren with infinite parameter values.
        /// Should handle infinite position and size values appropriately.
        /// Expected: Layout completes without error for infinite values.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, 0, 100, 100)]
        [InlineData(0, double.PositiveInfinity, 100, 100)]
        [InlineData(0, 0, double.PositiveInfinity, 100)]
        [InlineData(0, 0, 100, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0, 100, 100)]
        [InlineData(0, double.NegativeInfinity, 100, 100)]
        public void LayoutChildren_WithInfiniteParameters_HandlesGracefully(double x, double y, double width, double height)
        {
            // Arrange
            var child = MockPlatformSizeService.Sub<View>();
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = { child }
            };

            // Act & Assert - Should not throw
            layout.Layout(new Rect(x, y, width, height));
        }

        /// <summary>
        /// Tests LayoutChildren when flex item frame contains NaN values.
        /// Should throw Exception with specific message when frame has invalid coordinates.
        /// Expected: Exception with message "something is deeply wrong".
        /// </summary>
        [Fact]
        public void LayoutChildren_WithNaNFrameValues_ThrowsException()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var child = MockPlatformSizeService.Sub<View>();

            layout.IsPlatformEnabled = true;
            layout.Children.Add(child);

            // Set up the layout to have valid _root
            layout.Layout(new Rect(0, 0, 100, 100));

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => layout.TriggerLayoutChildrenWithNaNFrame(0, 0, 100, 100));
            Assert.Equal("something is deeply wrong", exception.Message);
        }

        /// <summary>
        /// Tests LayoutChildren with negative offset values.
        /// Should apply negative offsets correctly to child positions.
        /// Expected: Children positioned with negative offset applied.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithNegativeOffset_AppliesOffsetCorrectly()
        {
            // Arrange
            var child = MockPlatformSizeService.Sub<View>();
            var layout = new FlexLayout
            {
                IsPlatformEnabled = true,
                Children = { child }
            };

            // Act
            layout.Layout(new Rect(-50, -30, 200, 150));

            // Assert
            Assert.Equal(new Rect(-50, -30, 200, 150), layout.Bounds);
            // Child should be positioned with negative offset
            Assert.True(child.Bounds.X >= -50);
            Assert.True(child.Bounds.Y >= -30);
        }

        /// <summary>
        /// Helper class to test protected LayoutChildren method with NaN scenarios.
        /// </summary>
        private class TestableFlexLayout : FlexLayout
        {
            /// <summary>
            /// Exposes LayoutChildren for testing NaN frame scenarios.
            /// Creates a scenario where GetFrame returns NaN values to trigger exception.
            /// </summary>
            public void TriggerLayoutChildrenWithNaNFrame(double x, double y, double width, double height)
            {
                // Create a mock child that will return a frame with NaN height
                var mockChild = new MockViewWithNaNFrame();
                Children.Clear();
                Children.Add(mockChild);

                // Call the protected method
                LayoutChildren(x, y, width, height);
            }
        }

        /// <summary>
        /// Mock view that returns NaN frame values to test exception handling.
        /// </summary>
        private class MockViewWithNaNFrame : View
        {
            public override SizeRequest Measure(double widthConstraint, double heightConstraint, MeasureFlags flags = MeasureFlags.None)
            {
                return new SizeRequest(new Size(50, double.NaN));
            }
        }
    }



    public class FlexLayoutAlignContentTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the AlignContent property returns the default value when not explicitly set.
        /// Input: New FlexLayout instance.
        /// Expected: AlignContent should be FlexAlignContent.Stretch (the default value).
        /// </summary>
        [Fact]
        public void AlignContent_DefaultValue_ReturnsStretch()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            var result = flexLayout.AlignContent;

            // Assert
            Assert.Equal(FlexAlignContent.Stretch, result);
        }

        /// <summary>
        /// Tests that the AlignContent property correctly sets and gets all valid enum values.
        /// Input: Each valid FlexAlignContent enum value.
        /// Expected: Property should store and return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(FlexAlignContent.Stretch)]
        [InlineData(FlexAlignContent.Center)]
        [InlineData(FlexAlignContent.Start)]
        [InlineData(FlexAlignContent.End)]
        [InlineData(FlexAlignContent.SpaceBetween)]
        [InlineData(FlexAlignContent.SpaceAround)]
        [InlineData(FlexAlignContent.SpaceEvenly)]
        public void AlignContent_SetValidValues_ReturnsSetValue(FlexAlignContent alignContent)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.AlignContent = alignContent;
            var result = flexLayout.AlignContent;

            // Assert
            Assert.Equal(alignContent, result);
        }

        /// <summary>
        /// Tests that the AlignContent property accepts invalid enum values without throwing exceptions.
        /// Input: Invalid enum value cast from integer.
        /// Expected: Property should store and return the invalid value without validation errors.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void AlignContent_SetInvalidEnumValues_AcceptsWithoutValidation(int invalidValue)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var invalidAlignContent = (FlexAlignContent)invalidValue;

            // Act
            flexLayout.AlignContent = invalidAlignContent;
            var result = flexLayout.AlignContent;

            // Assert
            Assert.Equal(invalidAlignContent, result);
        }

        /// <summary>
        /// Tests that setting AlignContent multiple times with different values works correctly.
        /// Input: Sequence of different FlexAlignContent values.
        /// Expected: Property should correctly update and return each newly set value.
        /// </summary>
        [Fact]
        public void AlignContent_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert - Set and verify each value in sequence
            flexLayout.AlignContent = FlexAlignContent.Center;
            Assert.Equal(FlexAlignContent.Center, flexLayout.AlignContent);

            flexLayout.AlignContent = FlexAlignContent.End;
            Assert.Equal(FlexAlignContent.End, flexLayout.AlignContent);

            flexLayout.AlignContent = FlexAlignContent.SpaceBetween;
            Assert.Equal(FlexAlignContent.SpaceBetween, flexLayout.AlignContent);

            flexLayout.AlignContent = FlexAlignContent.Stretch;
            Assert.Equal(FlexAlignContent.Stretch, flexLayout.AlignContent);
        }
    }



    public class FlexLayoutOnParentSetTests : BaseTestFixture
    {
        /// <summary>
        /// Tests OnParentSet when Parent becomes non-null and _root is initially null.
        /// Should call PopulateLayout which initializes _root.
        /// </summary>
        [Fact]
        public void OnParentSet_ParentSetToNonNullWhenRootIsNull_CallsPopulateLayout()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var parent = new ContentView();

            // Verify initial state
            Assert.Null(layout.TestRoot);
            Assert.Null(layout.Parent);

            // Act
            layout.Parent = parent;

            // Assert
            Assert.NotNull(layout.TestRoot);
            Assert.Equal(parent, layout.Parent);
        }

        /// <summary>
        /// Tests OnParentSet when Parent becomes null and _root is not null.
        /// Should call ClearLayout which sets _root to null.
        /// </summary>
        [Fact]
        public void OnParentSet_ParentSetToNullWhenRootIsNotNull_CallsClearLayout()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var parent = new ContentView();

            // First set parent to initialize _root
            layout.Parent = parent;
            Assert.NotNull(layout.TestRoot);

            // Act
            layout.Parent = null;

            // Assert
            Assert.Null(layout.TestRoot);
            Assert.Null(layout.Parent);
        }

        /// <summary>
        /// Tests OnParentSet when Parent becomes non-null and _root is already not null.
        /// Should not call PopulateLayout or ClearLayout.
        /// </summary>
        [Fact]
        public void OnParentSet_ParentChangedFromNonNullToNonNullWhenRootIsNotNull_DoesNotCallLayoutMethods()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var parent1 = new ContentView();
            var parent2 = new ContentView();

            // First set parent to initialize _root
            layout.Parent = parent1;
            var initialRoot = layout.TestRoot;
            Assert.NotNull(initialRoot);

            // Act
            layout.Parent = parent2;

            // Assert
            Assert.Equal(initialRoot, layout.TestRoot); // _root should remain the same
            Assert.Equal(parent2, layout.Parent);
        }

        /// <summary>
        /// Tests OnParentSet when Parent is set to null and _root is already null.
        /// Should not call PopulateLayout or ClearLayout.
        /// </summary>
        [Fact]
        public void OnParentSet_ParentSetToNullWhenRootIsAlreadyNull_DoesNotCallLayoutMethods()
        {
            // Arrange
            var layout = new TestableFlexLayout();

            // Verify initial state
            Assert.Null(layout.TestRoot);
            Assert.Null(layout.Parent);

            // Act
            layout.Parent = null; // Redundant but tests the code path

            // Assert
            Assert.Null(layout.TestRoot); // Should remain null
            Assert.Null(layout.Parent);
        }

        /// <summary>
        /// Tests OnParentSet with various parent state transitions to ensure base.OnParentSet is always called.
        /// </summary>
        [Theory]
        [InlineData(true, true)]   // null -> non-null
        [InlineData(true, false)]  // non-null -> null  
        [InlineData(false, true)]  // null -> null (redundant)
        [InlineData(false, false)] // non-null -> non-null
        public void OnParentSet_VariousTransitions_AlwaysCallsBaseOnParentSet(bool startWithParent, bool endWithParent)
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var parent1 = new ContentView();
            var parent2 = new ContentView();

            if (startWithParent)
            {
                layout.Parent = parent1;
            }

            // Act & Assert - Setting parent should not throw and should complete successfully
            if (endWithParent)
            {
                layout.Parent = parent2;
                Assert.NotNull(layout.Parent);
            }
            else
            {
                layout.Parent = null;
                Assert.Null(layout.Parent);
            }
        }

        /// <summary>
        /// Helper class that exposes protected and private members for testing OnParentSet behavior.
        /// </summary>
        private class TestableFlexLayout : FlexLayout
        {
            public Item TestRoot => _root;
        }
    }


    public class FlexLayoutGetBasisTests
    {
        /// <summary>
        /// Tests that GetBasis throws NullReferenceException when bindable parameter is null.
        /// Input: null bindable object
        /// Expected: NullReferenceException
        /// </summary>
        [Fact]
        public void GetBasis_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlexLayout.GetBasis(bindable));
        }

        /// <summary>
        /// Tests that GetBasis returns the correct FlexBasis value for various FlexBasis types.
        /// Input: Valid BindableObject with different FlexBasis values set
        /// Expected: Returns the exact FlexBasis value that was previously set
        /// </summary>
        [Theory]
        [InlineData(0f, false)]
        [InlineData(10f, false)]
        [InlineData(100.5f, false)]
        [InlineData(0f, true)]
        [InlineData(0.5f, true)]
        [InlineData(1f, true)]
        public void GetBasis_ValidBindableWithVariousFlexBasisValues_ReturnsCorrectValue(float length, bool isRelative)
        {
            // Arrange
            var view = new Label();
            var expectedBasis = new FlexBasis(length, isRelative);
            FlexLayout.SetBasis(view, expectedBasis);

            // Act
            var result = FlexLayout.GetBasis(view);

            // Assert
            Assert.Equal(expectedBasis, result);
        }

        /// <summary>
        /// Tests that GetBasis returns FlexBasis.Auto when the Auto static value is set.
        /// Input: BindableObject with FlexBasis.Auto value set
        /// Expected: Returns FlexBasis.Auto
        /// </summary>
        [Fact]
        public void GetBasis_ValidBindableWithAutoValue_ReturnsFlexBasisAuto()
        {
            // Arrange
            var view = new Label();
            FlexLayout.SetBasis(view, FlexBasis.Auto);

            // Act
            var result = FlexLayout.GetBasis(view);

            // Assert
            Assert.Equal(FlexBasis.Auto, result);
        }

        /// <summary>
        /// Tests that GetBasis works with FlexBasis created via implicit conversion from float.
        /// Input: BindableObject with FlexBasis created via implicit float conversion
        /// Expected: Returns the equivalent FlexBasis value
        /// </summary>
        [Theory]
        [InlineData(0f)]
        [InlineData(25f)]
        [InlineData(float.MaxValue)]
        public void GetBasis_ValidBindableWithImplicitFloatConversion_ReturnsCorrectValue(float value)
        {
            // Arrange
            var view = new Label();
            FlexBasis expectedBasis = value; // Implicit conversion
            FlexLayout.SetBasis(view, expectedBasis);

            // Act
            var result = FlexLayout.GetBasis(view);

            // Assert
            Assert.Equal(expectedBasis, result);
        }

        /// <summary>
        /// Tests that GetBasis returns the default FlexBasis.Auto value when no value has been explicitly set.
        /// Input: BindableObject with no Basis value explicitly set
        /// Expected: Returns FlexBasis.Auto (the default value)
        /// </summary>
        [Fact]
        public void GetBasis_ValidBindableWithNoValueSet_ReturnsDefaultValue()
        {
            // Arrange
            var view = new Label();
            // No explicit SetBasis call, should return default value

            // Act
            var result = FlexLayout.GetBasis(view);

            // Assert
            Assert.Equal(FlexBasis.Auto, result);
        }

        /// <summary>
        /// Tests that GetBasis works correctly with different types of BindableObject implementations.
        /// Input: Various BindableObject implementations with FlexBasis values
        /// Expected: Returns correct FlexBasis values regardless of BindableObject type
        /// </summary>
        [Theory]
        [InlineData(typeof(Label))]
        [InlineData(typeof(Button))]
        [InlineData(typeof(Entry))]
        public void GetBasis_DifferentBindableObjectTypes_ReturnsCorrectValue(Type bindableType)
        {
            // Arrange
            var bindable = (BindableObject)Activator.CreateInstance(bindableType);
            var expectedBasis = new FlexBasis(42f);
            FlexLayout.SetBasis(bindable, expectedBasis);

            // Act
            var result = FlexLayout.GetBasis(bindable);

            // Assert
            Assert.Equal(expectedBasis, result);
        }
    }


    /// <summary>
    /// Unit tests for the FlexLayout.Direction property.
    /// </summary>
    public class FlexLayoutDirectionPropertyTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the Direction property returns the default value when a new FlexLayout instance is created.
        /// Verifies that the default Direction is FlexDirection.Row as specified in the DirectionProperty definition.
        /// </summary>
        [Fact]
        public void Direction_NewInstance_ReturnsDefaultRowValue()
        {
            // Arrange & Act
            var flexLayout = new FlexLayout();

            // Assert
            Assert.Equal(FlexDirection.Row, flexLayout.Direction);
        }

        /// <summary>
        /// Tests that the Direction property getter returns the correct value after setting it via the setter.
        /// Verifies that the property correctly stores and retrieves all valid FlexDirection enum values.
        /// </summary>
        /// <param name="direction">The FlexDirection value to test</param>
        [Theory]
        [InlineData(FlexDirection.Column)]
        [InlineData(FlexDirection.ColumnReverse)]
        [InlineData(FlexDirection.Row)]
        [InlineData(FlexDirection.RowReverse)]
        public void Direction_SetValidValue_GetterReturnsCorrectValue(FlexDirection direction)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.Direction = direction;
            var result = flexLayout.Direction;

            // Assert
            Assert.Equal(direction, result);
        }

        /// <summary>
        /// Tests that the Direction property setter correctly handles boundary enum values.
        /// Verifies that the minimum and maximum defined FlexDirection values work correctly.
        /// </summary>
        [Theory]
        [InlineData(FlexDirection.Column)]
        [InlineData(FlexDirection.RowReverse)]
        public void Direction_SetBoundaryValues_StoresCorrectly(FlexDirection direction)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.Direction = direction;

            // Assert
            Assert.Equal(direction, flexLayout.Direction);
        }

        /// <summary>
        /// Tests that the Direction property can be set multiple times and correctly updates the stored value.
        /// Verifies that consecutive property assignments work correctly without side effects.
        /// </summary>
        [Fact]
        public void Direction_MultipleSetOperations_UpdatesCorrectly()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert
            flexLayout.Direction = FlexDirection.Column;
            Assert.Equal(FlexDirection.Column, flexLayout.Direction);

            flexLayout.Direction = FlexDirection.RowReverse;
            Assert.Equal(FlexDirection.RowReverse, flexLayout.Direction);

            flexLayout.Direction = FlexDirection.Row;
            Assert.Equal(FlexDirection.Row, flexLayout.Direction);
        }

        /// <summary>
        /// Tests that the Direction property handles invalid enum values gracefully.
        /// Verifies behavior when an out-of-range enum value is cast and assigned.
        /// </summary>
        [Fact]
        public void Direction_SetInvalidEnumValue_StoresValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var invalidDirection = (FlexDirection)999;

            // Act
            flexLayout.Direction = invalidDirection;
            var result = flexLayout.Direction;

            // Assert
            Assert.Equal(invalidDirection, result);
        }

        /// <summary>
        /// Tests that setting the Direction property to the same value multiple times works correctly.
        /// Verifies that redundant assignments don't cause issues.
        /// </summary>
        [Fact]
        public void Direction_SetSameValueMultipleTimes_RemainsConsistent()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.Direction = FlexDirection.Column;
            flexLayout.Direction = FlexDirection.Column;
            flexLayout.Direction = FlexDirection.Column;

            // Assert
            Assert.Equal(FlexDirection.Column, flexLayout.Direction);
        }
    }



    public class FlexLayoutOnIsPlatformEnabledChangedTests : BaseTestFixture
    {
        /// <summary>
        /// Tests OnIsPlatformEnabledChanged when IsPlatformEnabled is true and _root is null.
        /// Should call PopulateLayout() and initialize the layout structure.
        /// </summary>
        [Fact]
        public void OnIsPlatformEnabledChanged_IsPlatformEnabledTrueAndRootNull_CallsPopulateLayout()
        {
            // Arrange
            var layout = new FlexLayout();
            var child1 = MockPlatformSizeService.Sub<View>();
            var child2 = MockPlatformSizeService.Sub<View>();
            layout.Children.Add(child1);
            layout.Children.Add(child2);

            // Ensure _root is null initially
            var rootField = typeof(FlexLayout).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);
            rootField.SetValue(layout, null);

            // Set IsPlatformEnabled to true to trigger the condition
            layout.IsPlatformEnabled = true;

            // Act
            layout.GetType().GetMethod("OnIsPlatformEnabledChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(layout, null);

            // Assert
            var rootAfter = rootField.GetValue(layout);
            Assert.NotNull(rootAfter);
            Assert.IsType<Microsoft.Maui.Layouts.Flex.Item>(rootAfter);
        }

        /// <summary>
        /// Tests OnIsPlatformEnabledChanged when IsPlatformEnabled is false and _root is not null.
        /// Should call ClearLayout() and set _root to null.
        /// </summary>
        [Fact]
        public void OnIsPlatformEnabledChanged_IsPlatformEnabledFalseAndRootNotNull_CallsClearLayout()
        {
            // Arrange
            var layout = new FlexLayout();
            var child = MockPlatformSizeService.Sub<View>();
            layout.Children.Add(child);

            // Set up initial state with _root not null
            var rootField = typeof(FlexLayout).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);
            rootField.SetValue(layout, new Microsoft.Maui.Layouts.Flex.Item());

            // Set IsPlatformEnabled to false
            layout.IsPlatformEnabled = false;

            // Act
            layout.GetType().GetMethod("OnIsPlatformEnabledChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(layout, null);

            // Assert
            var rootAfter = rootField.GetValue(layout);
            Assert.Null(rootAfter);
        }

        /// <summary>
        /// Tests OnIsPlatformEnabledChanged when IsPlatformEnabled is true but _root is already not null.
        /// Should not call PopulateLayout() or ClearLayout() and leave _root unchanged.
        /// </summary>
        [Fact]
        public void OnIsPlatformEnabledChanged_IsPlatformEnabledTrueAndRootNotNull_DoesNothing()
        {
            // Arrange
            var layout = new FlexLayout();
            var existingRoot = new Microsoft.Maui.Layouts.Flex.Item();

            var rootField = typeof(FlexLayout).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);
            rootField.SetValue(layout, existingRoot);

            layout.IsPlatformEnabled = true;

            // Act
            layout.GetType().GetMethod("OnIsPlatformEnabledChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(layout, null);

            // Assert
            var rootAfter = rootField.GetValue(layout);
            Assert.Same(existingRoot, rootAfter);
        }

        /// <summary>
        /// Tests OnIsPlatformEnabledChanged when IsPlatformEnabled is false and _root is null.
        /// Should not call PopulateLayout() or ClearLayout() and leave _root as null.
        /// </summary>
        [Fact]
        public void OnIsPlatformEnabledChanged_IsPlatformEnabledFalseAndRootNull_DoesNothing()
        {
            // Arrange
            var layout = new FlexLayout();

            var rootField = typeof(FlexLayout).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);
            rootField.SetValue(layout, null);

            layout.IsPlatformEnabled = false;

            // Act
            layout.GetType().GetMethod("OnIsPlatformEnabledChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(layout, null);

            // Assert
            var rootAfter = rootField.GetValue(layout);
            Assert.Null(rootAfter);
        }

        /// <summary>
        /// Tests OnIsPlatformEnabledChanged with various combinations of IsPlatformEnabled and _root states.
        /// Verifies correct behavior for all possible state combinations.
        /// </summary>
        [Theory]
        [InlineData(true, true, false)] // IsPlatformEnabled=true, _root!=null, should not change _root
        [InlineData(true, false, true)] // IsPlatformEnabled=true, _root==null, should create _root
        [InlineData(false, true, false)] // IsPlatformEnabled=false, _root!=null, should clear _root
        [InlineData(false, false, false)] // IsPlatformEnabled=false, _root==null, should keep _root null
        public void OnIsPlatformEnabledChanged_StateTransitions_BehavesCorrectly(bool isPlatformEnabled, bool rootInitiallyNotNull, bool expectedRootNotNullAfter)
        {
            // Arrange
            var layout = new FlexLayout();
            var rootField = typeof(FlexLayout).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);

            // Set initial _root state
            if (rootInitiallyNotNull)
                rootField.SetValue(layout, new Microsoft.Maui.Layouts.Flex.Item());
            else
                rootField.SetValue(layout, null);

            layout.IsPlatformEnabled = isPlatformEnabled;

            // Act
            layout.GetType().GetMethod("OnIsPlatformEnabledChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(layout, null);

            // Assert
            var rootAfter = rootField.GetValue(layout);
            if (expectedRootNotNullAfter)
                Assert.NotNull(rootAfter);
            else
                Assert.Null(rootAfter);
        }
    }
}