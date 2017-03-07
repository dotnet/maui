namespace Xamarin.Forms.Internals
{
	public interface IPlatform
	{
		SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint);
	}
}