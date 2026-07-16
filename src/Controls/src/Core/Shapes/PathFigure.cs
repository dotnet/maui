#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Represents a subsection of a geometry, containing a collection of path segments.
	/// </summary>
	[ContentProperty("Segments")]
	public sealed class PathFigure : BindableObject, IAnimatable
	{
		// The segment collection and the individual segments can outlive this PathFigure
		// (e.g. when a shared PathSegmentCollection is assigned to Segments). Subscribing
		// to their events directly would root this PathFigure through those longer-lived
		// objects, so the subscriptions are made through weak proxies instead.
		readonly WeakNotifyCollectionChangedProxy _segmentsProxy = new();
		readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;
		readonly List<WeakNotifyPropertyChangedProxy> _segmentProxies = new();
		readonly PropertyChangedEventHandler _propertyChangedHandler;

		/// <summary>
		/// Initializes a new instance of the <see cref="PathFigure"/> class.
		/// </summary>
		public PathFigure()
		{
			_collectionChangedHandler = OnPathSegmentCollectionChanged;
			_propertyChangedHandler = OnPathSegmentPropertyChanged;
			Segments = new PathSegmentCollection();
		}

		~PathFigure()
		{
			_segmentsProxy.Unsubscribe();
			UnsubscribeFromPathSegments();
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
			BindableProperty.Create(nameof(IsClosed), typeof(bool), typeof(PathFigure), false);

		/// <summary>Bindable property for <see cref="IsFilled"/>.</summary>
		public static readonly BindableProperty IsFilledProperty =
			BindableProperty.Create(nameof(IsFilled), typeof(bool), typeof(PathFigure), true);

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
			set { SetValue(IsClosedProperty, value); }
			get { return (bool)GetValue(IsClosedProperty); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to fill the interior of this figure. This is a bindable property.
		/// </summary>
		public bool IsFilled
		{
			set { SetValue(IsFilledProperty, value); }
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
			if (oldCollection != null)
			{
				_segmentsProxy.Unsubscribe();
				UnsubscribeFromPathSegments();
			}

			if (newCollection == null)
				return;

			_segmentsProxy.Subscribe(newCollection, _collectionChangedHandler);

			foreach (var newPathSegment in newCollection)
			{
				SubscribeToPathSegment(newPathSegment);
			}
		}

		void OnPathSegmentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				// PathSegmentCollection is sealed, so Reset follows Clear after the collection is empty.
				UnsubscribeFromPathSegments();
			}
			else if (e.Action != NotifyCollectionChangedAction.Move)
			{
				if (e.OldItems != null)
				{
					foreach (var oldItem in e.OldItems)
					{
						if (!(oldItem is PathSegment oldPathSegment))
							continue;

						UnsubscribeFromPathSegment(oldPathSegment);
					}
				}

				if (e.NewItems != null)
				{
					foreach (var newItem in e.NewItems)
					{
						if (!(newItem is PathSegment newPathSegment))
							continue;

						SubscribeToPathSegment(newPathSegment);
					}
				}
			}

			Invalidate();
		}

		void SubscribeToPathSegment(PathSegment pathSegment)
		{
			if (pathSegment == null)
				return;

			var proxy = new WeakNotifyPropertyChangedProxy();
			proxy.Subscribe(pathSegment, _propertyChangedHandler);
			_segmentProxies.Add(proxy);
		}

		void UnsubscribeFromPathSegment(PathSegment pathSegment)
		{
			if (pathSegment == null)
				return;

			for (int i = _segmentProxies.Count - 1; i >= 0; i--)
			{
				var proxy = _segmentProxies[i];
				if (proxy.TryGetSource(out var source) && ReferenceEquals(source, pathSegment))
				{
					proxy.Unsubscribe();
					_segmentProxies.RemoveAt(i);
					break;
				}
			}
		}

		void UnsubscribeFromPathSegments()
		{
			for (int i = 0; i < _segmentProxies.Count; i++)
				_segmentProxies[i].Unsubscribe();

			_segmentProxies.Clear();
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