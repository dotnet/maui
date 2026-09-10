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
		// A GeometryCollection and the geometries inside it can outlive the group that consumes them --
		// for example when they are declared in a ResourceDictionary and shared between groups. Both the
		// collection subscription and the per-geometry subscriptions are therefore weak, so a long-lived
		// collection or geometry never roots a discarded GeometryGroup.
		WeakNotifyCollectionChangedProxy _childrenProxy;
		NotifyCollectionChangedEventHandler _childrenChanged;
		PropertyChangedEventHandler _childPropertyChanged;
		readonly Dictionary<Geometry, (int Count, WeakNotifyPropertyChangedProxy Proxy)> _subscriptionRefCounts = new();

		~GeometryGroup()
		{
			_childrenProxy?.Unsubscribe();

			foreach (var entry in _subscriptionRefCounts.Values)
				entry.Proxy.Unsubscribe();
		}

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
			DetachCollection(oldCollection);
			AttachCollection(newCollection);

			Invalidate();
		}

		void AttachCollection(GeometryCollection collection)
		{
			if (collection == null)
				return;

			_childrenProxy ??= new WeakNotifyCollectionChangedProxy();
			_childrenChanged ??= OnChildrenCollectionChanged;
			_childrenProxy.Subscribe(collection, _childrenChanged);

			foreach (var geometry in collection)
			{
				SubscribeToGeometry(geometry);
			}
		}

		void DetachCollection(GeometryCollection collection)
		{
			if (collection == null)
				return;

			_childrenProxy?.Unsubscribe();

			foreach (var geometry in collection)
			{
				UnsubscribeFromGeometry(geometry);
			}
		}

		void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewItems != null)
					{
						foreach (Geometry geometry in e.NewItems)
						{
							SubscribeToGeometry(geometry);
						}
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldItems != null)
					{
						foreach (Geometry geometry in e.OldItems)
						{
							UnsubscribeFromGeometry(geometry);
						}
					}
					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.OldItems != null)
					{
						foreach (Geometry geometry in e.OldItems)
						{
							UnsubscribeFromGeometry(geometry);
						}
					}

					if (e.NewItems != null)
					{
						foreach (Geometry geometry in e.NewItems)
						{
							SubscribeToGeometry(geometry);
						}
					}
					break;

				case NotifyCollectionChangedAction.Move:
					// No subscription changes required.
					break;

				case NotifyCollectionChangedAction.Reset:
					ResubscribeCollection(sender as GeometryCollection);
					break;
			}

			Invalidate();
		}

		void ResubscribeCollection(GeometryCollection collection)
		{
			UnsubscribeFromAllChildren();

			if (collection == null)
				return;

			foreach (var geometry in collection)
			{
				SubscribeToGeometry(geometry);
			}
		}

		void SubscribeToGeometry(Geometry geometry)
		{
			if (geometry == null)
				return;

			if (_subscriptionRefCounts.TryGetValue(geometry, out var entry))
			{
				_subscriptionRefCounts[geometry] = (entry.Count + 1, entry.Proxy);
				return;
			}

			_childPropertyChanged ??= OnChildrenPropertyChanged;

			var proxy = new WeakNotifyPropertyChangedProxy();
			proxy.Subscribe(geometry, _childPropertyChanged);
			_subscriptionRefCounts[geometry] = (1, proxy);
		}

		void UnsubscribeFromGeometry(Geometry geometry)
		{
			if (geometry == null)
				return;

			if (!_subscriptionRefCounts.TryGetValue(geometry, out var entry))
				return;

			if (entry.Count > 1)
			{
				_subscriptionRefCounts[geometry] = (entry.Count - 1, entry.Proxy);
				return;
			}

			_subscriptionRefCounts.Remove(geometry);
			entry.Proxy.Unsubscribe();
		}

		void UnsubscribeFromAllChildren()
		{
			foreach (var entry in _subscriptionRefCounts.Values)
			{
				entry.Proxy.Unsubscribe();
			}

			_subscriptionRefCounts.Clear();
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
