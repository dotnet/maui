using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui.ExternalBackend
{
	/// <summary>
	/// iOS and Mac Catalyst variant of the external platform's native view type.
	/// </summary>
	/// <remarks>
	/// On a platform target framework, <see cref="Handlers.ViewHandler{TVirtualView, TPlatformView}"/>
	/// constrains <c>TPlatformView</c> to the platform's own base view type — here
	/// <c>UIKit.UIView</c>. A real external backend's views satisfy that too. What makes it an
	/// <em>external</em> backend is that its concrete types are its own, and therefore are not the types
	/// the aliased handler interfaces are pinned to (<c>MauiLabel</c> for <c>ILabelHandler</c> on iOS).
	/// That mismatch is what produces <c>CS0738</c>.
	/// </remarks>
	public class FakeNativeView : UIView
	{
		public List<FakeNativeView> FakeChildren { get; } = new List<FakeNativeView>();

		public double FakeOpacity { get; set; } = 1;
	}

	/// <summary>Native label type for the fake external platform.</summary>
	public class FakeNativeLabel : FakeNativeView
	{
		public string? FakeText { get; set; }
	}

	/// <summary>Native content-host type for the fake external platform.</summary>
	public class FakeNativeContentView : FakeNativeView
	{
		public FakeNativeView? FakeContent { get; set; }
	}

	/// <summary>Native layout container type for the fake external platform.</summary>
	public class FakeNativeLayoutView : FakeNativeView
	{
	}
}
