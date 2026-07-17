#nullable disable
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

		readonly WeakNotifyCollectionChangedProxy _childrenProxy = new();
		readonly NotifyCollectionChangedEventHandler _childrenCollectionChangedHandler;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformGroup"/> class.
		/// </summary>
		public TransformGroup()
		{
			_childrenCollectionChangedHandler = OnChildrenCollectionChanged;
			Children = new TransformCollection();
		}

		~TransformGroup() => _childrenProxy.Unsubscribe();

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
			var transformGroup = bindable as TransformGroup;

			if (oldValue != null)
			{
				transformGroup._childrenProxy.Unsubscribe();
			}

			if (newValue is TransformCollection newCollection)
			{
				transformGroup._childrenProxy.Subscribe(newCollection, transformGroup._childrenCollectionChangedHandler);
			}

			(bindable as TransformGroup).UpdateTransformMatrix();
		}

		void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.NewItems != null)
				foreach (INotifyPropertyChanged item in args.NewItems)
				{
					item.PropertyChanged += OnTransformPropertyChanged;
				}

			if (args.OldItems != null)
				foreach (INotifyPropertyChanged item in args.OldItems)
				{
					item.PropertyChanged -= OnTransformPropertyChanged;
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

			foreach (Transform child in Children)
				matrix = Matrix.Multiply(matrix, child.Value);

			Value = matrix;
		}
	}
}