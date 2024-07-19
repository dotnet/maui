#nullable disable
namespace Microsoft.Maui.Controls.Internals
{
	// TODO: Obsolete in .NET 9 as this is no longer used for anything.
	public interface IPlatformSizeService
	{
		SizeRequest GetPlatformSize(VisualElement view, double widthConstraint, double heightConstraint);
	}
}