namespace Xamarin.Forms
{
	interface IBorderElement
	{
		//note to implementor: implement this property publicly
		Color BorderColor { get; }

		//note to implementor: but implement the methods explicitly
		void OnBorderColorPropertyChanged(Color oldValue, Color newValue);
	}
}