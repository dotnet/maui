namespace Microsoft.Maui.Controls;
internal interface IPlatformAttachedObject
{
	void OnPlatformAttachBehavior(BindableObject bindable);
	void OnPlatformDeattachBehavior(BindableObject bindable);
}
