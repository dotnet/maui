using System;
using Microsoft.Maui.Controls.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	[Collection(AnimationsCollection.Name)]
	public class VisualTransitionTests : AnimationTestBase
	{
		static readonly BindableProperty OpacityProperty = VisualElement.OpacityProperty;

		public enum InstantCase
		{
			WithoutTransitions,
			InitialState,
			NotLoaded,
			NoHandler,
			ZeroDuration,
			NoMatchingTransition,
		}

		public enum ExclusionCase
		{
			TwoWayBinding,
			HandlerValue,
		}

		[Fact]
		public void TransitionPinsTheOldValueThenEasesToTheNewState()
		{
			var box = CreateLoadedBox();
			var group = AttachOpacityStates(box, 0.2, 1.0, new VisualTransition());
			VisualStateManager.GoToState(box, "A");
			var changes = new ChangeRecorder(box, OpacityProperty);

			Assert.True(VisualStateManager.GoToState(box, "B"));

			Assert.Equal("B", group.CurrentState.Name);
			Assert.Equal(0.2, box.Opacity);
			Assert.True(box.HasAnimatedValue(OpacityProperty));
			changes.AssertNone();

			// The default transition lasts 150ms and uses Easing.Default (CubicInOut).
			Advance(0.25, DefaultTransitionDuration);
			Assert.Equal(0.2 + 0.8 * Easing.CubicInOut.Ease(0.25), box.Opacity, 9);

			Advance(0.25, DefaultTransitionDuration);
			Assert.Equal(0.2 + 0.8 * Easing.CubicInOut.Ease(0.5), box.Opacity, 9);
			Assert.True(box.HasAnimatedValue(OpacityProperty));

			Advance(0.6, DefaultTransitionDuration);
			Assert.Equal(1.0, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
		}

		[Fact]
		public void TransitionUsesItsDurationAndEasingAndEndsExactlyOnTheNewValue()
		{
			var box = CreateLoadedBox();
			var duration = TimeSpan.FromMilliseconds(200);
			// An easing that never reaches 1: the end must still be the destination itself.
			AttachOpacityStates(box, 0.2, 1.0, new VisualTransition { Duration = duration, Easing = new Easing(x => x * 0.5) });
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");

			Advance(0.5, duration);
			Assert.Equal(0.4, box.Opacity, 9);

			Advance(0.49, duration);
			Assert.True(box.HasAnimatedValue(OpacityProperty));

			Advance(0.02, duration);
			Assert.Equal(1.0, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
		}

		[Fact]
		public void ReturningMidTransitionContinuesFromTheAnimatedValue()
		{
			var box = CreateLoadedBox();
			AttachOpacityStates(box, 0.2, 1.0, new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");
			Advance(0.5, DefaultTransitionDuration);
			Assert.Equal(0.6, box.Opacity, 9);
			var changes = new ChangeRecorder(box, OpacityProperty);

			Assert.True(VisualStateManager.GoToState(box, "A"));

			Assert.Equal(0.6, box.Opacity, 9);
			Assert.True(box.HasAnimatedValue(OpacityProperty));
			changes.AssertNone();

			Advance(0.5, DefaultTransitionDuration);
			Assert.Equal(0.4, box.Opacity, 9);

			Advance(0.6, DefaultTransitionDuration);
			Assert.Equal(0.2, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
		}

		[Theory]
		[InlineData(InstantCase.WithoutTransitions)]
		[InlineData(InstantCase.InitialState)]
		[InlineData(InstantCase.NotLoaded)]
		[InlineData(InstantCase.NoHandler)]
		[InlineData(InstantCase.ZeroDuration)]
		[InlineData(InstantCase.NoMatchingTransition)]
		public void StateChangeAppliesInstantly(InstantCase instantCase)
		{
			var box = instantCase switch
			{
				InstantCase.NotLoaded => new BoxView { Handler = new HandlerStub() },
				InstantCase.NoHandler => CreateWindowedBox(),
				_ => CreateLoadedBox(),
			};
			var transition = instantCase switch
			{
				InstantCase.ZeroDuration => new VisualTransition { Duration = TimeSpan.Zero },
				InstantCase.NoMatchingTransition => new VisualTransition { From = "B", To = "A" },
				_ => new VisualTransition(),
			};
			AttachOpacityStates(box, 0.2, 0.6, transition);
			if (instantCase != InstantCase.InitialState)
				VisualStateManager.GoToState(box, "A");

			Assert.True(VisualStateManager.GoToState(box, "B", useTransitions: instantCase != InstantCase.WithoutTransitions));

			Assert.Equal(0.6, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));

			// Nothing keeps running: the clock moves nothing afterwards.
			var changes = new ChangeRecorder(box, OpacityProperty);
			Advance(1.0, DefaultTransitionDuration);
			Assert.Equal(0.6, box.Opacity);
			changes.AssertNone();
		}

		[Fact]
		public void TransitionSelectionPrefersExactThenToThenFromThenAny()
		{
			var exact = new VisualTransition { From = "A", To = "B" };
			var toOnly = new VisualTransition { To = "B" };
			var fromOnly = new VisualTransition { From = "A" };
			var any = new VisualTransition();
			var group = new VisualStateGroup();
			// Declared in the reverse of the priority order, so the order of declaration can't decide.
			group.Transitions.Add(any);
			group.Transitions.Add(fromOnly);
			group.Transitions.Add(toOnly);
			group.Transitions.Add(exact);

			Assert.Same(exact, group.FindTransition("A", "B"));
			Assert.Same(toOnly, group.FindTransition("C", "B"));
			Assert.Same(fromOnly, group.FindTransition("A", "C"));
			Assert.Same(any, group.FindTransition("C", "D"));

			group.Transitions.Remove(exact);
			Assert.Same(toOnly, group.FindTransition("A", "B"));

			group.Transitions.Remove(toOnly);
			Assert.Same(fromOnly, group.FindTransition("A", "B"));

			group.Transitions.Remove(fromOnly);
			Assert.Same(any, group.FindTransition("A", "B"));

			group.Transitions.Remove(any);
			Assert.Null(group.FindTransition("A", "B"));
		}

		[Fact]
		public void SelectedTransitionDrivesTheAnimation()
		{
			var box = CreateLoadedBox();
			AttachOpacityStates(box, 0.2, 1.0,
				new VisualTransition { Duration = TimeSpan.FromSeconds(1) },
				new VisualTransition { From = "A", To = "B", Duration = TimeSpan.FromMilliseconds(100) });
			VisualStateManager.GoToState(box, "A");

			VisualStateManager.GoToState(box, "B");
			Advance(110);

			Assert.Equal(1.0, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));

			VisualStateManager.GoToState(box, "A");
			Advance(110);

			Assert.True(box.HasAnimatedValue(OpacityProperty));
		}

		[Fact]
		public void PropertySetOnlyByTheOutgoingStateAnimatesBackToItsBaseValue()
		{
			var box = CreateLoadedBox();
			box.Opacity = 0.9;
			Attach(box,
				new[] { State("A", OpacityProperty, 0.2), State("B", VisualElement.ScaleProperty, 2.0) },
				new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			Assert.Equal(0.2, box.Opacity);
			var changes = new ChangeRecorder(box, OpacityProperty);

			VisualStateManager.GoToState(box, "B");

			Assert.Equal(0.2, box.Opacity);
			Assert.Equal(1.0, box.Scale);
			changes.AssertNone();

			Advance(0.5, DefaultTransitionDuration);
			Assert.Equal(0.55, box.Opacity, 9);
			Assert.Equal(1.5, box.Scale, 9);

			Advance(0.6, DefaultTransitionDuration);
			Assert.Equal(0.9, box.Opacity);
			Assert.Equal(2.0, box.Scale);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			Assert.False(box.HasAnimatedValue(VisualElement.ScaleProperty));
		}

		[Fact]
		public void GroupsAppliedThroughAStyleKeepTheirTransitions()
		{
			var transition = new VisualTransition { Easing = Easing.Linear };
			var group = new VisualStateGroup
			{
				Name = "Test",
				States = { State("A", OpacityProperty, 0.2), State("B", OpacityProperty, 1.0) },
				Transitions = { transition },
			};
			var box = CreateLoadedBox();

			box.Style = new Style(typeof(BoxView))
			{
				Setters = { new Setter { Property = VisualStateManager.VisualStateGroupsProperty, Value = new VisualStateGroupList { group } } },
			};

			var applied = Assert.Single(VisualStateManager.GetVisualStateGroups(box));
			Assert.NotSame(group, applied);
			Assert.Same(transition, Assert.Single(applied.Transitions));

			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");
			Assert.Equal(0.2, box.Opacity);

			Advance(0.5, DefaultTransitionDuration);
			Assert.Equal(0.6, box.Opacity, 9);

			Advance(0.6, DefaultTransitionDuration);
			Assert.Equal(1.0, box.Opacity);
		}

		[Fact]
		public void ReducedMotionCompletesTheTransitionImmediately()
		{
			MotionPolicy.ReducedMotionOverride = true;
			var box = CreateLoadedBox();
			AttachOpacityStates(box, 0.2, 1.0, new VisualTransition());
			VisualStateManager.GoToState(box, "A");
			var changes = new ChangeRecorder(box, OpacityProperty);

			VisualStateManager.GoToState(box, "B");

			Assert.Equal(1.0, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			changes.AssertOnePair();
		}

		[Fact]
		public void BecomingVisibleShowsTheElementForTheWholeTransition()
		{
			var box = CreateLoadedBox();
			AttachVisibilityStates(box);
			VisualStateManager.GoToState(box, "Hidden");
			Assert.False(box.IsVisible);

			VisualStateManager.GoToState(box, "Shown");

			Assert.True(box.IsVisible);
			Advance(0.5, DefaultTransitionDuration);
			Assert.True(box.IsVisible);
			Advance(0.6, DefaultTransitionDuration);
			Assert.True(box.IsVisible);
			Assert.False(box.HasAnimatedValue(VisualElement.IsVisibleProperty));
		}

		[Fact]
		public void BecomingHiddenKeepsTheElementVisibleUntilTheEnd()
		{
			var box = CreateLoadedBox();
			AttachVisibilityStates(box);
			VisualStateManager.GoToState(box, "Shown");

			VisualStateManager.GoToState(box, "Hidden");

			Assert.True(box.IsVisible);
			Advance(0.5, DefaultTransitionDuration);
			Assert.True(box.IsVisible);
			Advance(0.6, DefaultTransitionDuration);
			Assert.False(box.IsVisible);
			Assert.False(box.HasAnimatedValue(VisualElement.IsVisibleProperty));
		}

		[Theory]
		[InlineData(ExclusionCase.TwoWayBinding)]
		[InlineData(ExclusionCase.HandlerValue)]
		public void ExcludedPropertiesChangeInstantly(ExclusionCase exclusion)
		{
			var box = CreateLoadedBox();
			if (exclusion == ExclusionCase.TwoWayBinding)
				box.SetBinding(OpacityProperty, new Binding(nameof(ValueSource.Value), BindingMode.TwoWay, source: new ValueSource { Value = 0.5 }));
			AttachOpacityStates(box, 0.2, 0.6, new VisualTransition());
			VisualStateManager.GoToState(box, "A");
			if (exclusion == ExclusionCase.HandlerValue)
				box.SetValueFromRenderer(OpacityProperty, 0.4);
			Assert.True(box.IsExcludedFromAnimation(OpacityProperty));

			VisualStateManager.GoToState(box, "B");

			Assert.Equal(0.6, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
		}

		[Fact]
		public void HandlerWriteMidTransitionEndsItAndTheUserValueStands()
		{
			var box = CreateLoadedBox();
			AttachOpacityStates(box, 0.2, 1.0, new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");
			Advance(0.5, DefaultTransitionDuration);
			var changes = new ChangeRecorder(box, OpacityProperty);

			box.SetValueFromRenderer(OpacityProperty, 0.35);

			Assert.Equal(0.35, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			changes.AssertOnePair();

			Advance(1.0, DefaultTransitionDuration);

			Assert.Equal(0.35, box.Opacity);
			changes.AssertOnePair();
		}

		[Fact]
		public void HandlerEchoMidTransitionStillEndsOnTheNewStateValue()
		{
			var box = CreateLoadedBox();
			AttachOpacityStates(box, 0.2, 1.0, new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");
			Advance(0.5, DefaultTransitionDuration);

			// The platform echoes the value it was just given; that must not end or outlive the transition.
			box.SetValueFromRenderer(OpacityProperty, box.Opacity);
			Assert.True(box.HasAnimatedValue(OpacityProperty));

			Advance(0.6, DefaultTransitionDuration);

			Assert.Equal(1.0, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
		}

		[Fact]
		public void HandlerEchoMidTransitionDoesNotChangeTheDestination()
		{
			var box = CreateLoadedBox();
			AttachOpacityStates(box, 0.2, 1.0, new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");
			Advance(0.5, DefaultTransitionDuration);

			box.SetValueFromRenderer(OpacityProperty, box.Opacity);
			Advance(0.25, DefaultTransitionDuration);

			// Linear, three quarters of the way from 0.2 to 1.0: the echo isn't a new destination.
			Assert.Equal(0.8, box.Opacity, 9);
		}

		[Fact]
		public void UnloadingMidTransitionCompletesIt()
		{
			var box = CreateLoadedBox();
			AttachOpacityStates(box, 0.2, 1.0, new VisualTransition());
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");
			Advance(0.5, DefaultTransitionDuration);

			((ContentPage)box.Parent).Content = null;

			Assert.False(box.IsLoaded);
			Assert.Equal(1.0, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
		}

		[Fact]
		public void GoingToAStateWithoutTransitionsMidTransitionAppliesItInstantly()
		{
			var box = CreateLoadedBox();
			AttachOpacityStates(box, 0.2, 1.0, new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");
			Advance(0.5, DefaultTransitionDuration);

			VisualStateManager.GoToState(box, "A", useTransitions: false);

			Assert.Equal(0.2, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));

			Advance(0.6, DefaultTransitionDuration);
			Assert.Equal(0.2, box.Opacity);
		}

		static VisualState State(string name, BindableProperty property, object value)
			=> new VisualState { Name = name, Setters = { new Setter { Property = property, Value = value } } };

		static VisualStateGroup Attach(VisualElement element, VisualState[] states, params VisualTransition[] transitions)
		{
			var group = new VisualStateGroup { Name = "Test" };
			foreach (var state in states)
				group.States.Add(state);
			foreach (var transition in transitions)
				group.Transitions.Add(transition);

			VisualStateManager.SetVisualStateGroups(element, new VisualStateGroupList { group });
			return Assert.Single(VisualStateManager.GetVisualStateGroups(element));
		}

		static VisualStateGroup AttachOpacityStates(VisualElement element, double a, double b, params VisualTransition[] transitions)
			=> Attach(element, new[] { State("A", OpacityProperty, a), State("B", OpacityProperty, b) }, transitions);

		static void AttachVisibilityStates(VisualElement element)
			=> Attach(element,
				new[] { State("Hidden", VisualElement.IsVisibleProperty, false), State("Shown", VisualElement.IsVisibleProperty, true) },
				new VisualTransition());
	}
}
