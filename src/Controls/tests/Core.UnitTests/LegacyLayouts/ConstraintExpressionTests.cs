#nullable disable

using System;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for the ConstraintExpression class.
    /// </summary>
    public class ConstraintExpressionTests
    {
        /// <summary>
        /// Tests that ProvideValue returns a Constant constraint when Type is Constant.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeConstant_ReturnsConstantConstraint()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.Constant,
                Constant = 42.5
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Constraint>(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns null when Type is RelativeToParent and Property is null.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToParentWithNullProperty_ReturnsNull()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToParent,
                Property = null
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns null when Type is RelativeToParent and Property is empty.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToParentWithEmptyProperty_ReturnsNull()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToParent,
                Property = ""
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns a RelativeToParent constraint when Type is RelativeToParent and Property is valid.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToParentWithValidProperty_ReturnsRelativeToParentConstraint()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToParent,
                Property = "Width",
                Factor = 0.5,
                Constant = 10.0
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Constraint>(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns null when Type is RelativeToView and Property is null.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToViewWithNullProperty_ReturnsNull()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = null,
                ElementName = "TestElement"
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns null when Type is RelativeToView and Property is empty.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToViewWithEmptyProperty_ReturnsNull()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = "",
                ElementName = "TestElement"
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns null when Type is RelativeToView and ElementName is null.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToViewWithNullElementName_ReturnsNull()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = "Width",
                ElementName = null
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns null when Type is RelativeToView and ElementName is empty.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToViewWithEmptyElementName_ReturnsNull()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = "Width",
                ElementName = ""
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns a RelativeToView constraint when IReferenceProvider is available and finds the view.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToViewWithIReferenceProvider_ReturnsRelativeToViewConstraint()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = "Width",
                ElementName = "TestElement",
                Factor = 1.5,
                Constant = 5.0
            };
            var serviceProvider = Substitute.For<IServiceProvider>();
            var referenceProvider = Substitute.For<IReferenceProvider>();
            var testView = new View();

            serviceProvider.GetService<IReferenceProvider>().Returns(referenceProvider);
            referenceProvider.FindByName("TestElement").Returns(testView);

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Constraint>(result);
        }

        /// <summary>
        /// Tests that ProvideValue uses legacy path when IReferenceProvider is null but IProvideValueTarget is available.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToViewWithoutIReferenceProviderButWithIProvideValueTarget_ReturnsRelativeToViewConstraint()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = "Height",
                ElementName = "TestElement"
            };
            var serviceProvider = Substitute.For<IServiceProvider>();
            var valueProvider = Substitute.For<IProvideValueTarget>();
            var nameScope = Substitute.For<INameScope>();
            var testView = new View();

            serviceProvider.GetService<IReferenceProvider>().Returns((IReferenceProvider)null);
            serviceProvider.GetService<IProvideValueTarget>().Returns(valueProvider);
            valueProvider.TargetObject.Returns(nameScope);
            nameScope.FindByName<View>("TestElement").Returns(testView);

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Constraint>(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns null when using legacy path but IProvideValueTarget is null.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToViewLegacyPathWithNullIProvideValueTarget_ReturnsNull()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = "Width",
                ElementName = "TestElement"
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            serviceProvider.GetService<IReferenceProvider>().Returns((IReferenceProvider)null);
            serviceProvider.GetService<IProvideValueTarget>().Returns((IProvideValueTarget)null);

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns null when using legacy path but TargetObject is not INameScope.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToViewLegacyPathWithNonINameScopeTargetObject_ReturnsNull()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = "Width",
                ElementName = "TestElement"
            };
            var serviceProvider = Substitute.For<IServiceProvider>();
            var valueProvider = Substitute.For<IProvideValueTarget>();
            var nonNameScopeObject = new object();

            serviceProvider.GetService<IReferenceProvider>().Returns((IReferenceProvider)null);
            serviceProvider.GetService<IProvideValueTarget>().Returns(valueProvider);
            valueProvider.TargetObject.Returns(nonNameScopeObject);

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ProvideValue throws InvalidOperationException when Property doesn't exist on View.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToParentWithInvalidProperty_ThrowsInvalidOperationException()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToParent,
                Property = "NonExistentProperty"
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => expression.ProvideValue(serviceProvider));
        }

        /// <summary>
        /// Tests that ProvideValue throws InvalidOperationException when Property doesn't exist on View for RelativeToView.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToViewWithInvalidProperty_ThrowsInvalidOperationException()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = "NonExistentProperty",
                ElementName = "TestElement"
            };
            var serviceProvider = Substitute.For<IServiceProvider>();
            var referenceProvider = Substitute.For<IReferenceProvider>();
            var testView = new View();

            serviceProvider.GetService<IReferenceProvider>().Returns(referenceProvider);
            referenceProvider.FindByName("TestElement").Returns(testView);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => expression.ProvideValue(serviceProvider));
        }

        /// <summary>
        /// Tests that ProvideValue works with various valid View properties for RelativeToParent.
        /// </summary>
        [Theory]
        [InlineData("Width")]
        [InlineData("Height")]
        [InlineData("X")]
        [InlineData("Y")]
        public void ProvideValue_TypeRelativeToParentWithValidProperties_ReturnsConstraint(string propertyName)
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToParent,
                Property = propertyName,
                Factor = 2.0,
                Constant = -5.0
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Constraint>(result);
        }

        /// <summary>
        /// Tests that ProvideValue works with various valid View properties for RelativeToView.
        /// </summary>
        [Theory]
        [InlineData("Width")]
        [InlineData("Height")]
        [InlineData("X")]
        [InlineData("Y")]
        public void ProvideValue_TypeRelativeToViewWithValidProperties_ReturnsConstraint(string propertyName)
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = propertyName,
                ElementName = "TestElement",
                Factor = 0.75,
                Constant = 2.5
            };
            var serviceProvider = Substitute.For<IServiceProvider>();
            var referenceProvider = Substitute.For<IReferenceProvider>();
            var testView = new View();

            serviceProvider.GetService<IReferenceProvider>().Returns(referenceProvider);
            referenceProvider.FindByName("TestElement").Returns(testView);

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Constraint>(result);
        }

        /// <summary>
        /// Tests that ProvideValue works with extreme Factor and Constant values.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(0.0, 0.0)]
        [InlineData(-1.0, 1.0)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
        public void ProvideValue_TypeConstantWithExtremeValues_ReturnsConstraint(double factor, double constant)
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.Constant,
                Factor = factor,
                Constant = constant
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Constraint>(result);
        }

        /// <summary>
        /// Tests that ProvideValue handles NaN values for Factor and Constant.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeConstantWithNaNValues_ReturnsConstraint()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.Constant,
                Factor = double.NaN,
                Constant = double.NaN
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Constraint>(result);
        }

        /// <summary>
        /// Tests that ProvideValue handles whitespace-only Property and ElementName values.
        /// </summary>
        [Theory]
        [InlineData("   ", "TestElement")]
        [InlineData("Width", "   ")]
        [InlineData("   ", "   ")]
        public void ProvideValue_TypeRelativeToViewWithWhitespaceValues_ReturnsNull(string property, string elementName)
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = property,
                ElementName = elementName
            };
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns null when IReferenceProvider returns null for FindByName.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToViewWhenFindByNameReturnsNull_ReturnsRelativeToViewConstraintWithNullView()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = "Width",
                ElementName = "NonExistentElement"
            };
            var serviceProvider = Substitute.For<IServiceProvider>();
            var referenceProvider = Substitute.For<IReferenceProvider>();

            serviceProvider.GetService<IReferenceProvider>().Returns(referenceProvider);
            referenceProvider.FindByName("NonExistentElement").Returns((object)null);

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Constraint>(result);
        }

        /// <summary>
        /// Tests that ProvideValue returns null when legacy path INameScope.FindByName returns null.
        /// </summary>
        [Fact]
        public void ProvideValue_TypeRelativeToViewLegacyPathWhenFindByNameReturnsNull_ReturnsRelativeToViewConstraintWithNullView()
        {
            // Arrange
            var expression = new ConstraintExpression
            {
                Type = ConstraintType.RelativeToView,
                Property = "Width",
                ElementName = "NonExistentElement"
            };
            var serviceProvider = Substitute.For<IServiceProvider>();
            var valueProvider = Substitute.For<IProvideValueTarget>();
            var nameScope = Substitute.For<INameScope>();

            serviceProvider.GetService<IReferenceProvider>().Returns((IReferenceProvider)null);
            serviceProvider.GetService<IProvideValueTarget>().Returns(valueProvider);
            valueProvider.TargetObject.Returns(nameScope);
            nameScope.FindByName<View>("NonExistentElement").Returns((View)null);

            // Act
            var result = expression.ProvideValue(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Constraint>(result);
        }

        /// <summary>
        /// Tests that the ConstraintExpression constructor properly initializes the object
        /// with Factor set to 1.0 and other properties to their default values.
        /// </summary>
        [Fact]
        public void Constructor_WhenCalled_InitializesFactorToOneAndOtherPropertiesToDefaults()
        {
            // Arrange & Act
            var constraintExpression = new ConstraintExpression();

            // Assert
            Assert.NotNull(constraintExpression);
            Assert.Equal(1.0, constraintExpression.Factor);
            Assert.Equal(0.0, constraintExpression.Constant);
            Assert.Null(constraintExpression.ElementName);
            Assert.Null(constraintExpression.Property);
            Assert.Equal(ConstraintType.RelativeToParent, constraintExpression.Type);
        }
    }
}