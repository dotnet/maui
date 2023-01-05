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
	}
}
