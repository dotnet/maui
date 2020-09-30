using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Xamarin.Forms.StyleSheets
{
	static class TextReaderExtensions
	{
		//ident		[-]?{nmstart}{nmchar}*
		public static string ReadIdent(this TextReader reader)
		{
			var sb = new StringBuilder();
			bool first = true;
			bool hasLeadingDash = false;
			int p;
			while ((p = reader.Peek()) > 0)
			{
				var c = unchecked((char)p);
				if (first && !hasLeadingDash && c == '-')
				{
					sb.Append((char)reader.Read());
					hasLeadingDash = true;
				}
				else if (first && c.IsNmStart())
				{
					sb.Append((char)reader.Read());
					first = false;
				}
				else if (first)
				{ //a nmstart is expected
					throw new Exception();
				}
				else if (c.IsNmChar())
					sb.Append((char)reader.Read());
				else
					break;
			}
			return sb.ToString();
		}

		//name		{nmchar}+
		public static string ReadName(this TextReader reader)
		{
			var sb = new StringBuilder();
			int p;
			while ((p = reader.Peek()) > 0)
			{
				var c = unchecked((char)p);
				if (c.IsNmChar())
					sb.Append((char)reader.Read());
				else
					break;
			}
			return sb.ToString();
		}

		public static string ReadUntil(this TextReader reader, params char[] limit)
		{
			var sb = new StringBuilder();
			int p;
			while ((p = reader.Peek()) > 0)
			{
				var c = unchecked((char)p);
				if (limit != null && limit.Contains(c))
					break;
				reader.Read();
				sb.Append(c);
			}
			return sb.ToString();
		}

		//w			[ \t\r\n\f]*
		public static void SkipWhiteSpaces(this TextReader reader)
		{
			int p;
			while ((p = reader.Peek()) > 0)
			{
				var c = unchecked((char)p);
				if (!c.IsW())
					break;
				reader.Read();
			}
		}
	}
}