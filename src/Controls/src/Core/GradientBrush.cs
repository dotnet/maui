using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(GradientStops))]
	public abstract partial class GradientBrush : Brush
	{
		public GradientBrush()
		{
			GradientStops = new GradientStopCollection();
		}

		public event EventHandler InvalidateGradientBrushRequested;

		public static readonly BindableProperty GradientStopsProperty =
			BindableProperty.Create(nameof(GradientStops), typeof(GradientStopCollection), typeof(GradientBrush), null,
				propertyChanged: OnGradientStopsChanged);

		public GradientStopCollection GradientStops
		{
			get => (GradientStopCollection)GetValue(GradientStopsProperty);
			set => SetValue(GradientStopsProperty, value);
		}

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
				oldCollection.CollectionChanged -= OnGradientStopCollectionChanged;

				foreach (var oldStop in oldCollection)
				{
					oldStop.Parent = null;
					oldStop.PropertyChanged -= OnGradientStopPropertyChanged;
				}
			}

			if (newCollection == null)
				return;

			newCollection.CollectionChanged += OnGradientStopCollectionChanged;

			foreach (var newStop in newCollection)
			{
				newStop.Parent = this;
				newStop.PropertyChanged += OnGradientStopPropertyChanged;
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
					oldStop.PropertyChanged -= OnGradientStopPropertyChanged;
				}
			}

			if (e.NewItems != null)
			{
				foreach (var newItem in e.NewItems)
				{
					if (!(newItem is GradientStop newStop))
						continue;

					newStop.Parent = this;
					newStop.PropertyChanged += OnGradientStopPropertyChanged;
				}
			}

			Invalidate();
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