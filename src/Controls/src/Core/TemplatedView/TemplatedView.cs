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
	public partial class TemplatedView : View, ILayout, ILayoutController, IPaddingElement, IView, IVisualTreeElement, IInputTransparentContainerElement, IControlTemplated, IContentView
	{
		/// <summary>The children contained in this layout.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IReadOnlyList<Element> Children => LogicalChildrenInternal;

		/// <summary>Bindable property for <see cref="ControlTemplate"/>.</summary>
		public static readonly BindableProperty ControlTemplateProperty = BindableProperty.Create(nameof(ControlTemplate), typeof(ControlTemplate), typeof(TemplatedView), null,
			propertyChanged: TemplateUtilities.OnControlTemplateChanged);

		/// <summary>
		/// Gets or sets the control template that is used to display content.
		/// This is a bindable property.
		/// </summary>
		public ControlTemplate ControlTemplate
		{
			get { return (ControlTemplate)GetValue(ControlTemplateProperty); }
			set { SetValue(ControlTemplateProperty, value); }
		}

		/// <inheritdoc cref="IInputTransparentContainerElement.CascadeInputTransparent"/>
		public bool CascadeInputTransparent
		{
			get => (bool)GetValue(InputTransparentContainerElement.CascadeInputTransparentProperty);
			set => SetValue(InputTransparentContainerElement.CascadeInputTransparentProperty, value);
		}

#pragma warning disable CS0067
		[Obsolete("Use SizeChanged.")]
		public event EventHandler LayoutChanged;
#pragma warning restore CS0067

		/// <inheritdoc cref="IPaddingElement.Padding"/>
		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingElement.PaddingProperty);
			set => SetValue(PaddingElement.PaddingProperty, value);
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator() => default;

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue) => InvalidateMeasure();

		IReadOnlyList<Element> ILayoutController.Children => LogicalChildrenInternal;

		IList<Element> IControlTemplated.InternalChildren => LogicalChildrenInternalBackingStore;

		Element IControlTemplated.TemplateRoot { get; set; }

		internal override void ComputeConstraintForView(View view)
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

			view.ComputedConstraint = result;
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

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			return this.ComputeDesiredSize(widthConstraint, heightConstraint);
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return this.MeasureContent(widthConstraint, heightConstraint);
		}

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
	}
}