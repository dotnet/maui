#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TemplatedView.xml" path="Type[@FullName='Microsoft.Maui.Controls.TemplatedView']/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
	public partial class TemplatedView : Compatibility.Layout, IControlTemplated, IContentView
#pragma warning restore CS0618 // Type or member is obsolete
	{
		/// <summary>Bindable property for <see cref="ControlTemplate"/>.</summary>
		public static readonly BindableProperty ControlTemplateProperty = BindableProperty.Create(nameof(ControlTemplate), typeof(ControlTemplate), typeof(TemplatedView), null,
			propertyChanged: TemplateUtilities.OnControlTemplateChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/TemplatedView.xml" path="//Member[@MemberName='ControlTemplate']/Docs/*" />
		public ControlTemplate ControlTemplate
		{
			get { return (ControlTemplate)GetValue(ControlTemplateProperty); }
			set { SetValue(ControlTemplateProperty, value); }
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

#nullable disable

	}
}