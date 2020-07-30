using Android.Util;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;

[assembly: ExportEffect(typeof(Xamarin.Forms.ControlGallery.Android.RippleEffect), nameof(Xamarin.Forms.ControlGallery.Android.RippleEffect))]
namespace Xamarin.Forms.ControlGallery.Android
{
    [Preserve(AllMembers = true)]
    public class RippleEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                if (Container is AView view)
                {
                    view.Clickable = true;
                    view.Focusable = true;

                    using (var outValue = new TypedValue())
                    {
                        view.Context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, outValue, true);
                        view.SetBackgroundResource(outValue.ResourceId);
                    }
                }
            }
            catch
            {
              
            }
        }

        protected override void OnDetached()
        {

        }
    }
}
