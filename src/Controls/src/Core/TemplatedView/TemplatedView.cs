#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TemplatedView.xml" path="Type[@FullName='Microsoft.Maui.Controls.TemplatedView']/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
	public partial class TemplatedView : Compatibility.Layout, IControlTemplated, IContentView, IClippedToBoundsElement
#pragma warning restore CS0618 // Type or member is obsolete
	{
		/// <summary>Bindable property for <see cref="ControlTemplate"/>.</summary>
		public static readonly BindableProperty ControlTemplateProperty = BindableProperty.Create(nameof(ControlTemplate), typeof(ControlTemplate), typeof(TemplatedView), null,
			propertyChanged: TemplateUtilities.OnControlTemplateChanged);

		/// <summary>Bindable property for <see cref="IsClippedToBounds"/>.</summary>
		public new static readonly BindableProperty IsClippedToBoundsProperty = ClippedToBoundsElement.IsClippedToBoundsProperty;

		/// <summary>Bindable property for <see cref="CascadeInputTransparent"/>.</summary>
		public new static readonly BindableProperty CascadeInputTransparentProperty = InputTransparentContainerElement.CascadeInputTransparentProperty;

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public new static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/TemplatedView.xml" path="//Member[@MemberName='ControlTemplate']/Docs/*" />
		public ControlTemplate ControlTemplate
		{
			get { return (ControlTemplate)GetValue(ControlTemplateProperty); }
			set { SetValue(ControlTemplateProperty, value); }
		}

		/// <inheritdoc cref="IClippedToBoundsElement.IsClippedToBounds"/>
		public new bool IsClippedToBounds
		{
			get => (bool)GetValue(ClippedToBoundsElement.IsClippedToBoundsProperty);
			set => SetValue(ClippedToBoundsElement.IsClippedToBoundsProperty, value);
		}

		/// <inheritdoc cref="IInputTransparentContainerElement.CascadeInputTransparent"/>
		public new bool CascadeInputTransparent
		{
			get => (bool)GetValue(InputTransparentContainerElement.CascadeInputTransparentProperty);
			set => SetValue(InputTransparentContainerElement.CascadeInputTransparentProperty, value);
		}

		/// <inheritdoc cref="IPaddingElement.Padding"/>
		public new Thickness Padding
		{
			get => (Thickness)GetValue(PaddingProperty);
			set => SetValue(PaddingProperty, value);
		}

		IList<Element> IControlTemplated.InternalChildren => InternalChildren;

		Element IControlTemplated.TemplateRoot { get; set; }

		[Obsolete("Use InvalidateArrange if you need to trigger a new arrange and then put your arrange logic inside ArrangeOverride instead")]
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			for (var i = 0; i < LogicalChildrenInternal.Count; i++)
			{
				Element element = LogicalChildrenInternal[i];
				var child = element as View;

				// For now we just leave the old path in place to avoid too much change in behavior
				// All of our types that inherit from TemplatedView overrides LayoutChildren and replaces
				// this behavior
				if (child != null)
					LayoutChildIntoBoundingRegion(child, new Rect(x, y, width, height));
			}
		}

		[Obsolete("Use MeasureOverride instead")]
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			double widthRequest = WidthRequest;
			double heightRequest = HeightRequest;
			var childRequest = new SizeRequest();

			if ((widthRequest == -1 || heightRequest == -1) && InternalChildren.Count > 0 && InternalChildren[0] is View view)
			{
				childRequest = view.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
			}

			return new SizeRequest
			{
				Request = new Size { Width = widthRequest != -1 ? widthRequest : childRequest.Request.Width, Height = heightRequest != -1 ? heightRequest : childRequest.Request.Height },
				Minimum = childRequest.Minimum
			};
		}

		protected override void ComputeConstraintForView(View view)
		{
			bool isFixedHorizontally = (Constraint & LayoutConstraint.HorizontallyFixed) != 0;
			bool isFixedVertically = (Constraint & LayoutConstraint.VerticallyFixed) != 0;

			var result = LayoutConstraint.None;
			if (isFixedVertically && view.VerticalOptions.Alignment == LayoutAlignment.Fill)
				result |= LayoutConstraint.VerticallyFixed;
			if (isFixedHorizontally && view.HorizontalOptions.Alignment == LayoutAlignment.Fill)
				result |= LayoutConstraint.HorizontallyFixed;
			view.ComputedConstraint = result;
		}

		internal override void SetChildInheritedBindingContext(Element child, object context)
		{
			if (ControlTemplate == null)
				base.SetChildInheritedBindingContext(child, context);
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

		/// <include file="../../docs/Microsoft.Maui.Controls/TemplatedView.xml" path="//Member[@MemberName='ResolveControlTemplate']/Docs/*" />
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

		/// <summary>
		/// Sends a child to the back of the visual stack.
		/// </summary>
		/// <param name="view">The view to lower in the visual stack.</param>
		/// <remarks>Children are internally stored in visual stack order.
		/// This means that raising or lowering a child also changes the order in which the children are enumerated.</remarks>
		[Obsolete("Use the ZIndex Property instead. This property no longer works on .NET 10 and later.")]
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
		[Obsolete("Use the ZIndex Property instead. This property no longer works on .NET 10 and later.")]
		public new void RaiseChild(View view)
		{
			base.RaiseChild(view);
		}

		/// <summary>
		/// Invalidates the current layout.
		/// </summary>
		/// <remarks>Calling this method will invalidate the measure and triggers a new layout cycle.</remarks>
		[Obsolete("Use InvalidateMeasure depending on your scenario. This method will no longer work on .NET 10 and later.")]
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
		/// When implemented, should return <see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" /> when added,
		/// and should return <see langword="false" /> if it should not call <see cref="VisualElement.InvalidateMeasure" />. The default value is <see langword="true" />.
		/// </summary>
		/// <param name="child">The child for which to specify whether or not to track invalidation.</param>
		/// <returns><see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" />, otherwise <see langword="false"/>.</returns>
		[Obsolete("If you want to influence invalidation override InvalidateMeasureOverride. This method will no longer work on .NET 10 and later.")]
		protected override bool ShouldInvalidateOnChildAdded(View child) => true;

		/// <summary>
		/// When implemented, should return <see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" /> when removed,
		/// and should return <see langword="false" /> if it should not call <see cref="VisualElement.InvalidateMeasure" />. The default value is <see langword="true" />.
		/// </summary>
		/// <param name="child">The child for which to specify whether or not to track invalidation.</param>
		/// <returns><see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" />, otherwise <see langword="false"/>.</returns>
		[Obsolete("If you want to influence invalidation override InvalidateMeasureOverride. This method will no longer work on .NET 10 and later.")]
		protected override bool ShouldInvalidateOnChildRemoved(View child) => true;

		/// <summary>
		/// Instructs the layout to relayout all of its children.
		/// </summary>
		/// <remarks>This method starts a new layout cycle for the layout. Invoking this method frequently can negatively impact performance.</remarks>
		[Obsolete("Use InvalidateMeasure depending on your scenario. This method will no longer work on .NET 10 and later.")]
		protected new void UpdateChildrenLayout()
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use IVisualTreeElement.GetVisualChildren() instead.", true)]
		public new IReadOnlyList<Element> Children => base.Children;

#nullable disable

	}
}