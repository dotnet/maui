#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

using static Microsoft.Maui.Controls.Core.UnitTests.VisualStateTestHelpers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class SwitchUnitTests : BaseTestFixture
    {
        const string CommonStatesName = "CommonStates";
        const string DisabledStateName = "Disabled";
        const string FocusedStateName = "Focused";
        const string NormalStateName = "Normal";
        const string OnStateName = "On";
        const string OffStateName = "Off";

        static VisualStateGroupList CreateTestStateGroups()
        {
            var stateGroups = new VisualStateGroupList();
            var visualStateGroup = new VisualStateGroup { Name = CommonStatesName };
            var disabledState = new VisualState { Name = DisabledStateName };
            var focusedState = new VisualState { Name = FocusedStateName };
            var normalState = new VisualState { Name = NormalStateName };
            var onState = new VisualState { Name = OnStateName };
            var offState = new VisualState { Name = OffStateName };

            visualStateGroup.States.Add(disabledState);
            visualStateGroup.States.Add(focusedState);
            visualStateGroup.States.Add(normalState);
            visualStateGroup.States.Add(onState);
            visualStateGroup.States.Add(offState);

            stateGroups.Add(visualStateGroup);

            return stateGroups;
        }

        static VisualStateGroupList CreateTestStateGroupsWithoutOnOffStates()
        {
            var stateGroups = new VisualStateGroupList();
            var visualStateGroup = new VisualStateGroup { Name = CommonStatesName };
            var disabledState = new VisualState { Name = DisabledStateName };
            var focusedState = new VisualState { Name = FocusedStateName };
            var normalState = new VisualState { Name = NormalStateName };

            visualStateGroup.States.Add(disabledState);
            visualStateGroup.States.Add(focusedState);
            visualStateGroup.States.Add(normalState);

            stateGroups.Add(visualStateGroup);

            return stateGroups;
        }

        static VisualStateGroupList CreateTestStateGroupsWithoutNormalState()
        {
            var stateGroups = new VisualStateGroupList();
            var visualStateGroup = new VisualStateGroup { Name = CommonStatesName };
            var disabledState = new VisualState { Name = DisabledStateName };

            visualStateGroup.States.Add(disabledState);

            stateGroups.Add(visualStateGroup);

            return stateGroups;
        }

        [Fact]
        public void TestConstructor()
        {
            Switch sw = new Switch();

            Assert.False(sw.IsToggled);
        }

        [Fact]
        public void TestOnEvent()
        {
            Switch sw = new Switch();

            bool fired = false;
            sw.Toggled += (sender, e) => fired = true;

            sw.IsToggled = true;

            Assert.True(fired);
        }

        [Fact]
        public void TestOnEventNotDoubleFired()
        {
            var sw = new Switch();

            bool fired = false;
            sw.IsToggled = true;

            sw.Toggled += (sender, args) => fired = true;
            sw.IsToggled = true;

            Assert.False(fired);
        }

        [Fact]
        public void VisualStateIsDisabledIfSwitchIsDisabled()
        {
            var switch1 = new Switch();
            switch1.IsEnabled = false;
            VisualStateManager.SetVisualStateGroups(switch1, CreateTestStateGroups());
            var groups1 = VisualStateManager.GetVisualStateGroups(switch1);
            Assert.Equal(DisabledStateName, groups1[0].CurrentState.Name);
        }

        [Fact]
        public void VisualStateIsOnIfAvailableAndSwitchIsEnabledAndOn()
        {
            var switch1 = new Switch();
            switch1.IsEnabled = true;
            switch1.IsToggled = true;
            VisualStateManager.SetVisualStateGroups(switch1, CreateTestStateGroups());
            var groups1 = VisualStateManager.GetVisualStateGroups(switch1);
            Assert.Equal(OnStateName, groups1[0].CurrentState.Name);
        }

        [Fact]
        public void VisualStateIsOffIfAvailableAndSwitchIsEnabledAndOff()
        {
            var switch1 = new Switch();
            switch1.IsEnabled = true;
            switch1.IsToggled = false;
            VisualStateManager.SetVisualStateGroups(switch1, CreateTestStateGroups());
            var groups1 = VisualStateManager.GetVisualStateGroups(switch1);
            Assert.Equal(OffStateName, groups1[0].CurrentState.Name);
        }

        [Fact]
        public void InitialStateIsNormalIfAvailableButOnOffNotAvailable()
        {
            var switch1 = new Switch();
            VisualStateManager.SetVisualStateGroups(switch1, CreateTestStateGroupsWithoutOnOffStates());
            var groups1 = VisualStateManager.GetVisualStateGroups(switch1);
            Assert.Equal(NormalStateName, groups1[0].CurrentState.Name);
        }

        [Fact]
        public void InitialStateIsNullIfNormalOnOffNotAvailable()
        {
            var switch1 = new Switch();
            VisualStateManager.SetVisualStateGroups(switch1, CreateTestStateGroupsWithoutNormalState());
            var groups1 = VisualStateManager.GetVisualStateGroups(switch1);
            Assert.Null(groups1[0].CurrentState);
        }

        [Fact]
        public void OnOffVisualStates()
        {
            var vsgList = VisualStateTestHelpers.CreateTestStateGroups();
            var stateGroup = vsgList[0];
            var element = new Switch();
            VisualStateManager.SetVisualStateGroups(element, vsgList);

            string onStateName = Switch.SwitchOnVisualState;
            string offStateName = Switch.SwitchOffVisualState;
            var onState = new VisualState() { Name = onStateName };
            var offState = new VisualState() { Name = offStateName };

            stateGroup.States.Add(onState);
            stateGroup.States.Add(offState);

            element.IsToggled = true;
            Assert.Equal(stateGroup.CurrentState.Name, onStateName);

            element.IsToggled = false;
            Assert.Equal(stateGroup.CurrentState.Name, offStateName);
        }
    }

    public partial class SwitchTests
    {
        /// <summary>
        /// Tests that the OffColor property returns the default value (null) when not explicitly set.
        /// Verifies the getter retrieves the correct default value from the bindable property.
        /// </summary>
        [Fact]
        public void OffColor_DefaultValue_ReturnsNull()
        {
            // Arrange
            var switchControl = new Switch();

            // Act
            var result = switchControl.OffColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the OffColor property setter and getter work correctly with various Color values.
        /// Verifies that the value set is properly stored and retrieved through the bindable property system.
        /// </summary>
        [Theory]
        [InlineData(0f, 0f, 0f, 1f)] // Black
        [InlineData(1f, 1f, 1f, 1f)] // White
        [InlineData(1f, 0f, 0f, 1f)] // Red
        [InlineData(0f, 1f, 0f, 1f)] // Green
        [InlineData(0f, 0f, 1f, 1f)] // Blue
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(0f, 0f, 0f, 0f)] // Transparent
        public void OffColor_SetValidColor_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var switchControl = new Switch();
            var color = new Color(red, green, blue, alpha);

            // Act
            switchControl.OffColor = color;
            var result = switchControl.OffColor;

            // Assert
            Assert.Equal(color.Red, result.Red);
            Assert.Equal(color.Green, result.Green);
            Assert.Equal(color.Blue, result.Blue);
            Assert.Equal(color.Alpha, result.Alpha);
        }

        /// <summary>
        /// Tests that the OffColor property can be set to null and returns null when retrieved.
        /// Verifies null handling in both setter and getter operations.
        /// </summary>
        [Fact]
        public void OffColor_SetNull_ReturnsNull()
        {
            // Arrange
            var switchControl = new Switch();
            switchControl.OffColor = new Color(1f, 0f, 0f); // Set initial non-null value

            // Act
            switchControl.OffColor = null;
            var result = switchControl.OffColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the OffColor property maintains its value across multiple get operations.
        /// Verifies the consistency of the getter behavior and proper value retention.
        /// </summary>
        [Fact]
        public void OffColor_MultipleGets_ReturnsSameValue()
        {
            // Arrange
            var switchControl = new Switch();
            var expectedColor = new Color(0.7f, 0.3f, 0.9f, 0.8f);
            switchControl.OffColor = expectedColor;

            // Act
            var result1 = switchControl.OffColor;
            var result2 = switchControl.OffColor;
            var result3 = switchControl.OffColor;

            // Assert
            Assert.Equal(expectedColor.Red, result1.Red);
            Assert.Equal(expectedColor.Green, result1.Green);
            Assert.Equal(expectedColor.Blue, result1.Blue);
            Assert.Equal(expectedColor.Alpha, result1.Alpha);

            Assert.Equal(result1.Red, result2.Red);
            Assert.Equal(result1.Green, result2.Green);
            Assert.Equal(result1.Blue, result2.Blue);
            Assert.Equal(result1.Alpha, result2.Alpha);

            Assert.Equal(result2.Red, result3.Red);
            Assert.Equal(result2.Green, result3.Green);
            Assert.Equal(result2.Blue, result3.Blue);
            Assert.Equal(result2.Alpha, result3.Alpha);
        }

        /// <summary>
        /// Tests that the OffColor property can be overwritten with different values.
        /// Verifies that subsequent sets properly update the stored value and getter returns the latest value.
        /// </summary>
        [Fact]
        public void OffColor_OverwriteValue_ReturnsLatestValue()
        {
            // Arrange
            var switchControl = new Switch();
            var initialColor = new Color(1f, 0f, 0f); // Red
            var updatedColor = new Color(0f, 1f, 0f); // Green

            // Act
            switchControl.OffColor = initialColor;
            var firstResult = switchControl.OffColor;

            switchControl.OffColor = updatedColor;
            var secondResult = switchControl.OffColor;

            // Assert
            Assert.Equal(initialColor.Red, firstResult.Red);
            Assert.Equal(initialColor.Green, firstResult.Green);
            Assert.Equal(initialColor.Blue, firstResult.Blue);
            Assert.Equal(initialColor.Alpha, firstResult.Alpha);

            Assert.Equal(updatedColor.Red, secondResult.Red);
            Assert.Equal(updatedColor.Green, secondResult.Green);
            Assert.Equal(updatedColor.Blue, secondResult.Blue);
            Assert.Equal(updatedColor.Alpha, secondResult.Alpha);
        }

        /// <summary>
        /// Tests the OffColor property with extreme boundary values for color components.
        /// Verifies proper handling of minimum and maximum float values within the valid Color range.
        /// </summary>
        [Theory]
        [InlineData(0f, 0f, 0f, 0f)] // All minimum values
        [InlineData(1f, 1f, 1f, 1f)] // All maximum values
        [InlineData(0f, 1f, 0f, 1f)] // Mixed boundary values
        [InlineData(1f, 0f, 1f, 0f)] // Mixed boundary values
        public void OffColor_BoundaryValues_HandledCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var switchControl = new Switch();
            var boundaryColor = new Color(red, green, blue, alpha);

            // Act
            switchControl.OffColor = boundaryColor;
            var result = switchControl.OffColor;

            // Assert
            Assert.Equal(red, result.Red);
            Assert.Equal(green, result.Green);
            Assert.Equal(blue, result.Blue);
            Assert.Equal(alpha, result.Alpha);
        }
    }
}