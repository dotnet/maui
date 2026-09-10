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
		// A TransformCollection and the transforms inside it can outlive the group that consumes them --
		// for example when they are declared in a ResourceDictionary and shared between groups. Both the
		// collection subscription and the per-transform subscriptions are therefore weak, so a long-lived
		// collection or transform never roots a discarded TransformGroup.
		WeakNotifyCollectionChangedProxy _childrenProxy;
		NotifyCollectionChangedEventHandler _childrenChanged;
		PropertyChangedEventHandler _transformPropertyChanged;
		readonly Dictionary<INotifyPropertyChanged, (int Count, WeakNotifyPropertyChangedProxy Proxy)> _subscribedTransforms = new();

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
			_childrenProxy?.Unsubscribe();

			foreach (var entry in _subscribedTransforms.Values)
				entry.Proxy.Unsubscribe();
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

			_childrenProxy ??= new WeakNotifyCollectionChangedProxy();
			_childrenChanged ??= OnChildrenCollectionChanged;
			_childrenProxy.Subscribe(collection, _childrenChanged);

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

			_childrenProxy?.Unsubscribe();

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
			if (_subscribedTransforms.TryGetValue(item, out var entry))
			{
				_subscribedTransforms[item] = (entry.Count + 1, entry.Proxy);
				return;
			}

			_transformPropertyChanged ??= OnTransformPropertyChanged;

			var proxy = new WeakNotifyPropertyChangedProxy();
			proxy.Subscribe(item, _transformPropertyChanged);
			_subscribedTransforms[item] = (1, proxy);
		}

		void UnsubscribeFromTransformPropertyChanged(INotifyPropertyChanged item)
		{
			if (!_subscribedTransforms.TryGetValue(item, out var entry))
			{
				return;
			}

			if (entry.Count > 1)
			{
				_subscribedTransforms[item] = (entry.Count - 1, entry.Proxy);
				return;
			}

			entry.Proxy.Unsubscribe();
			_subscribedTransforms.Remove(item);
		}

		// Unsubscribes all tracked transforms from PropertyChanged and clears the dictionary.
		void ClearAllTransformSubscriptions()
		{
			foreach (var entry in _subscribedTransforms.Values)
			{
				entry.Proxy.Unsubscribe();
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