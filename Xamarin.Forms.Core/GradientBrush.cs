using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms
{
	[ContentProperty(nameof(GradientStops))]
	public abstract class GradientBrush : Brush
	{
		static bool IsExperimentalFlagSet = false;

		public GradientBrush()
		{
			VerifyExperimental(nameof(GradientBrush));

			GradientStops = new GradientStopCollection();
		}

		public event EventHandler InvalidateGradientBrushRequested;

		internal static void VerifyExperimental([CallerMemberName] string memberName = "", string constructorHint = null)
		{
			if (IsExperimentalFlagSet)
				return;

			ExperimentalFlags.VerifyFlagEnabled(nameof(GradientBrush), ExperimentalFlags.BrushExperimental, constructorHint, memberName);

			IsExperimentalFlagSet = true;
		}

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

		void UpdateGradientStops(GradientStopCollection oldCollection, GradientStopCollection newCollection)
		{
			if (oldCollection != null)
			{
				oldCollection.CollectionChanged -= OnGradientStopCollectionChanged;

				foreach (var oldStop in oldCollection)
				{
					oldStop.PropertyChanged -= OnGradientStopPropertyChanged;
				}
			}

			if (newCollection == null)
				return;

			newCollection.CollectionChanged += OnGradientStopCollectionChanged;

			foreach (var newStop in newCollection)
			{
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

					oldStop.PropertyChanged -= OnGradientStopPropertyChanged;
				}
			}

			if (e.NewItems != null)
			{
				foreach (var newItem in e.NewItems)
				{
					if (!(newItem is GradientStop newStop))
						continue;

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