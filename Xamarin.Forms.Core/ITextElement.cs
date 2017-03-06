using System;

namespace Xamarin.Forms
{
	interface ITextElement
	{
		//note to implementor: implement the properties publicly
		Color TextColor { get; }

		//note to implementor: but implement the methods explicitly
		void OnTextColorPropertyChanged(Color oldValue, Color newValue);
	}
}