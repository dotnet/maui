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
		// still get independent subscription ref-counts. System.Collections.Generic.ReferenceEqualityComparer
		// is .NET 5+ only, but Controls.Core also targets netstandard2.0/2.1, so use a small
		// cross-TFM reference-equality comparer instead (avoids a netstandard build break).
		readonly Dictionary<GradientStop, int> _subscriptionRefCounts = new(GradientStopReferenceComparer.Instance);

		// Cached delegates: WeakNotifyXProxy only holds a WeakReference to the handler, so an
		// inline method-group delegate would have no other root and could be collected on its own.
		readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;
		readonly PropertyChangedEventHandler _stopChangedHandler;

		readonly WeakNotifyCollectionChangedProxy _collectionProxy = new();
		readonly Dictionary<GradientStop, WeakNotifyPropertyChangedProxy> _stopProxies = new(GradientStopReferenceComparer.Instance);

		/// <summary>Initializes a new instance of the <see cref="GradientBrush"/> class.</summary>
		public GradientBrush()
		{
			_collectionChangedHandler = OnGradientStopCollectionChanged;
			_stopChangedHandler = OnGradientStopPropertyChanged;
			GradientStops = new GradientStopCollection();
		}

		~GradientBrush()
		{
			_collectionProxy.Unsubscribe();

			foreach (var proxy in _stopProxies.Values)
			{
				proxy.Unsubscribe();
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

			if (_subscriptionRefCounts.TryGetValue(stop, out var count))
			{
				_subscriptionRefCounts[stop] = count + 1;
				return;
			}

			_subscriptionRefCounts[stop] = 1;
			stop.Parent = this;
			var proxy = new WeakNotifyPropertyChangedProxy();
			proxy.Subscribe(stop, _stopChangedHandler);
			_stopProxies[stop] = proxy;
		}

		void UnsubscribeFromGradientStop(GradientStop stop)
		{
			if (stop is null)
			{
				return;
			}

			if (!_subscriptionRefCounts.TryGetValue(stop, out var count))
			{
				return;
			}

			if (count > 1)
			{
				_subscriptionRefCounts[stop] = count - 1;
				return;
			}

			_subscriptionRefCounts.Remove(stop);
			stop.Parent = null;
			if (_stopProxies.TryGetValue(stop, out var proxy))
			{
				proxy.Unsubscribe();
				_stopProxies.Remove(stop);
			}
		}

		void UnsubscribeFromAllGradientStops()
		{
			foreach (var stop in _subscriptionRefCounts.Keys)
			{
				stop.Parent = null;
				if (_stopProxies.TryGetValue(stop, out var proxy))
				{
					proxy.Unsubscribe();
				}
			}

			_stopProxies.Clear();
			_subscriptionRefCounts.Clear();
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