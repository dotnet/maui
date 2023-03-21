namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class VisualStateTestHelpers
	{
		public const string NormalStateName = "Normal";
		public const string PressedStateName = "Pressed";
		public const string InvalidStateName = "Invalid";
		public const string UnfocusedStateName = "Unfocused";
		public const string FocusedStateName = "Focused";
		public const string DisabledStateName = "Disabled";
		public const string CommonStatesGroupName = "CommonStates";
		public const string FocusStatesGroupName = "FocusStates";

		public static VisualStateGroupList CreateTestStateGroups()
		{
			var stateGroups = new VisualStateGroupList();
			var commonStatesGroup = new VisualStateGroup { Name = CommonStatesGroupName };
			var normalState = new VisualState { Name = NormalStateName };
			var focusState = new VisualState { Name = FocusedStateName };
			var pressedState = new VisualState { Name = PressedStateName };
			var invalidState = new VisualState { Name = InvalidStateName };

			var disabledState = new VisualState { Name = DisabledStateName };

			commonStatesGroup.States.Add(normalState);
			commonStatesGroup.States.Add(pressedState);
			commonStatesGroup.States.Add(invalidState);
			commonStatesGroup.States.Add(focusState);
			commonStatesGroup.States.Add(disabledState);

			stateGroups.Add(commonStatesGroup);

			return stateGroups;
		}

		public static VisualStateGroupList CreateStateGroupsWithoutNormalState()
		{
			var stateGroups = new VisualStateGroupList();
			var visualStateGroup = new VisualStateGroup { Name = CommonStatesGroupName };
			var invalidState = new VisualState { Name = InvalidStateName };

			visualStateGroup.States.Add(invalidState);

			stateGroups.Add(visualStateGroup);

			return stateGroups;
		}
	}
}