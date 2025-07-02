#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Layout manager for templated views.
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	public class ContentPresenter : Compatibility.Layout, ILayout, ILayoutController, IPaddingElement, IView, IVisualTreeElement, IInputTransparentContainerElement, IContentView
#pragma warning restore CS0618 // Type or member is obsolete
	{
		/// <summary>Bindable property for <see cref="Content"/>.</summary>
		public static BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View),
			typeof(ContentPresenter), null, propertyChanged: OnContentChanged);

		/// <summary>Bindable property for <see cref="CascadeInputTransparent"/>.</summary>
		public new static readonly BindableProperty CascadeInputTransparentProperty = InputTransparentContainerElement.CascadeInputTransparentProperty;

		/// <inheritdoc cref="IInputTransparentContainerElement.CascadeInputTransparent"/>
		public new bool CascadeInputTransparent
		{
			get => (bool)GetValue(InputTransparentContainerElement.CascadeInputTransparentProperty);
			set => SetValue(InputTransparentContainerElement.CascadeInputTransparentProperty, value);
		}

		/// <summary>
		/// Creates a new empty <see cref="ContentPresenter"/> with default values.
		/// </summary>
		public ContentPresenter()
		{
			this.SetBinding(
				ContentProperty,
				static (IContentView view) => view.Content,
				source: RelativeBindingSource.TemplatedParent,
				converter: new ContentConverter(),
				converterParameter: this);
		}

		/// <summary>
		/// Gets or sets the view whose layout is managed by this <see cref="ContentPresenter"/>.
		/// </summary>
		public View Content
		{
			get { return (View)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		object IContentView.Content => Content;

		IView IContentView.PresentedContent => Content;

		IReadOnlyList<Element> ILayoutController.Children => LogicalChildrenInternal;

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public new static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <inheritdoc cref="IPaddingElement.Padding"/>
		public new Thickness Padding
		{
			get => (Thickness)GetValue(PaddingProperty);
			set => SetValue(PaddingProperty, value);
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator() => default(Thickness);

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue) => InvalidateMeasure();

		[Obsolete("Use InvalidateArrange if you need to trigger a new arrange and then put your arrange logic inside ArrangeOverride instead")]
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			for (var i = 0; i < LogicalChildrenInternal.Count; i++)
			{
				Element element = LogicalChildrenInternal[i];
				var child = element as View;
				child?.Arrange(new Rect(x, y, width, height));
			}
		}

		[Obsolete("Use MeasureOverride instead")]
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			double widthRequest = WidthRequest;
			double heightRequest = HeightRequest;
			var childRequest = new SizeRequest();
			if ((widthRequest == -1 || heightRequest == -1) && Content != null)
			{
				childRequest = Content.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
			}

			return new SizeRequest
			{
				Request = new Size { Width = widthRequest != -1 ? widthRequest : childRequest.Request.Width, Height = heightRequest != -1 ? heightRequest : childRequest.Request.Height },
				Minimum = childRequest.Minimum
			};
		}

		internal virtual void Clear()
		{
			Content = null;
		}

		internal override void ComputeConstraintForView(View view)
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
			// We never want to use the standard inheritance mechanism, we will get this set by our parent
		}

		static async void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (ContentPresenter)bindable;

			var oldView = (View)oldValue;
			var newView = (View)newValue;
			if (oldView != null)
			{
				self.InternalChildren.Remove(oldView);
				oldView.ParentOverride = null;
			}

			if (newView != null)
			{
				self.InternalChildren.Add(newView);
				newView.ParentOverride = await TemplateUtilities.FindTemplatedParentAsync((Element)bindable);
			}
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

		[Obsolete("Use Measure with no flags.")]
		public override SizeRequest Measure(double widthConstraint, double heightConstraint, MeasureFlags flags = MeasureFlags.None)
		{
			return base.Measure(widthConstraint, heightConstraint);
		}

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
		
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use IVisualTreeElement.GetVisualChildren() instead.", true)]
		public new IReadOnlyList<Element> Children => base.Children;
	}
}