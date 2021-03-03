namespace Microsoft.Maui.Controls
{
	internal interface IAttachedObject
	{
		void AttachTo(BindableObject bindable);
		void DetachFrom(BindableObject bindable);
	}
}