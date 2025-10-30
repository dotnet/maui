using Xunit;
using static Microsoft.Maui.Controls.Core.UnitTests.VisualStateTestHelpers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class CheckBoxUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			var checkBox = new CheckBox();

			Assert.False(checkBox.IsChecked);
		}

		[Fact]
		public void TestOnEvent()
		{
			var checkBox = new CheckBox();

			var fired = false;
			checkBox.CheckedChanged += (sender, e) => fired = true;

			checkBox.IsChecked = true;

			Assert.True(fired);
		}

		[Fact]
		public void TestOnEventNotDoubleFired()
		{
			var checkBox = new CheckBox();

			bool fired = false;
			checkBox.IsChecked = true;

			checkBox.CheckedChanged += (sender, args) => fired = true;
			checkBox.IsChecked = true;

			Assert.False(fired);
		}

		[Fact]
		public void CheckedChangedEventArgs_ShouldHaveCorrectValue()
		{
			var checkBox = new CheckBox();
			CheckedChangedEventArgs eventArgs = null;

			checkBox.CheckedChanged += (sender, e) => eventArgs = e;

			// Test changing from false to true
			checkBox.IsChecked = true;
			Assert.NotNull(eventArgs);
			Assert.True(eventArgs.Value);

			// Test changing from true to false
			checkBox.IsChecked = false;
			Assert.False(eventArgs.Value);
		}

		[Fact]
		public void CheckedChangedEvent_ShouldFireOnlyWhenValueChanges()
		{
			var checkBox = new CheckBox();
			int eventFireCount = 0;

			checkBox.CheckedChanged += (sender, e) => eventFireCount++;

			// Set to same value should not fire event
			checkBox.IsChecked = false;
			Assert.Equal(0, eventFireCount);

			// Change value should fire event
			checkBox.IsChecked = true;
			Assert.Equal(1, eventFireCount);

			// Set to same value again should not fire event
			checkBox.IsChecked = true;
			Assert.Equal(1, eventFireCount);

			// Change back should fire event
			checkBox.IsChecked = false;
			Assert.Equal(2, eventFireCount);
		}

		[Fact]
		public void CheckedVisualStates()
		{
			var vsgList = CreateTestStateGroups();
			string checkedStateName = CheckBox.IsCheckedVisualState;
			var checkedState = new VisualState() { Name = checkedStateName };
			var stateGroup = vsgList[0];
			stateGroup.States.Add(checkedState);

			var element = new CheckBox();
			VisualStateManager.SetVisualStateGroups(element, vsgList);

			element.IsChecked = true;
			Assert.Equal(checkedStateName, stateGroup.CurrentState.Name);

			element.IsChecked = false;
			Assert.NotEqual(checkedStateName, stateGroup.CurrentState.Name);
		}

		[Fact]
		public void CheckBoxClickWhenCommandCanExecuteFalse()
		{
			bool invoked = false;
			var checkBox = new CheckBox()
			{
				Command = new Command(() => invoked = true, () => false),
				IsChecked = false
			};

			checkBox.IsChecked = true;

			Assert.False(invoked);
		}

		[Fact]
		public void CheckBoxClickWhenCommandCanExecuteTrue()
		{
			bool invoked = false;
			var checkBox = new CheckBox()
			{
				Command = new Command(() => invoked = true, () => true),
				IsChecked = false
			};

			checkBox.IsChecked = true;

			Assert.True(invoked);
		}

		[Fact]
		public void Command_ShouldExecuteWithCorrectParameter()
		{
			object receivedParameter = null;
			var expectedParameter = "TestParameter";

			var checkBox = new CheckBox()
			{
				Command = new Command<object>(param => receivedParameter = param, param => true),
				CommandParameter = expectedParameter,
				IsChecked = false
			};

			checkBox.IsChecked = true;

			Assert.Equal(expectedParameter, receivedParameter);
		}

		[Fact]
		public void Command_ShouldNotExecuteWhenNull()
		{
			var checkBox = new CheckBox()
			{
				Command = null,
				IsChecked = false
			};

			// Should not throw exception when command is null
			checkBox.IsChecked = true;
			Assert.True(checkBox.IsChecked);
		}

		[Fact]
		public void CommandParameter_ShouldSupportNullValue()
		{
			object receivedParameter = "NotNull";

			var checkBox = new CheckBox()
			{
				Command = new Command<object>(param => receivedParameter = param, param => true),
				CommandParameter = null,
				IsChecked = false
			};

			checkBox.IsChecked = true;

			Assert.Null(receivedParameter);
		}

		[Fact]
		public void Command_ShouldExecuteOnlyOnCheckedStateChange()
		{
			int executeCount = 0;
			var checkBox = new CheckBox()
			{
				Command = new Command(() => executeCount++, () => true),
				IsChecked = false
			};

			// Change to true should execute command
			checkBox.IsChecked = true;
			Assert.Equal(1, executeCount);

			// Setting to same value should not execute command
			checkBox.IsChecked = true;
			Assert.Equal(1, executeCount);

			// Change to false should execute command
			checkBox.IsChecked = false;
			Assert.Equal(2, executeCount);
		}

		[Fact]
		public void Command_And_CheckedChanged_ShouldBothFire()
		{
			bool commandExecuted = false;
			bool eventFired = false;

			var checkBox = new CheckBox()
			{
				Command = new Command(() => commandExecuted = true, () => true),
				IsChecked = false
			};

			checkBox.CheckedChanged += (sender, e) => eventFired = true;

			checkBox.IsChecked = true;

			Assert.True(commandExecuted);
			Assert.True(eventFired);
		}
	}
}
