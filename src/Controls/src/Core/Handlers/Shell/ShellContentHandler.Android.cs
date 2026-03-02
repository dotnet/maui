#nullable disable
#if ANDROID
using System;
using Android.Views;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Android implementation of ShellContentHandler.
    /// 
    /// This handler simply returns the content page's platform view.
    /// It provides a clean separation between Shell structure and page content.
    /// </summary>
    public partial class ShellContentHandler : ElementHandler<ShellContent, AView>
    {
        /// <summary>
        /// Creates the platform view for the ShellContent.
        /// Returns the content page's Android view.
        /// </summary>
        protected override AView CreatePlatformElement()
        {
            // Get the page from ShellContent and convert it to platform view
            var page = ((IShellContentController)VirtualView).GetOrCreateContent();

            if (page is null)
            {
                // Return empty view if no content
                return new AView(MauiContext?.Context);
            }

            // Convert the page to its Android platform view
            return page.ToPlatform(MauiContext);
        }

        /// <summary>
        /// Connects the handler to the platform view.
        /// Minimal setup needed since this is just a wrapper.
        /// </summary>
        protected override void ConnectHandler(AView platformView)
        {
            base.ConnectHandler(platformView);
        }

        /// <summary>
        /// Disconnects the handler from the platform view.
        /// Cleanup any resources.
        /// </summary>
        protected override void DisconnectHandler(AView platformView)
        {

            base.DisconnectHandler(platformView);
        }
    }
}
#endif
