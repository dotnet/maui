using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class VisualStateManagerTests
	{
		const string NormalStateName = "Normal";
		const string InvalidStateName = "Invalid";
		const string FocusedStateName = "Focused";
		const string DisabledStateName = "Disabled";
		const string CommonStatesName = "CommonStates";

		static VisualStateGroupList CreateTestStateGroups()
		{
			var stateGroups = new VisualStateGroupList();
			var visualStateGroup = new VisualStateGroup { Name = CommonStatesName };
			var normalState = new VisualState { Name = NormalStateName };
			var invalidState = new VisualState { Name = InvalidStateName };
			var focusedState = new VisualState { Name = FocusedStateName };
			var disabledState = new VisualState { Name = DisabledStateName };

			visualStateGroup.States.Add(normalState);
			visualStateGroup.States.Add(invalidState);
			visualStateGroup.States.Add(focusedState);
			visualStateGroup.States.Add(disabledState);

			stateGroups.Add(visualStateGroup);

			return stateGroups;
		}

		static VisualStateGroupList CreateStateGroupsWithoutNormalState()
		{
			var stateGroups = new VisualStateGroupList();
			var visualStateGroup = new VisualStateGroup { Name = CommonStatesName };
			var invalidState = new VisualState { Name = InvalidStateName };

			visualStateGroup.States.Add(invalidState);

			stateGroups.Add(visualStateGroup);

			return stateGroups;
		}

		[Test]
		public void InitialStateIsNormalIfAvailable()
		{
			var label1 = new Label();

			VisualStateManager.SetVisualStateGroups(label1, CreateTestStateGroups());

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);

			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(NormalStateName));
		}

		[Test]
		public void InitialStateIsNullIfNormalNotAvailable()
		{
			var label1 = new Label();

			VisualStateManager.SetVisualStateGroups(label1, CreateStateGroupsWithoutNormalState());

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);

			Assert.Null(groups1[0].CurrentState);
		}

		[Test]
		public void VisualElementsStateGroupsAreDistinct()
		{
			var label1 = new Label();
			var label2 = new Label();

			VisualStateManager.SetVisualStateGroups(label1, CreateTestStateGroups());
			VisualStateManager.SetVisualStateGroups(label2, CreateTestStateGroups());

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);
			var groups2 = VisualStateManager.GetVisualStateGroups(label2);

			Assert.AreNotSame(groups1, groups2);

			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(NormalStateName));
			Assert.That(groups2[0].CurrentState.Name, Is.EqualTo(NormalStateName));

			VisualStateManager.GoToState(label1, InvalidStateName);

			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(InvalidStateName));
			Assert.That(groups2[0].CurrentState.Name, Is.EqualTo(NormalStateName));
		}

		[Test]
		public void VisualStateGroupsFromSettersAreDistinct()
		{
			var x = new Setter();
			x.Property = VisualStateManager.VisualStateGroupsProperty;
			x.Value = CreateTestStateGroups();

			var label1 = new Label();
			var label2 = new Label();

			x.Apply(label1);
			x.Apply(label2);

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);
			var groups2 = VisualStateManager.GetVisualStateGroups(label2);

			Assert.NotNull(groups1);
			Assert.NotNull(groups2);

			Assert.AreNotSame(groups1, groups2);

			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(NormalStateName));
			Assert.That(groups2[0].CurrentState.Name, Is.EqualTo(NormalStateName));

			VisualStateManager.GoToState(label1, InvalidStateName);

			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(InvalidStateName));
			Assert.That(groups2[0].CurrentState.Name, Is.EqualTo(NormalStateName));
		}

		[Test]
		public void ElementsDoNotHaveVisualStateGroupsCollectionByDefault()
		{
			var label1 = new Label();
			Assert.False(label1.HasVisualStateGroups());
		}

		[Test]
		public void StateNamesMustBeUniqueWithinGroup()
		{
			IList<VisualStateGroup> vsgs = CreateTestStateGroups();

			var duplicate = new VisualState { Name = NormalStateName };

			Assert.Throws<InvalidOperationException>(() => vsgs[0].States.Add(duplicate));
		}

		[Test]
		public void StateNamesMustBeUniqueWithinGroupList()
		{
			IList<VisualStateGroup> vsgs = CreateTestStateGroups();

			// Create and add a second VisualStateGroup
			var secondGroup = new VisualStateGroup { Name = "Foo" };
			vsgs.Add(secondGroup);

			// Create a VisualState with the same name as one in another group in this list
			var duplicate = new VisualState { Name = NormalStateName };

			Assert.Throws<InvalidOperationException>(() => secondGroup.States.Add(duplicate));
		}

		[Test]
		public void GroupNamesMustBeUniqueWithinGroupList()
		{
			IList<VisualStateGroup> vsgs = CreateTestStateGroups();
			var secondGroup = new VisualStateGroup { Name = CommonStatesName };

			Assert.Throws<InvalidOperationException>(() => vsgs.Add(secondGroup));
		}

		[Test]
		public void StateNamesInGroupMayNotBeNull()
		{
			IList<VisualStateGroup> vsgs = CreateTestStateGroups();

			var nullStateName = new VisualState();

			Assert.Throws<InvalidOperationException>(() => vsgs[0].States.Add(nullStateName));
		}

		[Test]
		public void StateNamesInGroupMayNotBeEmpty()
		{
			IList<VisualStateGroup> vsgs = CreateTestStateGroups();

			var emptyStateName = new VisualState { Name = "" };

			Assert.Throws<InvalidOperationException>(() => vsgs[0].States.Add(emptyStateName));
		}

		[Test]
		public void VerifyVisualStateChanges()
		{
			var label1 = new Label();
			VisualStateManager.SetVisualStateGroups(label1, CreateTestStateGroups());

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(NormalStateName));

			label1.IsEnabled = false;

			groups1 = VisualStateManager.GetVisualStateGroups(label1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(DisabledStateName));


			label1.SetValue(VisualElement.IsFocusedPropertyKey, true);
			groups1 = VisualStateManager.GetVisualStateGroups(label1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(DisabledStateName));

			label1.IsEnabled = true;
			groups1 = VisualStateManager.GetVisualStateGroups(label1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(FocusedStateName));


			label1.SetValue(VisualElement.IsFocusedPropertyKey, false);
			groups1 = VisualStateManager.GetVisualStateGroups(label1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(NormalStateName));

		}
	}
}