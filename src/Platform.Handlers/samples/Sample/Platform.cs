using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Platform;
using Xamarin.Platform.Handlers;
using RegistrarHandlers = Xamarin.Platform.Registrar;

namespace Sample
{
	public class Platform
	{
		static bool HasInit;

		public static void Init()
		{
			if (HasInit)
				return;

			HasInit = true;

			//RegistrarHandlers.Handlers.Register<Layout, LayoutHandler>();

			RegistrarHandlers.Handlers.Register<Button, ButtonHandler>();
			RegistrarHandlers.Handlers.Register<Slider, SliderHandler>();
			RegistrarHandlers.Handlers.Register<Sample.VerticalStackLayout, LayoutHandler>();
			RegistrarHandlers.Handlers.Register<Sample.HorizontalStackLayout, LayoutHandler>();
			RegistrarHandlers.Handlers.Register<Xamarin.Forms.FlexLayout, LayoutHandler>();
			RegistrarHandlers.Handlers.Register<Xamarin.Forms.StackLayout, LayoutHandler>();
			//RegistrarHandlers.Handlers.Register<Entry, EntryHandler>();
			RegistrarHandlers.Handlers.Register<Label, LabelHandler>();
		}


		void RegisterLegacyRendererAgainstFormsControl()
		{
#if MONOANDROID

			// register renderer with old registrar so it can get shimmed
			// This will move to some extension method
			Xamarin.Forms.Internals.Registrar.Registered.Register(
				typeof(Xamarin.Forms.Button),
				typeof(Xamarin.Forms.Platform.Android.FastRenderers.ButtonRenderer));

			// This registers the shim against the handler registrar
			// So when the handler.registrar returns the RendererToHandlerShim
			// Which will then forward the request to the old registrar
			Registrar.Handlers.Register<Xamarin.Forms.Button, RendererToHandlerShim>();

#endif
		}
	}
}