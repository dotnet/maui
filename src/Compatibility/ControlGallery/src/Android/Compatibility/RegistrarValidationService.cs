using AndroidX.Fragment.App;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.ControlGallery;
using Microsoft.Maui.Controls.ControlGallery.Android;
using Microsoft.Maui.Platform;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	public class RegistrarValidationService : IRegistrarValidationService
	{
		readonly global::Android.Content.Context _context;

		public RegistrarValidationService()
		{
			_context = MainApplication.ActivityContext;
		}

		public bool Validate(VisualElement element, out string message)
		{
			message = "Success";

			if (element == null)
				return true;

#pragma warning disable CS0612 // Type or member is obsolete
			object renderer = Platform.Android.Platform.CreateRendererWithContext(element, _context);
#pragma warning restore CS0612 // Type or member is obsolete

			if (renderer == null
				|| renderer.GetType().Name == "DefaultRenderer"
				)
			{
				var activity =
					DependencyService.Resolve<global::Android.Content.Context>() as MauiAppCompatActivity;
				var mc = activity.GetWindow().Handler.MauiContext;

				renderer = (element as IElement).ToHandler(mc);
				if (renderer is IElementHandler vh)
					vh.DisconnectHandler();
			}

			if (renderer == null
				|| renderer.GetType().Name == "DefaultRenderer"
				)
			{
				message = $"Failed to load Android renderer for {element.GetType().Name}";
				return false;
			}

			return true;
		}
	}
}