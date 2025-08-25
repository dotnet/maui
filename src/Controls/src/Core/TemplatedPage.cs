#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Controls
{

	/// <include file="../../docs/Microsoft.Maui.Controls/TemplatedPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.TemplatedPage']/Docs/*" />
	public class TemplatedPage : Page, IControlTemplated
	{
		/// <summary>Bindable property for <see cref="ControlTemplate"/>.</summary>
		public static readonly BindableProperty ControlTemplateProperty = BindableProperty.Create(nameof(ControlTemplate), typeof(ControlTemplate), typeof(TemplatedPage), null,
			propertyChanged: TemplateUtilities.OnControlTemplateChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/TemplatedPage.xml" path="//Member[@MemberName='ControlTemplate']/Docs/*" />
		public ControlTemplate ControlTemplate
		{
			get { return (ControlTemplate)GetValue(ControlTemplateProperty); }
			set { SetValue(ControlTemplateProperty, value); }
		}

		IReadOnlyList<Element> IControlTemplated.InternalChildren => InternalChildren;

		Element IControlTemplated.TemplateRoot { get; set; }

		protected override SizeConstraint ComputeConstraintForView(View view)
		{
			LayoutOptions vOptions = view.VerticalOptions;
			LayoutOptions hOptions = view.HorizontalOptions;

			var result = SizeConstraint.None;
			if (vOptions.Alignment == LayoutAlignment.Fill)
				result |= SizeConstraint.VerticallyFixed;
			if (hOptions.Alignment == LayoutAlignment.Fill)
				result |= SizeConstraint.HorizontallyFixed;

			return result;
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
		void OnApplyTemplateImpl()
		{
			Handler?.UpdateValue(nameof(IContentView.Content));
		}

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			base.OnChildRemoved(child, oldLogicalIndex);
			TemplateUtilities.OnChildRemoved(this, child);
		}

		protected object GetTemplateChild(string name) => TemplateUtilities.GetTemplateChild(this, name);

		bool IControlTemplated.RemoveAt(int index)
		{
			var ct = (IControlTemplated)this;
			var view = ct.InternalChildren[index];
			if (InternalChildren.Contains(view))
			{
				InternalChildren.Remove(view);
				return true;
			}

			return RemoveLogicalChild(ct.InternalChildren[index], index);
		}

		void IControlTemplated.AddLogicalChild(Element element)
		{
			if (!InternalChildren.Contains(element))
			{
				InternalChildren.Add(element);
			}
		}
	}
}