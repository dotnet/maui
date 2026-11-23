using System.ComponentModel;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	// The animation rank (SetterSpecificity.Animation): an animation shows a value without owning the property.
	public class AnimationLayerTests : AnimationTestBase
	{
		class ViewModel : INotifyPropertyChanged
		{
			double _value;

			public double Value
			{
				get => _value;
				set
				{
					_value = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
				}
			}

			public int SetCount { get; set; }

			public event PropertyChangedEventHandler PropertyChanged;
		}

		[Fact]
		public void AnimationRankSitsBetweenVisualStatesAndHandler()
		{
			var highestVsm = new SetterSpecificity(1, 0xFFFF, 1, 1, 0, 0xFF, 0xFF, 0xFF);

			Assert.True(SetterSpecificity.Animation > highestVsm);
			Assert.True(SetterSpecificity.Animation > SetterSpecificity.ManualValueSetter);
			Assert.True(SetterSpecificity.Animation > SetterSpecificity.VisualStateSetter);
			Assert.True(SetterSpecificity.Animation < SetterSpecificity.FromHandler);
			Assert.False(SetterSpecificity.Animation.IsVsm);
			Assert.False(SetterSpecificity.Animation.IsManual);
			Assert.False(SetterSpecificity.Animation.IsBinding);
			Assert.False(SetterSpecificity.Animation.IsDynamicResource);
			Assert.Equal(SetterSpecificity.Animation, SetterSpecificity.Animation.WithFullVsmPriority());
		}

		[Fact]
		public void HoldShowsValueAndRaisesOneNotification()
		{
			var label = new Label { Opacity = 0.5 };
			var counter = new ChangeCounter(label, nameof(Label.Opacity));

			var result = label.SetAnimationValue(VisualElement.OpacityProperty, 0.2, new TestOwner());

			Assert.Equal(AnimationWriteResult.Written, result);
			Assert.Equal(0.2, label.Opacity);
			Assert.Equal(0.5, label.GetAnimationBaseValue(VisualElement.OpacityProperty));
			Assert.Equal(1, counter.Changed);
			Assert.Equal(1, counter.Changing);
		}

		[Fact]
		public void LowerWritesDuringHoldAreStoredWithoutNotification()
		{
			var label = new Label();
			var owner = new TestOwner();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.2, owner);
			var counter = new ChangeCounter(label, nameof(Label.Opacity));

			label.Opacity = 0.7;

			Assert.Equal(0.2, label.Opacity);
			Assert.Equal(0.7, label.GetAnimationBaseValue(VisualElement.OpacityProperty));
			Assert.Equal(0, counter.Changed);
			Assert.Equal(1, owner.BaseValueChanges);
		}

		[Fact]
		public void ReleaseShowsLatestBaseValueWithExactlyOneNotification()
		{
			var label = new Label();
			var owner = new TestOwner();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.2, owner);
			label.Opacity = 0.7;
			var counter = new ChangeCounter(label, nameof(Label.Opacity));

			label.ClearAnimationValue(VisualElement.OpacityProperty, owner);

			Assert.Equal(0.7, label.Opacity);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
			Assert.Equal(1, counter.Changed);
			Assert.Equal(1, counter.Changing);
		}

		[Fact]
		public void ReleaseOnEqualValueRaisesNothing()
		{
			var label = new Label { Opacity = 0.3 };
			var owner = new TestOwner();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.3, owner);
			var counter = new ChangeCounter(label, nameof(Label.Opacity));

			label.ClearAnimationValue(VisualElement.OpacityProperty, owner);

			Assert.Equal(0, counter.Changed);
		}

		[Fact]
		public void ReleaseByAnotherOwnerIsIgnored()
		{
			var label = new Label();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.2, new TestOwner());

			label.ClearAnimationValue(VisualElement.OpacityProperty, new TestOwner());

			Assert.Equal(0.2, label.Opacity);
		}

		[Fact]
		public void TakeOverReplacesInPlaceWithoutNotifyingAtTheSwitch()
		{
			var label = new Label();
			var first = new TestOwner();
			var second = new TestOwner();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.4, first);
			var counter = new ChangeCounter(label, nameof(Label.Opacity));

			label.SetAnimationValue(VisualElement.OpacityProperty, 0.4, second);

			Assert.Equal(0, counter.Changed);
			Assert.Equal(AnimationHoldLossReason.TakenOver, first.LostReason);
			Assert.Null(second.LostReason);
			Assert.Same(second, label.GetAnimationOwner(VisualElement.OpacityProperty));
		}

		[Fact]
		public void HandlerWriteEndsHoldAndStands()
		{
			var entry = new Entry { Text = "base" };
			var owner = new TestOwner();
			entry.SetAnimationValue(Entry.TextProperty, "animated", owner);

			entry.SetValueFromRenderer(Entry.TextProperty, "typed");

			Assert.Equal("typed", entry.Text);
			Assert.False(entry.HasAnimationValue(Entry.TextProperty));
			Assert.Equal(AnimationHoldLossReason.HandlerWrite, owner.LostReason);

			// Releasing afterwards keeps the user's value.
			entry.ClearAnimationValue(Entry.TextProperty, owner);
			Assert.Equal("typed", entry.Text);
		}

		[Fact]
		public void HandlerEchoOfShownValueDoesNotEndHold()
		{
			var label = new Label { Opacity = 0.9 };
			var owner = new TestOwner();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.4, owner);

			label.SetValueFromRenderer(VisualElement.OpacityProperty, 0.4);

			Assert.True(label.HasAnimationValue(VisualElement.OpacityProperty));
			Assert.Null(owner.LostReason);
			label.ClearAnimationValue(VisualElement.OpacityProperty, owner);
			Assert.Equal(0.9, label.Opacity);
		}

		[Fact]
		public void AnimatedValuesNeverReachABindingSource()
		{
			var vm = new ViewModel { Value = 0.5 };
			var label = new Label { BindingContext = vm };
			label.SetBinding(VisualElement.OpacityProperty, new Binding(nameof(ViewModel.Value), BindingMode.OneWay));
			var owner = new TestOwner();

			label.SetAnimationValue(VisualElement.OpacityProperty, 0.1, owner);

			Assert.Equal(0.1, label.Opacity);
			Assert.Equal(0.5, vm.Value);
		}

		[Fact]
		public void TwoWayBoundPropertiesAreExcluded()
		{
			var vm = new ViewModel { Value = 0.5 };
			var label = new Label { BindingContext = vm };
			label.SetBinding(VisualElement.OpacityProperty, new Binding(nameof(ViewModel.Value), BindingMode.TwoWay));

			var result = label.SetAnimationValue(VisualElement.OpacityProperty, 0.1, new TestOwner());

			Assert.Equal(AnimationWriteResult.Excluded, result);
			Assert.Equal(0.5, label.Opacity);
			Assert.Equal(0.5, vm.Value);
		}

		[Fact]
		public void BindingUpdatesDuringHoldShowOnRelease()
		{
			var vm = new ViewModel { Value = 0.5 };
			var label = new Label { BindingContext = vm };
			label.SetBinding(VisualElement.OpacityProperty, new Binding(nameof(ViewModel.Value)));
			var owner = new TestOwner();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.1, owner);

			vm.Value = 0.8;
			Assert.Equal(0.1, label.Opacity);

			label.ClearAnimationValue(VisualElement.OpacityProperty, owner);
			Assert.Equal(0.8, label.Opacity);

			// The binding is still live.
			vm.Value = 0.6;
			Assert.Equal(0.6, label.Opacity);
		}

		[Fact]
		public void OneWaySetBindingPreservesHold()
		{
			var vm = new ViewModel { Value = 0.5 };
			var label = new Label { BindingContext = vm, Opacity = 0.9 };
			var owner = new TestOwner();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.1, owner);

			label.SetBinding(VisualElement.OpacityProperty, new Binding(nameof(ViewModel.Value)));

			Assert.Equal(0.1, label.Opacity);
			Assert.True(label.HasAnimationValue(VisualElement.OpacityProperty));
			label.ClearAnimationValue(VisualElement.OpacityProperty, owner);
			Assert.Equal(0.5, label.Opacity);
		}

		[Fact]
		public void TwoWaySetBindingDuringHoldEndsItAndSourceNeverSeesAnimatedValue()
		{
			var vm = new ViewModel { Value = 0.5 };
			var label = new Label { BindingContext = vm };
			var owner = new TestOwner();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.1, owner);

			label.SetBinding(VisualElement.OpacityProperty, new Binding(nameof(ViewModel.Value), BindingMode.TwoWay));

			Assert.Equal(AnimationHoldLossReason.Excluded, owner.LostReason);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
			Assert.Equal(0.5, label.Opacity);
			Assert.Equal(0.5, vm.Value);
		}

		[Fact]
		public void DirectDynamicResourceSurvivesAnimation()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary { ["Accent"] = Colors.Red };
			label.SetDynamicResource(Label.TextColorProperty, "Accent");
			var owner = new TestOwner();

			label.SetAnimationValue(Label.TextColorProperty, Colors.Blue, owner);
			label.ClearAnimationValue(Label.TextColorProperty, owner);
			label.Resources["Accent"] = Colors.Green;

			Assert.Equal(Colors.Green, label.TextColor);
		}

		[Fact]
		public void DynamicResourceChangeDuringHoldShowsOnRelease()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary { ["Accent"] = Colors.Red };
			label.SetDynamicResource(Label.TextColorProperty, "Accent");
			var owner = new TestOwner();
			label.SetAnimationValue(Label.TextColorProperty, Colors.Blue, owner);

			label.Resources["Accent"] = Colors.Green;
			Assert.Equal(Colors.Blue, label.TextColor);

			label.ClearAnimationValue(Label.TextColorProperty, owner);
			Assert.Equal(Colors.Green, label.TextColor);
		}

		[Fact]
		public void ManualWriteDuringHoldDetachesDynamicResourceAsItWouldWithoutAnimation()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary { ["Accent"] = Colors.Red };
			label.SetDynamicResource(Label.TextColorProperty, "Accent");
			var owner = new TestOwner();
			label.SetAnimationValue(Label.TextColorProperty, Colors.Blue, owner);

			label.TextColor = Colors.Yellow;
			label.ClearAnimationValue(Label.TextColorProperty, owner);
			label.Resources["Accent"] = Colors.Green;

			Assert.Equal(Colors.Yellow, label.TextColor);
		}

		[Fact]
		public void StyleValuesBeneathHoldWinAgainAfterRelease()
		{
			var style = new Style(typeof(Label)) { Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = 0.6 } } };
			var label = new Label { Style = style };
			var owner = new TestOwner();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.1, owner);

			label.ClearAnimationValue(VisualElement.OpacityProperty, owner);

			Assert.Equal(0.6, label.Opacity);
		}

		[Fact]
		public void CommitKeepsValueWithoutNotificationAndBindingStaysLive()
		{
			var vm = new ViewModel { Value = 0.5 };
			var label = new Label { BindingContext = vm };
			label.SetBinding(VisualElement.OpacityProperty, new Binding(nameof(ViewModel.Value)));
			var owner = new TestOwner();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.2, owner);
			var counter = new ChangeCounter(label, nameof(Label.Opacity));

			label.CommitAnimationValue(VisualElement.OpacityProperty, 0.2, owner);

			Assert.Equal(0.2, label.Opacity);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
			Assert.Equal(0, counter.Changed);
			Assert.Equal(0.5, vm.Value);

			vm.Value = 0.9;
			Assert.Equal(0.9, label.Opacity);
		}

		[Fact]
		public void CommitOverDefaultStaysAtDefaultRank()
		{
			var label = new Label();
			var owner = new TestOwner();
			label.SetAnimationValue(VisualElement.OpacityProperty, 0.2, owner);

			label.CommitAnimationValue(VisualElement.OpacityProperty, 0.2, owner);

			Assert.Equal(0.2, label.Opacity);
			Assert.False(label.IsSet(VisualElement.OpacityProperty));

			label.Opacity = 0.8;
			Assert.Equal(0.8, label.Opacity);
		}

		[Fact]
		public void IsSetReportsTheValueBeneathTheHold()
		{
			var label = new Label();

			label.SetAnimationValue(VisualElement.OpacityProperty, 0.2, new TestOwner());

			Assert.False(label.IsSet(VisualElement.OpacityProperty));
		}

		[Fact]
		public void WritesFromThePropertysOwnNotificationAreRefused()
		{
			var label = new Label();
			var owner = new TestOwner();
			AnimationWriteResult? inner = null;
			label.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.Opacity) && inner is null)
					inner = label.SetAnimationValue(VisualElement.OpacityProperty, 0.3, owner);
			};

			label.Opacity = 0.5;

			Assert.Equal(AnimationWriteResult.Refused, inner);
			Assert.Equal(0.5, label.Opacity);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
		}

		[Fact]
		public void ThrowingHandlerLeavesPropertyUsable()
		{
			var label = new Label();
			var owner = new TestOwner();
			var throwOnce = true;
			label.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.Opacity) && throwOnce)
				{
					throwOnce = false;
					throw new System.InvalidOperationException("handler");
				}
			};

			Assert.Throws<System.InvalidOperationException>(() => label.SetAnimationValue(VisualElement.OpacityProperty, 0.3, owner));

			label.ClearAnimationValue(VisualElement.OpacityProperty, owner);
			label.Opacity = 0.7;
			Assert.Equal(0.7, label.Opacity);
		}

		[Fact]
		public void ReadOnlyPropertiesAreExcluded()
		{
			var label = new Label();

			Assert.Equal(AnimationWriteResult.Excluded, label.SetAnimationValue(VisualElement.WidthProperty, 10.0, new TestOwner()));
		}

		[Fact]
		public void PlainWritesAreUnchangedWithoutAHold()
		{
			var label = new Label();
			var counter = new ChangeCounter(label, nameof(Label.Opacity));

			label.Opacity = 0.4;
			label.Opacity = 0.4;
			label.ClearValue(VisualElement.OpacityProperty);

			Assert.Equal(1.0, label.Opacity);
			Assert.Equal(2, counter.Changed);
		}
	}
}
