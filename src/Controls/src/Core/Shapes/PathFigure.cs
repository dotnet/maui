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
		readonly List<PathSegment> _subscribedSegments = new();

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
			oldCollection?.CollectionChanged -= OnPathSegmentCollectionChanged;

			UnsubscribeFromAllPathSegmentPropertyChanged();

			if (newCollection == null)
				return;

			newCollection.CollectionChanged += OnPathSegmentCollectionChanged;

			foreach (var newPathSegment in newCollection)
			{
				SubscribeToPathSegmentPropertyChanged(newPathSegment);
			}
		}

		void OnPathSegmentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				for (int i = _subscribedSegments.Count - 1; i >= 0; i--)
				{
					UnsubscribeFromPathSegmentPropertyChanged(_subscribedSegments[i]);
				}

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
			if (_subscribedSegments.Contains(pathSegment))
			{
				return;
			}

			pathSegment.PropertyChanged += OnPathSegmentPropertyChanged;
			_subscribedSegments.Add(pathSegment);
		}

		void UnsubscribeFromPathSegmentPropertyChanged(PathSegment pathSegment)
		{
			if (!_subscribedSegments.Contains(pathSegment))
			{
				return;
			}

			pathSegment.PropertyChanged -= OnPathSegmentPropertyChanged;
			_subscribedSegments.Remove(pathSegment);
		}

		void UnsubscribeFromAllPathSegmentPropertyChanged()
		{
			for (int i = _subscribedSegments.Count - 1; i >= 0; i--)
			{
				UnsubscribeFromPathSegmentPropertyChanged(_subscribedSegments[i]);
			}
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