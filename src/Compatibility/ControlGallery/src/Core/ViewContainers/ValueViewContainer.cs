//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;

namespace Microsoft.Maui.Controls.ControlGallery
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