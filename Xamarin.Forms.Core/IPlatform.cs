using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IPlatform
	{
		SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint);
	}
}