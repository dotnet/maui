using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;

namespace MacIdiom;

internal static class MacAppBuilderExtensions
{
    public static MauiAppBuilder ApplyPlatformStylingAdjustmentsForCurrentIdiom(this MauiAppBuilder builder)
    {
#if MACCATALYST
    // builder.ApplyPadAsPreferredBehavioralStyle();
    // UIButton
    ButtonHandler.Mapper.AppendToMapping("PadButtonPreferredBehavioralStyle", (handler, view) =>
    {
        handler.PlatformView.PreferredBehavioralStyle = UIKit.UIBehavioralStyle.Pad;
        handler.PlatformView.Layer.CornerRadius = 5;
        handler.PlatformView.ClipsToBounds = true;
    });
#endif
        return builder;
    }
}