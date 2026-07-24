#nullable disable
using System;
using System.Collections.Generic;
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

		ChildrenSubscriptions _childrenSubscriptions;
		NotifyCollectionChangedEventHandler _childrenCollectionChanged;
		PropertyChangedEventHandler _childrenPropertyChanged;

		void UpdateChildren(GeometryCollection oldCollection, GeometryCollection newCollection)
		{
			if (oldCollection != null)
			{
				_childrenSubscriptions?.UnsubscribeAll();
			}

			if (newCollection == null)
				return;

			_childrenCollectionChanged ??= OnChildrenCollectionChanged;
			_childrenPropertyChanged ??= OnChildrenPropertyChanged;

			var subscriptions = _childrenSubscriptions ??= new ChildrenSubscriptions();
			subscriptions.Subscribe(newCollection, _childrenCollectionChanged);

			foreach (var newChildren in newCollection)
			{
				if (newChildren is not null)
				{
					subscriptions.Add(newChildren, _childrenPropertyChanged);
				}
			}
		}

		void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				// GeometryCollection is sealed, so Reset follows Clear after the collection is empty.
				_childrenSubscriptions?.ResetChildren();
			}
			else if (e.OldItems != null)
			{
				foreach (var oldItem in e.OldItems)
				{
					if (!(oldItem is Geometry oldGeometry))
						continue;

					_childrenSubscriptions?.Remove(oldGeometry);
				}
			}

			if (e.NewItems != null)
			{
				foreach (var newItem in e.NewItems)
				{
					if (!(newItem is Geometry newGeometry))
						continue;

					_childrenSubscriptions?.Add(newGeometry, _childrenPropertyChanged);
				}
			}

			Invalidate();
		}

		sealed class ChildrenSubscriptions
		{
			readonly WeakNotifyCollectionChangedProxy _collectionProxy = new();
			readonly List<WeakNotifyPropertyChangedProxy> _childProxies = new();

			~ChildrenSubscriptions() => UnsubscribeAll();

			public void Subscribe(GeometryCollection source, NotifyCollectionChangedEventHandler handler)
			{
				_collectionProxy.Subscribe(source, handler);
			}

			public void Add(Geometry source, PropertyChangedEventHandler handler)
			{
				_childProxies.Add(new WeakNotifyPropertyChangedProxy(source, handler));
			}

			public void Remove(Geometry source)
			{
				for (int i = _childProxies.Count - 1; i >= 0; i--)
				{
					var proxy = _childProxies[i];
					if (proxy.TryGetSource(out var proxySource) && ReferenceEquals(proxySource, source))
					{
						proxy.Unsubscribe();
						_childProxies.RemoveAt(i);
						break;
					}
				}
			}

			public void ResetChildren()
			{
				UnsubscribeChildren();
			}

			public void UnsubscribeAll()
			{
				_collectionProxy.Unsubscribe();
				UnsubscribeChildren();
			}

			void UnsubscribeChildren()
			{
				for (int i = 0; i < _childProxies.Count; i++)
					_childProxies[i].Unsubscribe();

				_childProxies.Clear();
			}
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
			var children = Children;
			if (children is null)
				return;

			foreach (var c in children)
			{
				c?.AppendPath(path);
			}
		}
	}
}
