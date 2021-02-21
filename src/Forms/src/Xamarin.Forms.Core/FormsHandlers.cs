using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Platform.Handlers;

namespace Xamarin.Forms
{
	public static class FormsHandlers
	{
		// This is used to register the handler version against the xplat code so that the handler version will be used
		// when running a full Xamarin.Forms application. This lets us test the handler code inside the Control Gallery
		// And other scenarios
		public static void InitHandlers()
		{
			//Xamarin.Platform.Registrar.Handlers.Register<Label, LabelHandler>();
			Xamarin.Platform.Registrar.Handlers.Register<VerticalStackLayout, LayoutHandler>();
			Xamarin.Platform.Registrar.Handlers.Register<HorizontalStackLayout, LayoutHandler>();
		}
	}
}