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
		readonly Dictionary<INotifyPropertyChanged, (WeakNotifyPropertyChangedProxy Proxy, int Count)> _subscribedTransforms = new();
		WeakNotifyCollectionChangedProxy _childrenCollectionChangedProxy;
		NotifyCollectionChangedEventHandler _childrenCollectionChanged;
		PropertyChangedEventHandler _transformPropertyChanged;

		/// <summary>Bindable property for <see cref="Children"/>.</summary>
		public static readonly BindableProperty ChildrenProperty =
			BindableProperty.Create(nameof(Children), typeof(TransformCollection), typeof(TransformGroup), null, propertyChanged: OnChildrenChanged);

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformGroup"/> class.
		/// </summary>
		public TransformGroup()
		{
			Children = new TransformCollection();
		}

		~TransformGroup()
		{
			_childrenCollectionChangedProxy?.Unsubscribe();
			ClearAllTransformSubscriptions();
		}

		/// <summary>
		/// Gets or sets the collection of child <see cref="Transform"/> objects. This is a bindable property.
		/// </summary>
		public TransformCollection Children
		{
			set { SetValue(ChildrenProperty, value); }
			get { return (TransformCollection)GetValue(ChildrenProperty); }
		}

		static void OnChildrenChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var transformGroup = (TransformGroup)bindable;
			transformGroup.UpdateChildren(
			 oldValue as TransformCollection,
			 newValue as TransformCollection);
		}

		void UpdateChildren(TransformCollection oldCollection, TransformCollection newCollection)
		{
			DetachCollection(oldCollection);
			AttachCollection(newCollection);

			UpdateTransformMatrix();
		}

		void AttachCollection(TransformCollection collection)
		{
			if (collection is null)
			{
				return;
			}

			_childrenCollectionChanged ??= OnChildrenCollectionChanged;
			_childrenCollectionChangedProxy ??= new WeakNotifyCollectionChangedProxy();
			_childrenCollectionChangedProxy.Subscribe(collection, _childrenCollectionChanged);

			foreach (var transform in collection)
			{
				SubscribeToTransformPropertyChanged(transform);
			}
		}

		void DetachCollection(TransformCollection collection)
		{
			if (collection is null)
			{
				return;
			}

			_childrenCollectionChangedProxy?.Unsubscribe();

			ClearAllTransformSubscriptions();
		}

		void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				ClearAllTransformSubscriptions();

				if (sender is TransformCollection collection)
				{
					foreach (INotifyPropertyChanged item in collection)
					{
						SubscribeToTransformPropertyChanged(item);
					}
				}
			}
			else
			{
				if (args.OldItems is not null)
				{
					foreach (INotifyPropertyChanged item in args.OldItems)
					{
						UnsubscribeFromTransformPropertyChanged(item);
					}
				}

				if (args.NewItems is not null)
				{
					foreach (INotifyPropertyChanged item in args.NewItems)
					{
						SubscribeToTransformPropertyChanged(item);
					}
				}
			}

			UpdateTransformMatrix();
		}

		void SubscribeToTransformPropertyChanged(INotifyPropertyChanged item)
		{
			if (_subscribedTransforms.TryGetValue(item, out var subscription))
			{
				_subscribedTransforms[item] = (subscription.Proxy, subscription.Count + 1);
				return;
			}

			_transformPropertyChanged ??= OnTransformPropertyChanged;
			_subscribedTransforms[item] = (new WeakNotifyPropertyChangedProxy(item, _transformPropertyChanged), 1);
		}

		void UnsubscribeFromTransformPropertyChanged(INotifyPropertyChanged item)
		{
			if (!_subscribedTransforms.TryGetValue(item, out var subscription))
			{
				return;
			}

			if (subscription.Count > 1)
			{
				_subscribedTransforms[item] = (subscription.Proxy, subscription.Count - 1);
				return;
			}

			subscription.Proxy.Unsubscribe();
			_subscribedTransforms.Remove(item);
		}

		void ClearAllTransformSubscriptions()
		{
			foreach (var subscription in _subscribedTransforms.Values)
			{
				subscription.Proxy.Unsubscribe();
			}

			_subscribedTransforms.Clear();
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