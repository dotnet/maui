namespace Xamarin.Forms
{
	interface IPaddingElement
	{
		//note to implementor: implement this property publicly
		Thickness Padding { get; }

		//note to implementor: but implement this method explicitly
		void OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue);
		Thickness PaddingDefaultValueCreator();
	}
}