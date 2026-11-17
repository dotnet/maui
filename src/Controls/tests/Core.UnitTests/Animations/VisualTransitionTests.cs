using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Graphics;
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

		[Fact]
		public void GoingToTheCurrentStateWithoutTransitionsLandsItsRunningTransition()
		{
			var box = CreateLoadedBox();
			var group = Attach(box,
				new[] { State("Normal", OpacityProperty, 1.0), State("Pressed", OpacityProperty, 0.5) },
				new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "Normal");
			VisualStateManager.GoToState(box, "Pressed");
			Advance(0.5, DefaultTransitionDuration);
			Assert.Equal(0.75, box.Opacity, 9);
			Assert.True(box.HasAnimatedValue(OpacityProperty));
			var changes = new ChangeRecorder(box, OpacityProperty);

			Assert.True(VisualStateManager.GoToState(box, "Pressed", useTransitions: false));

			Assert.Equal("Pressed", group.CurrentState.Name);
			Assert.Equal(0.5, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			changes.AssertOnePair();

			Advance(1.0, DefaultTransitionDuration);
			Assert.Equal(0.5, box.Opacity);
			changes.AssertOnePair();
		}

		[Fact]
		public void InstantStateChangeFinishesEveryRunningTransitionOfTheGroup()
		{
			var box = CreateLoadedBox();
			Attach(box,
				new[] { State("Zero", VisualElement.ScaleProperty, 2.0), new VisualState { Name = "A" }, State("B", OpacityProperty, 0.5) },
				new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "Zero");
			VisualStateManager.GoToState(box, "A");
			Advance(0.5, DefaultTransitionDuration);
			Assert.Equal(1.5, box.Scale, 9);
			var scaleChanges = new ChangeRecorder(box, VisualElement.ScaleProperty);

			// Only the earlier state sets Scale; neither A nor B mentions it.
			VisualStateManager.GoToState(box, "B", useTransitions: false);

			Assert.Equal(1.0, box.Scale);
			Assert.False(box.HasAnimatedValue(VisualElement.ScaleProperty));
			scaleChanges.AssertOnePair();
			Assert.Equal(0.5, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));

			Advance(1.0, DefaultTransitionDuration);
			Assert.Equal(1.0, box.Scale);
			scaleChanges.AssertOnePair();
		}

		// An animated state change doesn't cut short what an earlier state is still animating. Whether it does
		// must not depend on whether the new states happen to set any property themselves.
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void AnimatedStateChangeLeavesEarlierTransitionsRunning(bool newStateSetsAProperty)
		{
			var box = CreateLoadedBox();
			var b = newStateSetsAProperty ? State("B", OpacityProperty, 0.5) : new VisualState { Name = "B" };
			Attach(box,
				new[] { State("Zero", VisualElement.ScaleProperty, 2.0), new VisualState { Name = "A" }, b },
				new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "Zero");
			VisualStateManager.GoToState(box, "A");
			Advance(0.5, DefaultTransitionDuration);

			VisualStateManager.GoToState(box, "B");

			Assert.Equal(1.5, box.Scale, 9);
			Assert.True(box.HasAnimatedValue(VisualElement.ScaleProperty));
			Advance(0.25, DefaultTransitionDuration);
			Assert.Equal(1.25, box.Scale, 9);

			Advance(0.3, DefaultTransitionDuration);
			Assert.Equal(1.0, box.Scale);
			Assert.False(box.HasAnimatedValue(VisualElement.ScaleProperty));
		}

		[Fact]
		public void InvalidatingVisualStatesMidTransitionFinishesIt()
		{
			var box = CreateLoadedBox();
			AttachOpacityStates(box, 0.2, 0.8, new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");
			Advance(0.5, DefaultTransitionDuration);

			VisualStateManager.InvalidateVisualStates(box);

			Assert.Equal(0.8, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			var changes = new ChangeRecorder(box, OpacityProperty);
			Advance(1.0, DefaultTransitionDuration);
			Assert.Equal(0.8, box.Opacity);
			changes.AssertNone();
		}

		[Fact]
		public void RemovingTheGroupMidTransitionFinishesIt()
		{
			var box = CreateLoadedBox();
			box.Opacity = 0.9;
			var group = AttachOpacityStates(box, 0.2, 0.8, new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");
			Advance(0.5, DefaultTransitionDuration);

			VisualStateManager.GetVisualStateGroups(box).Remove(group);

			Assert.Equal(0.9, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			var changes = new ChangeRecorder(box, OpacityProperty);
			Advance(1.0, DefaultTransitionDuration);
			Assert.Equal(0.9, box.Opacity);
			changes.AssertNone();
		}

		[Theory]
		[InlineData(400, 40)]
		[InlineData(2000, 100)]
		public void ReducedMotionShortenPlaysTheTransitionWithinTheShortenedTime(double durationMs, double shortenedMs)
		{
			MotionPolicy.ReducedMotionOverride = true;
			var box = CreateLoadedBox();
			AttachOpacityStates(box, 0.2, 1.0, new VisualTransition
			{
				Duration = TimeSpan.FromMilliseconds(durationMs),
				Easing = Easing.Linear,
				ReducedMotion = ReducedMotionBehavior.Shorten,
			});
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");

			Assert.True(box.HasAnimatedValue(OpacityProperty));
			Advance(shortenedMs / 2);
			Assert.Equal(0.6, box.Opacity, 9);
			Advance(shortenedMs / 2 - 1);
			Assert.True(box.HasAnimatedValue(OpacityProperty));

			Advance(2);
			Assert.Equal(1.0, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
		}

		[Fact]
		public void DestinationThatBecomesInterpolableMidTransitionIsInterpolated()
		{
			var box = CreateLoadedBox();
			box.Resources = new ResourceDictionary();
			// The resource doesn't exist yet, so beneath state A the background is null.
			box.SetDynamicResource(VisualElement.BackgroundColorProperty, "Background");
			Attach(box,
				new[] { State("A", VisualElement.BackgroundColorProperty, Colors.Red), new VisualState { Name = "B" } },
				new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");

			// Red -> null can't be interpolated: the start value holds.
			Advance(0.25, DefaultTransitionDuration);
			Assert.Same(Colors.Red, box.BackgroundColor);

			box.Resources["Background"] = Colors.Blue;
			Assert.Same(Colors.Red, box.BackgroundColor);

			Advance(0.25, DefaultTransitionDuration);
			var halfway = box.BackgroundColor;
			Assert.Equal(0.5, halfway.Red, 6);
			Assert.Equal(0, halfway.Green, 6);
			Assert.Equal(0.5, halfway.Blue, 6);
			Assert.Equal(1, halfway.Alpha, 6);

			Advance(0.6, DefaultTransitionDuration);
			Assert.Same(Colors.Blue, box.BackgroundColor);
			Assert.False(box.HasAnimatedValue(VisualElement.BackgroundColorProperty));
		}

		[Fact]
		public void StateChangeFromAPropertysOwnChangeNotificationLeavesNoOrphanHold()
		{
			var box = CreateLoadedBox();
			Attach(box,
				new[] { new VisualState { Name = "A" }, State("B", (OpacityProperty, 0.8), (VisualElement.ScaleProperty, 2.0)) },
				new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			var reentered = false;
			box.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == OpacityProperty.PropertyName && !reentered)
				{
					reentered = true;
					VisualStateManager.GoToState(box, "B");
				}
			};

			// B is entered while Opacity is still being set: Opacity changes without animation.
			box.Opacity = 0.3;

			Assert.True(reentered);
			Assert.Equal(0.8, box.Opacity); // B's visual-state value beats the manual value, as without animation
			Assert.False(box.HasAnimatedValue(OpacityProperty));

			// Scale wasn't being set: it animates as usual.
			Assert.True(box.HasAnimatedValue(VisualElement.ScaleProperty));
			Assert.Equal(1.0, box.Scale);
			Advance(0.5, DefaultTransitionDuration);
			Assert.Equal(1.5, box.Scale, 9);
			Advance(0.6, DefaultTransitionDuration);
			Assert.Equal(2.0, box.Scale);
			Assert.False(box.HasAnimatedValue(VisualElement.ScaleProperty));

			// Nothing is left above Opacity: once B lets go of it, the manual value shows and manual writes land at once.
			VisualStateManager.GoToState(box, "A", useTransitions: false);
			Assert.Equal(0.3, box.Opacity);
			box.Opacity = 0.4;
			Assert.Equal(0.4, box.Opacity);
		}

		[Fact]
		public void HandlerThatThrowsDuringATransitionFrameFinishesTheTransition()
		{
			var box = CreateLoadedBox();
			AttachOpacityStates(box, 0.2, 1.0, new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");
			var armed = true;
			box.PropertyChanged += (_, e) =>
			{
				if (armed && e.PropertyName == OpacityProperty.PropertyName)
				{
					armed = false;
					throw new InvalidOperationException("boom");
				}
			};

			Advance(0.5, DefaultTransitionDuration);

			Assert.False(box.HasAnimatedValue(OpacityProperty));
			Assert.Equal(1.0, box.Opacity);
			var changes = new ChangeRecorder(box, OpacityProperty);
			Advance(1.0, DefaultTransitionDuration);
			changes.AssertNone();
		}

		// Without a transition, GoToState catches nothing: exception dispatch is exactly as before, with the caller's
		// filter running in the first pass, before inner finally blocks. With one, the exception still propagates
		// after the pins are released; the order then isn't asserted.
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void SetterExceptionsPropagateFromGoToState(bool withTransition)
		{
			var box = Load(new LoggingBox());
			Attach(box,
				new[]
				{
					State("A", (LoggingBox.FragileProperty, 1.0), (OpacityProperty, 0.2)),
					State("X", (OpacityProperty, 1.0), (LoggingBox.FragileProperty, LoggingBox.Breaking)),
				},
				withTransition ? new[] { new VisualTransition() } : Array.Empty<VisualTransition>());
			VisualStateManager.GoToState(box, "A");
			var caught = false;

			try
			{
				VisualStateManager.GoToState(box, "X");
			}
			catch (SetterFailedException) when (Record(box.Log, "caller filter"))
			{
				caught = true;
			}

			Assert.True(caught);
			if (withTransition)
			{
				Assert.False(box.HasAnimatedValue(OpacityProperty));
				Assert.False(box.HasAnimatedValue(LoggingBox.FragileProperty));
			}
			else
			{
				Assert.Equal(new[] { "caller filter", "inner finally" }, box.Log);
			}
		}

		[Fact]
		public void InstantStateChangeFromAPropertyChangingHandlerLeavesNoOrphanHold()
		{
			var box = CreateLoadedBox();
			box.Opacity = 0.5;
			Attach(box,
				new[] { State("A", OpacityProperty, 0.2), State("B", OpacityProperty, 1.0), State("Other", VisualElement.ScaleProperty, 1.5) },
				new VisualTransition { Easing = Easing.Linear });
			VisualStateManager.GoToState(box, "A");
			VisualStateManager.GoToState(box, "B");
			Advance(0.5, DefaultTransitionDuration);
			var armed = true;
			box.PropertyChanging += (_, e) =>
			{
				if (armed && e.PropertyName == OpacityProperty.PropertyName)
				{
					armed = false;
					VisualStateManager.GoToState(box, "Other", useTransitions: false);
				}
			};

			// The next frame's write raises PropertyChanging, whose handler ends the transition before the frame is stored.
			Advance(0.1, DefaultTransitionDuration);

			Assert.False(armed);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			Assert.Equal(0.5, box.Opacity);
			var changes = new ChangeRecorder(box, OpacityProperty);
			Advance(1.0, DefaultTransitionDuration);
			changes.AssertNone();
			box.Opacity = 0.4;
			Assert.Equal(0.4, box.Opacity);
		}

		static bool Record(List<string> log, string entry)
		{
			log.Add(entry);
			return true;
		}

		// A pin is a write too: when one throws, the pins made before it must not stay held either.
		[Fact]
		public void StateChangeWhosePinThrowsReleasesWhatItPinned()
		{
			var box = Load(new ArmedBox());
			Attach(box,
				new[]
				{
					// Tracks pin in reverse order: Opacity is pinned before Armed throws.
					State("A", (ArmedBox.ArmedProperty, 1.0), (OpacityProperty, 0.2)),
					State("B", (OpacityProperty, 1.0)),
				},
				new VisualTransition());
			VisualStateManager.GoToState(box, "A");
			box.ThrowOnCoerce = true;

			Assert.Throws<InvalidOperationException>(() => VisualStateManager.GoToState(box, "B"));

			box.ThrowOnCoerce = false;
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			Assert.False(box.HasAnimatedValue(ArmedBox.ArmedProperty));
		}

		[Fact]
		public void StateChangeThatThrowsReleasesItsPins()
		{
			var box = Load(new FragileBox());
			Attach(box,
				new[]
				{
					State("A", (OpacityProperty, 0.2), (FragileBox.FragileProperty, 1.0)),
					State("B", (OpacityProperty, 0.7), (FragileBox.FragileProperty, FragileBox.Breaking)),
				},
				new VisualTransition());
			VisualStateManager.GoToState(box, "A");

			Assert.Throws<InvalidOperationException>(() => VisualStateManager.GoToState(box, "B"));

			Assert.False(box.HasAnimatedValue(OpacityProperty));
			Assert.False(box.HasAnimatedValue(FragileBox.FragileProperty));
			// As without a transition: setters applied before the failing one stay applied.
			Assert.Equal(0.7, box.Opacity);
			Assert.Equal(0.0, (double)box.GetValue(FragileBox.FragileProperty));

			var changes = new ChangeRecorder(box, OpacityProperty);
			Advance(1.0, DefaultTransitionDuration);
			Assert.Equal(0.7, box.Opacity);
			changes.AssertNone();
		}

		static VisualState State(string name, BindableProperty property, object value)
			=> new VisualState { Name = name, Setters = { new Setter { Property = property, Value = value } } };

		static VisualState State(string name, params (BindableProperty Property, object Value)[] setters)
		{
			var state = new VisualState { Name = name };
			foreach (var (property, value) in setters)
				state.Setters.Add(new Setter { Property = property, Value = value });
			return state;
		}

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

		// Coercion runs before any value is stored, so a setter of Breaking throws even beneath an animation hold.
		sealed class FragileBox : BoxView
		{
			public const double Breaking = 666;

			public static readonly BindableProperty FragileProperty = BindableProperty.Create("Fragile", typeof(double), typeof(FragileBox), 0.0,
				coerceValue: (_, value) => (double)value == Breaking ? throw new InvalidOperationException("The setter failed.") : value);
		}

		// Coercion of Breaking throws a SetterFailedException from inside a try block whose finally block logs.
		sealed class LoggingBox : BoxView
		{
			public const double Breaking = 666;

			public static readonly BindableProperty FragileProperty = BindableProperty.Create("Fragile", typeof(double), typeof(LoggingBox), 0.0,
				coerceValue: (bindable, value) =>
				{
					if ((double)value == Breaking)
					{
						try
						{
							throw new SetterFailedException();
						}
						finally
						{
							((LoggingBox)bindable).Log.Add("inner finally");
						}
					}

					return value;
				});

			public List<string> Log { get; } = new List<string>();
		}

		sealed class SetterFailedException : Exception
		{
		}

		// Coercion that throws for any value while armed, including the property's current value.
		sealed class ArmedBox : BoxView
		{
			public static readonly BindableProperty ArmedProperty = BindableProperty.Create("Armed", typeof(double), typeof(ArmedBox), 0.0,
				coerceValue: (bindable, value) => ((ArmedBox)bindable).ThrowOnCoerce ? throw new InvalidOperationException("The coercion failed.") : value);

			public bool ThrowOnCoerce { get; set; }
		}
	}
}
