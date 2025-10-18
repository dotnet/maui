#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class StateTriggerTests
    {
        const string NormalStateName = "Normal";
        const string RedStateName = "Red";
        const string GreenStateName = "Green";

        static readonly Entry TestEntry = new Entry();

        static VisualStateGroupList CreateTestStateGroups()
        {
            var stateGroups = new VisualStateGroupList();
            var visualStateGroup = new VisualStateGroup();

            var normalState = new VisualState { Name = NormalStateName };

            var greenStateTrigger = new CompareStateTrigger { Property = TestEntry.Text, Value = "Test" };
            var greenState = new VisualState { Name = GreenStateName };
            greenState.StateTriggers.Add(greenStateTrigger);

            var redStateTrigger = new CompareStateTrigger { Property = TestEntry.Text, Value = string.Empty };
            var redState = new VisualState { Name = RedStateName };
            redState.StateTriggers.Add(redStateTrigger);

            visualStateGroup.States.Add(normalState);
            visualStateGroup.States.Add(greenState);
            visualStateGroup.States.Add(redState);

            stateGroups.Add(visualStateGroup);

            return stateGroups;
        }

        [Fact]
        public void InitialStateIsNormalIfAvailable()
        {
            var label1 = new Label();

            VisualStateManager.SetVisualStateGroups(label1, CreateTestStateGroups());

            var groups1 = VisualStateManager.GetVisualStateGroups(label1);

            Assert.Equal(NormalStateName, groups1[0].CurrentState.Name);
        }

        [Fact]
        public void StateTriggerDefaultVisualState()
        {
            var grid = new Grid();

            TestEntry.Text = string.Empty;

            grid.Children.Add(TestEntry);

            VisualStateManager.SetVisualStateGroups(grid, CreateTestStateGroups());

            var groups = VisualStateManager.GetVisualStateGroups(grid);

            Assert.Equal(RedStateName, groups[0].CurrentState.Name);
        }

        [Fact]
        public void StateTriggerChangedVisualState()
        {
            var grid = new Grid();

            TestEntry.Text = "Test";

            grid.Children.Add(TestEntry);

            VisualStateManager.SetVisualStateGroups(grid, CreateTestStateGroups());

            var groups = VisualStateManager.GetVisualStateGroups(grid);

            Assert.Equal(GreenStateName, groups[0].CurrentState.Name);
        }

        /// <summary>
        /// Tests that the IsActive property getter returns the default value (false) when no value has been set.
        /// Verifies that the getter correctly retrieves and casts the underlying BindableProperty value.
        /// Expected result: IsActive should return false as the default value.
        /// </summary>
        [Fact]
        public void IsActive_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var stateTrigger = new StateTrigger();

            // Act
            bool result = stateTrigger.IsActive;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsActive property getter returns the correct value after the property has been set.
        /// Verifies that the getter correctly retrieves and casts the underlying BindableProperty value for both true and false cases.
        /// Expected result: IsActive should return the value that was previously set.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsActive_AfterSettingValue_ReturnsCorrectValue(bool expectedValue)
        {
            // Arrange
            var stateTrigger = new StateTrigger();

            // Act - Set the value first, then get it
            stateTrigger.IsActive = expectedValue;
            bool result = stateTrigger.IsActive;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the IsActive property getter consistently returns the same value when called multiple times.
        /// Verifies the stability and consistency of the getter implementation.
        /// Expected result: Multiple calls to IsActive should return the same value.
        /// </summary>
        [Fact]
        public void IsActive_MultipleGets_ReturnsConsistentValue()
        {
            // Arrange
            var stateTrigger = new StateTrigger();
            stateTrigger.IsActive = true;

            // Act
            bool firstCall = stateTrigger.IsActive;
            bool secondCall = stateTrigger.IsActive;
            bool thirdCall = stateTrigger.IsActive;

            // Assert
            Assert.True(firstCall);
            Assert.True(secondCall);
            Assert.True(thirdCall);
            Assert.Equal(firstCall, secondCall);
            Assert.Equal(secondCall, thirdCall);
        }
    }
}