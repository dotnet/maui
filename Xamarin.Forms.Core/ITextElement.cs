using System;

namespace Xamarin.Forms
{
	interface ITextElement
	{
		//note to implementor: implement this property publicly
		Color TextColor { get; }

		//note to implementor: but implement this method explicitly
		void OnTextColorPropertyChanged(Color oldValue, Color newValue);
	}
}