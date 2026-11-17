using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	// AnimationClocks.ClockOverride and MotionPolicy.ReducedMotionOverride are process-wide, so these
	// tests must never run at the same time as each other or as any other collection.
	[CollectionDefinition(Name, DisableParallelization = true)]
	public sealed class AnimationsCollection
	{
		public const string Name = "Animations";
	}

	public abstract class AnimationTestBase : BaseTestFixture
	{
		protected static readonly TimeSpan DefaultTransitionDuration = TimeSpan.FromMilliseconds(150);

		protected AnimationTestBase()
		{
			AnimationClocks.ClockOverride = Clock;
			MotionPolicy.ReducedMotionOverride = false;
		}

		protected ManualStoryboardClock Clock { get; } = new ManualStoryboardClock();

		protected void Advance(double milliseconds) => Clock.Advance(TimeSpan.FromMilliseconds(milliseconds));

		protected void Advance(double fraction, TimeSpan duration) => Clock.Advance(TimeSpan.FromTicks((long)(duration.Ticks * fraction)));

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				AnimationClocks.ClockOverride = null;
				MotionPolicy.ReducedMotionOverride = null;
			}

			base.Dispose(disposing);
		}

		// On this TFM IsLoaded means "has a Window"; visual-state transitions also need a handler.
		protected static BoxView CreateLoadedBox() => Load(new BoxView());

		protected static TView Load<TView>(TView view)
			where TView : View
		{
			_ = new Window(new ContentPage { Content = view });
			view.Handler = new HandlerStub();
			Assert.True(view.IsLoaded);
			return view;
		}

		protected static BoxView CreateWindowedBox()
		{
			var box = new BoxView();
			_ = new Window(new ContentPage { Content = box });
			return box;
		}
	}

	/// <summary>Counts PropertyChanging and PropertyChanged notifications for one property.</summary>
	internal sealed class ChangeRecorder
	{
		readonly string _propertyName;

		public ChangeRecorder(BindableObject target, BindableProperty property)
		{
			_propertyName = property.PropertyName;
			target.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == _propertyName)
					Changing++;
			};
			target.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == _propertyName)
					Changed++;
			};
		}

		public int Changing { get; private set; }

		public int Changed { get; private set; }

		public void AssertNone()
		{
			Assert.Equal(0, Changing);
			Assert.Equal(0, Changed);
		}

		public void AssertOnePair()
		{
			Assert.Equal(1, Changing);
			Assert.Equal(1, Changed);
		}
	}

	internal sealed class RecordingTrackOwner : IAnimationTrackOwner
	{
		public List<TrackLossReason> Losses { get; } = new List<TrackLossReason>();

		public void OnTrackLost(BindableObject target, BindableProperty property, TrackLossReason reason) => Losses.Add(reason);

		/// <summary>Takes ownership of <paramref name="property"/> and shows <paramref name="value"/> at animation rank.</summary>
		public static RecordingTrackOwner Hold(BindableObject target, BindableProperty property, object value)
		{
			var owner = new RecordingTrackOwner();
			AnimationLayer.Acquire(target, property, owner);
			AnimationLayer.Write(target, property, value);
			return owner;
		}
	}

	internal sealed class ValueSource : INotifyPropertyChanged
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

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
