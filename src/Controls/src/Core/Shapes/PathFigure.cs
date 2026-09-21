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
		readonly WeakNotifyCollectionChangedProxy _segmentsCollectionProxy = new();
		readonly List<WeakNotifyPropertyChangedProxy> _segmentProxies = new();
		NotifyCollectionChangedEventHandler _segmentsCollectionChanged;
		PropertyChangedEventHandler _segmentPropertyChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="PathFigure"/> class.
		/// </summary>
		public PathFigure()
		{
			Segments = new PathSegmentCollection();
		}

		~PathFigure() => UnsubscribeAllSegments();

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
			if (oldCollection != null)
				UnsubscribeAllSegments();

			if (newCollection == null)
				return;

			_segmentsCollectionChanged ??= OnPathSegmentCollectionChanged;
			_segmentPropertyChanged ??= OnPathSegmentPropertyChanged;

			_segmentsCollectionProxy.Subscribe(newCollection, _segmentsCollectionChanged);

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
			for (int i = 0; i < _segmentProxies.Count; i++)
			{
				if (_segmentProxies[i].TryGetSource(out var proxySource) && ReferenceEquals(proxySource, pathSegment))
					return;
			}

			_segmentProxies.Add(new WeakNotifyPropertyChangedProxy(pathSegment, _segmentPropertyChanged));
		}

		void UnsubscribeFromPathSegmentPropertyChanged(PathSegment pathSegment)
		{
			for (int i = _segmentProxies.Count - 1; i >= 0; i--)
			{
				var proxy = _segmentProxies[i];
				if (proxy.TryGetSource(out var proxySource) && ReferenceEquals(proxySource, pathSegment))
				{
					proxy.Unsubscribe();
					_segmentProxies.RemoveAt(i);
					break;
				}
			}
		}

		void UnsubscribeAllSegments()
		{
			_segmentsCollectionProxy.Unsubscribe();
			UnsubscribeFromAllPathSegmentPropertyChanged();
		}

		void UnsubscribeFromAllPathSegmentPropertyChanged()
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