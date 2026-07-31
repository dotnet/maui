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
		// Weak subscriptions so a shared/long-lived GradientStopCollection (or its stops) does not
		// keep this brush rooted in memory via the CollectionChanged / PropertyChanged events. See issue #36363.
		readonly NotifyCollectionChangedEventHandler _gradientStopsCollectionChangedHandler;
		readonly PropertyChangedEventHandler _gradientStopPropertyChangedHandler;
		WeakNotifyCollectionChangedProxy _gradientStopsProxy;
		readonly Dictionary<GradientStop, WeakNotifyPropertyChangedProxy> _gradientStopProxies = new();

		/// <summary>Initializes a new instance of the <see cref="GradientBrush"/> class.</summary>
		public GradientBrush()
		{
			_gradientStopsCollectionChangedHandler = OnGradientStopCollectionChanged;
			_gradientStopPropertyChangedHandler = OnGradientStopPropertyChanged;

			GradientStops = new GradientStopCollection();
		}

		~GradientBrush()
		{
			_gradientStopsProxy?.Unsubscribe();

			foreach (var proxy in _gradientStopProxies.Values)
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

				foreach (var oldStop in oldCollection)
				{
					oldStop.Parent = null;
					UnsubscribeGradientStop(oldStop);
				}
			}

			if (newCollection == null)
				return;

			_gradientStopsProxy ??= new WeakNotifyCollectionChangedProxy();
			_gradientStopsProxy.Subscribe(newCollection, _gradientStopsCollectionChangedHandler);

			foreach (var newStop in newCollection)
			{
				if (newStop is not null)
				{
					newStop.Parent = this;
					SubscribeGradientStop(newStop);
				}
			}
		}

		void OnGradientStopCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (var oldItem in e.OldItems)
				{
					if (!(oldItem is GradientStop oldStop))
						continue;

					oldStop.Parent = null;
					UnsubscribeGradientStop(oldStop);
				}
			}

			if (e.NewItems != null)
			{
				foreach (var newItem in e.NewItems)
				{
					if (!(newItem is GradientStop newStop))
						continue;

					newStop.Parent = this;
					SubscribeGradientStop(newStop);
				}
			}

			Invalidate();
		}

		void SubscribeGradientStop(GradientStop stop)
		{
			if (_gradientStopProxies.TryGetValue(stop, out var proxy))
			{
				proxy.Subscribe(stop, _gradientStopPropertyChangedHandler);
			}
			else
			{
				_gradientStopProxies[stop] = new WeakNotifyPropertyChangedProxy(stop, _gradientStopPropertyChangedHandler);
			}
		}

		void UnsubscribeGradientStop(GradientStop stop)
		{
			if (_gradientStopProxies.TryGetValue(stop, out var proxy))
			{
				proxy.Unsubscribe();
				_gradientStopProxies.Remove(stop);
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
	}
}