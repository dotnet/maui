using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility
{
	public abstract partial class Layout<T>
	{
		public static readonly BindableProperty StrokeShapeProperty =
			BindableProperty.Create(nameof(StrokeShape), typeof(IShape), typeof(Layout), null);

		public static readonly BindableProperty StrokeProperty =
			BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(Layout), null);

		public static readonly BindableProperty StrokeThicknessProperty =
			BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(Layout), 1.0);

		public static readonly BindableProperty StrokeDashArrayProperty =
			BindableProperty.Create(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(Layout), null,
				defaultValueCreator: bindable => new DoubleCollection());

		public static readonly BindableProperty StrokeDashOffsetProperty =
			BindableProperty.Create(nameof(StrokeDashOffset), typeof(double), typeof(Layout), 0.0);

		public static readonly BindableProperty StrokeLineCapProperty =
			BindableProperty.Create(nameof(StrokeLineCap), typeof(PenLineCap), typeof(Layout), PenLineCap.Flat);

		public static readonly BindableProperty StrokeLineJoinProperty =
			BindableProperty.Create(nameof(StrokeLineJoin), typeof(PenLineJoin), typeof(Layout), PenLineJoin.Miter);

		public static readonly BindableProperty StrokeMiterLimitProperty =
			BindableProperty.Create(nameof(StrokeMiterLimit), typeof(double), typeof(Layout), 10.0);

		[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
		public IShape StrokeShape
		{
			set { SetValue(StrokeShapeProperty, value); }
			get { return (IShape)GetValue(StrokeShapeProperty); }
		}

		public Brush Stroke
		{
			set { SetValue(StrokeProperty, value); }
			get { return (Brush)GetValue(StrokeProperty); }
		}

		public double StrokeThickness
		{
			set { SetValue(StrokeThicknessProperty, value); }
			get { return (double)GetValue(StrokeThicknessProperty); }
		}

		public DoubleCollection StrokeDashArray
		{
			set { SetValue(StrokeDashArrayProperty, value); }
			get { return (DoubleCollection)GetValue(StrokeDashArrayProperty); }
		}

		public double StrokeDashOffset
		{
			set { SetValue(StrokeDashOffsetProperty, value); }
			get { return (double)GetValue(StrokeDashOffsetProperty); }
		}

		public PenLineCap StrokeLineCap
		{
			set { SetValue(StrokeLineCapProperty, value); }
			get { return (PenLineCap)GetValue(StrokeLineCapProperty); }
		}

		public PenLineJoin StrokeLineJoin
		{
			set { SetValue(StrokeLineJoinProperty, value); }
			get { return (PenLineJoin)GetValue(StrokeLineJoinProperty); }
		}

		public double StrokeMiterLimit
		{
			set { SetValue(StrokeMiterLimitProperty, value); }
			get { return (double)GetValue(StrokeMiterLimitProperty); }
		}

		int ICollection<IView>.Count => _children.Count;
		bool ICollection<IView>.IsReadOnly => ((ICollection<IView>)_children).IsReadOnly;
		public IView this[int index] { get => _children[index]; set => _children[index] = (T)value; }

		void ICollection<IView>.Add(IView child)
		{
			if (child is T view)
			{
				_children.Add(view);
			}
		}

		bool ICollection<IView>.Remove(IView child)
		{
			if (child is T view)
			{
				_children.Remove(view);
				return true;
			}

			return false;
		}

		int IList<IView>.IndexOf(IView child)
		{
			return _children.IndexOf(child);
		}

		void IList<IView>.Insert(int index, IView child)
		{
			if (child is T view)
			{
				_children.Insert(index, view);
			}
		}

		void IList<IView>.RemoveAt(int index)
		{
			_children.RemoveAt(index);
		}

		void ICollection<IView>.Clear()
		{
			_children.Clear();
		}

		bool ICollection<IView>.Contains(IView child)
		{
			return _children.Contains(child);
		}

		void ICollection<IView>.CopyTo(IView[] array, int arrayIndex)
		{
			_children.CopyTo(array, arrayIndex);
		}

		IEnumerator<IView> IEnumerable<IView>.GetEnumerator()
		{
			return _children.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _children.GetEnumerator();
		}
	}
}
