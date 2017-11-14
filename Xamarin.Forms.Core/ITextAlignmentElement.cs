namespace Xamarin.Forms
{
	interface ITextAlignmentElement
	{
		//note to implementor: implement the properties publicly
		TextAlignment HorizontalTextAlignment { get; }

		//note to implementor: but implement the methods explicitly
		void OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue);
	}
}