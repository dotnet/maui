using System;
using Windows.UI.Xaml.Input;

namespace Xamarin.Forms.Platform.UWP
{
	public static class KeyboardExtensions
	{
		public static InputScope ToInputScope(this Keyboard self)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			var result = new InputScope();
			var name = new InputScopeName();
			if (self == Keyboard.Default)
			{
				name.NameValue = InputScopeNameValue.Default;
			}
			else if (self == Keyboard.Chat)
			{
				name.NameValue = InputScopeNameValue.Chat;
			}
			else if (self == Keyboard.Email)
			{
				name.NameValue = InputScopeNameValue.EmailSmtpAddress;
			}
			else if (self == Keyboard.Numeric)
			{
				name.NameValue = InputScopeNameValue.Number;
			}
			else if (self == Keyboard.Telephone)
			{
				name.NameValue = InputScopeNameValue.TelephoneNumber;
			}
			else if (self == Keyboard.Text)
			{
				name.NameValue = InputScopeNameValue.Default;
			}
			else if (self == Keyboard.Url)
			{
				name.NameValue = InputScopeNameValue.Url;
			}
			else
			{
				name.NameValue = InputScopeNameValue.Default;
			}
			result.Names.Add(name);
			return result;
		}
	}
}