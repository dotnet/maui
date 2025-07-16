#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Base class for layouts that allow you to arrange and group UI controls in your application.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public abstract partial class Layout : View, Maui.ILayout, IList<IView>, IBindableLayout, IPaddingElement, IVisualTreeElement, ISafeAreaView, IInputTransparentContainerElement, ISafeAreaPage, ISafeAreaElement
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
		readonly private protected List<IView> _children = new();

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

		/// <summary>Bindable property for <see cref="SafeAreaIgnore"/>.</summary>
		public static readonly BindableProperty SafeAreaIgnoreProperty = SafeAreaElement.SafeAreaIgnoreProperty;

		/// <summary>
		/// Gets or sets the safe area edges to ignore for this layout.
		/// The default value is SafeAreaEdges.Default.
		/// </summary>
		/// <remarks>
		/// This property controls which edges of the layout should ignore safe area insets.
		/// Use SafeAreaRegions.Default to respect safe area, SafeAreaRegions.All to ignore all insets, 
		/// SafeAreaRegions.None to ensure content never displays behind blocking UI, or SafeAreaRegions.SoftInput for soft input aware behavior.
		/// </remarks>
		public SafeAreaEdges SafeAreaIgnore
		{
			get => (SafeAreaEdges)GetValue(SafeAreaElement.SafeAreaIgnoreProperty);
			set => SetValue(SafeAreaElement.SafeAreaIgnoreProperty, value);
		}

		/// <inheritdoc cref="ISafeAreaView.IgnoreSafeArea"/>
		/// <remarks>
		/// This property is deprecated. Use SafeAreaElement.IgnoreSafeArea attached property instead for per-edge safe area control.
		/// </remarks>
		[System.Obsolete("Use SafeAreaElement.IgnoreSafeArea attached property instead for per-edge safe area control.")]
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

		SafeAreaEdges ISafeAreaElement.SafeAreaIgnoreDefaultValueCreator()
		{
			return SafeAreaEdges.Default;
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
		public static readonly BindableProperty CascadeInputTransparentProperty = InputTransparentContainerElement.CascadeInputTransparentProperty;

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

		private protected override string GetDebuggerDisplay()
		{
			return $"{base.GetDebuggerDisplay()}, ChildCount = {Count}";
		}

		#region ISafeAreaPage

		/// <inheritdoc cref="ISafeAreaPage.SafeAreaInsets"/>
		Thickness ISafeAreaPage.SafeAreaInsets { set { } } // Default no-op implementation for layouts

		/// <inheritdoc cref="ISafeAreaPage.IgnoreSafeAreaForEdge"/>
		bool ISafeAreaPage.IgnoreSafeAreaForEdge(int edge)
		{
			// Use direct property first, then fall back to attached property and legacy behavior
			var regionForEdge = SafeAreaIgnore.GetEdge(edge);
			
			// Handle the SafeAreaRegions behavior
			if (regionForEdge.HasFlag(SafeAreaRegions.All))
			{
				return true; // Ignore all insets - content may be positioned anywhere
			}

			if (regionForEdge == SafeAreaRegions.None || regionForEdge == SafeAreaRegions.SoftInput)
			{
				// Content will never display behind anything that could block it
				// Or treat SoftInput as respecting safe area for now
				return false;
			}

			if (regionForEdge == SafeAreaRegions.Default)
			{
				// Check if attached property is set, if not fall back to legacy behavior
				if (SafeAreaElement.GetIgnore(this) != SafeAreaEdges.Default)
				{
					return SafeAreaElement.ShouldIgnoreSafeAreaForEdge(this, edge);
				}
				
				// Fall back to legacy ISafeAreaView behavior
				return ((ISafeAreaView)this).IgnoreSafeArea;
			}

			return false;
		}

		/// <inheritdoc cref="ISafeAreaPage.GetSafeAreaRegionsForEdge"/>
		SafeAreaRegions ISafeAreaPage.GetSafeAreaRegionsForEdge(int edge)
		{
			// Use direct property first, then fall back to attached property
			var regionForEdge = SafeAreaIgnore.GetEdge(edge);
			
			if (regionForEdge != SafeAreaRegions.Default)
			{
				return regionForEdge;
			}
			
			// Fall back to attached property if direct property is Default
			var fallbackRegion = SafeAreaElement.GetIgnoreForEdge(this, edge);
			
			// For Layout views, never return Default - return None instead
			if (fallbackRegion == SafeAreaRegions.Default)
			{
				return SafeAreaRegions.None;
			}
			
			return fallbackRegion;
		}

		#endregion
	}
}
