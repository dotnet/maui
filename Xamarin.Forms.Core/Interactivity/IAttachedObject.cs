namespace Xamarin.Forms
{
	internal interface IAttachedObject
	{
		void AttachTo(BindableObject bindable);
		void DetachFrom(BindableObject bindable);
	}
}