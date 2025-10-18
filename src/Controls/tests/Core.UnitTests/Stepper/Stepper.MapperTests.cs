using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

public partial class StepperTests
{
    /// <summary>
    /// Tests that MapInterval calls UpdateValue on the handler with the correct property name when valid parameters are provided.
    /// </summary>
    [Fact]
    public void MapInterval_ValidParameters_CallsUpdateValueWithIntervalPropertyName()
    {
        // Arrange
        var handler = Substitute.For<IStepperHandler>();
        var stepper = Substitute.For<IStepper>();

        // Act
        Stepper.MapInterval(handler, stepper);

        // Assert
        handler.Received(1).UpdateValue("Interval");
    }

    /// <summary>
    /// Tests that MapInterval throws NullReferenceException when handler parameter is null.
    /// </summary>
    [Fact]
    public void MapInterval_NullHandler_ThrowsNullReferenceException()
    {
        // Arrange
        IStepperHandler handler = null;
        var stepper = Substitute.For<IStepper>();

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => Stepper.MapInterval(handler, stepper));
    }

    /// <summary>
    /// Tests that MapInterval works correctly when stepper parameter is null since stepper is not used in the method body.
    /// </summary>
    [Fact]
    public void MapInterval_NullStepper_CallsUpdateValueWithIntervalPropertyName()
    {
        // Arrange
        var handler = Substitute.For<IStepperHandler>();
        IStepper stepper = null;

        // Act
        Stepper.MapInterval(handler, stepper);

        // Assert
        handler.Received(1).UpdateValue("Interval");
    }

    /// <summary>
    /// Tests that MapInterval passes the exact string "Interval" to UpdateValue method.
    /// </summary>
    [Fact]
    public void MapInterval_ValidParameters_PassesCorrectPropertyNameToUpdateValue()
    {
        // Arrange
        var handler = Substitute.For<IStepperHandler>();
        var stepper = Substitute.For<IStepper>();

        // Act
        Stepper.MapInterval(handler, stepper);

        // Assert
        handler.Received().UpdateValue(Arg.Is<string>(s => s == "Interval"));
        handler.DidNotReceive().UpdateValue(Arg.Is<string>(s => s != "Interval"));
    }
}
