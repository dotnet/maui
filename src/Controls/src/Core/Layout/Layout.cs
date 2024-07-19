#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{

#pragma warning disable RS0016 // Add public types and members to the declared API
	public abstract class Layout<T> : Layout where T : View, ICrossPlatformLayout
	{
		public new IList<T> Children { get; }
		public Layout()
		{
			Children = new CastingList<T, IView>(this);
		}

		// If we want to match the APIS

		public void ForceLayout() => SizeAllocated(Width, Height);

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);

			if (child is T typedChild)
			{
				OnAdded(typedChild);
#pragma warning disable CS0618 // Type or member is obsolete
				typedChild.MeasureInvalidated += OnChildMeasureInvalidated;
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			base.OnChildRemoved(child, oldLogicalIndex);

			if (child is T typedChild)
			{
				OnRemoved(typedChild);
#pragma warning disable CS0618 // Type or member is obsolete
				typedChild.MeasureInvalidated -= OnChildMeasureInvalidated;
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}

		/// <summary>
		/// Invoked when a child is added to the layout. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="view">The view which was added.</param>
		protected virtual void OnAdded(T view)
		{
		}

		/// <summary>
		/// Invoked when a child is removed the layout. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="view">The view which was removed.</param>
		protected virtual void OnRemoved(T view)
		{
		}

		[Obsolete("Use ZIndex")]
		public void LowerChild(View view)
		{
			if (!this.Contains(view) || this.First() == view)
			{
				return;
			}

			this.Remove(view);
			this.Insert(view, 0);
			OnChildrenReordered();
		}

		[Obsolete("Use ZIndex")]
		public void RaiseChild(View view)
		{
			if (!this.Contains(view) || this.Last() == view)
			{
				return;
			}

			this.Remove(view);
			this.Insert(view,this.Count - 1);
			OnChildrenReordered();
		}

		/// <summary>
		/// Instructs the layout to relayout all of its children.
		/// </summary>
		/// <remarks>This method starts a new layout cycle for the layout. Invoking this method frequently can negatively impact performance.</remarks>
		protected virtual void InvalidateLayout()
		{
			// Todo call this when InvalidateMeasure is called?
			(this as IView).InvalidateMeasure();
		}

		Graphics.Size _layoutChildren = Graphics.Size.Zero;
		Graphics.Size ICrossPlatformLayout.CrossPlatformArrange(Graphics.Rect bounds)
		{
			_layoutChildren = bounds.Size;
#pragma warning disable CS0618 // Type or member is obsolete
			LayoutChildren(bounds.X, bounds.Y, bounds.Width, bounds.Height);
#pragma warning restore CS0618 // Type or member is obsolete
			return _layoutChildren;
		}


		[Obsolete("Use your own LayoutManager")]
		protected virtual void LayoutChildren(double x, double y, double width, double height)
		{
			_layoutChildren = base._layoutManager.ArrangeChildren(new Graphics.Rect(x, y, width, height));
		}

		[Obsolete("This method on the original class makes no sense and was probably accidently made protected. I'll write a better message before merging.")]
		protected void OnChildMeasureInvalidated(object sender, EventArgs e)
		{
			OnChildMeasureInvalidated();
		}

		protected virtual void OnChildMeasureInvalidated()
		{
		}

		
		[Obsolete("Obsolete. This never really did anything because the platform is going to invalidate the layout anyway when views are removed/added.")]
		protected virtual bool ShouldInvalidateOnChildAdded(View child) => true;
		
		[Obsolete("Obsolete. This never really did anything because the platform is going to invalidate the layout anyway when views are removed/added.")]
		protected virtual bool ShouldInvalidateOnChildRemoved(View child) => true;
		
		/// <summary>
		/// Instructs the layout to relayout all of its children.
		/// </summary>
		/// <remarks>This method starts a new layout cycle for the layout. Invoking this method frequently can negatively impact performance.</remarks>
		protected void UpdateChildrenLayout()
		{
			(this as IView).InvalidateMeasure();
		}
	}

#pragma warning restore RS0016 // Add public types and members to the declared API

	/// <summary>
	/// Base class for layouts that allow you to arrange and group UI controls in your application.
	/// </summary>
	[ContentProperty(nameof(Children))]
	public abstract partial class Layout : View, Maui.ILayout, IList<IView>, IBindableLayout, IPaddingElement, IVisualTreeElement, ISafeAreaView, IInputTransparentContainerElement
	{
		protected ILayoutManager _layoutManager;

		ILayoutManager LayoutManager
		{
			get
			{
				return _layoutManager ??= GetLayoutManagerFromFactory(this) ?? CreateLayoutManager();
			}
		}

		static ILayoutManager GetLayoutManagerFromFactory(Layout layout)
		{
			var factory = layout.FindMauiContext()?.Services?.GetService<ILayoutManagerFactory>();
			return factory?.CreateLayoutManager(layout);
		}

		// The actual backing store for the IViews in the ILayout
		readonly List<IView> _children = new();

		/// <summary>
		/// Gets the child objects contained in this layout.
		/// </summary>
		public IList<IView> Children => this;

		IList IBindableLayout.Children => _children;

		/// <summary>
		/// Gets the child object count in this layout.
		/// </summary>
		public int Count => _children.Count;

		/// <summary>
		/// Gets whether this layout is readonly.
		/// </summary>
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

				_children.RemoveAt(index);
				if (old is Element oldElement)
				{
					RemoveLogicalChild(oldElement);
				}

				_children.Insert(index, value);

				if (value is Element newElement)
				{
					InsertLogicalChild(index, newElement);
				}

				OnUpdate(index, value, old);
			}
		}

		/// <summary>Bindable property for <see cref="IsClippedToBounds"/>.</summary>
		public static readonly BindableProperty IsClippedToBoundsProperty =
			BindableProperty.Create(nameof(IsClippedToBounds), typeof(bool), typeof(Layout), false,
				propertyChanged: IsClippedToBoundsPropertyChanged);

		/// <summary>
		/// Gets or sets a value which determines if the layout should clip its children to its bounds.
		/// The default value is <see langword="false"/>.
		/// </summary>
		public bool IsClippedToBounds
		{
			get => (bool)GetValue(IsClippedToBoundsProperty);
			set => SetValue(IsClippedToBoundsProperty, value);
		}

		static void IsClippedToBoundsPropertyChanged(BindableObject bindableObject, object oldValue, object newValue)
		{
			if (bindableObject is IView view)
			{
				view.Handler?.UpdateValue(nameof(Maui.ILayout.ClipsToBounds));
			}
		}

		bool Maui.ILayout.ClipsToBounds => IsClippedToBounds;

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <summary>
		/// Gets or sets the inner padding of the layout.
		/// The default value is a <see cref="Thickness"/> with all values set to 0.
		/// </summary>
		/// <remarks>The padding is the space between the bounds of a layout and the bounding region into which its children should be arranged into.</remarks>
		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingElement.PaddingProperty);
			set => SetValue(PaddingElement.PaddingProperty, value);
		}

		/// <inheritdoc cref="ISafeAreaView.IgnoreSafeArea"/>
		public bool IgnoreSafeArea { get; set; }

		/// <summary>
		/// Creates a manager object that can measure this layout and arrange its children.
		/// </summary>
		/// <returns>An object that implements <see cref="ILayoutManager"/> that manages this layout.</returns>
		protected abstract ILayoutManager CreateLayoutManager();

		/// <summary>
		/// Returns an enumerator that lists all of the children in this layout.
		/// </summary>
		/// <returns>A <see cref="IEnumerator{T}"/> of type <see cref="IView"/> with all the children in this layout.</returns>
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
		}

		/// <summary>
		/// Adds a child view to the end of this layout.
		/// </summary>
		/// <param name="child">The child view to add.</param>
		public void Add(IView child)
		{
			if (child == null)
			{
				return;
			}

			var index = _children.Count;
			_children.Add(child);

			if (child is Element element)
			{
				AddLogicalChild(element);
			}

			OnAdd(index, child);
		}

		/// <summary>
		/// Clears all child views from this layout.
		/// </summary>
		public void Clear()
		{
			for (int i = _children.Count - 1; i >= 0; i--)
			{
				var child = _children[i];
				_children.RemoveAt(i);
				if (child is Element element)
				{
					RemoveLogicalChild(element);
				}
			}
			OnClear();
		}

		/// <summary>
		/// Determines if the specified child view is contained in this layout.
		/// </summary>
		/// <param name="item">The child view for which to determine if it is contained in this layout.</param>
		/// <returns><see langword="true"/> if <paramref name="item"/> exists in this layout, otherwise <see langword="false"/>.</returns>
		public bool Contains(IView item)
		{
			return _children.Contains(item);
		}

		/// <summary>
		/// Copies the child views to the specified array.
		/// </summary>
		/// <param name="array">The target array to copy the child views to.</param>
		/// <param name="arrayIndex">The index at which the copying needs to start.</param>
		public void CopyTo(IView[] array, int arrayIndex)
		{
			_children.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets the index of a specified child view.
		/// </summary>
		/// <param name="item">The child view of which to determine the index.</param>
		/// <returns>The index of the specified view, if the view was not found this will return <c>-1</c>.</returns>
		public int IndexOf(IView item)
		{
			return _children.IndexOf(item);
		}

		/// <summary>
		/// Inserts a child view at the specified index.
		/// </summary>
		/// <param name="index">The index at which to specify the child view.</param>
		/// <param name="child">The child view to insert.</param>
		public void Insert(int index, IView child)
		{
			if (child == null)
			{
				return;
			}

			_children.Insert(index, child);

			if (child is Element element)
			{
				InsertLogicalChild(index, element);
			}

			OnInsert(index, child);
		}

		/// <summary>
		/// Removes a child view.
		/// </summary>
		/// <param name="child">The child view to remove.</param>
		/// <returns><see langword="true"/> if the view was removed successfully, otherwise <see langword="false"/>.</returns>
		public bool Remove(IView child)
		{
			if (child == null)
			{
				return false;
			}

			var index = _children.IndexOf(child);

			if (index == -1)
			{
				return false;
			}

			RemoveAt(index);

			return true;
		}

		/// <summary>
		/// Removes a child view at the specified index.
		/// </summary>
		/// <param name="index">The index at which to remove the child view.</param>
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
				RemoveLogicalChild(element);
			}

			OnRemove(index, child);
		}

		/// <summary>
		/// Invoked when <see cref="Add(IView)"/> is called and notifies the handler associated to this layout.
		/// </summary>
		/// <param name="index">The index at which the child view was inserted.</param>
		/// <param name="view">The child view which was inserted.</param>
		protected virtual void OnAdd(int index, IView view)
		{
			NotifyHandler(nameof(ILayoutHandler.Add), index, view);
		}

		/// <summary>
		/// Invoked when <see cref="Clear"/> is called and notifies the handler associated to this layout.
		/// </summary>
		protected virtual void OnClear()
		{
			Handler?.Invoke(nameof(ILayoutHandler.Clear));
		}

		/// <summary>
		/// Invoked when <see cref="Insert(int, IView)"/> is called and notifies the handler associated to this layout.
		/// </summary>
		/// <param name="index">The index at which the child view was inserted.</param>
		/// <param name="view">The child view which was inserted.</param>
		protected virtual void OnRemove(int index, IView view)
		{
			NotifyHandler(nameof(ILayoutHandler.Remove), index, view);
		}

		/// <summary>
		/// Invoked when <see cref="RemoveAt(int)"/> is called and notifies the handler associated to this layout.
		/// </summary>
		/// <param name="index">The index at which the child view was removed.</param>
		/// <param name="view">The child view which was removed.</param>
		protected virtual void OnInsert(int index, IView view)
		{
			NotifyHandler(nameof(ILayoutHandler.Insert), index, view);
		}

		/// <summary>
		/// Invoked when <see cref="this[int]"/> is called and notifies the handler associated to this layout.
		/// </summary>
		/// <param name="index">The index at which the child view was updated.</param>
		/// <param name="view">The new child view which was added at <paramref name="index"/>.</param>
		/// <param name="oldView">The previous child view which was at <paramref name="index"/>.</param>
		protected virtual void OnUpdate(int index, IView view, IView oldView)
		{
			NotifyHandler(nameof(ILayoutHandler.Update), index, view);
		}

		void NotifyHandler(string action, int index, IView view)
		{
			Handler?.Invoke(action, new Maui.Handlers.LayoutHandlerUpdate(index, view));
		}

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			(this as IView).InvalidateMeasure();
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return new Thickness(0);
		}

		public Graphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return LayoutManager.Measure(widthConstraint, heightConstraint);
		}

		public Graphics.Size CrossPlatformArrange(Graphics.Rect bounds)
		{
			return LayoutManager.ArrangeChildren(bounds);
		}

		/// <summary>Bindable property for <see cref="CascadeInputTransparent"/>.</summary>
		public static readonly BindableProperty CascadeInputTransparentProperty =
			BindableProperty.Create(nameof(CascadeInputTransparent), typeof(bool), typeof(Layout), true,
				propertyChanged: OnCascadeInputTransparentPropertyChanged);

		/// <summary>
		/// Gets or sets a value that controls whether child elements
		/// inherit the input transparency of this layout when the tranparency is <see langword="true"/>.
		/// </summary>
		/// <value>
		/// <see langword="true" /> to cause child elements to inherit the input transparency of this layout,
		/// when this layout's <see cref="VisualElement.InputTransparent" /> property is <see langword="true" />.
		/// <see langword="false" /> to cause child elements to ignore the input tranparency of this layout.
		/// </value>
		public bool CascadeInputTransparent
		{
			get => (bool)GetValue(CascadeInputTransparentProperty);
			set => SetValue(CascadeInputTransparentProperty, value);
		}

		static void OnCascadeInputTransparentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			// We only need to update if the cascade changes anything, namely when InputTransparent=true.
			// When InputTransparent=false, then the cascade property has no effect.
			if (bindable is Layout layout && layout.InputTransparent)
			{
				layout.RefreshInputTransparentProperty();
			}
		}
	}
}
