#nullable disable
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Represents a composite <see cref="Transform"/> composed of multiple transforms applied in sequence.
	/// </summary>
	[ContentProperty("Children")]
	public sealed class TransformGroup : Transform
	{
		/// <summary>Bindable property for <see cref="Children"/>.</summary>
		public static readonly BindableProperty ChildrenProperty =
			BindableProperty.Create(nameof(Children), typeof(TransformCollection), typeof(TransformGroup), null,
				propertyChanged: OnTransformGroupChanged);

		// Subscribe via weak proxies so a shared / long-lived TransformCollection (and its child
		// transforms) does not keep this TransformGroup rooted in memory. See issue #36367.
		readonly WeakNotifyCollectionChangedProxy _childrenProxy = new();
		readonly NotifyCollectionChangedEventHandler _childrenCollectionChangedHandler;
		readonly PropertyChangedEventHandler _childPropertyChangedHandler;
		readonly List<WeakNotifyPropertyChangedProxy> _childProxies = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformGroup"/> class.
		/// </summary>
		public TransformGroup()
		{
			_childrenCollectionChangedHandler = OnChildrenCollectionChanged;
			_childPropertyChangedHandler = OnTransformPropertyChanged;
			Children = new TransformCollection();
		}

		~TransformGroup()
		{
			_childrenProxy.Unsubscribe();

			foreach (var proxy in _childProxies)
			{
				proxy.Unsubscribe();
			}
		}

		/// <summary>
		/// Gets or sets the collection of child <see cref="Transform"/> objects. This is a bindable property.
		/// </summary>
		public TransformCollection Children
		{
			set { SetValue(ChildrenProperty, value); }
			get { return (TransformCollection)GetValue(ChildrenProperty); }
		}

		static void OnTransformGroupChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var transformGroup = (TransformGroup)bindable;

			if (oldValue != null)
			{
				transformGroup._childrenProxy.Unsubscribe();
				transformGroup.ClearChildPropertyChangedSubscriptions();
			}

			if (newValue is INotifyCollectionChanged newCollection)
			{
				transformGroup._childrenProxy.Subscribe(newCollection, transformGroup._childrenCollectionChangedHandler);
			}

			transformGroup.UpdateTransformMatrix();
		}

		void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.NewItems != null)
				foreach (INotifyPropertyChanged item in args.NewItems)
				{
					SubscribeChildPropertyChanged(item);
				}

			if (args.OldItems != null)
				foreach (INotifyPropertyChanged item in args.OldItems)
				{
					UnsubscribeChildPropertyChanged(item);
				}

			if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				ClearChildPropertyChangedSubscriptions();
			}

			UpdateTransformMatrix();
		}

		void SubscribeChildPropertyChanged(INotifyPropertyChanged child)
		{
			if (child == null)
				return;

			_childProxies.Add(new WeakNotifyPropertyChangedProxy(child, _childPropertyChangedHandler));
		}

		void UnsubscribeChildPropertyChanged(INotifyPropertyChanged child)
		{
			for (int i = _childProxies.Count - 1; i >= 0; i--)
			{
				if (_childProxies[i].TryGetSource(out var source) && ReferenceEquals(source, child))
				{
					_childProxies[i].Unsubscribe();
					_childProxies.RemoveAt(i);
					return;
				}
			}
		}

		void ClearChildPropertyChangedSubscriptions()
		{
			foreach (var proxy in _childProxies)
			{
				proxy.Unsubscribe();
			}

			_childProxies.Clear();
		}

		void OnTransformPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			UpdateTransformMatrix();
		}

		void UpdateTransformMatrix()
		{
			var matrix = new Matrix();

			foreach (Transform child in Children)
				matrix = Matrix.Multiply(matrix, child.Value);

			Value = matrix;
		}
	}
}
