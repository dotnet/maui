using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TPlatformView>
	{
		// The platform-less assemblies have no platform views of their own: every layout operation is delegated to the
		// registered PlatformViewBackend (e.g. the GTK 4 backend) and is a no-op when none is registered.

		public override void PlatformArrange(Rect rect) =>
			PlatformViewBackend.Current?.PlatformArrange(this, rect);

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			PlatformViewBackend.Current?.GetDesiredSize(this, widthConstraint, heightConstraint) ?? Size.Zero;

		protected override void SetupContainer() =>
			PlatformViewBackend.Current?.SetupContainer(this);

		protected override void RemoveContainer() =>
			PlatformViewBackend.Current?.RemoveContainer(this);
	}
}
