namespace Xamarin.Forms
{
	internal interface IPlatform
	{
		SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint);
	}
}