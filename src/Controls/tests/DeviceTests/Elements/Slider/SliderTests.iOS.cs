using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Slider = Microsoft.Maui.Controls.Slider;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Microsoft.Maui.DeviceTests
{
    public partial class SliderTests
    {
        [Fact("Slider Does Not Leak")]
        public async Task DoesNotLeak()
        {
            SetupBuilder();
            WeakReference platformViewReference = null;
            WeakReference handlerReference = null;

            await InvokeOnMainThreadAsync(() =>
            {
                var layout = new VerticalStackLayout();
                var slider = new Slider();
                //slider.On<iOS>().SetUpdateOnTap(true);
                layout.Add(slider);

                var handler = CreateHandler<LayoutHandler>(layout);
                handlerReference = new WeakReference(slider.Handler);
                platformViewReference = new WeakReference(slider.Handler.PlatformView);
            });

            await AssertionExtensions.WaitForGC(handlerReference, platformViewReference);
        }
    }
}