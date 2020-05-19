using System;
using System.Collections.Generic;

using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class KeyboardCoreGallery : ContentPage
	{
		public KeyboardCoreGallery ()
		{
			var keyboardTypes = new[] {
				Keyboard.Chat,
				Keyboard.Default,
				Keyboard.Email,
				Keyboard.Numeric,
				Keyboard.Plain,
				Keyboard.Telephone,
				Keyboard.Text,
				Keyboard.Url
			};

			var layout = new StackLayout ();

			foreach (var keyboardType in keyboardTypes) {
				var viewContainer = new ViewContainer<Entry> (Test.InputView.Keyboard, new Entry { Placeholder = keyboardType.ToString (), Keyboard = keyboardType } );
				layout.Children.Add (viewContainer.ContainerLayout);
			}

			var customKeyboards = new [] {
				Tuple.Create ("None", Keyboard.Create (KeyboardFlags.None)),
				Tuple.Create ("Suggestions", Keyboard.Create (KeyboardFlags.Suggestions)),
				Tuple.Create ("Spellcheck", Keyboard.Create (KeyboardFlags.Spellcheck)),
				Tuple.Create ("SpellcheckSuggestions", Keyboard.Create (KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions)),
				Tuple.Create ("Capitalize", Keyboard.Create (KeyboardFlags.CapitalizeSentence)),
				Tuple.Create ("CapitalizeSuggestions", Keyboard.Create (KeyboardFlags.CapitalizeSentence | KeyboardFlags.Suggestions)),
				Tuple.Create ("CapitalizeSpellcheck", Keyboard.Create (KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck)),
				Tuple.Create ("CapitalizeSpellcheckSuggestions",  Keyboard.Create (KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions)),
				Tuple.Create ("All",  Keyboard.Create (KeyboardFlags.All)),
			};

			foreach (var customKeyboard in customKeyboards) {
				var viewContainer = new ViewContainer<Entry> (Test.InputView.Keyboard, new Entry { Placeholder = customKeyboard.Item1, Keyboard = customKeyboard.Item2 } );
				layout.Children.Add (viewContainer.ContainerLayout);
			}

			Content = new ScrollView { Content = layout };
		}
		
	}
}