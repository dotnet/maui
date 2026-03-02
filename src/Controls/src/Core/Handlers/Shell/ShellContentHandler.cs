#nullable disable
#if ANDROID
using System;
using Microsoft.Maui.Handlers;

#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Handler for ShellContent - lightweight wrapper for page content.
    /// Phase 4 of Shell Handler Migration.
    /// </summary>
    public partial class ShellContentHandler
    {

        public static PropertyMapper<ShellContent, ShellContentHandler> Mapper =
            new PropertyMapper<ShellContent, ShellContentHandler>(ElementMapper)
            {
                [nameof(ShellContent.Title)] = MapTitle
            };

        public static CommandMapper<ShellContent, ShellContentHandler> CommandMapper =
            new CommandMapper<ShellContent, ShellContentHandler>(ElementCommandMapper);

        public ShellContentHandler() : base(Mapper, CommandMapper)
        {
        }

        /// <summary>
        /// Maps the Title property to notify parent handlers to update.
        /// </summary>
        internal static void MapTitle(ShellContentHandler handler, ShellContent item)
        {
            // Notify parent ShellSection/ShellItem handlers that title changed
            var shellSection = item?.Parent as ShellSection;
            var shellItem = shellSection?.Parent as ShellItem;
            var shellItemHandler = shellItem?.Handler as ShellItemHandler;
            // shellItemHandler?.UpdateTitle();
        }
    }
}
#endif
