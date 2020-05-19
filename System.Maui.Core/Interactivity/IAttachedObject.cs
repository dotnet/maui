namespace System.Maui
{
	internal interface IAttachedObject
	{
		void AttachTo(BindableObject bindable);
		void DetachFrom(BindableObject bindable);
	}
}