using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
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
			Assert.False(checkBox.IsThreeState);
		}

		[Fact]
		public void NativeWriteBackUsesNullableCapability()
		{
			var capable = Substitute.For<ICheckBox2>();
			capable.IsThreeState.Returns(true);
			ICheckBox checkBox = capable;

			Microsoft.Maui.Handlers.CheckBoxHandler.SetCheckedState(checkBox, null);
			capable.Received().IsChecked = null;
			((ICheckBox)capable).DidNotReceive().IsChecked = Arg.Any<bool>();
		}

		[Fact]
		public void LegacyViewsKeepBooleanBehavior()
		{
			var checkBox = Substitute.For<ICheckBox>();

			Microsoft.Maui.Handlers.CheckBoxHandler.SetCheckedState(checkBox, null);
			checkBox.DidNotReceive().IsChecked = Arg.Any<bool>();
			Microsoft.Maui.Handlers.CheckBoxHandler.SetCheckedState(checkBox, false);
			checkBox.Received().IsChecked = false;
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		[InlineData(true, null, null)]
		[InlineData(false, true, true)]
		[InlineData(false, false, false)]
		[InlineData(false, null, false)]
		public void AssignedStateHonorsIsThreeState(bool threeState, bool? requested, bool? expected)
		{
			var checkBox = new CheckBox { IsThreeState = threeState };
			var events = 0;
			var commands = 0;
			checkBox.CheckedChanged += (_, _) => events++;
			checkBox.Command = new Command(() => commands++);

			checkBox.IsChecked = requested;
			checkBox.IsChecked = requested;

			Assert.Equal(expected, checkBox.IsChecked);
			Assert.Equal(expected == false ? 0 : 1, events);
			Assert.Equal(events, commands);
		}

		[Fact]
		public void ThreeStateFlagBindingUpdatesBothDirectionsAndClearsMixedState()
		{
			var source = new CheckBox { IsThreeState = true };
			var target = new CheckBox();
			target.SetBinding(CheckBox.IsThreeStateProperty, static (CheckBox value) => value.IsThreeState,
				source: source);
			Assert.True(target.IsThreeState);

			target.IsChecked = null;
			target.IsThreeState = false;
			Assert.False(source.IsThreeState);
			Assert.False(target.IsChecked);

			source.IsThreeState = true;
			Assert.True(target.IsThreeState);
			target.IsChecked = null;
			source.IsThreeState = false;
			Assert.False(target.IsThreeState);
			Assert.False(target.IsChecked);
		}

		[Fact]
		public void DisablingThreeStateResetsMixedOnceWithoutReplacingBinding()
		{
			var source = new CheckBox { IsThreeState = true };
			var target = new CheckBox { IsThreeState = true };
			target.SetBinding(CheckBox.IsCheckedProperty, static (CheckBox value) => value.IsChecked,
				source: source);
			((ICheckBox2)target).IsChecked = null;
			Assert.Null(source.IsChecked);
			var events = 0;
			var commands = 0;
			CheckedChangedEventArgs args = null;
			target.CheckedChanged += (_, e) =>
			{
				events++;
				args = e;
			};
			target.Command = new Command(() => commands++);

			target.IsThreeState = false;
			target.IsThreeState = false;

			Assert.False(target.IsChecked);
			Assert.False(source.IsChecked);
			Assert.Equal(1, events);
			Assert.Equal(1, commands);
			Assert.False(args.Value);
			Assert.Equal((bool?)false, args.CheckedValue);

			target.IsThreeState = true;
			Assert.False(target.IsChecked);
			Assert.Equal(1, events);
			Assert.Equal(1, commands);
			source.IsChecked = true;
			Assert.True(target.IsChecked);
			Assert.Equal(2, events);
			Assert.Equal(2, commands);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void FlagChangesPreserveBooleanStateWithoutNotifications(bool value)
		{
			var checkBox = new CheckBox { IsThreeState = true, IsChecked = value };
			var events = 0;
			var commands = 0;
			checkBox.CheckedChanged += (_, _) => events++;
			checkBox.Command = new Command(() => commands++);

			checkBox.IsThreeState = false;
			checkBox.IsThreeState = true;

			Assert.Equal((bool?)value, checkBox.IsChecked);
			Assert.Equal(0, events);
			Assert.Equal(0, commands);
		}

		[Fact]
		public void DisablingThreeStateNormalizesHiddenStyleValue()
		{
			var checkBox = new CheckBox
			{
				IsThreeState = true,
				Style = new Style(typeof(CheckBox))
				{
					Setters = { new Setter { Property = CheckBox.IsCheckedProperty, Value = null } }
				}
			};
			Assert.Null(checkBox.IsChecked);
			checkBox.IsChecked = true;
			var events = 0;
			var commands = 0;
			checkBox.CheckedChanged += (_, e) =>
			{
				Assert.Equal((bool?)false, e.CheckedValue);
				events++;
			};
			checkBox.Command = new Command(() => commands++);

			checkBox.IsThreeState = false;

			Assert.True(checkBox.IsChecked);
			Assert.Equal(0, events);
			Assert.Equal(0, commands);

			checkBox.ClearValue(CheckBox.IsCheckedProperty);

			Assert.False(checkBox.IsChecked);
			Assert.Equal(1, events);
			Assert.Equal(1, commands);
			checkBox.IsThreeState = true;
			Assert.False(checkBox.IsChecked);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(true, true)]
		public void DisablingThreeStateNormalizesReentrantAssignments(bool fromCommand, bool queueCheckedAfterNull)
		{
			var checkBox = new CheckBox { IsThreeState = true };
			var states = new List<bool?>();
			var commands = 0;
			var reentered = false;

			void QueueReentrantChanges()
			{
				if (reentered || checkBox.IsChecked != true)
					return;

				reentered = true;
				checkBox.IsChecked = null;
				if (queueCheckedAfterNull)
					checkBox.IsChecked = true;
				checkBox.IsThreeState = false;
			}

			checkBox.CheckedChanged += (_, e) =>
			{
				states.Add(e.CheckedValue);
				if (!fromCommand)
					QueueReentrantChanges();
			};
			checkBox.Command = new Command(() =>
			{
				commands++;
				if (fromCommand)
					QueueReentrantChanges();
			});

			checkBox.IsChecked = true;

			Assert.True(reentered);
			Assert.False(checkBox.IsThreeState);
			Assert.Equal((bool?)queueCheckedAfterNull, checkBox.IsChecked);
			Assert.Equal(queueCheckedAfterNull ? new bool?[] { true, false, true } : new bool?[] { true, false }, states);
			Assert.Equal(states.Count, commands);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DisablingThreeStateDuringPropertyChangingNormalizesPendingValue(bool initial)
		{
			var checkBox = new CheckBox { IsThreeState = true, IsChecked = initial };
			var changingValues = new List<bool?>();
			var changedStates = new List<bool?>();
			var propertyChangedCount = 0;
			var commands = 0;

			checkBox.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == nameof(CheckBox.IsChecked))
				{
					changingValues.Add(checkBox.IsChecked);
					checkBox.IsThreeState = false;
				}
			};
			checkBox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(CheckBox.IsChecked))
				{
					Assert.False(checkBox.IsChecked);
					propertyChangedCount++;
				}
			};
			checkBox.CheckedChanged += (_, e) => changedStates.Add(e.CheckedValue);
			checkBox.Command = new Command(() =>
			{
				Assert.False(checkBox.IsChecked);
				commands++;
			});

			checkBox.IsChecked = null;

			Assert.False(checkBox.IsThreeState);
			Assert.False(checkBox.IsChecked);
			Assert.Equal(new bool?[] { initial }, changingValues);
			Assert.Equal(initial ? new bool?[] { false } : Array.Empty<bool?>(), changedStates);
			Assert.Equal(initial ? 1 : 0, propertyChangedCount);
			Assert.Equal(propertyChangedCount, commands);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void PropertyChangingNormalizationDoesNotPublishNullToBinding(bool initial)
		{
			var source = new CheckBox { IsThreeState = true, IsChecked = initial };
			var target = new CheckBox { IsThreeState = true };
			target.SetBinding(CheckBox.IsCheckedProperty, static (CheckBox value) => value.IsChecked,
				source: source);
			var sourceStates = new List<bool?>();
			source.CheckedChanged += (_, e) => sourceStates.Add(e.CheckedValue);
			target.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == nameof(CheckBox.IsChecked))
					target.IsThreeState = false;
			};

			target.IsChecked = null;

			Assert.False(target.IsThreeState);
			Assert.False(target.IsChecked);
			Assert.False(source.IsChecked);
			Assert.Equal(initial ? new bool?[] { false } : Array.Empty<bool?>(), sourceStates);

			source.IsChecked = true;
			Assert.True(target.IsChecked);
		}

		[Theory]
		[InlineData(false, true)]
		[InlineData(false, null)]
		[InlineData(true, false)]
		[InlineData(true, null)]
		[InlineData(null, false)]
		[InlineData(null, true)]
		public void NullableChangesNotifyOnce(bool? initial, bool? next)
		{
			var checkBox = new CheckBox { IsThreeState = true, IsChecked = initial };
			var events = 0;
			var commands = 0;
			CheckedChangedEventArgs args = null;
			var parameter = new object();
			checkBox.CommandParameter = parameter;
			checkBox.Command = new Command<object>(value =>
			{
				Assert.Same(parameter, value);
				commands++;
			});
			checkBox.CheckedChanged += (_, e) =>
			{
				events++;
				args = e;
			};

			checkBox.IsChecked = next;
			checkBox.IsChecked = next;

			Assert.Equal(next, checkBox.IsChecked);
			Assert.Equal(1, events);
			Assert.Equal(1, commands);
			Assert.Equal(next, args.CheckedValue);
			Assert.Equal(next == true, args.Value);
		}

		[Fact]
		public void MixedChangeHonorsCanExecute()
		{
			var commands = 0;
			var events = 0;
			var checkBox = new CheckBox { IsThreeState = true, Command = new Command(() => commands++, () => false) };
			checkBox.CheckedChanged += (_, _) => events++;

			checkBox.IsChecked = null;

			Assert.Null(checkBox.IsChecked);
			Assert.Equal(1, events);
			Assert.Equal(0, commands);
		}

		[Fact]
		public void LegacyAndNullableInterfacesStayConsistent()
		{
			var checkBox = new CheckBox { IsThreeState = true, IsChecked = null };
			var nullable = Assert.IsAssignableFrom<ICheckBox2>(checkBox);
			ICheckBox legacy = checkBox;
			Assert.Null(nullable.IsChecked);
			Assert.False(legacy.IsChecked);

			legacy.IsChecked = true;
			Assert.True(nullable.IsChecked);
			nullable.IsChecked = null;
			Assert.False(legacy.IsChecked);
			legacy.IsChecked = false;
			Assert.False(nullable.IsChecked);
		}

		[Fact]
		public void NullableHandlerUpdatesPreserveTwoWayBinding()
		{
			var source = new CheckBox { IsThreeState = true, IsChecked = null };
			var target = new CheckBox { IsThreeState = true };
			target.SetBinding(CheckBox.IsCheckedProperty, static (CheckBox value) => value.IsChecked,
				source: source);
			Assert.Null(target.IsChecked);

			((ICheckBox2)target).IsChecked = true;
			Assert.True(source.IsChecked);
			source.IsChecked = false;
			Assert.False(target.IsChecked);
			((ICheckBox2)target).IsChecked = null;
			Assert.Null(source.IsChecked);
			source.IsChecked = true;
			Assert.True(target.IsChecked);
		}

		[Theory]
		[InlineData("True", true)]
		[InlineData("False", false)]
		public void DragAndDropRoundTripsBooleanState(string text, bool? expected)
		{
			var checkBox = new CheckBox();
			Assert.True(checkBox.TrySetValue(text));
			Assert.Equal(expected, checkBox.IsChecked);
			Assert.Equal(text, checkBox.GetStringValue());
			Assert.False(checkBox.TrySetValue("not a state"));
			Assert.Equal(expected, checkBox.IsChecked);
		}

		[Theory]
		[InlineData(true, null)]
		[InlineData(false, false)]
		public async Task DragAndDropRoundTripsIndeterminateState(bool threeState, bool? expected)
		{
			var source = new CheckBox { IsThreeState = true, IsChecked = null };
			var target = new CheckBox { IsThreeState = threeState, IsChecked = true };
			var drag = new DragGestureRecognizer();
			var drop = new DropGestureRecognizer { AllowDrop = true };
			source.GestureRecognizers.Add(drag);
			target.GestureRecognizers.Add(drop);

			var data = drag.SendDragStarting(source).Data;
			Assert.Equal("Indeterminate", data.Text);
			await drop.SendDrop(new DropEventArgs(new DataPackageView(data)));

			Assert.Equal(expected, target.IsChecked);
			Assert.Null(source.IsChecked);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void RadioButtonRetainsBooleanEventContract(bool value)
		{
			var radio = new RadioButton { IsChecked = !value };
			var events = 0;
			radio.CheckedChanged += (_, e) =>
			{
				Assert.Equal(value, e.Value);
				Assert.Equal(value, e.CheckedValue);
				events++;
			};
			radio.IsChecked = value;
			radio.IsChecked = value;
			Assert.Equal(1, events);
		}

		[Fact]
		public void RadioButtonGroupRetainsBooleanEventContract()
		{
			var first = new RadioButton { GroupName = "Compatibility" };
			var second = new RadioButton { GroupName = "Compatibility" };
			var layout = new Grid();
			layout.Children.Add(first);
			layout.Children.Add(second);
			first.IsChecked = true;
			var firstChanges = 0;
			var secondChanges = 0;
			first.CheckedChanged += (_, e) =>
			{
				Assert.False(e.Value);
				Assert.Equal((bool?)false, e.CheckedValue);
				firstChanges++;
			};
			second.CheckedChanged += (_, e) =>
			{
				Assert.True(e.Value);
				Assert.Equal((bool?)true, e.CheckedValue);
				secondChanges++;
			};

			second.IsChecked = true;
			second.IsChecked = true;

			Assert.False(first.IsChecked);
			Assert.True(second.IsChecked);
			Assert.Equal(1, firstChanges);
			Assert.Equal(1, secondChanges);
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
		public void IndeterminateVisualStateTransitionsAndDisabledPrecedence()
		{
			var groups = CreateTestStateGroups();
			var group = groups[0];
			group.States.Add(new VisualState { Name = CheckBox.IsCheckedVisualState });
			group.States.Add(new VisualState { Name = CheckBox.IsIndeterminateVisualState });
			var checkBox = new CheckBox { IsThreeState = true };
			VisualStateManager.SetVisualStateGroups(checkBox, groups);

			checkBox.IsChecked = true;
			Assert.Equal(CheckBox.IsCheckedVisualState, group.CurrentState.Name);
			checkBox.IsChecked = null;
			Assert.Equal(CheckBox.IsIndeterminateVisualState, group.CurrentState.Name);
			checkBox.IsChecked = true;
			Assert.Equal(CheckBox.IsCheckedVisualState, group.CurrentState.Name);
			checkBox.IsChecked = null;
			checkBox.IsEnabled = false;
			Assert.Equal(DisabledStateName, group.CurrentState.Name);
			checkBox.IsChecked = true;
			Assert.Equal(DisabledStateName, group.CurrentState.Name);
			checkBox.IsChecked = null;
			Assert.Equal(DisabledStateName, group.CurrentState.Name);
			checkBox.IsEnabled = true;
			Assert.Equal(CheckBox.IsIndeterminateVisualState, group.CurrentState.Name);
			checkBox.IsChecked = false;
			Assert.Equal(NormalStateName, group.CurrentState.Name);
			checkBox.IsChecked = null;
			checkBox.IsThreeState = false;
			Assert.False(checkBox.IsChecked);
			Assert.Equal(NormalStateName, group.CurrentState.Name);
		}

		[Fact]
		public void MissingIndeterminateVisualStateFallsBackToNormal()
		{
			var groups = CreateTestStateGroups();
			var group = groups[0];
			group.States.Add(new VisualState { Name = CheckBox.IsCheckedVisualState });
			var checkBox = new CheckBox { IsThreeState = true };
			VisualStateManager.SetVisualStateGroups(checkBox, groups);

			checkBox.IsChecked = true;
			Assert.Equal(CheckBox.IsCheckedVisualState, group.CurrentState.Name);
			checkBox.IsChecked = null;
			Assert.Equal(NormalStateName, group.CurrentState.Name);
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
