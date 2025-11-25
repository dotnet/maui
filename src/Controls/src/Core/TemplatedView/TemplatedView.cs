#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A view that displays content with a control template, and the base class for <see cref="ContentView" />.
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	public partial class TemplatedView : Compatibility.Layout, ILayout, ILayoutController, IPaddingElement, IView, IVisualTreeElement, IInputTransparentContainerElement, IControlTemplated, IContentView, IClippedToBoundsElement
	{
#pragma warning restore CS0618
		/// <summary>Bindable property for <see cref="ControlTemplate"/>.</summary>
		public static readonly BindableProperty ControlTemplateProperty = BindableProperty.Create(nameof(ControlTemplate), typeof(ControlTemplate), typeof(TemplatedView), null,
			propertyChanged: TemplateUtilities.OnControlTemplateChanged);

		/// <summary>Bindable property for <see cref="IsClippedToBounds"/>.</summary>
		public new static readonly BindableProperty IsClippedToBoundsProperty =
			ClippedToBoundsElement.IsClippedToBoundsProperty;

		/// <summary>
		/// Gets or sets a value which determines if the layout should clip its children to its bounds.
		/// The default value is <see langword="false"/>.
		/// </summary>
		public new bool IsClippedToBounds
		{
			get => (bool)GetValue(IsClippedToBoundsProperty);
			set => SetValue(IsClippedToBoundsProperty, value);
		}

		/// <summary>
		/// Gets or sets the control template that is used to display content.
		/// This is a bindable property.
		/// </summary>
		public ControlTemplate ControlTemplate
		{
			get { return (ControlTemplate)GetValue(ControlTemplateProperty); }
			set { SetValue(ControlTemplateProperty, value); }
		}

		/// <summary>Bindable property for <see cref="CascadeInputTransparent"/>.</summary>
		public new static readonly BindableProperty CascadeInputTransparentProperty = InputTransparentContainerElement.CascadeInputTransparentProperty;

		/// <summary>
		/// Gets or sets a value that controls whether child elements
		/// inherit the input transparency of this layout when the transparency is <see langword="true"/>.
		/// </summary>
		/// <value>
		/// <see langword="true" /> to cause child elements to inherit the input transparency of this layout,
		/// when this layout's <see cref="VisualElement.InputTransparent" /> property is <see langword="true" />.
		/// <see langword="false" /> to cause child elements to ignore the input transparency of this layout.
		/// </value>
		public new bool CascadeInputTransparent
		{
			get => (bool)GetValue(CascadeInputTransparentProperty);
			set => SetValue(CascadeInputTransparentProperty, value);
		}

		/// <summary>
		/// The backing store for the <see cref="Padding" /> bindable property.
		/// </summary>
		public new static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <inheritdoc cref="IPaddingElement.Padding"/>
		public new Thickness Padding
		{
			get => (Thickness)GetValue(PaddingProperty);
			set => SetValue(PaddingProperty, value);
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator() => default;

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue) => InvalidateMeasure();

		IReadOnlyList<Element> ILayoutController.Children => LogicalChildrenInternal;

		IReadOnlyList<Element> IControlTemplated.InternalChildren => LogicalChildrenInternal;

		bool IControlTemplated.RemoveAt(int index)
		{
			return RemoveLogicalChild(LogicalChildrenInternalBackingStore[index], index);
		}

		Element IControlTemplated.TemplateRoot { get; set; }

		protected override LayoutConstraint ComputeConstraintForView(View view)
		{
			bool isFixedHorizontally = (Constraint & LayoutConstraint.HorizontallyFixed) != 0;
			bool isFixedVertically = (Constraint & LayoutConstraint.VerticallyFixed) != 0;

			var result = LayoutConstraint.None;
			if (isFixedVertically && view.VerticalOptions.Alignment == LayoutAlignment.Fill)
			{
				result |= LayoutConstraint.VerticallyFixed;
			}

			if (isFixedHorizontally && view.HorizontalOptions.Alignment == LayoutAlignment.Fill)
			{
				result |= LayoutConstraint.HorizontallyFixed;
			}

			return result;
		}

		internal override void SetChildInheritedBindingContext(Element child, object context)
		{
			if (ControlTemplate is null)
			{
				base.SetChildInheritedBindingContext(child, context);
			}
		}

		void IControlTemplated.OnControlTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue)
		{
			OnControlTemplateChanged(oldValue, newValue);
		}

		internal virtual void OnControlTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue)
		{
		}

		void IControlTemplated.OnApplyTemplate()
		{
			OnApplyTemplate();
		}

		protected virtual void OnApplyTemplate()
		{
			OnApplyTemplateImpl();
		}

		partial void OnApplyTemplateImpl();

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			base.OnChildRemoved(child, oldLogicalIndex);
			TemplateUtilities.OnChildRemoved(this, child);
		}

		protected object GetTemplateChild(string name) => TemplateUtilities.GetTemplateChild(this, name);

		/// <summary>
		/// Resolves and returns the <see cref="ControlTemplate"/> associated with this instance.
		/// </summary>
		/// <returns>The <see cref="ControlTemplate"/> currently assigned to this instance. If no template is assigned, this method returns <see langword="null"/>.</returns>
		public virtual ControlTemplate ResolveControlTemplate()
		{
			return ControlTemplate;
		}

#nullable enable
		object? IContentView.Content => null;

		IView? IContentView.PresentedContent =>
			(this as IControlTemplated).TemplateRoot as IView;

		partial void OnApplyTemplateImpl()
		{
			Handler?.UpdateValue(nameof(IContentView.Content));
		}

		// Don't delete this override. At some point in the future we'd like to delete Compatibility.Layout
		// and this is the only way to ensure binary compatibility with code that's already compiled against MAUI
		// and is overriding MeasureOverride.
		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			return this.ComputeDesiredSize(widthConstraint, heightConstraint);
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return this.MeasureContent(widthConstraint, heightConstraint);
		}

		// Don't delete this override. At some point in the future we'd like to delete Compatibility.Layout
		// and this is the only way to ensure binary compatibility with code that's already compiled against MAUI
		// and is overriding OnSizeAllocated.
		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
		}


		// Don't delete this override. At some point in the future we'd like to delete Compatibility.Layout
		// and this is the only way to ensure binary compatibility with code that's already compiled against MAUI
		// and is overriding ArrangeOverride.
		protected override Size ArrangeOverride(Rect bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.PlatformArrange(Frame);
			return Frame.Size;
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			this.ArrangeContent(bounds);
			return bounds.Size;
		}

		Size IContentView.CrossPlatformMeasure(double widthConstraint, double heightConstraint) => ((ICrossPlatformLayout)this).CrossPlatformMeasure(widthConstraint, heightConstraint);

		Size IContentView.CrossPlatformArrange(Rect bounds) =>
			((ICrossPlatformLayout)this).CrossPlatformArrange(bounds);



		/// <summary>
		/// Sends a child to the back of the visual stack.
		/// </summary>
		/// <param name="view">The view to lower in the visual stack.</param>
		/// <remarks>Children are internally stored in visual stack order.
		/// This means that raising or lowering a child also changes the order in which the children are enumerated.</remarks>
		[Obsolete("Use the ZIndex Property instead")]
		public new void LowerChild(View view)
		{
			base.LowerChild(view);
		}

		/// <summary>
		/// Sends a child to the front of the visual stack.
		/// </summary>
		/// <param name="view">The view to raise in the visual stack.</param>
		/// <remarks>Children are internally stored in visual stack order.
		/// This means that raising or lowering a child also changes the order in which the children are enumerated.</remarks>
		[Obsolete("Use the ZIndex Property instead")]
		public new void RaiseChild(View view)
		{
			base.RaiseChild(view);
		}

		/// <summary>
		/// Invalidates the current layout.
		/// </summary>
		/// <remarks>Calling this method will invalidate the measure and triggers a new layout cycle.</remarks>
		[Obsolete("Use InvalidateMeasure depending on your scenario")]
		protected override void InvalidateLayout()
		{
			base.InvalidateLayout();
		}

		/// <summary>
		/// Invoked whenever a child of the layout has emitted <see cref="VisualElement.MeasureInvalidated" />.
		/// Implement this method to add class handling for this event.
		/// </summary>
		[Obsolete("Subscribe to the MeasureInvalidated Event on the Children.")]
		protected override void OnChildMeasureInvalidated()
		{
		}

		/// <summary>
		/// If you want to influence invalidation override InvalidateMeasureOverride. This method will no longer work on .NET 10 and later.
		/// </summary>
		/// <param name="child">The child for which to specify whether or not to track invalidation.</param>
		/// <returns><see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" />, otherwise <see langword="false"/>.</returns>
		[Obsolete("If you want to influence invalidation override InvalidateMeasureOverride. This method will no longer work on .NET 10 and later.")]
		protected override bool ShouldInvalidateOnChildAdded(View child) => true;

		/// <summary>
		/// If you want to influence invalidation override InvalidateMeasureOverride. This method will no longer work on .NET 10 and later.
		/// </summary>
		/// <param name="child">The child for which to specify whether or not to track invalidation.</param>
		/// <returns><see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" />, otherwise <see langword="false"/>.</returns>
		[Obsolete("If you want to influence invalidation override InvalidateMeasureOverride. This method will no longer work on .NET 10 and later.")]
		protected override bool ShouldInvalidateOnChildRemoved(View child) => true;

		/// <summary>
		/// Use InvalidateMeasure depending on your scenario. This method will no longer work on .NET 10 and later.
		/// </summary>
		[Obsolete("Use InvalidateMeasure depending on your scenario. This method will no longer work on .NET 10 and later.")]
		protected new void UpdateChildrenLayout()
		{
		}

		[Obsolete("Use ArrangeOverride instead")]
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
		}

		[Obsolete("Use MeasureOverride instead")]
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return base.OnMeasure(widthConstraint, heightConstraint);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use IVisualTreeElement.GetVisualChildren() instead.", true)]
		public new IReadOnlyList<Element> Children => base.Children;
	}
}