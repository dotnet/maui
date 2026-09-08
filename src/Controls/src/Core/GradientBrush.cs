#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>Base class for brushes that paint an area with a gradient of multiple colors.</summary>
	[ContentProperty(nameof(GradientStops))]
	public abstract class GradientBrush : Brush
	{
		// Keyed by reference identity so distinct GradientStop instances that compare equal by value
		// still get independent subscriptions. System.Collections.Generic.ReferenceEqualityComparer
		// is .NET 5+ only, but Controls.Core also targets netstandard2.0/2.1, so use a small
		// cross-TFM reference-equality comparer instead (avoids a netstandard build break).
		Dictionary<GradientStop, (int Count, WeakNotifyPropertyChangedProxy Proxy)> _stopSubscriptions;

		// Cached delegates: WeakNotifyXProxy only holds a WeakReference to the handler, so an
		// inline method-group delegate would have no other root and could be collected on its own.
		readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;
		PropertyChangedEventHandler _stopChangedHandler;

		readonly WeakNotifyCollectionChangedProxy _collectionProxy = new();

		/// <summary>Initializes a new instance of the <see cref="GradientBrush"/> class.</summary>
		public GradientBrush()
		{
			_collectionChangedHandler = OnGradientStopCollectionChanged;
			GradientStops = new GradientStopCollection();
		}

		~GradientBrush()
		{
			_collectionProxy.Unsubscribe();

			if (_stopSubscriptions is null)
			{
				return;
			}

			foreach (var subscription in _stopSubscriptions.Values)
			{
				subscription.Proxy.Unsubscribe();
			}
		}

		public event EventHandler InvalidateGradientBrushRequested;

		/// <summary>Bindable property for <see cref="GradientStops"/>.</summary>
		public static readonly BindableProperty GradientStopsProperty =
			BindableProperty.Create(nameof(GradientStops), typeof(GradientStopCollection), typeof(GradientBrush), null,
				propertyChanged: OnGradientStopsChanged);

		/// <summary>Gets or sets the collection of <see cref="GradientStop"/> objects that define the gradient colors. This is a bindable property.</summary>
		public GradientStopCollection GradientStops
		{
			get => (GradientStopCollection)GetValue(GradientStopsProperty);
			set => SetValue(GradientStopsProperty, value);
		}

		public override bool IsEmpty =>
			GradientStops is null || GradientStops.Count == 0;

		static void OnGradientStopsChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as GradientBrush)?.UpdateGradientStops(oldValue as GradientStopCollection, newValue as GradientStopCollection);
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			foreach (var gradientStop in GradientStops)
			{
				SetInheritedBindingContext(gradientStop, BindingContext);
			}
		}

		void UpdateGradientStops(GradientStopCollection oldCollection, GradientStopCollection newCollection)
		{
			DetachCollection(oldCollection);
			AttachCollection(newCollection);
			Invalidate();
		}

		void AttachCollection(GradientStopCollection collection)
		{
			if (collection is null)
			{
				return;
			}

			_collectionProxy.Subscribe(collection, _collectionChangedHandler);

			foreach (var stop in collection)
			{
				SubscribeToGradientStop(stop);
			}
		}

		void DetachCollection(GradientStopCollection collection)
		{
			if (collection is null)
			{
				return;
			}

			_collectionProxy.Unsubscribe();
			UnsubscribeFromAllGradientStops();
		}

		void OnGradientStopCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewItems is not null)
					{
						foreach (GradientStop stop in e.NewItems)
						{
							SubscribeToGradientStop(stop);
						}
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldItems is not null)
					{
						foreach (GradientStop stop in e.OldItems)
						{
							UnsubscribeFromGradientStop(stop);
						}
					}
					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.OldItems is not null)
					{
						foreach (GradientStop stop in e.OldItems)
						{
							UnsubscribeFromGradientStop(stop);
						}
					}
					if (e.NewItems is not null)
					{
						foreach (GradientStop stop in e.NewItems)
						{
							SubscribeToGradientStop(stop);
						}
					}
					break;

				case NotifyCollectionChangedAction.Move:
					// No subscription changes required.
					break;

				case NotifyCollectionChangedAction.Reset:
					ResubscribeCollection(sender as GradientStopCollection);
					break;
			}

			Invalidate();
		}

		void SubscribeToGradientStop(GradientStop stop)
		{
			if (stop is null)
			{
				return;
			}

			var subscriptions = _stopSubscriptions ??= new(GradientStopReferenceComparer.Instance);

			if (subscriptions.TryGetValue(stop, out var subscription))
			{
				subscriptions[stop] = (subscription.Count + 1, subscription.Proxy);
				return;
			}

			stop.Parent = this;
			var proxy = new WeakNotifyPropertyChangedProxy();
			_stopChangedHandler ??= OnGradientStopPropertyChanged;
			proxy.Subscribe(stop, _stopChangedHandler);
			subscriptions[stop] = (1, proxy);
		}

		void UnsubscribeFromGradientStop(GradientStop stop)
		{
			if (stop is null)
			{
				return;
			}

			var subscriptions = _stopSubscriptions;
			if (subscriptions is null || !subscriptions.TryGetValue(stop, out var subscription))
			{
				return;
			}

			if (subscription.Count > 1)
			{
				subscriptions[stop] = (subscription.Count - 1, subscription.Proxy);
				return;
			}

			subscriptions.Remove(stop);
			stop.Parent = null;
			subscription.Proxy.Unsubscribe();
		}

		void UnsubscribeFromAllGradientStops()
		{
			var subscriptions = _stopSubscriptions;
			if (subscriptions is null)
			{
				return;
			}

			foreach (var subscription in subscriptions)
			{
				subscription.Key.Parent = null;
				subscription.Value.Proxy.Unsubscribe();
			}

			subscriptions.Clear();
		}

		void ResubscribeCollection(GradientStopCollection collection)
		{
			UnsubscribeFromAllGradientStops();

			if (collection is null)
			{
				return;
			}

			foreach (var stop in collection)
			{
				SubscribeToGradientStop(stop);
			}
		}

		void OnGradientStopPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Invalidate();
		}

		void Invalidate()
		{
			InvalidateGradientBrushRequested?.Invoke(this, EventArgs.Empty);
		}

		// Reference-identity comparer usable on every Controls.Core TFM (netstandard2.0/2.1 lack the
		// BCL System.Collections.Generic.ReferenceEqualityComparer). Matches its semantics: equality by
		// object reference, hash from RuntimeHelpers.GetHashCode.
		sealed class GradientStopReferenceComparer : IEqualityComparer<GradientStop>
		{
			public static readonly GradientStopReferenceComparer Instance = new();
			public bool Equals(GradientStop x, GradientStop y) => ReferenceEquals(x, y);
			public int GetHashCode(GradientStop obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
		}
	}
}