using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
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

		[Test]
		public void TestConstructor()
		{
			Switch sw = new Switch();

			Assert.IsFalse(sw.IsToggled);
		}

		[Test]
		public void TestOnEvent()
		{
			Switch sw = new Switch();

			bool fired = false;
			sw.Toggled += (sender, e) => fired = true;

			sw.IsToggled = true;

			Assert.IsTrue(fired);
		}

		[Test]
		public void TestOnEventNotDoubleFired()
		{
			var sw = new Switch();

			bool fired = false;
			sw.IsToggled = true;

			sw.Toggled += (sender, args) => fired = true;
			sw.IsToggled = true;

			Assert.IsFalse(fired);
		}

		[Test]
		public void VisualStateIsDisabledIfSwitchIsDisabled()
		{
			var switch1 = new Switch();
			switch1.IsEnabled = false;
			VisualStateManager.SetVisualStateGroups(switch1, CreateTestStateGroups());
			var groups1 = VisualStateManager.GetVisualStateGroups(switch1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(DisabledStateName));
		}

		[Test]
		public void VisualStateIsOnIfAvailableAndSwitchIsEnabledAndOn()
		{
			var switch1 = new Switch();
			switch1.IsEnabled = true;
			switch1.IsToggled = true;
			VisualStateManager.SetVisualStateGroups(switch1, CreateTestStateGroups());
			var groups1 = VisualStateManager.GetVisualStateGroups(switch1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(OnStateName));
		}

		[Test]
		public void VisualStateIsOffIfAvailableAndSwitchIsEnabledAndOff()
		{
			var switch1 = new Switch();
			switch1.IsEnabled = true;
			switch1.IsToggled = false;
			VisualStateManager.SetVisualStateGroups(switch1, CreateTestStateGroups());
			var groups1 = VisualStateManager.GetVisualStateGroups(switch1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(OffStateName));
		}

		[Test]
		public void InitialStateIsNormalIfAvailableButOnOffNotAvailable()
		{
			var switch1 = new Switch();
			VisualStateManager.SetVisualStateGroups(switch1, CreateTestStateGroupsWithoutOnOffStates());
			var groups1 = VisualStateManager.GetVisualStateGroups(switch1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(NormalStateName));
		}

		[Test]
		public void InitialStateIsNullIfNormalOnOffNotAvailable()
		{
			var switch1 = new Switch();
			VisualStateManager.SetVisualStateGroups(switch1, CreateTestStateGroupsWithoutNormalState());
			var groups1 = VisualStateManager.GetVisualStateGroups(switch1);
			Assert.Null(groups1[0].CurrentState);
		}
	}

}