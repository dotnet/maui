using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Children))]
	public abstract class Layout : View, Microsoft.Maui.ILayout, IList<IView>, IBindableLayout, IPaddingElement, IVisualTreeElement, ISafeAreaView
	{
		ReadOnlyCastingList<Element, IView> _logicalChildren;

		protected ILayoutManager _layoutManager;

		public ILayoutManager LayoutManager => _layoutManager ??= CreateLayoutManager();

		// The actual backing store for the IViews in the ILayout
		readonly List<IView> _children = new();

		// This provides a Children property for XAML 
		public IList<IView> Children => this;

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
					VisualDiagnostics.OnChildRemoved(this, oldElement, index);
				}

				_children[index] = value;

				if (value is Element newElement)
				{
					newElement.Parent = this;
					VisualDiagnostics.OnChildAdded(this, newElement);
				}

				UpdateInHandler(index, value);
			}
		}

		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingElement.PaddingProperty);
			set => SetValue(PaddingElement.PaddingProperty, value);
		}

		public bool IgnoreSafeArea { get; set; }

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
			{
				element.Parent = this;
				VisualDiagnostics.OnChildAdded(this, element);
			}

			AddToHandler(_children.Count, child);
		}

		public void Clear()
		{
			foreach (var child in this)
			{
				if (child is Element element)
					element.Parent = null;
			}

			_children.Clear();
			ClearHandler();
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
			{
				element.Parent = this;
				VisualDiagnostics.OnChildAdded(this, element);
			}

			AddToHandler(index, child);
		}

		public virtual bool Remove(IView child)
		{
			if (child == null)
				return false;

			var index = _children.IndexOf(child);

			if (index == -1)
			{
				return false;
			}

			RemoveAt(index);

			return true;
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
			{
				element.Parent = null;
				VisualDiagnostics.OnChildRemoved(this, element, index);
			}

			RemoveFromHandler(index, child);
		}

		void RemoveFromHandler(IView view)
		{
			Handler?.Invoke(nameof(ILayoutHandler.Remove), view);
		}

		void AddToHandler(int index, IView view)
		{
			var args = new Maui.Handlers.LayoutHandlerUpdate(index, view);
			Handler?.Invoke(nameof(ILayoutHandler.Add), args);
		}

		void ClearHandler()
		{
			Handler?.Invoke(nameof(ILayoutHandler.Clear));
		}

		void RemoveFromHandler(int index, IView view)
		{
			var args = new Maui.Handlers.LayoutHandlerUpdate(index, view);
			Handler?.Invoke(nameof(ILayoutHandler.Remove), args);
		}

		void InsertIntoHandler(int index, IView view)
		{
			var args = new Maui.Handlers.LayoutHandlerUpdate(index, view);
			Handler?.Invoke(nameof(ILayoutHandler.Insert), args);
		}

		void UpdateInHandler(int index, IView view)
		{
			var args = new Maui.Handlers.LayoutHandlerUpdate(index, view);
			Handler?.Invoke(nameof(ILayoutHandler.Update), args);
		}

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			(this as IView).InvalidateMeasure();
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return new Thickness(0);
		}

		IReadOnlyList<IVisualTreeElement> IVisualTreeElement.GetVisualChildren() => Children.Cast<IVisualTreeElement>().ToList().AsReadOnly();
	}
}
