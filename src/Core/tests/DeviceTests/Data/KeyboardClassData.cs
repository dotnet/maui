using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Core.DeviceTests.Data
{
	public class NumericKeyboardClassData : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[] { Keyboard.Numeric, true };
			yield return new object[] { Keyboard.Default, false };
			yield return new object[] { Keyboard.Chat, false };
			yield return new object[] { Keyboard.Email, false };
			yield return new object[] { Keyboard.Plain, false };
			yield return new object[] { Keyboard.Telephone, false };
			yield return new object[] { Keyboard.Text, false };
			yield return new object[] { Keyboard.Url, false };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class ChatKeyboardClassData : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[] { Keyboard.Chat, true };
			yield return new object[] { Keyboard.Default, false };
			yield return new object[] { Keyboard.Numeric, false };
			yield return new object[] { Keyboard.Email, false };
			yield return new object[] { Keyboard.Plain, false };
			yield return new object[] { Keyboard.Telephone, false };
			yield return new object[] { Keyboard.Text, false };
			yield return new object[] { Keyboard.Url, false };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class EmailKeyboardClassData : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[] { Keyboard.Email, true };
			yield return new object[] { Keyboard.Default, false };
			yield return new object[] { Keyboard.Numeric, false };
			yield return new object[] { Keyboard.Chat, false };
			yield return new object[] { Keyboard.Plain, false };
			yield return new object[] { Keyboard.Telephone, false };
			yield return new object[] { Keyboard.Text, false };
			yield return new object[] { Keyboard.Url, false };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class PlainKeyboardClassData : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[] { Keyboard.Plain, true };
			yield return new object[] { Keyboard.Default, false };
			yield return new object[] { Keyboard.Numeric, false };
			yield return new object[] { Keyboard.Chat, false };
			yield return new object[] { Keyboard.Email, false };
			yield return new object[] { Keyboard.Telephone, false };
			yield return new object[] { Keyboard.Text, false };
			yield return new object[] { Keyboard.Url, false };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class TelephoneKeyboardClassData : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[] { Keyboard.Telephone, true };
			yield return new object[] { Keyboard.Default, false };
			yield return new object[] { Keyboard.Numeric, false };
			yield return new object[] { Keyboard.Chat, false };
			yield return new object[] { Keyboard.Email, false };
			yield return new object[] { Keyboard.Plain, false };
			yield return new object[] { Keyboard.Text, false };
			yield return new object[] { Keyboard.Url, false };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class TextKeyboardClassData : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[] { Keyboard.Text, true };
			yield return new object[] { Keyboard.Default, false };
			yield return new object[] { Keyboard.Numeric, false };
			yield return new object[] { Keyboard.Chat, false };
			yield return new object[] { Keyboard.Email, false };
			yield return new object[] { Keyboard.Plain, false };
			yield return new object[] { Keyboard.Telephone, false };
			yield return new object[] { Keyboard.Url, false };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class UrlKeyboardClassData : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[] { Keyboard.Url, true };
			yield return new object[] { Keyboard.Default, false };
			yield return new object[] { Keyboard.Numeric, false };
			yield return new object[] { Keyboard.Chat, false };
			yield return new object[] { Keyboard.Email, false };
			yield return new object[] { Keyboard.Plain, false };
			yield return new object[] { Keyboard.Telephone, false };
			yield return new object[] { Keyboard.Text, false };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
