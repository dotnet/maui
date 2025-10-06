#nullable enable
using System;

namespace Microsoft.Maui.Controls
{
    /// <summary>
    /// Defines the core interface for elements that participate in handler-based rendering.
    /// </summary>
    /// <remarks>
    /// This interface extends <see cref="Maui.IElement"/> to add handler lifecycle management specific to the Controls layer.
    /// Handlers are the bridge between cross-platform Controls elements and platform-specific native views. 
    /// Implementations of this interface can respond to handler attachment, detachment, and replacement events,
    /// enabling cleanup of platform-specific resources and re-initialization when handlers change.
    /// </remarks>
    public interface IControlsElement : Maui.IElement
    {
        /// <summary>
        /// Occurs when the handler for this element is about to change, before the new handler is set.
        /// </summary>
        /// <remarks>
        /// This event fires before <see cref="HandlerChanged"/> and provides access to both the old and new handler
        /// through <see cref="HandlerChangingEventArgs"/>. This is the appropriate place to perform cleanup
        /// operations on the old handler or platform view before they are replaced. The event allows subscribers
        /// to react to handler changes before they take effect, enabling proper resource disposal and state transfer.
        /// </remarks>
        event EventHandler<HandlerChangingEventArgs>? HandlerChanging;

        /// <summary>
        /// Occurs after the handler for this element has been changed and the new handler is fully attached.
        /// </summary>
        /// <remarks>
        /// This event fires after <see cref="HandlerChanging"/> when the handler transition is complete.
        /// Subscribers can use this event to initialize resources, set up event handlers, or configure
        /// the new platform-specific view. This is commonly used for accessing native platform views
        /// after they have been created and attached to the element.
        /// Common scenarios include:
        /// <list type="bullet">
        /// <item>Accessing the native platform view for customization</item>
        /// <item>Setting up platform-specific event subscriptions</item>
        /// <item>Initializing platform-dependent resources</item>
        /// <item>Responding to handler recreation during hot reload scenarios</item>
        /// </list>
        /// </remarks>
        event EventHandler? HandlerChanged;
    }
}