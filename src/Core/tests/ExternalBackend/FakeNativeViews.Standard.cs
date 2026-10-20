using System.Collections.Generic;

namespace Microsoft.Maui.ExternalBackend
{
	/// <summary>
	/// Stands in for the native view type of a platform .NET MAUI knows nothing about, on the
	/// platform-neutral target framework.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Deliberately does not derive from, or reference, any .NET MAUI type. This is the whole point of
	/// the test assembly: a backend's native types are its own.
	/// </para>
	/// <para>
	/// On the platform-neutral target framework, <see cref="Handlers.ViewHandler{TVirtualView, TPlatformView}"/>
	/// constrains <c>TPlatformView</c> only to <c>class</c>, so a completely unrelated type is legal.
	/// The platform variants of this file derive from the platform's own base view type, because that
	/// is what the constraint requires there.
	/// </para>
	/// <para>
	/// Members are prefixed <c>Fake</c> so that the same member names are usable on every target
	/// framework without colliding with a real platform base type's members (for example
	/// <c>FrameworkElement.Opacity</c> on Windows).
	/// </para>
	/// </remarks>
	public class FakeNativeView
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
