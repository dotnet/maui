using System;
using Microsoft.Maui.Controls.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	[Collection(AnimationsCollection.Name)]
	public class AnimationLayerTests : AnimationTestBase
	{
		static readonly BindableProperty OpacityProperty = VisualElement.OpacityProperty;

		public enum LowerSource
		{
			Manual,
			Style,
			Binding,
			DynamicResource,
			VisualState,
		}

		[Fact]
		public void AnimationRanksAboveEverySourceExceptHandler()
		{
			var lower = new (string Name, SetterSpecificity Specificity)[]
			{
				("default value", SetterSpecificity.DefaultValue),
				("binding", SetterSpecificity.FromBinding),
				("dynamic resource", SetterSpecificity.DynamicResourceSetter),
				("manual value", SetterSpecificity.ManualValueSetter),
				("trigger", SetterSpecificity.Trigger),
				("visual state", SetterSpecificity.VisualStateSetter),
				("implicit style", new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0)),
				("BasedOn style", new SetterSpecificity(SetterSpecificity.StyleBasedOn, 0, 0, 0)),
				("local style", new SetterSpecificity(SetterSpecificity.StyleLocal, 0, 0, 0)),
				("visual state from a local style", new SetterSpecificity(SetterSpecificity.StyleLocal, 0, 0, 0).CopyStyle(1, 0, 0, 0)),
				("promoted visual state from an implicit style", new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0).CopyStyle(1, 0, 0, 0).WithFullVsmPriority()),
				("highest visual state + trigger + resource + binding combination", new SetterSpecificity(1, ushort.MaxValue, 1, 1, 0, 0xFF, 0xFF, 0xFF)),
			};

			foreach (var (name, specificity) in lower)
				Assert.True(SetterSpecificity.Animation > specificity, $"Animation must outrank a {name}.");

			Assert.True(SetterSpecificity.FromHandler > SetterSpecificity.Animation, "A handler write must outrank Animation.");

			var animation = SetterSpecificity.Animation;
			Assert.False(animation.IsVsm);
			Assert.False(animation.IsVsmImplicit);
			Assert.False(animation.IsManual);
			Assert.False(animation.IsBinding);
			Assert.False(animation.IsDynamicResource);
			Assert.False(animation.IsHandler);
		}

		[Theory]
		[InlineData(LowerSource.Manual)]
		[InlineData(LowerSource.Style)]
		[InlineData(LowerSource.Binding)]
		[InlineData(LowerSource.DynamicResource)]
		[InlineData(LowerSource.VisualState)]
		public void LowerWriteBeneathHoldIsSilentAndSurfacesOnRelease(LowerSource source)
		{
			var box = new BoxView();
			var updateTo07 = ArrangeOpacity(box, source);
			Assert.Equal(0.5, box.Opacity);

			var owner = RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);
			var changes = new ChangeRecorder(box, OpacityProperty);

			updateTo07();

			Assert.Equal(0.2, box.Opacity);
			Assert.True(box.HasAnimatedValue(OpacityProperty));
			changes.AssertNone();
			Assert.Equal(0.7, (double)AnimationLayer.GetBaseValue(box, OpacityProperty));

			AnimationLayer.Release(box, OpacityProperty, owner);

			Assert.Equal(0.7, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			Assert.Null(AnimationLayer.GetOwner(box, OpacityProperty));
			changes.AssertOnePair();
		}

		[Fact]
		public void ReleaseIsSilentWhenTheBaseValueEqualsTheShownValue()
		{
			var box = new BoxView { Opacity = 0.5 };
			var owner = RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);
			box.Opacity = 0.2;
			var changes = new ChangeRecorder(box, OpacityProperty);

			AnimationLayer.Release(box, OpacityProperty, owner);

			Assert.Equal(0.2, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			changes.AssertNone();
		}

		[Fact]
		public void BaseValueIsTheHighestEntryBeneathTheHold()
		{
			var box = new BoxView();
			Assert.Equal(1.0, (double)box.GetAnimationBaseValue(OpacityProperty, out var specificity));
			Assert.Equal(SetterSpecificity.DefaultValue, specificity);

			box.Opacity = 0.5;
			VisualStateManager.SetVisualStateGroups(box, new VisualStateGroupList
			{
				new VisualStateGroup
				{
					States = { new VisualState { Name = "X", Setters = { new Setter { Property = OpacityProperty, Value = 0.3 } } } },
				},
			});
			VisualStateManager.GoToState(box, "X");
			RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);

			Assert.Equal(0.2, box.Opacity);
			Assert.Equal(0.3, (double)box.GetAnimationBaseValue(OpacityProperty, out specificity));
			Assert.True(specificity.IsVsm);
		}

		[Fact]
		public void AcquireByAnotherOwnerTakesTheTrackOverInPlace()
		{
			var box = new BoxView { Opacity = 0.5 };
			var first = RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);
			var changes = new ChangeRecorder(box, OpacityProperty);
			var second = new RecordingTrackOwner();

			AnimationLayer.Acquire(box, OpacityProperty, second);

			Assert.Equal(TrackLossReason.TakenOver, Assert.Single(first.Losses));
			Assert.Same(second, AnimationLayer.GetOwner(box, OpacityProperty));
			Assert.True(box.HasAnimatedValue(OpacityProperty));
			Assert.Equal(0.2, box.Opacity);
			changes.AssertNone();

			// Re-acquiring by the current owner is a no-op, and the previous owner can no longer release.
			AnimationLayer.Acquire(box, OpacityProperty, second);
			AnimationLayer.Release(box, OpacityProperty, first);

			Assert.Empty(second.Losses);
			Assert.True(box.HasAnimatedValue(OpacityProperty));
			Assert.Equal(0.2, box.Opacity);
			changes.AssertNone();
		}

		[Fact]
		public void CommitKeepsTheShownValueSilentlyWithoutTakingOwnership()
		{
			var box = new BoxView { Opacity = 0.5 };
			var owner = RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);
			var changes = new ChangeRecorder(box, OpacityProperty);

			AnimationLayer.CommitAndRelease(box, OpacityProperty, 0.2, owner);

			Assert.Equal(0.2, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			Assert.Null(AnimationLayer.GetOwner(box, OpacityProperty));
			changes.AssertNone();
			Assert.Equal(0.2, (double)box.GetAnimationBaseValue(OpacityProperty, out var specificity));
			Assert.Equal(SetterSpecificity.ManualValueSetter, specificity);
		}

		[Fact]
		public void CommitKeepsAOneWayBindingAttached()
		{
			var source = new ValueSource { Value = 0.5 };
			var box = new BoxView();
			box.SetBinding(OpacityProperty, new Binding(nameof(ValueSource.Value), source: source));
			var owner = RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);
			var changes = new ChangeRecorder(box, OpacityProperty);

			AnimationLayer.CommitAndRelease(box, OpacityProperty, 0.2, owner);

			Assert.Equal(0.2, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			changes.AssertNone();

			source.Value = 0.9;

			Assert.Equal(0.9, box.Opacity);
		}

		[Fact]
		public void CommitKeepsADynamicResourceAttached()
		{
			var box = new BoxView { Resources = new ResourceDictionary { { "opacity", 0.5 } } };
			box.SetDynamicResource(OpacityProperty, "opacity");
			var owner = RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);
			var changes = new ChangeRecorder(box, OpacityProperty);

			AnimationLayer.CommitAndRelease(box, OpacityProperty, 0.2, owner);

			Assert.Equal(0.2, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			changes.AssertNone();

			box.Resources["opacity"] = 0.8;

			Assert.Equal(0.8, box.Opacity);
		}

		[Fact]
		public void CommitOverTheDefaultValueLeavesThePropertyUnset()
		{
			var box = new BoxView();
			var owner = RecordingTrackOwner.Hold(box, VisualElement.TranslationXProperty, 40.0);

			AnimationLayer.CommitAndRelease(box, VisualElement.TranslationXProperty, 40.0, owner);

			Assert.Equal(40, box.TranslationX);
			Assert.False(box.HasAnimatedValue(VisualElement.TranslationXProperty));
			Assert.False(box.IsSet(VisualElement.TranslationXProperty));
		}

		[Fact]
		public void TwoWayOneWayToSourceAndHandlerValuesAreExcludedFromAnimation()
		{
			var twoWay = new BoxView();
			twoWay.SetBinding(OpacityProperty, new Binding(nameof(ValueSource.Value), BindingMode.TwoWay, source: new ValueSource { Value = 0.5 }));
			var toSource = new BoxView();
			toSource.SetBinding(OpacityProperty, new Binding(nameof(ValueSource.Value), BindingMode.OneWayToSource, source: new ValueSource()));
			var fromHandler = new BoxView();
			fromHandler.SetValueFromRenderer(OpacityProperty, 0.4);
			var oneWay = new BoxView();
			oneWay.SetBinding(OpacityProperty, new Binding(nameof(ValueSource.Value), BindingMode.OneWay, source: new ValueSource { Value = 0.5 }));

			Assert.True(twoWay.IsExcludedFromAnimation(OpacityProperty));
			Assert.True(toSource.IsExcludedFromAnimation(OpacityProperty));
			Assert.True(fromHandler.IsExcludedFromAnimation(OpacityProperty));
			Assert.False(oneWay.IsExcludedFromAnimation(OpacityProperty));
			Assert.False(new BoxView { Opacity = 0.3 }.IsExcludedFromAnimation(OpacityProperty));
			Assert.False(new BoxView().IsExcludedFromAnimation(OpacityProperty));
		}

		[Fact]
		public void AnimatedValuesNeverFlowBackToABindingSource()
		{
			var source = new ValueSource { Value = 0.5 };
			var box = new BoxView();
			box.SetBinding(OpacityProperty, new Binding(nameof(ValueSource.Value), BindingMode.OneWayToSource, source: source));
			Assert.Equal(1.0, source.Value);

			RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);
			AnimationLayer.Write(box, OpacityProperty, 0.3);

			Assert.Equal(0.3, box.Opacity);
			Assert.Equal(1.0, source.Value);
		}

		[Fact]
		public void HandlerWriteOfANewValueEndsTheHold()
		{
			var box = new BoxView { Opacity = 0.5 };
			var owner = RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);
			var changes = new ChangeRecorder(box, OpacityProperty);

			box.SetValueFromRenderer(OpacityProperty, 0.9);

			Assert.Equal(0.9, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			Assert.Null(AnimationLayer.GetOwner(box, OpacityProperty));
			Assert.Equal(TrackLossReason.HandlerWrite, Assert.Single(owner.Losses));
			changes.AssertOnePair();
		}

		[Fact]
		public void HandlerEchoOfTheShownValueKeepsTheHold()
		{
			var box = new BoxView { Opacity = 0.5 };
			var owner = RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);
			var changes = new ChangeRecorder(box, OpacityProperty);

			box.SetValueFromRenderer(OpacityProperty, 0.2);

			Assert.Equal(0.2, box.Opacity);
			Assert.True(box.HasAnimatedValue(OpacityProperty));
			Assert.Same(owner, AnimationLayer.GetOwner(box, OpacityProperty));
			Assert.Empty(owner.Losses);
			changes.AssertNone();

			// The hold still hides lower writes and keeps animating.
			box.Opacity = 0.6;
			AnimationLayer.Write(box, OpacityProperty, 0.25);

			Assert.Equal(0.25, box.Opacity);
			Assert.Equal(0.6, (double)AnimationLayer.GetBaseValue(box, OpacityProperty));
		}

		[Fact]
		public void ReleaseAfterAHandlerEchoPromotesTheBaseValue()
		{
			var box = new BoxView { Opacity = 0.5 };
			var owner = RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);
			box.SetValueFromRenderer(OpacityProperty, 0.2);
			var changes = new ChangeRecorder(box, OpacityProperty);

			AnimationLayer.Release(box, OpacityProperty, owner);

			// The echo was the animation's own value reflected back; it must not outlive the hold.
			Assert.Equal(0.5, box.Opacity);
			Assert.False(box.HasAnimatedValue(OpacityProperty));
			Assert.False(box.IsExcludedFromAnimation(OpacityProperty));
			changes.AssertOnePair();
		}

		[Fact]
		public void SetBindingOnAHeldPropertyKeepsTheHoldAndBecomesTheBase()
		{
			var box = new BoxView { Opacity = 0.5 };
			var owner = RecordingTrackOwner.Hold(box, OpacityProperty, 0.2);
			var changes = new ChangeRecorder(box, OpacityProperty);
			var source = new ValueSource { Value = 0.8 };

			box.SetBinding(OpacityProperty, new Binding(nameof(ValueSource.Value), source: source));

			Assert.Equal(0.2, box.Opacity);
			Assert.True(box.HasAnimatedValue(OpacityProperty));
			changes.AssertNone();
			Assert.Equal(0.8, (double)box.GetAnimationBaseValue(OpacityProperty, out var specificity));
			Assert.Equal(SetterSpecificity.FromBinding, specificity);

			AnimationLayer.Release(box, OpacityProperty, owner);

			Assert.Equal(0.8, box.Opacity);
			changes.AssertOnePair();

			source.Value = 0.6;

			Assert.Equal(0.6, box.Opacity);
		}

		static Action ArrangeOpacity(BoxView box, LowerSource source)
		{
			switch (source)
			{
				case LowerSource.Manual:
					box.Opacity = 0.5;
					return () => box.Opacity = 0.7;
				case LowerSource.Style:
					box.Style = OpacityStyle(0.5);
					return () => box.Style = OpacityStyle(0.7);
				case LowerSource.Binding:
					var valueSource = new ValueSource { Value = 0.5 };
					box.SetBinding(OpacityProperty, new Binding(nameof(ValueSource.Value), source: valueSource));
					return () => valueSource.Value = 0.7;
				case LowerSource.DynamicResource:
					box.Resources = new ResourceDictionary { { "opacity", 0.5 } };
					box.SetDynamicResource(OpacityProperty, "opacity");
					return () => box.Resources["opacity"] = 0.7;
				case LowerSource.VisualState:
					VisualStateManager.SetVisualStateGroups(box, new VisualStateGroupList
					{
						new VisualStateGroup
						{
							States =
							{
								new VisualState { Name = "X", Setters = { new Setter { Property = OpacityProperty, Value = 0.5 } } },
								new VisualState { Name = "Y", Setters = { new Setter { Property = OpacityProperty, Value = 0.7 } } },
							},
						},
					});
					VisualStateManager.GoToState(box, "X");
					return () => VisualStateManager.GoToState(box, "Y");
				default:
					throw new ArgumentOutOfRangeException(nameof(source));
			}
		}

		static Style OpacityStyle(double opacity) => new Style(typeof(BoxView))
		{
			Setters = { new Setter { Property = OpacityProperty, Value = opacity } },
		};
	}
}
