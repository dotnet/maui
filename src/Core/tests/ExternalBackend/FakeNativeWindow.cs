namespace Microsoft.Maui.ExternalBackend
{
	/// <summary>
	/// Native window type for the fake external platform. Not a view, and not related to any platform
	/// type on any target framework.
	/// </summary>
	/// <remarks>
	/// <see cref="Handlers.ElementHandler{TVirtualView, TPlatformView}"/> constrains
	/// <c>TPlatformView</c> only to <c>class</c> on every target framework, so this single definition
	/// works everywhere — unlike the view types, which must satisfy the platform's base view constraint.
	/// </remarks>
	public class FakeNativeWindow
	{
		public string? FakeTitle { get; set; }

		public object? FakeContent { get; set; }
	}
}
