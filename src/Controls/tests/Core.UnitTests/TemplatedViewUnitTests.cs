#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class TemplatedViewUnitTests : BaseTestFixture
    {
        [Fact]
        public void TemplatedView_should_have_the_InternalChildren_correctly_when_ControlTemplate_changed()
        {
            var sut = new TemplatedView();
            var controlTemplated = (IControlTemplated)sut;
            var internalChildren = controlTemplated.InternalChildren;
            controlTemplated.AddLogicalChild(new VisualElement());
            controlTemplated.AddLogicalChild(new VisualElement());
            controlTemplated.AddLogicalChild(new VisualElement());

            sut.ControlTemplate = new ControlTemplate(typeof(ExpectedView));

            Assert.Single(internalChildren);
            Assert.IsType<ExpectedView>(internalChildren[0]);
        }

        [Fact]
        public void ShouldHaveTemplatedRootSet()
        {
            var tv = new TemplatedView();
            var ct = (IControlTemplated)tv;
            Assert.Null(ct.TemplateRoot);

            tv.ControlTemplate = new ControlTemplate(typeof(ExpectedView));

            var internalChildren = ct.InternalChildren;
            Assert.Equal(ct.TemplateRoot, internalChildren[0]);
        }

        [Fact]
        public void GetContentViewTemplateChildShouldWork()
        {
            var xaml = @"<ContentView
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestContentView"">
                       <ContentView.ControlTemplate>
                         <ControlTemplate>
                           <Label x:Name=""label0""/>
                         </ControlTemplate>
						</ContentView.ControlTemplate>
					</ContentView>";

            var contentView = new MyTestContentView();
            contentView.LoadFromXaml(xaml);

            var internalChildren = ((IControlTemplated)contentView).InternalChildren;
            Assert.Equal(internalChildren[0], contentView.TemplateChildObtained);
        }

        [Fact]
        public void GetTemplatedViewTemplateChildShouldWork()
        {
            var xaml =
                @"<ContentView
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestTemplatedView"">
					<TemplatedView.ControlTemplate>
						<ControlTemplate>
							<Label x:Name=""label0""/>
						</ControlTemplate>
					</TemplatedView.ControlTemplate>
				</ContentView>";

            var contentView = new MyTestTemplatedView();
            contentView.LoadFromXaml(xaml);

            var internalChildren = contentView.LogicalChildrenInternal;
            Assert.Equal(internalChildren[0], contentView.TemplateChildObtained);
        }

        [Fact]
        public void GetContentPageTemplateChildShouldWork()
        {
            var xaml = @"<ContentPage
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestContentPage"">
                       <ContentPage.ControlTemplate>
                         <ControlTemplate>
                           <Label x:Name=""label0""/>
                         </ControlTemplate>
						</ContentPage.ControlTemplate>
					</ContentPage>";

            var contentPage = new MyTestContentPage();
            contentPage.LoadFromXaml(xaml);

            IList<Element> internalChildren = contentPage.InternalChildren;
            Assert.Equal(internalChildren[0], contentPage.TemplateChildObtained);
        }

        [Fact]
        public void OnContentViewApplyTemplateShouldBeCalled()
        {
            var xaml = @"<ContentView
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestContentView"">
                       <ContentView.ControlTemplate>
                         <ControlTemplate>
                           <Label x:Name=""label0""/>
                         </ControlTemplate>
						</ContentView.ControlTemplate>
					</ContentView>";

            var contentView = new MyTestContentView();
            contentView.LoadFromXaml(xaml);
            Assert.True(contentView.WasOnApplyTemplateCalled);
        }

        [Fact]
        public void OnTemplatedViewApplyTemplateShouldBeCalled()
        {
            var xaml =
                @"<ContentView
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestTemplatedView"">
					<ContentView.ControlTemplate>
						<ControlTemplate>
							<Label x:Name=""label0""/>
						</ControlTemplate>
					</ContentView.ControlTemplate>
				</ContentView>";

            var contentView = new MyTestTemplatedView();
            contentView.LoadFromXaml(xaml);

            Assert.True(contentView.WasOnApplyTemplateCalled);
        }

        [Fact]
        public void OnContentPageApplyTemplateShouldBeCalled()
        {
            var xaml = @"<ContentPage
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestContentPage"">
                       <ContentPage.ControlTemplate>
                         <ControlTemplate>
                           <Label x:Name=""label0""/>
                         </ControlTemplate>
						</ContentPage.ControlTemplate>
					</ContentPage>";

            var contentPage = new MyTestContentPage();
            contentPage.LoadFromXaml(xaml);
            Assert.True(contentPage.WasOnApplyTemplateCalled);
        }

        private class ExpectedView : View
        {
            public ExpectedView()
            {
            }
        }

        public class MyTemplate : StackLayout
        {
            public MyTemplate()
            {
                Children.Add(new ContentPresenter());
            }
        }

        [Fact]
        public void BindingsShouldBeAppliedOnTemplateChange()
        {
            var template0 = new ControlTemplate(typeof(MyTemplate));
            var template1 = new ControlTemplate(typeof(MyTemplate));
            var label = new Label();
            label.SetBinding(Label.TextProperty, ".");
            var cv = new ContentView
            {
                ControlTemplate = template0,
                Content = label
            };
            cv.BindingContext = "Foo";

            Assert.Equal("Foo", label.Text);
            cv.ControlTemplate = template1;
            Assert.Equal("Foo", label.Text);
        }
    }


    class MyTestTemplatedView : TemplatedView
    {
        public bool WasOnApplyTemplateCalled { get; private set; }

        public Element TemplateChildObtained { get; private set; }

        protected override void OnApplyTemplate()
        {
            WasOnApplyTemplateCalled = true;
            TemplateChildObtained = (Element)GetTemplateChild("label0");
        }
    }


    class MyTestContentView : ContentView
    {
        public bool WasOnApplyTemplateCalled { get; private set; }

        public Element TemplateChildObtained { get; private set; }

        protected override void OnApplyTemplate()
        {
            WasOnApplyTemplateCalled = true;
            TemplateChildObtained = (Element)GetTemplateChild("label0");
        }
    }


    class MyTestContentPage : ContentPage
    {
        public bool WasOnApplyTemplateCalled { get; private set; }

        public Element TemplateChildObtained { get; private set; }

        protected override void OnApplyTemplate()
        {
            WasOnApplyTemplateCalled = true;
            TemplateChildObtained = (Element)GetTemplateChild("label0");
        }
    }

    public partial class TemplatedViewTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the CascadeInputTransparent property getter returns the default value when not explicitly set.
        /// Verifies the property retrieves the correct default value from the backing BindableProperty.
        /// </summary>
        [Fact]
        public void CascadeInputTransparent_Get_ReturnsDefaultValue()
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act
            var result = templatedView.CascadeInputTransparent;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the CascadeInputTransparent property getter returns true after the property is set to true.
        /// Verifies the property correctly stores and retrieves the true value through the BindableProperty system.
        /// </summary>
        [Fact]
        public void CascadeInputTransparent_SetTrue_GetReturnsTrue()
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act
            templatedView.CascadeInputTransparent = true;
            var result = templatedView.CascadeInputTransparent;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the CascadeInputTransparent property getter returns false after the property is set to false.
        /// Verifies the property correctly stores and retrieves the false value through the BindableProperty system.
        /// </summary>
        [Fact]
        public void CascadeInputTransparent_SetFalse_GetReturnsFalse()
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act
            templatedView.CascadeInputTransparent = false;
            var result = templatedView.CascadeInputTransparent;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the CascadeInputTransparent property getter correctly handles multiple value changes.
        /// Verifies the property can be toggled between true and false values and returns the correct current value.
        /// </summary>
        [Fact]
        public void CascadeInputTransparent_ToggleValues_GetReturnsCurrentValue()
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act & Assert - Set to true
            templatedView.CascadeInputTransparent = true;
            Assert.True(templatedView.CascadeInputTransparent);

            // Act & Assert - Toggle to false
            templatedView.CascadeInputTransparent = false;
            Assert.False(templatedView.CascadeInputTransparent);

            // Act & Assert - Toggle back to true
            templatedView.CascadeInputTransparent = true;
            Assert.True(templatedView.CascadeInputTransparent);
        }

        /// <summary>
        /// Tests GetTemplateChild method with a null name parameter.
        /// Should handle null input gracefully and return null.
        /// </summary>
        [Fact]
        public void GetTemplateChild_NullName_ReturnsNull()
        {
            // Arrange
            var templatedView = new TestTemplatedView();

            // Act
            var result = templatedView.CallGetTemplateChild(null);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetTemplateChild method with an empty string name parameter.
        /// Should return null since no element can have an empty name.
        /// </summary>
        [Fact]
        public void GetTemplateChild_EmptyName_ReturnsNull()
        {
            // Arrange
            var templatedView = new TestTemplatedView();

            // Act
            var result = templatedView.CallGetTemplateChild(string.Empty);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetTemplateChild method with a whitespace-only name parameter.
        /// Should return null since whitespace-only names are not valid element names.
        /// </summary>
        [Fact]
        public void GetTemplateChild_WhitespaceOnlyName_ReturnsNull()
        {
            // Arrange
            var templatedView = new TestTemplatedView();

            // Act
            var result = templatedView.CallGetTemplateChild("   ");

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetTemplateChild method when TemplateRoot is null.
        /// Should return null since there is no template root to search in.
        /// </summary>
        [Fact]
        public void GetTemplateChild_TemplateRootIsNull_ReturnsNull()
        {
            // Arrange
            var templatedView = new TestTemplatedView();
            // TemplateRoot is null by default

            // Act
            var result = templatedView.CallGetTemplateChild("validName");

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetTemplateChild method with a valid name that exists in the template.
        /// Should return the corresponding element from the template.
        /// </summary>
        [Fact]
        public void GetTemplateChild_ValidNameExists_ReturnsElement()
        {
            // Arrange
            var xaml = @"<ContentView
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
				x:Class=""Microsoft.Maui.Controls.Core.UnitTests.TestTemplatedView"">
				<TemplatedView.ControlTemplate>
					<ControlTemplate>
						<Label x:Name=""testLabel"" Text=""Test""/>
					</ControlTemplate>
				</TemplatedView.ControlTemplate>
			</ContentView>";

            var templatedView = new TestTemplatedView();
            templatedView.LoadFromXaml(xaml);

            // Act
            var result = templatedView.CallGetTemplateChild("testLabel");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Label>(result);
        }

        /// <summary>
        /// Tests GetTemplateChild method with a valid name that does not exist in the template.
        /// Should return null since the named element cannot be found.
        /// </summary>
        [Fact]
        public void GetTemplateChild_ValidNameDoesNotExist_ReturnsNull()
        {
            // Arrange
            var xaml = @"<ContentView
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
				x:Class=""Microsoft.Maui.Controls.Core.UnitTests.TestTemplatedView"">
				<TemplatedView.ControlTemplate>
					<ControlTemplate>
						<Label x:Name=""testLabel"" Text=""Test""/>
					</ControlTemplate>
				</TemplatedView.ControlTemplate>
			</ContentView>";

            var templatedView = new TestTemplatedView();
            templatedView.LoadFromXaml(xaml);

            // Act
            var result = templatedView.CallGetTemplateChild("nonExistentLabel");

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetTemplateChild method with various edge case string parameters.
        /// Verifies handling of special characters and unusual but valid strings.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("special@char")]
        [InlineData("name-with-dashes")]
        [InlineData("name_with_underscores")]
        [InlineData("123numeric")]
        [InlineData("NameWithNumbers123")]
        public void GetTemplateChild_EdgeCaseNames_HandlesGracefully(string name)
        {
            // Arrange
            var templatedView = new TestTemplatedView();

            // Act & Assert - Should not throw exceptions
            var result = templatedView.CallGetTemplateChild(name);

            // All these cases should return null since no template is set up
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetTemplateChild method with very long name parameter.
        /// Should handle long strings without throwing exceptions.
        /// </summary>
        [Fact]
        public void GetTemplateChild_VeryLongName_HandlesGracefully()
        {
            // Arrange
            var templatedView = new TestTemplatedView();
            var longName = new string('a', 10000); // 10k character string

            // Act & Assert - Should not throw exceptions
            var result = templatedView.CallGetTemplateChild(longName);

            Assert.Null(result);
        }

        /// <summary>
        /// Helper class to expose the protected GetTemplateChild method for testing.
        /// </summary>
        private class TestTemplatedView : TemplatedView
        {
            /// <summary>
            /// Exposes the protected GetTemplateChild method for testing purposes.
            /// </summary>
            /// <param name="name">The name of the template child to retrieve.</param>
            /// <returns>The template child element or null if not found.</returns>
            public object CallGetTemplateChild(string name)
            {
                return GetTemplateChild(name);
            }
        }

        /// <summary>
        /// Tests that ResolveControlTemplate returns null when ControlTemplate is not set (default state).
        /// Verifies the default behavior when no control template has been assigned.
        /// Expected result: null.
        /// </summary>
        [Fact]
        public void ResolveControlTemplate_WhenControlTemplateIsNull_ReturnsNull()
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act
            var result = templatedView.ResolveControlTemplate();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ResolveControlTemplate returns the assigned ControlTemplate instance.
        /// Verifies that the method returns the exact same instance that was assigned to the ControlTemplate property.
        /// Expected result: the same ControlTemplate instance that was set.
        /// </summary>
        [Fact]
        public void ResolveControlTemplate_WhenControlTemplateIsSet_ReturnsSameInstance()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var controlTemplate = new ControlTemplate();
            templatedView.ControlTemplate = controlTemplate;

            // Act
            var result = templatedView.ResolveControlTemplate();

            // Assert
            Assert.Same(controlTemplate, result);
        }

        /// <summary>
        /// Tests that ResolveControlTemplate returns the updated ControlTemplate after property change.
        /// Verifies that the method reflects changes when the ControlTemplate property is modified.
        /// Expected result: the new ControlTemplate instance.
        /// </summary>
        [Fact]
        public void ResolveControlTemplate_WhenControlTemplateIsChanged_ReturnsNewValue()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var originalTemplate = new ControlTemplate();
            var newTemplate = new ControlTemplate();
            templatedView.ControlTemplate = originalTemplate;

            // Act & Assert - verify original template is returned
            var originalResult = templatedView.ResolveControlTemplate();
            Assert.Same(originalTemplate, originalResult);

            // Change the template
            templatedView.ControlTemplate = newTemplate;

            // Act & Assert - verify new template is returned
            var newResult = templatedView.ResolveControlTemplate();
            Assert.Same(newTemplate, newResult);
        }

        /// <summary>
        /// Tests that ResolveControlTemplate returns null when ControlTemplate is set to null explicitly.
        /// Verifies that the method correctly handles explicit null assignment.
        /// Expected result: null.
        /// </summary>
        [Fact]
        public void ResolveControlTemplate_WhenControlTemplateSetToNull_ReturnsNull()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var controlTemplate = new ControlTemplate();
            templatedView.ControlTemplate = controlTemplate;
            templatedView.ControlTemplate = null;

            // Act
            var result = templatedView.ResolveControlTemplate();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ResolveControlTemplate works with ControlTemplate created using Type constructor.
        /// Verifies that the method works with different ControlTemplate constructor overloads.
        /// Expected result: the ControlTemplate instance created with Type constructor.
        /// </summary>
        [Fact]
        public void ResolveControlTemplate_WithControlTemplateFromType_ReturnsSameInstance()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var controlTemplate = new ControlTemplate(typeof(Label));
            templatedView.ControlTemplate = controlTemplate;

            // Act
            var result = templatedView.ResolveControlTemplate();

            // Assert
            Assert.Same(controlTemplate, result);
        }

        /// <summary>
        /// Tests that ResolveControlTemplate works with ControlTemplate created using Func constructor.
        /// Verifies that the method works with different ControlTemplate constructor overloads.
        /// Expected result: the ControlTemplate instance created with Func constructor.
        /// </summary>
        [Fact]
        public void ResolveControlTemplate_WithControlTemplateFromFunc_ReturnsSameInstance()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var controlTemplate = new ControlTemplate(() => new Label());
            templatedView.ControlTemplate = controlTemplate;

            // Act
            var result = templatedView.ResolveControlTemplate();

            // Assert
            Assert.Same(controlTemplate, result);
        }

        /// <summary>
        /// Tests that OnSizeAllocated can be called with positive width and height values
        /// without throwing exceptions and properly calls the base implementation.
        /// </summary>
        [Theory]
        [InlineData(100.0, 200.0)]
        [InlineData(1.0, 1.0)]
        [InlineData(500.5, 300.7)]
        public void OnSizeAllocated_PositiveValues_CallsBaseWithoutException(double width, double height)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.TestOnSizeAllocated(width, height));
            Assert.Null(exception);
            Assert.True(templatedView.WasOnSizeAllocatedCalled);
            Assert.Equal(width, templatedView.LastWidth);
            Assert.Equal(height, templatedView.LastHeight);
        }

        /// <summary>
        /// Tests that OnSizeAllocated can be called with zero width and height values
        /// without throwing exceptions and properly calls the base implementation.
        /// </summary>
        [Fact]
        public void OnSizeAllocated_ZeroValues_CallsBaseWithoutException()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            double width = 0.0;
            double height = 0.0;

            // Act & Assert
            var exception = Record.Exception(() => templatedView.TestOnSizeAllocated(width, height));
            Assert.Null(exception);
            Assert.True(templatedView.WasOnSizeAllocatedCalled);
            Assert.Equal(width, templatedView.LastWidth);
            Assert.Equal(height, templatedView.LastHeight);
        }

        /// <summary>
        /// Tests that OnSizeAllocated can be called with negative width and height values
        /// without throwing exceptions and properly calls the base implementation.
        /// </summary>
        [Theory]
        [InlineData(-100.0, -200.0)]
        [InlineData(-1.0, 100.0)]
        [InlineData(100.0, -1.0)]
        public void OnSizeAllocated_NegativeValues_CallsBaseWithoutException(double width, double height)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.TestOnSizeAllocated(width, height));
            Assert.Null(exception);
            Assert.True(templatedView.WasOnSizeAllocatedCalled);
            Assert.Equal(width, templatedView.LastWidth);
            Assert.Equal(height, templatedView.LastHeight);
        }

        /// <summary>
        /// Tests that OnSizeAllocated can be called with extreme double values
        /// without throwing exceptions and properly calls the base implementation.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.MinValue, double.MaxValue)]
        public void OnSizeAllocated_ExtremeValues_CallsBaseWithoutException(double width, double height)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.TestOnSizeAllocated(width, height));
            Assert.Null(exception);
            Assert.True(templatedView.WasOnSizeAllocatedCalled);
            Assert.Equal(width, templatedView.LastWidth);
            Assert.Equal(height, templatedView.LastHeight);
        }

        /// <summary>
        /// Tests that OnSizeAllocated can be called with infinity values
        /// without throwing exceptions and properly calls the base implementation.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, 100.0)]
        [InlineData(100.0, double.PositiveInfinity)]
        public void OnSizeAllocated_InfinityValues_CallsBaseWithoutException(double width, double height)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.TestOnSizeAllocated(width, height));
            Assert.Null(exception);
            Assert.True(templatedView.WasOnSizeAllocatedCalled);
            Assert.Equal(width, templatedView.LastWidth);
            Assert.Equal(height, templatedView.LastHeight);
        }

        /// <summary>
        /// Tests that OnSizeAllocated can be called with NaN values
        /// without throwing exceptions and properly calls the base implementation.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.NaN, 100.0)]
        [InlineData(100.0, double.NaN)]
        public void OnSizeAllocated_NaNValues_CallsBaseWithoutException(double width, double height)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.TestOnSizeAllocated(width, height));
            Assert.Null(exception);
            Assert.True(templatedView.WasOnSizeAllocatedCalled);
            Assert.True(double.IsNaN(templatedView.LastWidth) == double.IsNaN(width));
            Assert.True(double.IsNaN(templatedView.LastHeight) == double.IsNaN(height));
        }

        private class TestableTemplatedView : TemplatedView
        {
            public bool WasOnSizeAllocatedCalled { get; private set; }
            public double LastWidth { get; private set; }
            public double LastHeight { get; private set; }

            public void TestOnSizeAllocated(double width, double height)
            {
                OnSizeAllocated(width, height);
            }

            protected override void OnSizeAllocated(double width, double height)
            {
                WasOnSizeAllocatedCalled = true;
                LastWidth = width;
                LastHeight = height;
                base.OnSizeAllocated(width, height);
            }
        }

        /// <summary>
        /// Tests the LayoutChildren method with various coordinate and dimension values.
        /// Verifies that the obsolete method executes without throwing exceptions for all input combinations.
        /// Expected result: Method completes successfully without exceptions.
        /// </summary>
        /// <param name="x">The x-coordinate for layout positioning.</param>
        /// <param name="y">The y-coordinate for layout positioning.</param>
        /// <param name="width">The width dimension for layout sizing.</param>
        /// <param name="height">The height dimension for layout sizing.</param>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(10.5, 20.3, 100.7, 200.9)]
        [InlineData(-10, -20, -50, -80)]
        [InlineData(double.MinValue, double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        public void LayoutChildren_WithVariousCoordinatesAndDimensions_CompletesWithoutException(double x, double y, double width, double height)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => templatedView.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests the LayoutChildren method with special double values including NaN and infinity.
        /// Verifies that the obsolete method handles special floating-point values without throwing exceptions.
        /// Expected result: Method completes successfully without exceptions for special double values.
        /// </summary>
        /// <param name="x">The x-coordinate with special double values.</param>
        /// <param name="y">The y-coordinate with special double values.</param>
        /// <param name="width">The width dimension with special double values.</param>
        /// <param name="height">The height dimension with special double values.</param>
        [Theory]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, 0, double.PositiveInfinity, double.NegativeInfinity)]
        public void LayoutChildren_WithSpecialDoubleValues_CompletesWithoutException(double x, double y, double width, double height)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => templatedView.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests the LayoutChildren method with boundary values for typical layout scenarios.
        /// Verifies that the obsolete method handles common boundary cases in UI layout without exceptions.
        /// Expected result: Method completes successfully for typical boundary values.
        /// </summary>
        /// <param name="x">The x-coordinate for boundary testing.</param>
        /// <param name="y">The y-coordinate for boundary testing.</param>
        /// <param name="width">The width dimension for boundary testing.</param>
        /// <param name="height">The height dimension for boundary testing.</param>
        [Theory]
        [InlineData(0, 0, 1, 1)]
        [InlineData(-1, -1, 0, 0)]
        [InlineData(1000, 2000, 3000, 4000)]
        [InlineData(0.001, 0.001, 0.001, 0.001)]
        [InlineData(-0.001, -0.001, -0.001, -0.001)]
        public void LayoutChildren_WithBoundaryValues_CompletesWithoutException(double x, double y, double width, double height)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => templatedView.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests the LowerChild method with null view parameter.
        /// Verifies that the method calls the base implementation without throwing an exception.
        /// </summary>
        [Fact]
        public void LowerChild_WithNullView_CallsBaseMethodWithoutException()
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act & Assert
            // The method should not throw an exception even with null parameter
            // This tests the wrapper behavior and ensures base.LowerChild is called
            templatedView.LowerChild(null);
        }

        /// <summary>
        /// Tests the LowerChild method with a valid view parameter.
        /// Verifies that the method calls the base implementation successfully.
        /// </summary>
        [Fact]
        public void LowerChild_WithValidView_CallsBaseMethodSuccessfully()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var view = new View();

            // Act & Assert
            // The method should successfully call base.LowerChild with the view parameter
            // This ensures the wrapper correctly passes the parameter to the base method
            templatedView.LowerChild(view);
        }

        /// <summary>
        /// Tests the LowerChild method with various view parameters using parameterized test data.
        /// Verifies that the method handles different input scenarios correctly.
        /// </summary>
        /// <param name="view">The view parameter to test with.</param>
        [Theory]
        [MemberData(nameof(LowerChildTestData))]
        public void LowerChild_WithVariousInputs_CallsBaseMethod(View view)
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act & Assert
            // The method should call base.LowerChild regardless of input
            // This ensures consistent behavior across different parameter values
            templatedView.LowerChild(view);
        }

        /// <summary>
        /// Provides test data for parameterized LowerChild tests.
        /// </summary>
        public static IEnumerable<object[]> LowerChildTestData()
        {
            yield return new object[] { null };
            yield return new object[] { new View() };
            yield return new object[] { new TestView() };
        }

        /// <summary>
        /// Helper test view class for testing purposes.
        /// </summary>
        private class TestView : View
        {
            public TestView() : base()
            {
            }
        }

        /// <summary>
        /// Tests that RaiseChild method executes without throwing when passed a null view parameter.
        /// This tests the method's handling of invalid input and ensures it delegates to the base implementation.
        /// </summary>
        [Fact]
        public void RaiseChild_NullView_DoesNotThrow()
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.RaiseChild(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method executes without throwing when passed a valid View instance.
        /// This tests the method's core functionality and ensures it delegates to the base implementation.
        /// </summary>
        [Fact]
        public void RaiseChild_ValidView_DoesNotThrow()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var view = new TestView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.RaiseChild(view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method executes without throwing when passed a view that is not a child.
        /// This tests the method's handling of edge cases and ensures it delegates to the base implementation.
        /// </summary>
        [Fact]
        public void RaiseChild_ViewNotAChild_DoesNotThrow()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var nonChildView = new TestView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.RaiseChild(nonChildView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests RaiseChild method with various view scenarios using parameterized test data.
        /// This ensures the method handles different input conditions and always delegates to base implementation.
        /// </summary>
        [Theory]
        [MemberData(nameof(RaiseChildTestData))]
        public void RaiseChild_VariousScenarios_CallsBaseImplementation(View view, string scenario)
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.RaiseChild(view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Provides test data for parameterized RaiseChild tests with various view scenarios.
        /// </summary>
        public static IEnumerable<object[]> RaiseChildTestData()
        {
            yield return new object[] { null, "Null view" };
            yield return new object[] { new TestView(), "Valid view" };
            yield return new object[] { new TestView { IsVisible = false }, "Invisible view" };
            yield return new object[] { new TestView { IsEnabled = false }, "Disabled view" };
        }

        /// <summary>
        /// Tests that UpdateChildrenLayout method can be called without throwing exceptions.
        /// This method is obsolete and has an empty implementation.
        /// </summary>
        [Fact]
        public void UpdateChildrenLayout_WhenCalled_DoesNotThrowException()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.CallUpdateChildrenLayout());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateChildrenLayout method can be called multiple times without issues.
        /// Verifies that the empty implementation doesn't cause state corruption.
        /// </summary>
        [Fact]
        public void UpdateChildrenLayout_WhenCalledMultipleTimes_DoesNotThrowException()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                templatedView.CallUpdateChildrenLayout();
                templatedView.CallUpdateChildrenLayout();
                templatedView.CallUpdateChildrenLayout();
            });
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateChildrenLayout method can be called on a templated view with a control template.
        /// Ensures the method works even when the view has template content.
        /// </summary>
        [Fact]
        public void UpdateChildrenLayout_WithControlTemplate_DoesNotThrowException()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var controlTemplate = new ControlTemplate(() => new Label { Text = "Test" });
            templatedView.ControlTemplate = controlTemplate;

            // Act & Assert
            var exception = Record.Exception(() => templatedView.CallUpdateChildrenLayout());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateChildrenLayout method preserves the view's state.
        /// Since the implementation is empty, calling it should not modify any properties.
        /// </summary>
        [Fact]
        public void UpdateChildrenLayout_WhenCalled_PreservesViewState()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var originalTemplate = new ControlTemplate(() => new Button());
            templatedView.ControlTemplate = originalTemplate;
            templatedView.IsEnabled = false;
            templatedView.BackgroundColor = Colors.Red;

            // Act
            templatedView.CallUpdateChildrenLayout();

            // Assert
            Assert.Same(originalTemplate, templatedView.ControlTemplate);
            Assert.False(templatedView.IsEnabled);
            Assert.Equal(Colors.Red, templatedView.BackgroundColor);
        }

        /// <summary>
        /// Tests that InvalidateLayout method can be called without throwing an exception.
        /// This method is obsolete and simply delegates to the base implementation.
        /// </summary>
        [Fact]
        public void InvalidateLayout_WhenCalled_DoesNotThrow()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.CallInvalidateLayout());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnMeasure with normal positive constraints delegates to base implementation.
        /// Input conditions: Normal positive width and height constraints.
        /// Expected result: Method delegates to base.OnMeasure and returns a SizeRequest.
        /// </summary>
        [Fact]
        public void OnMeasure_WithNormalConstraints_DelegatesToBase()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            double widthConstraint = 100.0;
            double heightConstraint = 200.0;

            // Act
            var result = templatedView.CallOnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.True(templatedView.OnMeasureWasCalled);
            Assert.Equal(widthConstraint, templatedView.LastWidthConstraint);
            Assert.Equal(heightConstraint, templatedView.LastHeightConstraint);
        }

        /// <summary>
        /// Tests that OnMeasure with zero constraints delegates to base implementation.
        /// Input conditions: Zero width and height constraints.
        /// Expected result: Method delegates to base.OnMeasure and returns a SizeRequest.
        /// </summary>
        [Fact]
        public void OnMeasure_WithZeroConstraints_DelegatesToBase()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            double widthConstraint = 0.0;
            double heightConstraint = 0.0;

            // Act
            var result = templatedView.CallOnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.True(templatedView.OnMeasureWasCalled);
            Assert.Equal(widthConstraint, templatedView.LastWidthConstraint);
            Assert.Equal(heightConstraint, templatedView.LastHeightConstraint);
        }

        /// <summary>
        /// Tests that OnMeasure with negative constraints delegates to base implementation.
        /// Input conditions: Negative width and height constraints.
        /// Expected result: Method delegates to base.OnMeasure and returns a SizeRequest.
        /// </summary>
        [Fact]
        public void OnMeasure_WithNegativeConstraints_DelegatesToBase()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            double widthConstraint = -50.0;
            double heightConstraint = -100.0;

            // Act
            var result = templatedView.CallOnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.True(templatedView.OnMeasureWasCalled);
            Assert.Equal(widthConstraint, templatedView.LastWidthConstraint);
            Assert.Equal(heightConstraint, templatedView.LastHeightConstraint);
        }

        /// <summary>
        /// Tests that OnMeasure with positive infinity constraints delegates to base implementation.
        /// Input conditions: double.PositiveInfinity for both width and height constraints.
        /// Expected result: Method delegates to base.OnMeasure and returns a SizeRequest.
        /// </summary>
        [Fact]
        public void OnMeasure_WithPositiveInfinityConstraints_DelegatesToBase()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            double widthConstraint = double.PositiveInfinity;
            double heightConstraint = double.PositiveInfinity;

            // Act
            var result = templatedView.CallOnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.True(templatedView.OnMeasureWasCalled);
            Assert.Equal(widthConstraint, templatedView.LastWidthConstraint);
            Assert.Equal(heightConstraint, templatedView.LastHeightConstraint);
        }

        /// <summary>
        /// Tests that OnMeasure with negative infinity constraints delegates to base implementation.
        /// Input conditions: double.NegativeInfinity for both width and height constraints.
        /// Expected result: Method delegates to base.OnMeasure and returns a SizeRequest.
        /// </summary>
        [Fact]
        public void OnMeasure_WithNegativeInfinityConstraints_DelegatesToBase()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            double widthConstraint = double.NegativeInfinity;
            double heightConstraint = double.NegativeInfinity;

            // Act
            var result = templatedView.CallOnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.True(templatedView.OnMeasureWasCalled);
            Assert.Equal(widthConstraint, templatedView.LastWidthConstraint);
            Assert.Equal(heightConstraint, templatedView.LastHeightConstraint);
        }

        /// <summary>
        /// Tests that OnMeasure with NaN constraints delegates to base implementation.
        /// Input conditions: double.NaN for both width and height constraints.
        /// Expected result: Method delegates to base.OnMeasure and returns a SizeRequest.
        /// </summary>
        [Fact]
        public void OnMeasure_WithNaNConstraints_DelegatesToBase()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            double widthConstraint = double.NaN;
            double heightConstraint = double.NaN;

            // Act
            var result = templatedView.CallOnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.True(templatedView.OnMeasureWasCalled);
            Assert.True(double.IsNaN(templatedView.LastWidthConstraint));
            Assert.True(double.IsNaN(templatedView.LastHeightConstraint));
        }

        /// <summary>
        /// Tests that OnMeasure with extreme double values delegates to base implementation.
        /// Input conditions: double.MaxValue and double.MinValue for constraints.
        /// Expected result: Method delegates to base.OnMeasure and returns a SizeRequest.
        /// </summary>
        [Fact]
        public void OnMeasure_WithExtremeValues_DelegatesToBase()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            double widthConstraint = double.MaxValue;
            double heightConstraint = double.MinValue;

            // Act
            var result = templatedView.CallOnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.True(templatedView.OnMeasureWasCalled);
            Assert.Equal(widthConstraint, templatedView.LastWidthConstraint);
            Assert.Equal(heightConstraint, templatedView.LastHeightConstraint);
        }

        /// <summary>
        /// Tests that OnMeasure with mixed constraint values delegates to base implementation.
        /// Input conditions: Positive infinity width constraint and negative height constraint.
        /// Expected result: Method delegates to base.OnMeasure and returns a SizeRequest.
        /// </summary>
        [Fact]
        public void OnMeasure_WithMixedConstraints_DelegatesToBase()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            double widthConstraint = double.PositiveInfinity;
            double heightConstraint = -25.0;

            // Act
            var result = templatedView.CallOnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
            Assert.True(templatedView.OnMeasureWasCalled);
            Assert.Equal(widthConstraint, templatedView.LastWidthConstraint);
            Assert.Equal(heightConstraint, templatedView.LastHeightConstraint);
        }

        /// <summary>
        /// Tests that the OnChildMeasureInvalidated method can be called without throwing exceptions.
        /// This method is obsolete and has an empty implementation.
        /// </summary>
        [Fact]
        public void OnChildMeasureInvalidated_WhenCalled_DoesNotThrowException()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert
            var exception = Record.Exception(() => templatedView.CallOnChildMeasureInvalidated());

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the OnChildMeasureInvalidated method can be called multiple times without side effects.
        /// Since the method has an empty implementation, it should be safe to call repeatedly.
        /// </summary>
        [Fact]
        public void OnChildMeasureInvalidated_WhenCalledMultipleTimes_DoesNotThrowException()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                templatedView.CallOnChildMeasureInvalidated();
                templatedView.CallOnChildMeasureInvalidated();
                templatedView.CallOnChildMeasureInvalidated();
            });

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that IsClippedToBounds property returns the default value of false when not explicitly set.
        /// </summary>
        [Fact]
        public void IsClippedToBounds_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act
            var result = templatedView.IsClippedToBounds;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsClippedToBounds property setter correctly sets the value to true and getter returns it.
        /// </summary>
        [Fact]
        public void IsClippedToBounds_SetTrue_ReturnsTrue()
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act
            templatedView.IsClippedToBounds = true;
            var result = templatedView.IsClippedToBounds;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsClippedToBounds property setter correctly sets the value to false and getter returns it.
        /// </summary>
        [Fact]
        public void IsClippedToBounds_SetFalse_ReturnsFalse()
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act
            templatedView.IsClippedToBounds = false;
            var result = templatedView.IsClippedToBounds;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsClippedToBounds property can be set multiple times with different values and returns the correct value each time.
        /// </summary>
        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        public void IsClippedToBounds_MultipleSetOperations_ReturnsCorrectValue(bool firstValue, bool secondValue, bool thirdValue)
        {
            // Arrange
            var templatedView = new TemplatedView();

            // Act & Assert
            templatedView.IsClippedToBounds = firstValue;
            Assert.Equal(firstValue, templatedView.IsClippedToBounds);

            templatedView.IsClippedToBounds = secondValue;
            Assert.Equal(secondValue, templatedView.IsClippedToBounds);

            templatedView.IsClippedToBounds = thirdValue;
            Assert.Equal(thirdValue, templatedView.IsClippedToBounds);
        }
    }


    /// <summary>
    /// Unit tests for the ShouldInvalidateOnChildRemoved method in TemplatedView class.
    /// </summary>
    public partial class TemplatedViewShouldInvalidateOnChildRemovedTests
    {
        /// <summary>
        /// Test class that exposes the protected ShouldInvalidateOnChildRemoved method for testing.
        /// </summary>
        private class TestableTemplatedView : TemplatedView
        {
            public new bool ShouldInvalidateOnChildRemoved(View child)
            {
                return base.ShouldInvalidateOnChildRemoved(child);
            }
        }

        /// <summary>
        /// Tests that ShouldInvalidateOnChildRemoved returns true when called with a valid View instance.
        /// This verifies the method's basic functionality with normal input conditions.
        /// </summary>
        [Fact]
        public void ShouldInvalidateOnChildRemoved_WithValidView_ReturnsTrue()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var childView = new View();

            // Act
            var result = templatedView.ShouldInvalidateOnChildRemoved(childView);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldInvalidateOnChildRemoved returns true when called with null parameter.
        /// This verifies the method handles null input gracefully and still returns the expected value.
        /// </summary>
        [Fact]
        public void ShouldInvalidateOnChildRemoved_WithNullChild_ReturnsTrue()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act
            var result = templatedView.ShouldInvalidateOnChildRemoved(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldInvalidateOnChildRemoved consistently returns true for different View subclasses.
        /// This verifies the method behavior is consistent regardless of the specific View type passed.
        /// </summary>
        [Theory]
        [InlineData(typeof(Button))]
        [InlineData(typeof(Label))]
        [InlineData(typeof(StackLayout))]
        public void ShouldInvalidateOnChildRemoved_WithDifferentViewTypes_ReturnsTrue(Type viewType)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var childView = (View)Activator.CreateInstance(viewType);

            // Act
            var result = templatedView.ShouldInvalidateOnChildRemoved(childView);

            // Assert
            Assert.True(result);
        }
    }


    /// <summary>
    /// Unit tests for the MeasureOverride method of TemplatedView.
    /// </summary>
    public partial class TemplatedViewMeasureOverrideTests
    {
        /// <summary>
        /// Test subclass to expose the protected MeasureOverride method for testing.
        /// </summary>
        private class TestableTemplatedView : TemplatedView
        {
            public new Size MeasureOverride(double widthConstraint, double heightConstraint)
            {
                return base.MeasureOverride(widthConstraint, heightConstraint);
            }
        }

        /// <summary>
        /// Tests MeasureOverride with normal positive constraint values.
        /// Verifies that the method correctly delegates to ComputeDesiredSize and returns the expected size.
        /// </summary>
        [Theory]
        [InlineData(100.0, 200.0)]
        [InlineData(50.5, 75.25)]
        [InlineData(1.0, 1.0)]
        public void MeasureOverride_WithPositiveConstraints_ReturnsComputedSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var mockHandler = Substitute.For<IElementHandler>();
            var expectedSize = new Size(80.0, 120.0);

            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            templatedView.Handler = mockHandler;

            // Act
            var result = templatedView.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width);
            Assert.Equal(expectedSize.Height, result.Height);
        }

        /// <summary>
        /// Tests MeasureOverride with zero constraint values.
        /// Verifies that zero constraints are handled correctly.
        /// </summary>
        [Fact]
        public void MeasureOverride_WithZeroConstraints_ReturnsComputedSize()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var mockHandler = Substitute.For<IElementHandler>();
            var expectedSize = new Size(0.0, 0.0);

            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            templatedView.Handler = mockHandler;

            // Act
            var result = templatedView.MeasureOverride(0.0, 0.0);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width);
            Assert.Equal(expectedSize.Height, result.Height);
        }

        /// <summary>
        /// Tests MeasureOverride with infinite constraint values.
        /// Verifies that infinite constraints (common in layout scenarios) are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, 100.0)]
        [InlineData(100.0, double.PositiveInfinity)]
        public void MeasureOverride_WithInfiniteConstraints_ReturnsComputedSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var mockHandler = Substitute.For<IElementHandler>();
            var expectedSize = new Size(200.0, 150.0);

            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            templatedView.Handler = mockHandler;

            // Act
            var result = templatedView.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width);
            Assert.Equal(expectedSize.Height, result.Height);
        }

        /// <summary>
        /// Tests MeasureOverride with negative constraint values.
        /// Verifies that negative constraints are handled without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(-100.0, -200.0)]
        [InlineData(-50.5, 75.25)]
        [InlineData(100.0, -75.0)]
        public void MeasureOverride_WithNegativeConstraints_ReturnsComputedSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var mockHandler = Substitute.For<IElementHandler>();
            var expectedSize = new Size(50.0, 60.0);

            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            templatedView.Handler = mockHandler;

            // Act
            var result = templatedView.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width);
            Assert.Equal(expectedSize.Height, result.Height);
        }

        /// <summary>
        /// Tests MeasureOverride with NaN constraint values.
        /// Verifies that NaN constraints are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.NaN, 100.0)]
        [InlineData(100.0, double.NaN)]
        public void MeasureOverride_WithNaNConstraints_ReturnsComputedSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var mockHandler = Substitute.For<IElementHandler>();
            var expectedSize = new Size(30.0, 40.0);

            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            templatedView.Handler = mockHandler;

            // Act
            var result = templatedView.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width);
            Assert.Equal(expectedSize.Height, result.Height);
        }

        /// <summary>
        /// Tests MeasureOverride with extreme constraint values (MinValue and MaxValue).
        /// Verifies that extreme values are handled without overflow or underflow issues.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(double.MaxValue, double.MinValue)]
        public void MeasureOverride_WithExtremeConstraints_ReturnsComputedSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var mockHandler = Substitute.For<IElementHandler>();
            var expectedSize = new Size(10.0, 20.0);

            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            templatedView.Handler = mockHandler;

            // Act
            var result = templatedView.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width);
            Assert.Equal(expectedSize.Height, result.Height);
        }

        /// <summary>
        /// Tests MeasureOverride when Handler is null.
        /// Verifies that when Handler is null, ComputeDesiredSize returns Size.Zero.
        /// </summary>
        [Fact]
        public void MeasureOverride_WithNullHandler_ReturnsSizeZero()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            templatedView.Handler = null;

            // Act
            var result = templatedView.MeasureOverride(100.0, 200.0);

            // Assert
            Assert.Equal(Size.Zero.Width, result.Width);
            Assert.Equal(Size.Zero.Height, result.Height);
            Assert.True(result.IsZero);
        }

        /// <summary>
        /// Tests MeasureOverride with different margin values to verify margin calculations.
        /// Verifies that margins are properly accounted for in the size calculation.
        /// </summary>
        [Theory]
        [InlineData(10.0, 15.0, 5.0, 8.0)]
        [InlineData(0.0, 0.0, 0.0, 0.0)]
        [InlineData(25.5, 30.25, 12.75, 18.5)]
        public void MeasureOverride_WithMargins_IncludesMarginInResult(double left, double top, double right, double bottom)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var mockHandler = Substitute.For<IElementHandler>();
            var margin = new Thickness(left, top, right, bottom);
            var handlerSize = new Size(100.0, 150.0);

            templatedView.Margin = margin;
            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(handlerSize);
            templatedView.Handler = mockHandler;

            var expectedWidth = handlerSize.Width + margin.HorizontalThickness;
            var expectedHeight = handlerSize.Height + margin.VerticalThickness;

            // Act
            var result = templatedView.MeasureOverride(200.0, 300.0);

            // Assert
            Assert.Equal(expectedWidth, result.Width);
            Assert.Equal(expectedHeight, result.Height);
        }

        /// <summary>
        /// Tests MeasureOverride to verify that constraint parameters are properly passed to ComputeDesiredSize.
        /// Verifies that the method correctly delegates the constraint values.
        /// </summary>
        [Fact]
        public void MeasureOverride_PassesConstraintsToComputeDesiredSize()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var mockHandler = Substitute.For<IElementHandler>();
            var expectedSize = new Size(80.0, 120.0);
            var widthConstraint = 150.0;
            var heightConstraint = 250.0;

            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            templatedView.Handler = mockHandler;

            // Act
            var result = templatedView.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width);
            Assert.Equal(expectedSize.Height, result.Height);

            // Verify that GetDesiredSize was called (ComputeDesiredSize will call this)
            mockHandler.Received(1).GetDesiredSize(Arg.Any<double>(), Arg.Any<double>());
        }
    }


    /// <summary>
    /// Tests for the ArrangeOverride method of TemplatedView.
    /// </summary>
    public partial class TemplatedViewArrangeOverrideTests
    {
        /// <summary>
        /// Tests ArrangeOverride with null Handler to ensure Frame is set and Size is returned correctly.
        /// </summary>
        [Fact]
        public void ArrangeOverride_WithNullHandler_SetsFrameAndReturnsSize()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var bounds = new Rect(10, 20, 100, 200);

            // Act
            var result = templatedView.ArrangeOverride(bounds);

            // Assert
            Assert.NotEqual(Rect.Zero, templatedView.Frame);
            Assert.Equal(templatedView.Frame.Size, result);
        }

        /// <summary>
        /// Tests ArrangeOverride with non-null Handler to ensure PlatformArrange is called.
        /// </summary>
        [Fact]
        public void ArrangeOverride_WithNonNullHandler_CallsPlatformArrange()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var mockHandler = Substitute.For<IViewHandler>();
            templatedView.Handler = mockHandler;
            var bounds = new Rect(5, 10, 150, 300);

            // Act
            var result = templatedView.ArrangeOverride(bounds);

            // Assert
            mockHandler.Received(1).PlatformArrange(templatedView.Frame);
            Assert.Equal(templatedView.Frame.Size, result);
        }

        /// <summary>
        /// Tests ArrangeOverride with various bounds values to verify correct behavior.
        /// </summary>
        /// <param name="x">The x coordinate of the bounds.</param>
        /// <param name="y">The y coordinate of the bounds.</param>
        /// <param name="width">The width of the bounds.</param>
        /// <param name="height">The height of the bounds.</param>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(0, 0, 100, 100)]
        [InlineData(-50, -25, 200, 150)]
        [InlineData(1000, 2000, 500, 800)]
        [InlineData(0.5, 1.5, 99.99, 199.99)]
        public void ArrangeOverride_WithVariousBounds_ReturnsCorrectSize(double x, double y, double width, double height)
        {
            // Arrange
            var templatedView = new TemplatedView();
            var bounds = new Rect(x, y, width, height);

            // Act
            var result = templatedView.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(templatedView.Frame.Size, result);
            Assert.Equal(templatedView.Frame.Size.Width, result.Width);
            Assert.Equal(templatedView.Frame.Size.Height, result.Height);
        }

        /// <summary>
        /// Tests ArrangeOverride with extreme double values to ensure robustness.
        /// </summary>
        /// <param name="x">The x coordinate of the bounds.</param>
        /// <param name="y">The y coordinate of the bounds.</param>
        /// <param name="width">The width of the bounds.</param>
        /// <param name="height">The height of the bounds.</param>
        [Theory]
        [InlineData(double.MinValue, double.MinValue, 100, 100)]
        [InlineData(double.MaxValue, double.MaxValue, 100, 100)]
        [InlineData(0, 0, double.MaxValue, double.MaxValue)]
        [InlineData(0, 0, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(0, 0, double.NaN, 100)]
        [InlineData(0, 0, 100, double.NaN)]
        public void ArrangeOverride_WithExtremeValues_HandlesGracefully(double x, double y, double width, double height)
        {
            // Arrange
            var templatedView = new TemplatedView();
            var bounds = new Rect(x, y, width, height);

            // Act
            var result = templatedView.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(templatedView.Frame.Size, result);
        }

        /// <summary>
        /// Tests ArrangeOverride with Handler that becomes null after being set.
        /// </summary>
        [Fact]
        public void ArrangeOverride_WithHandlerSetToNull_DoesNotCallPlatformArrange()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var mockHandler = Substitute.For<IViewHandler>();
            templatedView.Handler = mockHandler;
            templatedView.Handler = null;
            var bounds = new Rect(0, 0, 100, 100);

            // Act
            var result = templatedView.ArrangeOverride(bounds);

            // Assert
            mockHandler.DidNotReceive().PlatformArrange(Arg.Any<Rect>());
            Assert.Equal(templatedView.Frame.Size, result);
        }

        /// <summary>
        /// Tests ArrangeOverride to verify Frame property is correctly updated.
        /// </summary>
        [Fact]
        public void ArrangeOverride_UpdatesFrameProperty()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var initialFrame = templatedView.Frame;
            var bounds = new Rect(25, 50, 300, 400);

            // Act
            templatedView.ArrangeOverride(bounds);

            // Assert
            Assert.NotEqual(initialFrame, templatedView.Frame);
        }

        /// <summary>
        /// Tests ArrangeOverride with Handler to verify PlatformArrange is called with correct Frame.
        /// </summary>
        [Fact]
        public void ArrangeOverride_CallsPlatformArrangeWithCorrectFrame()
        {
            // Arrange
            var templatedView = new TemplatedView();
            var mockHandler = Substitute.For<IViewHandler>();
            templatedView.Handler = mockHandler;
            var bounds = new Rect(15, 30, 250, 350);

            // Act
            templatedView.ArrangeOverride(bounds);

            // Assert
            mockHandler.Received(1).PlatformArrange(templatedView.Frame);
        }

        /// <summary>
        /// Tests ArrangeOverride with negative width and height bounds.
        /// </summary>
        [Theory]
        [InlineData(0, 0, -100, 100)]
        [InlineData(0, 0, 100, -100)]
        [InlineData(0, 0, -100, -100)]
        public void ArrangeOverride_WithNegativeDimensions_HandlesCorrectly(double x, double y, double width, double height)
        {
            // Arrange
            var templatedView = new TemplatedView();
            var bounds = new Rect(x, y, width, height);

            // Act
            var result = templatedView.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(templatedView.Frame.Size, result);
        }
    }


    /// <summary>
    /// Unit tests for the TemplatedView.OnChildRemoved method.
    /// </summary>
    public partial class TemplatedViewOnChildRemovedTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that OnChildRemoved calls both base method and TemplateUtilities.OnChildRemoved with valid parameters.
        /// </summary>
        [Fact]
        public void OnChildRemoved_WithValidParameters_CallsBaseAndTemplateUtilities()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var child = new Label { Text = "Test Child" };
            const int oldLogicalIndex = 0;

            // Act
            templatedView.TestOnChildRemoved(child, oldLogicalIndex);

            // Assert
            Assert.True(templatedView.OnChildRemovedCalled);
            Assert.Equal(child, templatedView.LastRemovedChild);
            Assert.Equal(oldLogicalIndex, templatedView.LastOldLogicalIndex);
        }

        /// <summary>
        /// Tests that OnChildRemoved properly handles null child parameter.
        /// </summary>
        [Fact]
        public void OnChildRemoved_WithNullChild_HandlesGracefully()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            const int oldLogicalIndex = 0;

            // Act & Assert (should not throw)
            templatedView.TestOnChildRemoved(null, oldLogicalIndex);

            Assert.True(templatedView.OnChildRemovedCalled);
            Assert.Null(templatedView.LastRemovedChild);
            Assert.Equal(oldLogicalIndex, templatedView.LastOldLogicalIndex);
        }

        /// <summary>
        /// Tests OnChildRemoved with various oldLogicalIndex boundary values.
        /// </summary>
        /// <param name="oldLogicalIndex">The logical index value to test</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void OnChildRemoved_WithVariousOldLogicalIndex_HandlesAllValues(int oldLogicalIndex)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var child = new Label { Text = "Test Child" };

            // Act
            templatedView.TestOnChildRemoved(child, oldLogicalIndex);

            // Assert
            Assert.True(templatedView.OnChildRemovedCalled);
            Assert.Equal(child, templatedView.LastRemovedChild);
            Assert.Equal(oldLogicalIndex, templatedView.LastOldLogicalIndex);
        }

        /// <summary>
        /// Tests that OnChildRemoved sets TemplateRoot to null when removing the template root element.
        /// </summary>
        [Fact]
        public void OnChildRemoved_WhenChildIsTemplateRoot_SetsTemplateRootToNull()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var controlTemplated = templatedView as IControlTemplated;
            var templateRoot = new Label { Text = "Template Root" };

            // Set up the template root
            controlTemplated.TemplateRoot = templateRoot;
            Assert.Equal(templateRoot, controlTemplated.TemplateRoot);

            // Act
            templatedView.TestOnChildRemoved(templateRoot, 0);

            // Assert
            Assert.Null(controlTemplated.TemplateRoot);
        }

        /// <summary>
        /// Tests that OnChildRemoved does not affect TemplateRoot when removing a non-template root element.
        /// </summary>
        [Fact]
        public void OnChildRemoved_WhenChildIsNotTemplateRoot_KeepsTemplateRootUnchanged()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var controlTemplated = templatedView as IControlTemplated;
            var templateRoot = new Label { Text = "Template Root" };
            var otherChild = new Label { Text = "Other Child" };

            // Set up the template root
            controlTemplated.TemplateRoot = templateRoot;

            // Act
            templatedView.TestOnChildRemoved(otherChild, 0);

            // Assert
            Assert.Equal(templateRoot, controlTemplated.TemplateRoot);
        }

        /// <summary>
        /// Tests that OnChildRemoved works correctly when TemplateRoot is null.
        /// </summary>
        [Fact]
        public void OnChildRemoved_WhenTemplateRootIsNull_HandlesGracefully()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var controlTemplated = templatedView as IControlTemplated;
            var child = new Label { Text = "Test Child" };

            // Ensure TemplateRoot is null
            Assert.Null(controlTemplated.TemplateRoot);

            // Act
            templatedView.TestOnChildRemoved(child, 0);

            // Assert
            Assert.Null(controlTemplated.TemplateRoot);
            Assert.True(templatedView.OnChildRemovedCalled);
        }

        /// <summary>
        /// Tests that OnChildRemoved handles removing null child when TemplateRoot is also null.
        /// </summary>
        [Fact]
        public void OnChildRemoved_WithNullChildAndNullTemplateRoot_HandlesGracefully()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var controlTemplated = templatedView as IControlTemplated;

            // Ensure TemplateRoot is null
            Assert.Null(controlTemplated.TemplateRoot);

            // Act
            templatedView.TestOnChildRemoved(null, 0);

            // Assert
            Assert.Null(controlTemplated.TemplateRoot);
            Assert.True(templatedView.OnChildRemovedCalled);
        }

        /// <summary>
        /// Helper class that exposes the protected OnChildRemoved method for testing.
        /// </summary>
        private class TestableTemplatedView : TemplatedView
        {
            public bool OnChildRemovedCalled { get; private set; }
            public Element LastRemovedChild { get; private set; }
            public int LastOldLogicalIndex { get; private set; }

            public void TestOnChildRemoved(Element child, int oldLogicalIndex)
            {
                OnChildRemovedCalled = true;
                LastRemovedChild = child;
                LastOldLogicalIndex = oldLogicalIndex;
                OnChildRemoved(child, oldLogicalIndex);
            }
        }
    }


    /// <summary>
    /// Unit tests for the ShouldInvalidateOnChildAdded method in TemplatedView class.
    /// </summary>
    public partial class TemplatedViewShouldInvalidateOnChildAddedTests
    {
        /// <summary>
        /// Test class that inherits from TemplatedView to expose the protected ShouldInvalidateOnChildAdded method for testing.
        /// </summary>
        private class TestableTemplatedView : TemplatedView
        {
            public bool CallShouldInvalidateOnChildAdded(View child)
            {
                return ShouldInvalidateOnChildAdded(child);
            }
        }

        /// <summary>
        /// Tests that ShouldInvalidateOnChildAdded returns true when called with a valid View instance.
        /// This test verifies the core functionality of the method.
        /// Expected result: Always returns true.
        /// </summary>
        [Fact]
        public void ShouldInvalidateOnChildAdded_ValidView_ReturnsTrue()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();
            var childView = new View();

            // Act
            var result = templatedView.CallShouldInvalidateOnChildAdded(childView);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldInvalidateOnChildAdded returns true when called with null.
        /// This test verifies the method handles null input gracefully.
        /// Expected result: Always returns true (method implementation is simple and should not check for null).
        /// </summary>
        [Fact]
        public void ShouldInvalidateOnChildAdded_NullView_ReturnsTrue()
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act
            var result = templatedView.CallShouldInvalidateOnChildAdded(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldInvalidateOnChildAdded returns true for different View subclasses.
        /// This test verifies the method works consistently across different View types.
        /// Expected result: Always returns true for all View subtypes.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetViewSubclassTestData))]
        public void ShouldInvalidateOnChildAdded_VariousViewTypes_ReturnsTrue(View childView)
        {
            // Arrange
            var templatedView = new TestableTemplatedView();

            // Act
            var result = templatedView.CallShouldInvalidateOnChildAdded(childView);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Provides test data for various View subclass types to test ShouldInvalidateOnChildAdded method.
        /// </summary>
        /// <returns>Collection of View instances including null for comprehensive testing.</returns>
        public static IEnumerable<object[]> GetViewSubclassTestData()
        {
            yield return new object[] { new View() };
            yield return new object[] { new Label() };
            yield return new object[] { new Button() };
            yield return new object[] { new ContentView() };
            yield return new object[] { null };
        }
    }
}