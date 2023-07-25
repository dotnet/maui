using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PointerGestureRecognizerTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var pointer = new PointerGestureRecognizer();
		}

		[Fact]
		public void PointerOverVsmGestureIsOnlyPresentWithVsm()
		{
			var layout = new VerticalStackLayout();
			IGestureController gestureControllers = layout;
			var gesture = gestureControllers
				.CompositeGestureRecognizers
				.OfType<PointerGestureRecognizer>()
				.FirstOrDefault();

			Assert.Null(gesture);

			var visualStateGroups = AddPointerOverVisualState(layout);

			gesture = gestureControllers
				.CompositeGestureRecognizers
				.OfType<PointerGestureRecognizer>()
				.FirstOrDefault();

			Assert.NotNull(gesture);

			visualStateGroups.Remove(visualStateGroups[0]);
			layout.ChangeVisualState();
			gesture = gestureControllers
				.CompositeGestureRecognizers
				.OfType<PointerGestureRecognizer>()
				.FirstOrDefault();

			Assert.Null(gesture);
		}


		[Fact]
		public void ClearingGestureRecognizers()
		{
			var view = new View();
			AddPointerOverVisualState(view);
			var gestureRecognizer = new TapGestureRecognizer();

			view.GestureRecognizers.Add(gestureRecognizer);
			Assert.Equal(2, (view as IGestureController).CompositeGestureRecognizers.Count);

			view.GestureRecognizers.Clear();
			Assert.Single((view as IGestureController).CompositeGestureRecognizers);
			Assert.Null(gestureRecognizer.Parent);
		}

		[Fact]
		public void PointerEnteredCommandFires()
		{
			var gesture = new PointerGestureRecognizer();
			var parameter = new object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			gesture.PointerEnteredCommand = cmd;
			gesture.PointerEnteredCommandParameter = parameter;
			cmd?.Execute(parameter);

			Assert.Equal(commandExecuted, parameter);
		}

		[Fact]
		public void PointerExitedCommandFires()
		{
			var gesture = new PointerGestureRecognizer();
			var parameter = new object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			gesture.PointerExitedCommand = cmd;
			gesture.PointerExitedCommandParameter = parameter;
			cmd?.Execute(parameter);

			Assert.Equal(commandExecuted, parameter);
		}

		[Fact]
		public void PointerMovedCommandFires()
		{
			var gesture = new PointerGestureRecognizer();
			var parameter = new object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			gesture.PointerMovedCommand = cmd;
			gesture.PointerMovedCommandParameter = parameter;
			cmd?.Execute(parameter);

			Assert.Equal(commandExecuted, parameter);
		}

		VisualStateGroupList AddPointerOverVisualState(VisualElement visualElement)
		{
			VisualStateGroupList visualStateGroups = new VisualStateGroupList();
			var pointerOverGroup = new VisualStateGroup() { Name = "CommonStates" };
			pointerOverGroup.States.Add(new VisualState() { Name = VisualStateManager.CommonStates.PointerOver });
			visualStateGroups.Add(pointerOverGroup);
			VisualStateManager.SetVisualStateGroups(visualElement, visualStateGroups);
			return visualStateGroups;
		}
	}
}

