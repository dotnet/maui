#nullable enable
using System;

namespace Microsoft.Maui.Controls
{
    /// <summary>
    /// Defines a visual element that can be rendered and interact with platform-specific containers.
    /// </summary>
    /// <remarks>
    /// This interface extends both <see cref="IControlsElement"/> for logical tree participation and 
    /// <see cref="IView"/> for visual rendering capabilities. It provides window and platform container 
    /// lifecycle management for visual elements that need to respond to changes in their hosting environment.
    /// </remarks>
    public interface IControlsVisualElement : IControlsElement, IView
    {
        /// <summary>
        /// Occurs when the <see cref="Window"/> property value changes.
        /// </summary>
        /// <remarks>
        /// This event is raised when the visual element is added to or removed from a window,
        /// or when the element is moved between windows. Subscribers can use this event to perform
        /// initialization or cleanup tasks related to window-specific resources.
        /// </remarks>
        event EventHandler? WindowChanged;

        /// <summary>
        /// Gets the window that contains this visual element, or <see langword="null"/> if the element 
        /// is not currently part of a window's visual tree.
        /// </summary>
        /// <value>
        /// The <see cref="Window"/> instance that hosts this element, or <see langword="null"/> 
        /// if the element is detached from any window.
        /// </value>
        /// <remarks>
        /// This property traverses up the visual tree to find the containing window. Elements that are not
        /// yet added to a page or have been removed from the visual tree will return <see langword="null"/>.
        /// </remarks>
        Window? Window { get; }

        /// <summary>
        /// Occurs when the platform-specific container view that hosts this element changes.
        /// </summary>
        /// <remarks>
        /// This event is raised when the native platform view (e.g., UIView on iOS, Android.Views.View on Android,
        /// FrameworkElement on Windows) that contains this element is created, replaced, or destroyed.
        /// This is particularly useful for scenarios requiring direct interaction with native views or 
        /// for responding to platform-specific lifecycle events.
        /// </remarks>
        event EventHandler? PlatformContainerViewChanged;
    }
}