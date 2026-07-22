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
		readonly Dictionary<GradientStop, int> _subscriptionRefCounts = new();

		/// <summary>Initializes a new instance of the <see cref="GradientBrush"/> class.</summary>
		public GradientBrush()
		{
			GradientStops = new GradientStopCollection();
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
			DetachCollection(oldCollection);
			AttachCollection(newCollection);
			Invalidate();
		}

		void AttachCollection(GradientStopCollection collection)
		{
			if (collection == null)
				return;

			collection.CollectionChanged += OnGradientStopCollectionChanged;

			foreach (var stop in collection)
				SubscribeToStop(stop);
		}

		void DetachCollection(GradientStopCollection collection)
		{
			if (collection == null)
				return;

			collection.CollectionChanged -= OnGradientStopCollectionChanged;

			foreach (var stop in collection)
				UnsubscribeFromStop(stop);
		}

		void OnGradientStopCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewItems != null)
					{
						foreach (GradientStop stop in e.NewItems)
							SubscribeToStop(stop);
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldItems != null)
					{
						foreach (GradientStop stop in e.OldItems)
							UnsubscribeFromStop(stop);
					}
					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.OldItems != null)
					{
						foreach (GradientStop stop in e.OldItems)
							UnsubscribeFromStop(stop);
					}
					if (e.NewItems != null)
					{
						foreach (GradientStop stop in e.NewItems)
							SubscribeToStop(stop);
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

		void SubscribeToStop(GradientStop stop)
		{
			if (stop == null)
				return;

			if (_subscriptionRefCounts.TryGetValue(stop, out var count))
			{
				_subscriptionRefCounts[stop] = count + 1;
				return;
			}

			_subscriptionRefCounts[stop] = 1;
			stop.Parent = this;
			stop.PropertyChanged += OnGradientStopPropertyChanged;
		}

		void UnsubscribeFromStop(GradientStop stop)
		{
			if (stop == null)
				return;

			if (!_subscriptionRefCounts.TryGetValue(stop, out var count))
				return;

			if (count > 1)
			{
				_subscriptionRefCounts[stop] = count - 1;
				return;
			}

			_subscriptionRefCounts.Remove(stop);
			stop.Parent = null;
			stop.PropertyChanged -= OnGradientStopPropertyChanged;
		}

		void UnsubscribeFromAllStops()
		{
			foreach (var stop in _subscriptionRefCounts.Keys)
			{
				stop.Parent = null;
				stop.PropertyChanged -= OnGradientStopPropertyChanged;
			}

			_subscriptionRefCounts.Clear();
		}

		void ResubscribeCollection(GradientStopCollection collection)
		{
			UnsubscribeFromAllStops();

			if (collection == null)
				return;

			foreach (var stop in collection)
				SubscribeToStop(stop);
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