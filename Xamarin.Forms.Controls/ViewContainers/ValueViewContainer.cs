using System;

namespace Xamarin.Forms.Controls
{
	internal class ValueViewContainer<T> : ViewContainer<T> where T : View
	{
		public ValueViewContainer(Enum formsMember, T view, string bindingPath, Func<object, object> converterAction) : base(formsMember, view)
		{

			var valueLabel = new Label { BindingContext = View };
			valueLabel.SetBinding(Label.TextProperty, bindingPath, converter: new GenericValueConverter(converterAction));

			ContainerLayout.Children.Add(valueLabel);
		}
	}
}