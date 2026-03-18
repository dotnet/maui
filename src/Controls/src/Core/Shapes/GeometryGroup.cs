#nullable disable
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A composite <see cref="Geometry"/> that combines multiple <see cref="Geometry"/> objects into a single shape.
	/// </summary>
	[ContentProperty("Children")]
	public class GeometryGroup : Geometry
	{
		/// <summary>Bindable property for <see cref="Children"/>.</summary>
		public static readonly BindableProperty ChildrenProperty =
			BindableProperty.Create(nameof(Children), typeof(GeometryCollection), typeof(GeometryGroup), null,
				propertyChanged: OnChildrenChanged);

		static void OnChildrenChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as GeometryGroup)?.UpdateChildren(oldValue as GeometryCollection, newValue as GeometryCollection);
		}

		/// <summary>Bindable property for <see cref="FillRule"/>.</summary>
		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(GeometryGroup), FillRule.EvenOdd);

		/// <summary>
		/// Initializes a new instance of the <see cref="GeometryGroup"/> class.
		/// </summary>
		public GeometryGroup()
		{
			Children = new GeometryCollection();
		}

		/// <summary>
		/// Gets or sets the collection of <see cref="Geometry"/> objects that make up this group. This is a bindable property.
		/// </summary>
		public GeometryCollection Children
		{
			set { SetValue(ChildrenProperty, value); }
			get { return (GeometryCollection)GetValue(ChildrenProperty); }
		}

		/// <summary>
		/// Gets or sets the <see cref="Shapes.FillRule"/> that determines how the intersecting areas of the child geometries are combined. This is a bindable property.
		/// </summary>
		public FillRule FillRule
		{
			set { SetValue(FillRuleProperty, value); }
			get { return (FillRule)GetValue(FillRuleProperty); }
		}

		public event EventHandler InvalidateGeometryRequested;

		void UpdateChildren(GeometryCollection oldCollection, GeometryCollection newCollection)
		{
			if (oldCollection != null)
			{
				oldCollection.CollectionChanged -= OnChildrenCollectionChanged;

				foreach (var oldChildren in oldCollection)
				{
					oldChildren.PropertyChanged -= OnChildrenPropertyChanged;
				}
			}

			if (newCollection == null)
				return;

			newCollection.CollectionChanged += OnChildrenCollectionChanged;

			foreach (var newChildren in newCollection)
			{
				newChildren.PropertyChanged += OnChildrenPropertyChanged;
			}
		}

		void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (var oldItem in e.OldItems)
				{
					if (!(oldItem is Geometry oldGeometry))
						continue;

					oldGeometry.PropertyChanged -= OnChildrenPropertyChanged;
				}
			}

			if (e.NewItems != null)
			{
				foreach (var newItem in e.NewItems)
				{
					if (!(newItem is Geometry newGeometry))
						continue;

					newGeometry.PropertyChanged += OnChildrenPropertyChanged;
				}
			}

			Invalidate();
		}

		void OnChildrenPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Invalidate();
		}

		void Invalidate()
		{
			InvalidateGeometryRequested?.Invoke(this, EventArgs.Empty);
		}

		public override void AppendPath(Graphics.PathF path)
		{
			foreach (var c in Children)
			{
				c.AppendPath(path);
			}
		}
	}
}
