using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PropertyChangeEventArgsCacheUnitTests
	{
		static PropertyChangeEventArgsCache<PropertyChangedEventArgs> CreateCache(int capacity)
			=> new(static name => new PropertyChangedEventArgs(name), capacity);

		[Fact]
		public void ReturnsTheSameInstanceForTheSameName()
		{
			var cache = CreateCache(8);

			Assert.Same(cache.Get("Text"), cache.Get("Text"));
		}

		[Fact]
		public void ReturnsArgsCarryingTheRequestedName()
		{
			var cache = CreateCache(8);

			Assert.Equal("Text", cache.Get("Text").PropertyName);
		}

		[Fact]
		public void DistinctNamesGetDistinctArgs()
		{
			var cache = CreateCache(8);

			Assert.NotSame(cache.Get("Text"), cache.Get("Title"));
		}

		[Fact]
		public void ComparesNamesOrdinally()
		{
			var cache = CreateCache(8);

			Assert.NotSame(cache.Get("Text"), cache.Get("text"));
			Assert.Equal("text", cache.Get("text").PropertyName);
		}

		[Fact]
		public void StopsGrowingAtCapacity()
		{
			var cache = CreateCache(4);

			for (var i = 0; i < 100; i++)
			{
				cache.Get("Property" + i);
			}

			Assert.Equal(4, cache.Count);
		}

		[Fact]
		public void KeepsServingTheNamesItCachedOnceFull()
		{
			var cache = CreateCache(4);
			var first = cache.Get("First");

			for (var i = 0; i < 100; i++)
			{
				cache.Get("Overflow" + i);
			}

			Assert.Same(first, cache.Get("First"));
		}

		[Fact]
		public void StillReturnsCorrectArgsForNamesItRefusedToCache()
		{
			var cache = CreateCache(1);
			cache.Get("Cached");

			var args = cache.Get("Uncached");

			Assert.Equal("Uncached", args.PropertyName);
			// Refused names are created fresh every time; that is the point of never evicting.
			Assert.NotSame(args, cache.Get("Uncached"));
		}

		[Fact]
		public void ClearMakesRoomAgain()
		{
			var cache = CreateCache(2);
			cache.Get("A");
			cache.Get("B");
			cache.Get("C");

			Assert.Equal(2, cache.Count);

			cache.Clear();
			cache.Get("C");

			Assert.Equal(1, cache.Count);
			Assert.Same(cache.Get("C"), cache.Get("C"));
		}

		[Fact]
		public void RejectsAnInvalidCapacity()
		{
			Assert.Throws<ArgumentOutOfRangeException>(
				() => new PropertyChangeEventArgsCache<PropertyChangedEventArgs>(static name => new PropertyChangedEventArgs(name), 0));
		}

		[Fact]
		public void RejectsAMissingFactory()
		{
			Assert.Throws<ArgumentNullException>(
				() => new PropertyChangeEventArgsCache<PropertyChangedEventArgs>(null));
		}
	}

	public class PropertyChangeEventArgsUnitTests
	{
		class Notifier : BindableObject
		{
			public static readonly BindableProperty TextProperty =
				BindableProperty.Create(nameof(Text), typeof(string), typeof(Notifier), default(string));

			public string Text
			{
				get => (string)GetValue(TextProperty);
				set => SetValue(TextProperty, value);
			}

			public void RaiseByName(string propertyName) => OnPropertyChanged(propertyName);
		}

		// Mirrors what controls in the wild do: override the virtual and expect it to still be called.
		class OverridingNotifier : Notifier
		{
			public List<PropertyChangedEventArgs> ChangedSeen { get; } = new();
			public List<PropertyChangingEventArgs> ChangingSeen { get; } = new();

			protected override void OnPropertyChanged(string propertyName = null)
			{
				ChangedSeen.Add(new PropertyChangedEventArgs(propertyName));
				base.OnPropertyChanged(propertyName);
			}

			protected override void OnPropertyChanging(string propertyName = null)
			{
				ChangingSeen.Add(new PropertyChangingEventArgs(propertyName));
				base.OnPropertyChanging(propertyName);
			}
		}

		// An override that renames the notification has to keep working, and must not be handed the parked property's args.
		class RenamingNotifier : Notifier
		{
			protected override void OnPropertyChanged(string propertyName = null)
				=> base.OnPropertyChanged("Renamed");
		}

		[Fact]
		public void BindablePropertyReusesItsChangedArgs()
		{
			Assert.Same(Notifier.TextProperty.ChangedEventArgs, Notifier.TextProperty.ChangedEventArgs);
			Assert.Equal("Text", Notifier.TextProperty.ChangedEventArgs.PropertyName);
		}

		[Fact]
		public void BindablePropertyReusesItsChangingArgs()
		{
			Assert.Same(Notifier.TextProperty.ChangingEventArgs, Notifier.TextProperty.ChangingEventArgs);
			Assert.Equal("Text", Notifier.TextProperty.ChangingEventArgs.PropertyName);
		}

		[Fact]
		public void SettingAPropertyRaisesTheArgsOwnedByThatProperty()
		{
			var notifier = new Notifier();
			PropertyChangedEventArgs changed = null;
			PropertyChangingEventArgs changing = null;

			notifier.PropertyChanged += (_, e) => changed = e;
			notifier.PropertyChanging += (_, e) => changing = e;

			notifier.Text = "hello";

			Assert.Same(Notifier.TextProperty.ChangedEventArgs, changed);
			Assert.Same(Notifier.TextProperty.ChangingEventArgs, changing);
		}

		[Fact]
		public void RepeatedChangesReuseTheSameArgsInstance()
		{
			var notifier = new Notifier();
			var seen = new List<PropertyChangedEventArgs>();
			notifier.PropertyChanged += (_, e) => seen.Add(e);

			notifier.Text = "one";
			notifier.Text = "two";
			notifier.Text = "three";

			Assert.Equal(3, seen.Count);
			Assert.Same(seen[0], seen[1]);
			Assert.Same(seen[1], seen[2]);
		}

		[Fact]
		public void OverridesStillReceiveTheNotification()
		{
			var notifier = new OverridingNotifier();

			notifier.Text = "hello";

			Assert.Contains(notifier.ChangedSeen, e => e.PropertyName == "Text");
			Assert.Contains(notifier.ChangingSeen, e => e.PropertyName == "Text");
		}

		[Fact]
		public void AnOverrideThatRenamesTheNotificationGetsArgsForTheNewName()
		{
			var notifier = new RenamingNotifier();
			PropertyChangedEventArgs changed = null;
			notifier.PropertyChanged += (_, e) => changed = e;

			notifier.Text = "hello";

			Assert.Equal("Renamed", changed.PropertyName);
			Assert.NotSame(Notifier.TextProperty.ChangedEventArgs, changed);
		}

		[Fact]
		public void NotificationsRaisedByNameStillWork()
		{
			var notifier = new Notifier();
			PropertyChangedEventArgs changed = null;
			notifier.PropertyChanged += (_, e) => changed = e;

			// A plain CLR property has no BindableProperty to hang args off, so it goes through the keyed cache.
			notifier.RaiseByName("NotABindableProperty");

			Assert.Equal("NotABindableProperty", changed.PropertyName);
			Assert.Same(BindableProperty.GetCachedPropertyChangedEventArgs("NotABindableProperty"), changed);
		}

		[Fact]
		public void SettingAnotherPropertyFromAHandlerDoesNotCorruptTheOuterNotification()
		{
			var notifier = new NestingNotifier();
			var seen = new List<PropertyChangedEventArgs>();

			notifier.PropertyChanged += (_, e) =>
			{
				seen.Add(e);

				if (e.PropertyName == nameof(NestingNotifier.Outer) && notifier.Inner is null)
				{
					notifier.Inner = "set from the handler";
				}
			};

			notifier.Outer = "hello";

			Assert.Collection(
				seen,
				e => Assert.Same(NestingNotifier.OuterProperty.ChangedEventArgs, e),
				e => Assert.Same(NestingNotifier.InnerProperty.ChangedEventArgs, e));
		}

		[Fact]
		public void AnOverrideThatNotifiesBeforeCallingBaseStillGetsTheOuterPropertysArgs()
		{
			// This is what makes restoring the parked property matter rather than just clearing it: by the time the
			// override calls base, a nested notification has already come and gone.
			var notifier = new NestingBeforeBaseNotifier();
			var seen = new List<PropertyChangedEventArgs>();
			notifier.PropertyChanged += (_, e) => seen.Add(e);

			notifier.Outer = "hello";

			Assert.Contains(seen, e => ReferenceEquals(e, NestingBeforeBaseNotifier.InnerProperty.ChangedEventArgs));
			Assert.Contains(seen, e => ReferenceEquals(e, NestingBeforeBaseNotifier.OuterProperty.ChangedEventArgs));
		}

		class NestingBeforeBaseNotifier : NestingNotifier
		{
			bool _nesting;

			protected override void OnPropertyChanged(string propertyName = null)
			{
				if (propertyName == nameof(Outer) && !_nesting)
				{
					_nesting = true;
					Inner = "set before calling base";
					_nesting = false;
				}

				base.OnPropertyChanged(propertyName);
			}
		}

		class NestingNotifier : BindableObject
		{
			public static readonly BindableProperty OuterProperty =
				BindableProperty.Create(nameof(Outer), typeof(string), typeof(NestingNotifier), default(string));

			public static readonly BindableProperty InnerProperty =
				BindableProperty.Create(nameof(Inner), typeof(string), typeof(NestingNotifier), default(string));

			public string Outer
			{
				get => (string)GetValue(OuterProperty);
				set => SetValue(OuterProperty, value);
			}

			public string Inner
			{
				get => (string)GetValue(InnerProperty);
				set => SetValue(InnerProperty, value);
			}
		}
	}
}
