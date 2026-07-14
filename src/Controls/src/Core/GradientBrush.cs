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
		/// <summary>Initializes a new instance of the <see cref="GradientBrush"/> class.</summary>
		public GradientBrush()
		{
			GradientStops = new GradientStopCollection();
		}

		GradientStopSubscriptions _gradientStopSubscriptions;
		NotifyCollectionChangedEventHandler _gradientStopsCollectionChanged;
		PropertyChangedEventHandler _gradientStopPropertyChanged;

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

			var gradientStops = GradientStops;
			if (gradientStops is null)
				return;

			foreach (var gradientStop in gradientStops)
			{
				if (gradientStop is null)
					continue;

				SetInheritedBindingContext(gradientStop, BindingContext);
			}
		}

		void UpdateGradientStops(GradientStopCollection oldCollection, GradientStopCollection newCollection)
		{
			if (oldCollection != null)
			{
				_gradientStopSubscriptions?.UnsubscribeAll();

				foreach (var oldStop in oldCollection)
				{
					if (oldStop is null)
						continue;

					ClearGradientStopParentIfUnused(oldStop);
				}
			}

			if (newCollection != null)
			{
				_gradientStopsCollectionChanged ??= OnGradientStopCollectionChanged;
				_gradientStopPropertyChanged ??= OnGradientStopPropertyChanged;

				var subscriptions = _gradientStopSubscriptions ??= new GradientStopSubscriptions();
				subscriptions.Subscribe(newCollection, _gradientStopsCollectionChanged);

				foreach (var newStop in newCollection)
				{
					if (newStop is not null)
					{
						newStop.Parent = this;
						subscriptions.Add(newStop, _gradientStopPropertyChanged);
					}
				}
			}

			// Collection replacement must invalidate even when the new value is null or empty.
			Invalidate();
		}

		void OnGradientStopCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				_gradientStopSubscriptions?.ResetStops(this);
			}
			else if (e.OldItems != null)
			{
				foreach (var oldItem in e.OldItems)
				{
					if (!(oldItem is GradientStop oldStop))
						continue;

					_gradientStopSubscriptions?.Remove(oldStop);
					ClearGradientStopParentIfUnused(oldStop);
				}
			}

			if (e.NewItems != null)
			{
				foreach (var newItem in e.NewItems)
				{
					if (!(newItem is GradientStop newStop))
						continue;

					newStop.Parent = this;
					_gradientStopSubscriptions?.Add(newStop, _gradientStopPropertyChanged);
				}
			}

			Invalidate();
		}

		void ClearGradientStopParentIfUnused(GradientStop gradientStop)
		{
			if (!ReferenceEquals(gradientStop.Parent, this))
				return;

			var gradientStops = GradientStops;
			if (gradientStops is not null)
			{
				for (int i = 0; i < gradientStops.Count; i++)
				{
					if (ReferenceEquals(gradientStops[i], gradientStop))
						return;
				}
			}

			gradientStop.Parent = null;
		}

		sealed class GradientStopSubscriptions
		{
			readonly WeakNotifyCollectionChangedProxy _collectionProxy = new();
			readonly List<WeakNotifyPropertyChangedProxy> _stopProxies = new();

			~GradientStopSubscriptions() => UnsubscribeAll();

			public void Subscribe(GradientStopCollection source, NotifyCollectionChangedEventHandler handler)
			{
				_collectionProxy.Subscribe(source, handler);
			}

			public void Add(GradientStop source, PropertyChangedEventHandler handler)
			{
				_stopProxies.Add(new WeakNotifyPropertyChangedProxy(source, handler));
			}

			public void Remove(GradientStop source)
			{
				bool removed = false;
				for (int i = _stopProxies.Count - 1; i >= 0; i--)
				{
					var proxy = _stopProxies[i];
					if (!proxy.TryGetSource(out var proxySource))
					{
						proxy.Unsubscribe();
						_stopProxies.RemoveAt(i);
					}
					else if (!removed && ReferenceEquals(proxySource, source))
					{
						proxy.Unsubscribe();
						_stopProxies.RemoveAt(i);
						removed = true;
					}
				}
			}

			public void ResetStops(GradientBrush owner)
			{
				UnsubscribeStops(owner);
			}

			public void UnsubscribeAll()
			{
				_collectionProxy.Unsubscribe();
				UnsubscribeStops(owner: null);
			}

			void UnsubscribeStops(GradientBrush owner)
			{
				var proxies = _stopProxies.ToArray();
				_stopProxies.Clear();

				foreach (var proxy in proxies)
				{
					GradientStop gradientStop = null;
					if (proxy.TryGetSource(out var source))
						gradientStop = source as GradientStop;

					proxy.Unsubscribe();

					if (gradientStop is null)
						continue;

					if (owner is not null)
						owner.ClearGradientStopParentIfUnused(gradientStop);
					else
						gradientStop.ClearRealParentIfCollected();
				}
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