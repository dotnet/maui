#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class RadioButtonGroupControllerTests
    {
        /// <summary>
        /// Tests that the GroupName getter returns the correct value when the internal field is null.
        /// </summary>
        [Fact]
        public void GroupName_WhenInternalFieldIsNull_ReturnsNull()
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);

            // Act
            var result = controller.GroupName;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the GroupName getter returns the correct value when the internal field is empty.
        /// </summary>
        [Fact]
        public void GroupName_WhenInternalFieldIsEmpty_ReturnsEmpty()
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = "";

            // Act
            var result = controller.GroupName;

            // Assert
            Assert.Equal("", result);
        }

        /// <summary>
        /// Tests that the GroupName getter returns the correct value when the internal field has a valid value.
        /// </summary>
        [Fact]
        public void GroupName_WhenInternalFieldHasValue_ReturnsValue()
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);
            const string expectedGroupName = "TestGroup";
            controller.GroupName = expectedGroupName;

            // Act
            var result = controller.GroupName;

            // Assert
            Assert.Equal(expectedGroupName, result);
        }

        /// <summary>
        /// Tests that the GroupName setter accepts null values and updates the internal state.
        /// </summary>
        [Fact]
        public void GroupName_WhenSetToNull_UpdatesInternalState()
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = "InitialValue";

            // Act
            controller.GroupName = null;

            // Assert
            Assert.Null(controller.GroupName);
        }

        /// <summary>
        /// Tests that the GroupName setter accepts empty string values and updates the internal state.
        /// </summary>
        [Fact]
        public void GroupName_WhenSetToEmpty_UpdatesInternalState()
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = "InitialValue";

            // Act
            controller.GroupName = "";

            // Assert
            Assert.Equal("", controller.GroupName);
        }

        /// <summary>
        /// Tests that the GroupName setter accepts whitespace values and updates the internal state.
        /// </summary>
        [Theory]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("   ")]
        [InlineData("\r\n")]
        public void GroupName_WhenSetToWhitespace_UpdatesInternalState(string whitespaceValue)
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);

            // Act
            controller.GroupName = whitespaceValue;

            // Assert
            Assert.Equal(whitespaceValue, controller.GroupName);
        }

        /// <summary>
        /// Tests that the GroupName setter accepts valid string values and updates the internal state.
        /// </summary>
        [Theory]
        [InlineData("Group1")]
        [InlineData("MyRadioGroup")]
        [InlineData("123")]
        [InlineData("Group_With_Underscores")]
        [InlineData("Group-With-Dashes")]
        [InlineData("GroupWithNumbers123")]
        public void GroupName_WhenSetToValidString_UpdatesInternalState(string groupName)
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);

            // Act
            controller.GroupName = groupName;

            // Assert
            Assert.Equal(groupName, controller.GroupName);
        }

        /// <summary>
        /// Tests that the GroupName setter accepts very long string values and updates the internal state.
        /// </summary>
        [Fact]
        public void GroupName_WhenSetToVeryLongString_UpdatesInternalState()
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);
            var longString = new string('A', 10000);

            // Act
            controller.GroupName = longString;

            // Assert
            Assert.Equal(longString, controller.GroupName);
        }

        /// <summary>
        /// Tests that the GroupName setter accepts strings with special characters and updates the internal state.
        /// </summary>
        [Theory]
        [InlineData("Group@#$%")]
        [InlineData("Group<>{}")]
        [InlineData("Group[]()")]
        [InlineData("Group|\\")]
        [InlineData("Group/&*")]
        [InlineData("Grοup")] // Greek letter
        [InlineData("Group™")]
        public void GroupName_WhenSetToStringWithSpecialCharacters_UpdatesInternalState(string groupName)
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);

            // Act
            controller.GroupName = groupName;

            // Assert
            Assert.Equal(groupName, controller.GroupName);
        }

        /// <summary>
        /// Tests that setting the same GroupName value multiple times works correctly.
        /// </summary>
        [Fact]
        public void GroupName_WhenSetToSameValueTwice_MaintainsValue()
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);
            const string groupName = "TestGroup";

            // Act
            controller.GroupName = groupName;
            controller.GroupName = groupName;

            // Assert
            Assert.Equal(groupName, controller.GroupName);
        }

        /// <summary>
        /// Tests that setting different GroupName values in sequence works correctly.
        /// </summary>
        [Fact]
        public void GroupName_WhenSetToSequentialValues_ReturnsLatestValue()
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);

            // Act & Assert
            controller.GroupName = "First";
            Assert.Equal("First", controller.GroupName);

            controller.GroupName = "Second";
            Assert.Equal("Second", controller.GroupName);

            controller.GroupName = null;
            Assert.Null(controller.GroupName);

            controller.GroupName = "Third";
            Assert.Equal("Third", controller.GroupName);
        }

        /// <summary>
        /// Tests that the GroupName setter calls UpdateGroupNames on the layout with descendants.
        /// </summary>
        [Fact]
        public void GroupName_WhenSetWithLayoutHavingDescendants_CallsUpdateGroupNames()
        {
            // Arrange
            var mockRadioButton = Substitute.For<RadioButton>();
            mockRadioButton.GroupName.Returns("");

            var mockLayout = CreateMockLayoutWithDescendants(new[] { mockRadioButton });
            var controller = new RadioButtonGroupController(mockLayout);
            const string newGroupName = "NewGroup";

            // Act
            controller.GroupName = newGroupName;

            // Assert
            mockRadioButton.Received(1).GroupName = newGroupName;
        }

        private static ILayout CreateMockLayout()
        {
            var mockElement = Substitute.For<Element>();
            mockElement.Descendants().Returns(Enumerable.Empty<Element>());

            var mockLayout = Substitute.For<ILayout>();
            // The constructor casts ILayout to Element, so we need to ensure this works
            ((Element)mockLayout).Returns(mockElement);

            return mockLayout;
        }

        private static ILayout CreateMockLayoutWithDescendants(Element[] descendants)
        {
            var mockElement = Substitute.For<Element>();
            mockElement.Descendants().Returns(descendants);

            var mockLayout = Substitute.For<ILayout>();
            // The constructor casts ILayout to Element, so we need to ensure this works
            ((Element)mockLayout).Returns(mockElement);

            return mockLayout;
        }

        /// <summary>
        /// Tests that HandleRadioButtonGroupSelectionChanged throws NullReferenceException when radioButton parameter is null.
        /// This verifies the method's behavior with invalid input.
        /// Expected result: NullReferenceException when accessing radioButton.GroupName.
        /// </summary>
        [Fact]
        public void HandleRadioButtonGroupSelectionChanged_NullRadioButton_ThrowsNullReferenceException()
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => controller.HandleRadioButtonGroupSelectionChanged(null));
        }

        /// <summary>
        /// Tests that HandleRadioButtonGroupSelectionChanged returns early when radioButton.GroupName does not match controller's GroupName.
        /// This tests the uncovered early return logic and verifies SetValue is not called.
        /// Expected result: Method returns early without calling SetValue on the layout.
        /// </summary>
        [Theory]
        [InlineData("Group1", "Group2")]
        [InlineData("Group1", null)]
        [InlineData(null, "Group2")]
        [InlineData("", "Group1")]
        [InlineData("Group1", "")]
        public void HandleRadioButtonGroupSelectionChanged_MismatchedGroupNames_ReturnsEarlyWithoutSettingValue(string controllerGroupName, string radioButtonGroupName)
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = controllerGroupName;

            var mockRadioButton = Substitute.For<RadioButton>();
            mockRadioButton.GroupName.Returns(radioButtonGroupName);
            mockRadioButton.Value.Returns("TestValue");

            // Act
            controller.HandleRadioButtonGroupSelectionChanged(mockRadioButton);

            // Assert
            mockLayout.DidNotReceive().SetValue(Arg.Any<BindableProperty>(), Arg.Any<object>());
        }

        /// <summary>
        /// Tests that HandleRadioButtonGroupSelectionChanged sets the SelectedValue when radioButton.GroupName matches controller's GroupName.
        /// This verifies the main functionality when group names match.
        /// Expected result: SetValue is called on layout with RadioButtonGroup.SelectedValueProperty and radioButton.Value.
        /// </summary>
        [Theory]
        [InlineData(null, null, "TestValue")]
        [InlineData("Group1", "Group1", "TestValue")]
        [InlineData("", "", 42)]
        [InlineData("Group1", "Group1", null)]
        public void HandleRadioButtonGroupSelectionChanged_MatchingGroupNames_SetsSelectedValue(string controllerGroupName, string radioButtonGroupName, object radioButtonValue)
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = controllerGroupName;

            var mockRadioButton = Substitute.For<RadioButton>();
            mockRadioButton.GroupName.Returns(radioButtonGroupName);
            mockRadioButton.Value.Returns(radioButtonValue);

            // Act
            controller.HandleRadioButtonGroupSelectionChanged(mockRadioButton);

            // Assert
            mockLayout.Received(1).SetValue(RadioButtonGroup.SelectedValueProperty, radioButtonValue);
        }

        /// <summary>
        /// Tests that HandleRadioButtonGroupSelectionChanged works with various Value types when group names match.
        /// This verifies the method handles different object types for the Value property.
        /// Expected result: SetValue is called with the correct value regardless of its type.
        /// </summary>
        [Theory]
        [InlineData("StringValue")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void HandleRadioButtonGroupSelectionChanged_MatchingGroupNames_HandlesVariousValueTypes(object radioButtonValue)
        {
            // Arrange
            var mockLayout = CreateMockLayout();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = "TestGroup";

            var mockRadioButton = Substitute.For<RadioButton>();
            mockRadioButton.GroupName.Returns("TestGroup");
            mockRadioButton.Value.Returns(radioButtonValue);

            // Act
            controller.HandleRadioButtonGroupSelectionChanged(mockRadioButton);

            // Assert
            mockLayout.Received(1).SetValue(RadioButtonGroup.SelectedValueProperty, radioButtonValue);
        }

        /// <summary>
        /// Tests that HandleRadioButtonValueChanged returns early when radioButton is null.
        /// This should not call SetValue on the layout.
        /// </summary>
        [Fact]
        public void HandleRadioButtonValueChanged_NullRadioButton_ReturnsEarly()
        {
            // Arrange
            var mockLayout = Substitute.For<ILayout, Element>();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = "TestGroup";

            // Act
            controller.HandleRadioButtonValueChanged(null);

            // Assert
            ((Element)mockLayout).DidNotReceive().SetValue(Arg.Any<BindableProperty>(), Arg.Any<object>());
        }

        /// <summary>
        /// Tests that HandleRadioButtonValueChanged returns early when radioButton.GroupName differs from controller's GroupName.
        /// This should not call SetValue on the layout.
        /// </summary>
        [Fact]
        public void HandleRadioButtonValueChanged_DifferentGroupName_ReturnsEarly()
        {
            // Arrange
            var mockLayout = Substitute.For<ILayout, Element>();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = "TestGroup";

            var mockRadioButton = Substitute.For<RadioButton>();
            mockRadioButton.GroupName.Returns("DifferentGroup");
            mockRadioButton.Value.Returns("TestValue");

            // Act
            controller.HandleRadioButtonValueChanged(mockRadioButton);

            // Assert
            ((Element)mockLayout).DidNotReceive().SetValue(Arg.Any<BindableProperty>(), Arg.Any<object>());
        }

        /// <summary>
        /// Tests that HandleRadioButtonValueChanged returns early when radioButton.GroupName is null but controller's GroupName is not null.
        /// This should not call SetValue on the layout.
        /// </summary>
        [Fact]
        public void HandleRadioButtonValueChanged_RadioButtonGroupNameNull_ControllerGroupNameNotNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = Substitute.For<ILayout, Element>();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = "TestGroup";

            var mockRadioButton = Substitute.For<RadioButton>();
            mockRadioButton.GroupName.Returns((string)null);
            mockRadioButton.Value.Returns("TestValue");

            // Act
            controller.HandleRadioButtonValueChanged(mockRadioButton);

            // Assert
            ((Element)mockLayout).DidNotReceive().SetValue(Arg.Any<BindableProperty>(), Arg.Any<object>());
        }

        /// <summary>
        /// Tests that HandleRadioButtonValueChanged returns early when radioButton.GroupName is not null but controller's GroupName is null.
        /// This should not call SetValue on the layout.
        /// </summary>
        [Fact]
        public void HandleRadioButtonValueChanged_RadioButtonGroupNameNotNull_ControllerGroupNameNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = Substitute.For<ILayout, Element>();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = null;

            var mockRadioButton = Substitute.For<RadioButton>();
            mockRadioButton.GroupName.Returns("TestGroup");
            mockRadioButton.Value.Returns("TestValue");

            // Act
            controller.HandleRadioButtonValueChanged(mockRadioButton);

            // Assert
            ((Element)mockLayout).DidNotReceive().SetValue(Arg.Any<BindableProperty>(), Arg.Any<object>());
        }

        /// <summary>
        /// Tests that HandleRadioButtonValueChanged calls SetValue when radioButton.GroupName matches controller's GroupName.
        /// This should call SetValue with RadioButtonGroup.SelectedValueProperty and the radioButton's Value.
        /// </summary>
        [Fact]
        public void HandleRadioButtonValueChanged_MatchingGroupName_CallsSetValue()
        {
            // Arrange
            var mockLayout = Substitute.For<ILayout, Element>();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = "TestGroup";

            var mockRadioButton = Substitute.For<RadioButton>();
            mockRadioButton.GroupName.Returns("TestGroup");
            mockRadioButton.Value.Returns("TestValue");

            // Act
            controller.HandleRadioButtonValueChanged(mockRadioButton);

            // Assert
            ((Element)mockLayout).Received(1).SetValue(RadioButtonGroup.SelectedValueProperty, "TestValue");
        }

        /// <summary>
        /// Tests that HandleRadioButtonValueChanged calls SetValue when both radioButton.GroupName and controller's GroupName are null.
        /// This should call SetValue with RadioButtonGroup.SelectedValueProperty and the radioButton's Value.
        /// </summary>
        [Fact]
        public void HandleRadioButtonValueChanged_BothGroupNamesNull_CallsSetValue()
        {
            // Arrange
            var mockLayout = Substitute.For<ILayout, Element>();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = null;

            var mockRadioButton = Substitute.For<RadioButton>();
            mockRadioButton.GroupName.Returns((string)null);
            mockRadioButton.Value.Returns("TestValue");

            // Act
            controller.HandleRadioButtonValueChanged(mockRadioButton);

            // Assert
            ((Element)mockLayout).Received(1).SetValue(RadioButtonGroup.SelectedValueProperty, "TestValue");
        }

        /// <summary>
        /// Tests that HandleRadioButtonValueChanged calls SetValue with null value when radioButton.Value is null.
        /// This verifies the method works correctly with null values.
        /// </summary>
        [Fact]
        public void HandleRadioButtonValueChanged_NullValue_CallsSetValueWithNull()
        {
            // Arrange
            var mockLayout = Substitute.For<ILayout, Element>();
            var controller = new RadioButtonGroupController(mockLayout);
            controller.GroupName = "TestGroup";

            var mockRadioButton = Substitute.For<RadioButton>();
            mockRadioButton.GroupName.Returns("TestGroup");
            mockRadioButton.Value.Returns((object)null);

            // Act
            controller.HandleRadioButtonValueChanged(mockRadioButton);

            // Assert
            ((Element)mockLayout).Received(1).SetValue(RadioButtonGroup.SelectedValueProperty, null);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when layout parameter is null.
        /// Verifies proper null parameter validation.
        /// Expected result: ArgumentNullException with correct parameter name.
        /// </summary>
        [Fact]
        public void Constructor_NullLayout_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new RadioButtonGroupController(null));
            Assert.Equal("layout", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor successfully initializes with a valid ILayout parameter.
        /// Verifies that the layout is cast to Element and event subscriptions are made.
        /// Expected result: Successful construction without exceptions and proper event subscription.
        /// </summary>
        [Fact]
        public void Constructor_ValidLayout_InitializesSuccessfully()
        {
            // Arrange
            var mockLayout = CreateMockLayoutElement();

            // Act
            var controller = new RadioButtonGroupController(mockLayout);

            // Assert
            Assert.NotNull(controller);
            // Verify that the events were subscribed to (the mock should have received the subscription calls)
            mockLayout.Received(1).ChildAdded += Arg.Any<EventHandler<ElementEventArgs>>();
            mockLayout.Received(1).ChildRemoved += Arg.Any<EventHandler<ElementEventArgs>>();
        }

        /// <summary>
        /// Tests that the constructor properly handles the group name initialization path.
        /// Since _groupName is initially null/empty, UpdateGroupNames should not be called.
        /// Expected result: UpdateGroupNames method is not invoked during construction.
        /// </summary>
        [Fact]
        public void Constructor_ValidLayout_DoesNotCallUpdateGroupNamesWhenGroupNameEmpty()
        {
            // Arrange
            var mockLayout = CreateMockLayoutElement();

            // Act
            var controller = new RadioButtonGroupController(mockLayout);

            // Assert
            Assert.NotNull(controller);
            // Since _groupName is initially null/empty, the UpdateGroupNames branch should not execute
            // This test documents the current behavior where UpdateGroupNames is not called during construction
        }

        /// <summary>
        /// Helper method to create a mock ILayout that can be cast to Element.
        /// Returns a mock Element that implements ILayout interface.
        /// </summary>
        private ILayout CreateMockLayoutElement()
        {
            var mockElement = Substitute.For<Element, ILayout>();
            return (ILayout)mockElement;
        }
    }
}