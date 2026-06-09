#nullable disable
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
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
		readonly WeakNotifyCollectionChangedProxy _segmentCollectionChangedProxy = new();
		readonly Dictionary<PathSegment, WeakNotifyPropertyChangedProxy> _segmentPropertyChangedProxies = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="PathFigure"/> class.
		/// </summary>
		public PathFigure()
		{
			Segments = new PathSegmentCollection();
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
			_segmentCollectionChangedProxy.Unsubscribe();
			UnsubscribeFromAllPathSegmentPropertyChanged();

			if (newCollection == null)
				return;

			_segmentCollectionChangedProxy.Subscribe(newCollection, OnPathSegmentCollectionChanged);

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

		void OnPathSegmentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Invalidate();
		}

		void SubscribeToPathSegmentPropertyChanged(PathSegment pathSegment)
		{
			if (_segmentPropertyChangedProxies.ContainsKey(pathSegment))
				return;

			var weakProxy = new WeakNotifyPropertyChangedProxy(pathSegment, OnPathSegmentPropertyChanged);
			_segmentPropertyChangedProxies[pathSegment] = weakProxy;
		}

		void UnsubscribeFromPathSegmentPropertyChanged(PathSegment pathSegment)
		{
			if (!_segmentPropertyChangedProxies.TryGetValue(pathSegment, out var weakProxy))
				return;

			weakProxy.Unsubscribe();
			_segmentPropertyChangedProxies.Remove(pathSegment);
		}

		void UnsubscribeFromAllPathSegmentPropertyChanged()
		{
			foreach (var weakProxy in _segmentPropertyChangedProxies.Values)
			{
				weakProxy.Unsubscribe();
			}

			_segmentPropertyChangedProxies.Clear();
		}

		void Invalidate()
		{
			InvalidatePathSegmentRequested?.Invoke(this, EventArgs.Empty);
		}
	}
}