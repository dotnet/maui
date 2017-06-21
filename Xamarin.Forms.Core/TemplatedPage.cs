using System.Collections.Generic;

namespace Xamarin.Forms
{
	public class TemplatedPage : Page, IControlTemplated
	{
		public static readonly BindableProperty ControlTemplateProperty = BindableProperty.Create(nameof(ControlTemplate), typeof(ControlTemplate), typeof(TemplatedPage), null,
			propertyChanged: TemplateUtilities.OnControlTemplateChanged);

		public ControlTemplate ControlTemplate
		{
			get { return (ControlTemplate)GetValue(ControlTemplateProperty); }
			set { SetValue(ControlTemplateProperty, value); }
		}

		IList<Element> IControlTemplated.InternalChildren => InternalChildren;

		internal override void ComputeConstraintForView(View view)
		{
			LayoutOptions vOptions = view.VerticalOptions;
			LayoutOptions hOptions = view.HorizontalOptions;

			var result = LayoutConstraint.None;
			if (vOptions.Alignment == LayoutAlignment.Fill)
				result |= LayoutConstraint.VerticallyFixed;
			if (hOptions.Alignment == LayoutAlignment.Fill)
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
	}
}