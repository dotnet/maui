#nullable disable
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Represents a subsection of a geometry, containing a collection of path segments.
	/// </summary>
	[ContentProperty("Segments")]
	public sealed class PathFigure : BindableObject, IAnimatable
	{
		// A PathSegmentCollection and the segments inside it can outlive the figure that consumes them --
		// for example when they are declared in a ResourceDictionary and shared between figures. Both the
		// collection subscription and the per-segment subscriptions are therefore weak, so a long-lived
		// collection or segment never roots a discarded PathFigure.
		WeakNotifyCollectionChangedProxy _segmentsProxy;
		NotifyCollectionChangedEventHandler _segmentsChanged;
		PropertyChangedEventHandler _segmentPropertyChanged;
		readonly Dictionary<PathSegment, WeakNotifyPropertyChangedProxy> _subscribedSegments = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="PathFigure"/> class.
		/// </summary>
		public PathFigure()
		{
			Segments = new PathSegmentCollection();
		}

		~PathFigure()
		{
			_segmentsProxy?.Unsubscribe();

			foreach (var proxy in _subscribedSegments.Values)
				proxy.Unsubscribe();
		}

		/// <summary>Bindable property for <see cref="Segments"/>.</summary>
		public static readonly BindableProperty SegmentsProperty =
			BindableProperty.Create(nameof(Segments), typeof(PathSegmentCollection), typeof(PathFigure), null,
				propertyChanged: OnPathSegmentCollectionChanged);

		static void OnPathSegmentCollectionChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as PathFigure)?.UpdatePathSegmentCollection(oldValue as PathSegmentCollection, newValue as PathSegmentCollection);
		}

		/// <summary>Bindable property for <see cref="StartPoint"/>.</summary>
		public static readonly BindableProperty StartPointProperty =
			BindableProperty.Create(nameof(StartPoint), typeof(Point), typeof(PathFigure), new Point(0, 0));

		/// <summary>Bindable property for <see cref="IsClosed"/>.</summary>
		public static readonly BindableProperty IsClosedProperty =
			BindableProperty.Create(nameof(IsClosed), typeof(bool), typeof(PathFigure), BooleanBoxes.FalseBox);

		/// <summary>Bindable property for <see cref="IsFilled"/>.</summary>
		public static readonly BindableProperty IsFilledProperty =
			BindableProperty.Create(nameof(IsFilled), typeof(bool), typeof(PathFigure), BooleanBoxes.TrueBox);

		/// <summary>
		/// Gets or sets the collection of path segments that define this figure. This is a bindable property.
		/// </summary>
		public PathSegmentCollection Segments
		{
			set { SetValue(SegmentsProperty, value); }
			get { return (PathSegmentCollection)GetValue(SegmentsProperty); }
		}

		/// <summary>
		/// Gets or sets the point where this figure starts. This is a bindable property.
		/// </summary>
		public Point StartPoint
		{
			set { SetValue(StartPointProperty, value); }
			get { return (Point)GetValue(StartPointProperty); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to connect the last segment to the start point. This is a bindable property.
		/// </summary>
		public bool IsClosed
		{
			set { SetValue(IsClosedProperty, BooleanBoxes.Box(value)); }
			get { return (bool)GetValue(IsClosedProperty); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to fill the interior of this figure. This is a bindable property.
		/// </summary>
		public bool IsFilled
		{
			set { SetValue(IsFilledProperty, BooleanBoxes.Box(value)); }
			get { return (bool)GetValue(IsFilledProperty); }
		}

		internal event EventHandler InvalidatePathSegmentRequested;

		/// <inheritdoc/>
		public void BatchBegin()
		{

		}

		/// <inheritdoc/>
		public void BatchCommit()
		{

		}

		void UpdatePathSegmentCollection(PathSegmentCollection oldCollection, PathSegmentCollection newCollection)
		{
			_segmentsProxy?.Unsubscribe();

			UnsubscribeFromAllPathSegmentPropertyChanged();

			if (newCollection == null)
				return;

			_segmentsProxy ??= new WeakNotifyCollectionChangedProxy();
			_segmentsChanged ??= OnPathSegmentCollectionChanged;
			_segmentsProxy.Subscribe(newCollection, _segmentsChanged);

			foreach (var newPathSegment in newCollection)
			{
				SubscribeToPathSegmentPropertyChanged(newPathSegment);
			}
		}

		void OnPathSegmentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				UnsubscribeFromAllPathSegmentPropertyChanged();

				if (sender is PathSegmentCollection pathSegmentCollection)
				{
					foreach (var pathSegment in pathSegmentCollection)
					{
						SubscribeToPathSegmentPropertyChanged(pathSegment);
					}
				}

				Invalidate();
				return;
			}

			if (e.OldItems != null)
			{
				foreach (var oldItem in e.OldItems)
				{
					if (!(oldItem is PathSegment oldPathSegment))
						continue;

					UnsubscribeFromPathSegmentPropertyChanged(oldPathSegment);
				}
			}

			if (e.NewItems != null)
			{
				foreach (var newItem in e.NewItems)
				{
					if (!(newItem is PathSegment newPathSegment))
						continue;

					SubscribeToPathSegmentPropertyChanged(newPathSegment);
				}
			}

			Invalidate();
		}

		void SubscribeToPathSegmentPropertyChanged(PathSegment pathSegment)
		{
			if (pathSegment is null || _subscribedSegments.ContainsKey(pathSegment))
			{
				return;
			}

			_segmentPropertyChanged ??= OnPathSegmentPropertyChanged;

			var proxy = new WeakNotifyPropertyChangedProxy();
			proxy.Subscribe(pathSegment, _segmentPropertyChanged);
			_subscribedSegments[pathSegment] = proxy;
		}

		void UnsubscribeFromPathSegmentPropertyChanged(PathSegment pathSegment)
		{
			if (pathSegment is null || !_subscribedSegments.Remove(pathSegment, out var proxy))
			{
				return;
			}

			proxy.Unsubscribe();
		}

		void UnsubscribeFromAllPathSegmentPropertyChanged()
		{
			foreach (var proxy in _subscribedSegments.Values)
			{
				proxy.Unsubscribe();
			}

			_subscribedSegments.Clear();
		}

		void OnPathSegmentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Invalidate();
		}

		void Invalidate()
		{
			InvalidatePathSegmentRequested?.Invoke(this, EventArgs.Empty);
		}
	}
}