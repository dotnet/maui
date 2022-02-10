using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.PathFigure']/Docs" />
	[ContentProperty("Segments")]
	public sealed class PathFigure : BindableObject, IAnimatable
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public PathFigure()
		{
			Segments = new PathSegmentCollection();
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='SegmentsProperty']/Docs" />
		public static readonly BindableProperty SegmentsProperty =
			BindableProperty.Create(nameof(Segments), typeof(PathSegmentCollection), typeof(PathFigure), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='StartPointProperty']/Docs" />
		public static readonly BindableProperty StartPointProperty =
			BindableProperty.Create(nameof(StartPoint), typeof(Point), typeof(PathFigure), new Point(0, 0));

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='IsClosedProperty']/Docs" />
		public static readonly BindableProperty IsClosedProperty =
			BindableProperty.Create(nameof(IsClosed), typeof(bool), typeof(PathFigure), false);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='IsFilledProperty']/Docs" />
		public static readonly BindableProperty IsFilledProperty =
			BindableProperty.Create(nameof(IsFilled), typeof(bool), typeof(PathFigure), true);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='Segments']/Docs" />
		public PathSegmentCollection Segments
		{
			set { SetValue(SegmentsProperty, value); }
			get { return (PathSegmentCollection)GetValue(SegmentsProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='StartPoint']/Docs" />
		public Point StartPoint
		{
			set { SetValue(StartPointProperty, value); }
			get { return (Point)GetValue(StartPointProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='IsClosed']/Docs" />
		public bool IsClosed
		{
			set { SetValue(IsClosedProperty, value); }
			get { return (bool)GetValue(IsClosedProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='IsFilled']/Docs" />
		public bool IsFilled
		{
			set { SetValue(IsFilledProperty, value); }
			get { return (bool)GetValue(IsFilledProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='BatchBegin']/Docs" />
		public void BatchBegin()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigure.xml" path="//Member[@MemberName='BatchCommit']/Docs" />
		public void BatchCommit()
		{

		}
	}
}