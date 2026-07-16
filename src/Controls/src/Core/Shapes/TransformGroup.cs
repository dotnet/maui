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

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformGroup"/> class.
		/// </summary>
		public TransformGroup()
		{
			Children = new TransformCollection();
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
			(bindable as TransformGroup)?.UpdateChildren(oldValue as TransformCollection, newValue as TransformCollection);
		}

		ChildrenSubscriptions _childrenSubscriptions;
		NotifyCollectionChangedEventHandler _childrenCollectionChanged;
		PropertyChangedEventHandler _childPropertyChanged;

		void UpdateChildren(TransformCollection oldCollection, TransformCollection newCollection)
		{
			if (oldCollection != null)
			{
				_childrenSubscriptions?.UnsubscribeAll();
				// Keep the empty helper for reuse; UnsubscribeAll releases every source and child proxy.
			}

			if (newCollection != null)
			{
				_childrenCollectionChanged ??= OnChildrenCollectionChanged;
				_childPropertyChanged ??= OnTransformPropertyChanged;

				var subscriptions = _childrenSubscriptions ??= new ChildrenSubscriptions();
				subscriptions.Subscribe(newCollection, _childrenCollectionChanged, _childPropertyChanged);
			}

			UpdateTransformMatrix();
		}

		void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				_childrenSubscriptions?.ResetChildren();
			}
			else if (args.Action != NotifyCollectionChangedAction.Move)
			{
				if (args.OldItems != null)
				{
					foreach (var oldItem in args.OldItems)
					{
						if (oldItem is Transform oldTransform)
						{
							_childrenSubscriptions?.Remove(oldTransform);
						}
					}
				}

				if (args.NewItems != null)
				{
					_childPropertyChanged ??= OnTransformPropertyChanged;

					foreach (var newItem in args.NewItems)
					{
						if (newItem is Transform newTransform)
						{
							_childrenSubscriptions?.Add(newTransform, _childPropertyChanged);
						}
					}
				}
			}

			UpdateTransformMatrix();
		}

		void OnTransformPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			UpdateTransformMatrix();
		}

		void UpdateTransformMatrix()
		{
			var matrix = new Matrix();

			if (Children is not null)
			{
				foreach (var child in Children)
				{
					if (child is not null)
						matrix = Matrix.Multiply(matrix, child.Value);
				}
			}

			Value = matrix;
		}

		// Keeps the CollectionChanged and per-child PropertyChanged subscriptions weak so a shared
		// or long-lived TransformCollection cannot root the TransformGroup. The finalizer tears the
		// subscriptions down, mirroring the pattern used by other WeakEventProxy owners.
		sealed class ChildrenSubscriptions
		{
			readonly WeakNotifyCollectionChangedProxy _collectionProxy = new();
			readonly List<WeakNotifyPropertyChangedProxy> _childProxies = new();

			~ChildrenSubscriptions() => UnsubscribeAll();

			public void Subscribe(
				TransformCollection source,
				NotifyCollectionChangedEventHandler collectionChangedHandler,
				PropertyChangedEventHandler childPropertyChangedHandler)
			{
				_collectionProxy.Subscribe(source, collectionChangedHandler);

				foreach (var child in source)
				{
					if (child is not null)
					{
						Add(child, childPropertyChangedHandler);
					}
				}
			}

			public void Add(Transform source, PropertyChangedEventHandler handler)
			{
				_childProxies.Add(new WeakNotifyPropertyChangedProxy(source, handler));
			}

			public void Remove(Transform source)
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
				// TransformCollection is sealed, so Reset means the current children were cleared.
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
	}
}
