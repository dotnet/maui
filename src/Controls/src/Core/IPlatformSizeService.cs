#nullable disable
namespace Microsoft.Maui.Controls.Internals
{
	public interface IPlatformSizeService
	{
		SizeRequest GetPlatformSize(VisualElement view, double widthConstraint, double heightConstraint);
	}
}