using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Children))]
	public abstract class Layout : View, Microsoft.Maui.ILayout, IList<IView>, IBindableLayout, IPaddingElement, IVisualTreeElement
	{
		ReadOnlyCastingList<Element, IView> _logicalChildren;

		protected ILayoutManager _layoutManager;

		public ILayoutManager LayoutManager => _layoutManager ??= CreateLayoutManager();

		// The actual backing store for the IViews in the ILayout
		readonly List<IView> _children = new();

		// This provides a Children property for XAML 
		public IList<IView> Children => this;

		public ILayoutHandler LayoutHandler => Handler as ILayoutHandler;
		IList IBindableLayout.Children => _children;

		internal override IReadOnlyList<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCastingList<Element, IView>(_children);

		public int Count => _children.Count;

		public bool IsReadOnly => ((ICollection<IView>)_children).IsReadOnly;

		public IView this[int index]
		{
			get => _children[index];
			set
			{
				var old = _children[index];

				if (old == value)
				{
					return;
				}

				if (old is Element oldElement)
				{
					oldElement.Parent = null;
				}

				LayoutHandler?.Remove(old);

				_children[index] = value;

				if (value is Element newElement)
				{
					newElement.Parent = this;
				}

				LayoutHandler?.Add(value);

				InvalidateMeasure();
			}
		}

		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingElement.PaddingProperty);
			set => SetValue(PaddingElement.PaddingProperty, value);
		}

		protected abstract ILayoutManager CreateLayoutManager();

		public IEnumerator<IView> GetEnumerator() => _children.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _children.GetEnumerator();

		public override SizeRequest Measure(double widthConstraint, double heightConstraint, MeasureFlags flags = MeasureFlags.None)
		{
			var size = (this as IView).Measure(widthConstraint, heightConstraint);
			return new SizeRequest(size);
		}

		protected override void InvalidateMeasureOverride()
		{
			base.InvalidateMeasureOverride();
			foreach (var child in Children)
			{
				child.InvalidateMeasure();
			}
		}

		public virtual void Add(IView child)
		{
			if (child == null)
				return;
			_children.Add(child);
			if (child is Element element)
				element.Parent = this;
			InvalidateMeasure();
			LayoutHandler?.Add(child);
		}

		public void Clear()
		{
			for (int n = _children.Count - 1; n >= 0; n--)
			{
				Remove(this[n]);
			}
		}

		public bool Contains(IView item)
		{
			return _children.Contains(item);
		}

		public void CopyTo(IView[] array, int arrayIndex)
		{
			_children.CopyTo(array, arrayIndex);
		}

		public int IndexOf(IView item)
		{
			return _children.IndexOf(item);
		}

		public void Insert(int index, IView child)
		{
			if (child == null)
				return;

			_children.Insert(index, child);

			if (child is Element element)
				element.Parent = this;

			InvalidateMeasure();

			LayoutHandler?.Add(child);
		}

		public virtual bool Remove(IView child)
		{
			if (child == null)
				return false;

			var result = _children.Remove(child);

			if (child is Element element)
				element.Parent = null;

			InvalidateMeasure();

			LayoutHandler?.Remove(child);

			return result;
		}

		public void RemoveAt(int index)
		{
			if (index >= Count)
			{
				return;
			}

			var child = _children[index];

			_children.RemoveAt(index);

			if (child is Element element)
				element.Parent = null;

			InvalidateMeasure();

			LayoutHandler?.Remove(child);
		}

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			InvalidateMeasure();
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return new Thickness(0);
		}

		IReadOnlyList<IVisualTreeElement> IVisualTreeElement.GetVisualChildren() => Children.Cast<IVisualTreeElement>().ToList().AsReadOnly();
	}
}
