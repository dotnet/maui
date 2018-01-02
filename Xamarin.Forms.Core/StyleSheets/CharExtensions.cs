using System.Runtime.CompilerServices;

namespace Xamarin.Forms.StyleSheets
{
	static class CharExtensions
	{
		//w			[ \t\r\n\f]*
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsW(this char c)
		{
			return c == ' '
				|| c == '\t'
				|| c == '\r'
				|| c == '\n'
				|| c == '\f';
		}

		//nmstart	[_a-z]|{nonascii}|{escape}
		//escape	{unicode}|\\[^\n\r\f0-9a-f]
		//nonascii	[^\0-\237]
		// TODO support escape and nonascii
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNmStart(this char c)
		{
			return c == '_' || char.IsLetter(c);
		}

		//nmchar	[_a-z0-9-]|{nonascii}|{escape}
		//unicode	\\[0-9a-f]{1,6}(\r\n|[ \n\r\t\f])?
		//escape	{unicode}|\\[^\n\r\f0-9a-f]
		//nonascii	[^\0-\237]
		//TODO support escape, nonascii and unicode
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNmChar(this char c)
		{
			return c == '_'
				|| c == '-'
				|| char.IsLetterOrDigit(c);
		}
	}
}