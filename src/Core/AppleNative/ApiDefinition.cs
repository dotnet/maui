using System;
using CoreAnimation;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Behavior that automatically resizes a CALayer to match its superlayer's bounds
	/// </summary>
	[BaseType(typeof(NSObject), Name = "CALayerAutosizeToSuperLayerBehavior")]
	[Internal]
	interface CALayerAutosizeToSuperLayerBehavior
	{
		/// <summary>
		/// Attaches this behavior to the given layer.
		/// The layer must have a superlayer when this method is called.
		/// The layer's frame will be kept in sync with the superlayer's bounds.
		/// </summary>
		/// <param name="layer">The layer that needs to be resized to match the superlayer's bounds.</param>
		[Export("attachWithLayer:")]
		void Attach(CALayer layer);

		/// <summary>
		/// Detaches this behavior from the current layer and stops observing
		/// </summary>
		[Export("detach")]
		void Detach();
	}
}