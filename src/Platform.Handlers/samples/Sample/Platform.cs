using Xamarin.Forms;
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

			RegistrarHandlers.Handlers.Register<Button, ButtonHandler>();
			RegistrarHandlers.Handlers.Register<Label, LabelHandler>();
			RegistrarHandlers.Handlers.Register<Slider, SliderHandler>();
			RegistrarHandlers.Handlers.Register<VerticalStackLayout, LayoutHandler>();
			RegistrarHandlers.Handlers.Register<HorizontalStackLayout, LayoutHandler>();
			RegistrarHandlers.Handlers.Register<FlexLayout, LayoutHandler>();
			RegistrarHandlers.Handlers.Register<Xamarin.Forms.StackLayout, LayoutHandler>();
			RegistrarHandlers.Handlers.Register<Switch, SwitchHandler>();
		}

		void RegisterLegacyRendererAgainstFormsControl()
		{
#if MONOANDROID && !NET6_0
			// register renderer with old registrar so it can get shimmed
			// This will move to some extension method
			Xamarin.Forms.Internals.Registrar.Registered.Register(
				typeof(Xamarin.Forms.Button),
				typeof(Xamarin.Forms.Platform.Android.FastRenderers.ButtonRenderer));

			// This registers the shim against the handler registrar
			// So when the handler.registrar returns the RendererToHandlerShim
			// Which will then forward the request to the old registrar
			RegistrarHandlers.Handlers.Register<Xamarin.Forms.Button, RendererToHandlerShim>();
#endif
		}
	}
}
