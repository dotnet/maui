using System.Collections.Generic;
using Tizen.NUI.BaseComponents;

namespace Microsoft.Maui.ExternalBackend
{
	/// <summary>
	/// Tizen variant of the external platform's native view type.
	/// </summary>
	/// <remarks>
	/// <para>
	/// On a platform target framework, <see cref="Handlers.ViewHandler{TVirtualView, TPlatformView}"/>
	/// constrains <c>TPlatformView</c> to the platform's own base view type — here
	/// <c>Tizen.NUI.BaseComponents.View</c>, which comes from the Tizen platform SDK itself.
	/// </para>
	/// <para>
	/// Deliberately does <em>not</em> derive from <c>Tizen.UIExtensions.NUI.ViewGroup</c>, which is what
	/// .NET MAUI's own in-box Tizen views derive from. An external backend takes a dependency on the
	/// platform SDK, not on .NET MAUI's platform helper packages — and that is exactly the situation
	/// these contracts have to support. It is also why the aliased interfaces are unimplementable here:
	/// on Tizen <c>ILayoutHandler.PlatformView</c> is pinned to
	/// <c>Microsoft.Maui.Platform.LayoutViewGroup</c> and <c>ILabelHandler.PlatformView</c> to
	/// <c>Tizen.UIExtensions.NUI.Label</c> — .NET MAUI's types, not the backend's. That mismatch is what
	/// produces <c>CS0738</c>.
	/// </para>
	/// </remarks>
	public class FakeNativeView : View
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
