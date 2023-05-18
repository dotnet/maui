#nullable disable
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.PathFigure']/Docs/*" />
	[ContentProperty("Segments")]
	public sealed class PathFigure : BindableObject, IAnimatable
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='Segments']/Docs/*" />
		public PathSegmentCollection Segments
		{
			set { SetValue(SegmentsProperty, value); }
			get { return (PathSegmentCollection)GetValue(SegmentsProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='StartPoint']/Docs/*" />
		public Point StartPoint
		{
			set { SetValue(StartPointProperty, value); }
			get { return (Point)GetValue(StartPointProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='IsClosed']/Docs/*" />
		public bool IsClosed
		{
			set { SetValue(IsClosedProperty, value); }
			get { return (bool)GetValue(IsClosedProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='IsFilled']/Docs/*" />
		public bool IsFilled
		{
			set { SetValue(IsFilledProperty, value); }
			get { return (bool)GetValue(IsFilledProperty); }
		}

		internal event EventHandler InvalidatePathSegmentRequested;

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='BatchBegin']/Docs/*" />
		public void BatchBegin()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='BatchCommit']/Docs/*" />
		public void BatchCommit()
		{

		}

		void UpdatePathSegmentCollection(PathSegmentCollection oldCollection, PathSegmentCollection newCollection)
		{
			if (oldCollection != null)
			{
				oldCollection.CollectionChanged -= OnPathSegmentCollectionChanged;

				foreach (var oldPathSegment in oldCollection)
				{
					oldPathSegment.PropertyChanged -= OnPathSegmentPropertyChanged;
				}
			}

			if (newCollection == null)
				return;

			newCollection.CollectionChanged += OnPathSegmentCollectionChanged;

			foreach (var newPathSegment in newCollection)
			{
				newPathSegment.PropertyChanged += OnPathSegmentPropertyChanged;
			}
		}

		void OnPathSegmentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (var oldItem in e.OldItems)
				{
					if (!(oldItem is PathSegment oldPathSegment))
						continue;

					oldPathSegment.PropertyChanged -= OnPathSegmentPropertyChanged;
				}
			}

			if (e.NewItems != null)
			{
				foreach (var newItem in e.NewItems)
				{
					if (!(newItem is PathSegment newPathSegment))
						continue;

					newPathSegment.PropertyChanged += OnPathSegmentPropertyChanged;
				}
			}

			Invalidate();
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