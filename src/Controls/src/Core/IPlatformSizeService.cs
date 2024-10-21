#nullable disable
using System;
namespace Microsoft.Maui.Controls.Internals
{
	[Obsolete("No .NET MAUI code makes use of IPlatformSizeService and this interface will be removed in a future .NET MAUI release.")]
	public interface IPlatformSizeService
	{
		SizeRequest GetPlatformSize(VisualElement view, double widthConstraint, double heightConstraint);
	}
}
