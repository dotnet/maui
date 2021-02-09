using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms
{
	public static class FormsHandlers
	{
		// This is used to register the handler version against the xplat code so that the handler version will be used
		// when running a full Xamarin.Forms application. This lets us test the handler code inside the Control Gallery
		// And other scenarios

		public static void InitHandlers()
		{
			//Xamarin.Platform.Registrar.Handlers.Register(typeof(Slider), typeof(Xamarin.Platform.Handlers.SliderHandler));
			//Xamarin.Platform.Registrar.Handlers.Register(typeof(Button), typeof(Xamarin.Platform.Handlers.ButtonHandler));
			//RegistrarHandlers.Handlers.Register<Xamarin.Forms.StackLayout, Xamarin.Platform.Handlers.LayoutHandler>();
		}
	}
}
