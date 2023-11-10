using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform;

public class ShellView : NotImplementedView
{

	public ShellView() : base(nameof(ShellView)) { }

	[MissingMapper]
	public bool IsOpened { get; set; }

	[MissingMapper]
	public void UpdateFlyout(IView flyoutViewFlyout)
	{ }

	[MissingMapper]
	public void UpdateFlyoutBehavior(FlyoutBehavior flyoutViewFlyoutBehavior)
	{ }

	[MissingMapper]
	public void UpdateDrawerWidth(double flyoutViewFlyoutWidth)
	{ }

	[MissingMapper]
	public void UpdateBackgroundColor(Color viewBackgroundColor)
	{ }

	[MissingMapper]
	public void UpdateCurrentItem(ShellItem viewCurrentItem)
	{ }

	[MissingMapper]
	public void UpdateFlyoutBackDrop(Brush viewFlyoutBackdrop)
	{ }

	[MissingMapper]
	public void UpdateFlyoutFooter(Shell view)
	{ }

	[MissingMapper]
	public void UpdateFlyoutHeader(Shell view)
	{ }

	[MissingMapper]
	public void UpdateItems()
	{ }

}