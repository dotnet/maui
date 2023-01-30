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
			Assert.Equal(1, (view as IGestureController).CompositeGestureRecognizers.Count);
			Assert.Null(gestureRecognizer.Parent);
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

