using System;

namespace Xamarin.Forms
{
	interface ITextElement
	{
		//note to implementor: implement this property publicly
		Color TextColor { get; }

		//note to implementor: but implement this method explicitly
		void OnTextColorPropertyChanged(Color oldValue, Color newValue);

		double CharacterSpacing { get; }

		//note to implementor: but implement these methods explicitly
		void OnCharacterSpacingPropertyChanged(double oldValue, double newValue);
	}
}
