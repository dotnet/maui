using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xamarin.Forms.Shapes
{
	[ContentProperty("Children")]
	public class GeometryGroup : Geometry
	{
		public static readonly BindableProperty ChildrenProperty =
			BindableProperty.Create(nameof(Children), typeof(GeometryCollection), typeof(GeometryGroup), null,
				propertyChanged: OnChildrenChanged);

		static void OnChildrenChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as GeometryGroup)?.UpdateChildren(oldValue as GeometryCollection, newValue as GeometryCollection);
		}

		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(GeometryGroup), FillRule.EvenOdd);

		public GeometryGroup()
		{
			Children = new GeometryCollection();
		}

		public GeometryCollection Children
		{
			set { SetValue(ChildrenProperty, value); }
			get { return (GeometryCollection)GetValue(ChildrenProperty); }
		}

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
	}
}