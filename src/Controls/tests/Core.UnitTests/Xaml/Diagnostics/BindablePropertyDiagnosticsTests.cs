using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public partial class ValueSourceTests
{
    /// <summary>
    /// Tests the ValueSource constructor with invalid enum values (outside defined range).
    /// Verifies that the constructor accepts cast enum values and assigns them correctly.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(12)]
    [InlineData(100)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Constructor_WithInvalidEnumValues_AcceptsAndAssignsValues(int invalidEnumValue)
    {
        // Arrange
        var invalidBaseValueSource = (BaseValueSource)invalidEnumValue;

        // Act
        var valueSource = new ValueSource(invalidBaseValueSource);

        // Assert
        Assert.Equal(invalidBaseValueSource, valueSource.BaseValueSource);
        Assert.False(valueSource.IsCoerced);
        Assert.False(valueSource.IsCurrent);
        Assert.False(valueSource.IsExpression);
    }

}

/// <summary>
/// Unit tests for the BindablePropertyDiagnostics.GetValueSource method
/// </summary>
public partial class BindablePropertyDiagnosticsTests
{
    /// <summary>
    /// Tests that GetValueSource throws ArgumentNullException when bindable parameter is null
    /// </summary>
    [Fact]
    public void GetValueSource_NullBindable_ThrowsArgumentNullException()
    {
        // Arrange
        BindableObject bindable = null;
        var property = Substitute.For<BindableProperty>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            BindablePropertyDiagnostics.GetValueSource(bindable, property));
        Assert.Equal("bindable", exception.ParamName);
    }

    /// <summary>
    /// Tests that GetValueSource throws ArgumentNullException when property parameter is null
    /// </summary>
    [Fact]
    public void GetValueSource_NullProperty_ThrowsArgumentNullException()
    {
        // Arrange
        var bindable = Substitute.For<BindableObject>();
        BindableProperty property = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            BindablePropertyDiagnostics.GetValueSource(bindable, property));
        Assert.Equal("property", exception.ParamName);
    }

    /// <summary>
    /// Tests that GetValueSource throws ArgumentNullException when both parameters are null, with bindable checked first
    /// </summary>
    [Fact]
    public void GetValueSource_BothParametersNull_ThrowsArgumentNullExceptionForBindable()
    {
        // Arrange
        BindableObject bindable = null;
        BindableProperty property = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            BindablePropertyDiagnostics.GetValueSource(bindable, property));
        Assert.Equal("bindable", exception.ParamName);
    }

    /// <summary>
    /// Tests that GetValueSource returns Unknown BaseValueSource when context is null
    /// </summary>
    [Fact]
    public void GetValueSource_NullContext_ReturnsUnknownValueSource()
    {
        // Arrange
        var bindable = Substitute.For<BindableObject>();
        var property = Substitute.For<BindableProperty>();
        bindable.GetContext(property).Returns((BindableObject.BindablePropertyContext)null);

        // Act
        var result = BindablePropertyDiagnostics.GetValueSource(bindable, property);

        // Assert
        Assert.Equal(BaseValueSource.Unknown, result.BaseValueSource);
        Assert.False(result.IsExpression);
        Assert.False(result.IsCurrent);
        Assert.False(result.IsCoerced);
    }

    /// <summary>
    /// Tests GetValueSource behavior when IsVsm property is true
    /// </summary>
    [Fact]
    public void GetValueSource_IsVsmTrue_ReturnsLocalValueSource()
    {
        // Arrange
        var bindable = Substitute.For<BindableObject>();
        var property = Substitute.For<BindableProperty>();
        var context = new BindableObject.BindablePropertyContext
        {
            Property = property
        };

        // Create a specificity with VSM flag set
        var vsmSpecificity = new SetterSpecificity(1, 0, 0, 0, 0, 0, 0, 0); // extras = 1 for VSM
        context.Values.Add(vsmSpecificity, new object());
        bindable.GetContext(property).Returns(context);

        // Act
        var result = BindablePropertyDiagnostics.GetValueSource(bindable, property);

        // Assert
        Assert.Equal(BaseValueSource.Local, result.BaseValueSource);
        Assert.False(result.IsExpression);
        Assert.False(result.IsCurrent);
    }

    /// <summary>
    /// Tests GetValueSource behavior when style info has non-zero values
    /// </summary>
    [Theory]
    [InlineData(1, 0, 0, 0, "sStyle > 0")]
    [InlineData(0, 1, 0, 0, "sId > 0")]
    [InlineData(0, 0, 1, 0, "sClass > 0")]
    [InlineData(0, 0, 0, 1, "sType > 0")]
    [InlineData(1, 1, 1, 1, "All style components > 0")]
    public void GetValueSource_StyleInfoNonZero_ReturnsStyleValueSource(
        ushort sStyle, byte sId, byte sClass, byte sType, string testCase)
    {
        // Arrange
        var bindable = Substitute.For<BindableObject>();
        var property = Substitute.For<BindableProperty>();
        var context = new BindableObject.BindablePropertyContext
        {
            Property = property
        };

        var specificity = new SetterSpecificity(sStyle, sId, sClass, sType);
        context.Values.Add(specificity, new object());
        bindable.GetContext(property).Returns(context);

        // Act
        var result = BindablePropertyDiagnostics.GetValueSource(bindable, property);

        // Assert
        Assert.Equal(BaseValueSource.Style, result.BaseValueSource);
        Assert.False(result.IsExpression);
        Assert.False(result.IsCurrent);
    }

    /// <summary>
    /// Tests GetValueSource behavior when IsManual property is true
    /// </summary>
    [Fact]
    public void GetValueSource_IsManualTrue_ReturnsLocalValueSource()
    {
        // Arrange
        var bindable = Substitute.For<BindableObject>();
        var property = Substitute.For<BindableProperty>();
        var context = new BindableObject.BindablePropertyContext
        {
            Property = property
        };

        // Create a manual specificity (manual = 1)
        var manualSpecificity = new SetterSpecificity(0, 1, 0, 0, 0, 0, 0, 0);
        context.Values.Add(manualSpecificity, new object());
        bindable.GetContext(property).Returns(context);

        // Act
        var result = BindablePropertyDiagnostics.GetValueSource(bindable, property);

        // Assert
        Assert.Equal(BaseValueSource.Local, result.BaseValueSource);
        Assert.False(result.IsExpression);
        Assert.False(result.IsCurrent);
    }

    /// <summary>
    /// Tests GetValueSource behavior when IsDynamicResource property is true
    /// </summary>
    [Fact]
    public void GetValueSource_IsDynamicResourceTrue_ReturnsUnknownWithExpressionValueSource()
    {
        // Arrange
        var bindable = Substitute.For<BindableObject>();
        var property = Substitute.For<BindableProperty>();
        var context = new BindableObject.BindablePropertyContext
        {
            Property = property
        };

        // Create a dynamic resource specificity
        var dynamicResourceSpecificity = new SetterSpecificity(0, 0, 1, 0, 0, 0, 0, 0);
        context.Values.Add(dynamicResourceSpecificity, new object());
        bindable.GetContext(property).Returns(context);

        // Act
        var result = BindablePropertyDiagnostics.GetValueSource(bindable, property);

        // Assert
        Assert.Equal(BaseValueSource.Unknown, result.BaseValueSource);
        Assert.True(result.IsExpression);
        Assert.False(result.IsCurrent);
    }

    /// <summary>
    /// Tests GetValueSource behavior when IsBinding property is true
    /// </summary>
    [Fact]
    public void GetValueSource_IsBindingTrue_ReturnsUnknownWithExpressionValueSource()
    {
        // Arrange
        var bindable = Substitute.For<BindableObject>();
        var property = Substitute.For<BindableProperty>();
        var context = new BindableObject.BindablePropertyContext
        {
            Property = property
        };

        // Create a binding specificity
        var bindingSpecificity = new SetterSpecificity(0, 0, 0, 1, 0, 0, 0, 0);
        context.Values.Add(bindingSpecificity, new object());
        bindable.GetContext(property).Returns(context);

        // Act
        var result = BindablePropertyDiagnostics.GetValueSource(bindable, property);

        // Assert
        Assert.Equal(BaseValueSource.Unknown, result.BaseValueSource);
        Assert.True(result.IsExpression);
        Assert.False(result.IsCurrent);
    }

    /// <summary>
    /// Tests GetValueSource fallback behavior when no specific conditions are met
    /// </summary>
    [Fact]
    public void GetValueSource_NoSpecificConditions_ReturnsUnknownValueSource()
    {
        // Arrange
        var bindable = Substitute.For<BindableObject>();
        var property = Substitute.For<BindableProperty>();
        var context = new BindableObject.BindablePropertyContext
        {
            Property = property
        };

        // Create a default specificity that doesn't match any specific conditions
        var defaultSpecificity = new SetterSpecificity();
        context.Values.Add(defaultSpecificity, new object());
        bindable.GetContext(property).Returns(context);

        // Act
        var result = BindablePropertyDiagnostics.GetValueSource(bindable, property);

        // Assert
        Assert.Equal(BaseValueSource.Unknown, result.BaseValueSource);
        Assert.False(result.IsExpression);
        Assert.False(result.IsCurrent);
    }

    /// <summary>
    /// Provides test cases for different SetterSpecificity values and their expected results
    /// </summary>
    public static IEnumerable<object[]> GetSetterSpecificityTestCases()
    {
        yield return new object[]
        {
                SetterSpecificity.DefaultValue,
                BaseValueSource.Default,
                false, false,
                "DefaultValue specificity"
        };

        yield return new object[]
        {
                SetterSpecificity.FromBinding,
                BaseValueSource.Unknown,
                true, false,
                "FromBinding specificity"
        };

        yield return new object[]
        {
                SetterSpecificity.ManualValueSetter,
                BaseValueSource.Local,
                false, false,
                "ManualValueSetter specificity"
        };

        yield return new object[]
        {
                SetterSpecificity.DynamicResourceSetter,
                BaseValueSource.Unknown,
                true, false,
                "DynamicResourceSetter specificity"
        };

        yield return new object[]
        {
                SetterSpecificity.VisualStateSetter,
                BaseValueSource.Style,
                false, false,
                "VisualStateSetter specificity"
        };

        yield return new object[]
        {
                SetterSpecificity.Trigger,
                BaseValueSource.StyleTrigger,
                false, false,
                "Trigger specificity"
        };

        yield return new object[]
        {
                SetterSpecificity.FromHandler,
                BaseValueSource.Unknown,
                false, true,
                "FromHandler specificity"
        };
    }
}