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
		// A GradientStopCollection and the stops inside it can outlive the brush that consumes them --
		// for example when they are declared in a ResourceDictionary and shared between brushes, or
		// simply kept alive by the app while the page that owned the brush is discarded. Both the
		// collection subscription and the per-stop subscriptions are therefore made weakly, so a
		// long-lived collection or stop never roots a transient brush.
		WeakNotifyCollectionChangedProxy _gradientStopsProxy;
		NotifyCollectionChangedEventHandler _gradientStopsChanged;
		PropertyChangedEventHandler _gradientStopPropertyChanged;

		// Tracks the stops we are currently subscribed to. A Reset (raised by GradientStopCollection.Clear)
		// supplies no OldItems and the collection has already been emptied by the time we are notified,
		// so this is the only record of which stops still need to be detached.
		readonly Dictionary<GradientStop, WeakNotifyPropertyChangedProxy> _subscribedStops = new();

		/// <summary>Initializes a new instance of the <see cref="GradientBrush"/> class.</summary>
		public GradientBrush()
		{
			GradientStops = new GradientStopCollection();
		}

		~GradientBrush()
		{
			_gradientStopsProxy?.Unsubscribe();

			foreach (var proxy in _subscribedStops.Values)
				proxy.Unsubscribe();
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
				SetInheritedBindingContext(gradientStop, BindingContext);
		}

		void UpdateGradientStops(GradientStopCollection oldCollection, GradientStopCollection newCollection)
		{
			if (oldCollection != null)
			{
				_gradientStopsProxy?.Unsubscribe();
				DetachAllStops();
			}

			if (newCollection == null)
				return;

			_gradientStopsProxy ??= new WeakNotifyCollectionChangedProxy();
			_gradientStopsChanged ??= OnGradientStopCollectionChanged;
			_gradientStopsProxy.Subscribe(newCollection, _gradientStopsChanged);

			foreach (var newStop in newCollection)
				AttachStop(newStop);
		}

		void OnGradientStopCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				// Clear() raises a Reset with no OldItems, so detach everything we know about and
				// rebuild from whatever the collection holds now.
				DetachAllStops();

				if (sender is GradientStopCollection collection)
				{
					foreach (var stop in collection)
						AttachStop(stop);
				}
			}
			else
			{
				if (e.OldItems != null)
				{
					foreach (var oldItem in e.OldItems)
					{
						if (oldItem is GradientStop oldStop)
							DetachStop(oldStop, sender as GradientStopCollection);
					}
				}

				if (e.NewItems != null)
				{
					foreach (var newItem in e.NewItems)
					{
						if (newItem is GradientStop newStop)
							AttachStop(newStop);
					}
				}
			}

			Invalidate();
		}

		void AttachStop(GradientStop stop)
		{
			if (stop is null || _subscribedStops.ContainsKey(stop))
				return;

			stop.Parent = this;

			_gradientStopPropertyChanged ??= OnGradientStopPropertyChanged;

			var proxy = new WeakNotifyPropertyChangedProxy();
			proxy.Subscribe(stop, _gradientStopPropertyChanged);
			_subscribedStops[stop] = proxy;
		}

		void DetachStop(GradientStop stop, GradientStopCollection collection)
		{
			if (stop is null)
				return;

			// The same stop instance may legally appear more than once in a collection; only tear the
			// subscription down once the last occurrence has gone.
			if (collection is not null && collection.Contains(stop))
				return;

			if (_subscribedStops.Remove(stop, out var proxy))
			{
				proxy.Unsubscribe();
				stop.Parent = null;
			}
		}

		void DetachAllStops()
		{
			foreach (var pair in _subscribedStops)
			{
				pair.Value.Unsubscribe();
				pair.Key.Parent = null;
			}

			_subscribedStops.Clear();
		}

		void OnGradientStopPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Invalidate();
		}

		void Invalidate()
		{
			InvalidateGradientBrushRequested?.Invoke(this, EventArgs.Empty);
		}
	}
}
