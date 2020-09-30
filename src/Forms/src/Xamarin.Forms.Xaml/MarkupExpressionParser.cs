//
// MarkupExpressionParser.cs
//
// This code is partly salvaged from moonlight. Following licence apply.
//
//
// Author(s):
//   Moonlight List (moonlight-list@lists.ximian.com)
//   Stephane Delcroix (stephane@mi8.be)
//
// Copyright 2009 Novell, Inc.
// Copyright 2013 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;

namespace Xamarin.Forms.Xaml
{
	abstract class MarkupExpressionParser
	{
		protected struct Property
		{
			public bool last;
			public string name;
			public string strValue;
			public object value;
		}

		public object ParseExpression(ref string expression, IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (expression.StartsWith("{}", StringComparison.Ordinal))
				return expression.Substring(2);

			if (expression[expression.Length - 1] != '}')
				throw new XamlParseException("Expression must end with '}'", serviceProvider);

			int len;
			string match;
			if (!MatchMarkup(out match, expression, out len))
				return false;
			expression = expression.Substring(len).TrimStart();
			if (expression.Length == 0)
				throw new XamlParseException("Expression did not end in '}'", serviceProvider);

			var parser = Activator.CreateInstance(GetType()) as IExpressionParser;
			return parser.Parse(match, ref expression, serviceProvider);
		}

		internal static bool MatchMarkup(out string match, string expression, out int end)
		{
			if (expression.Length < 2)
			{
				end = 1;
				match = null;
				return false;
			}

			if (expression[0] != '{')
			{
				end = 2;
				match = null;
				return false;
			}

			int i;
			bool found = false;
			for (i = 1; i < expression.Length; i++)
			{
				if (expression[i] == ' ')
					continue;
				found = true;
				break;
			}

			if (!found)
			{
				end = 3;
				match = null;
				return false;
			}

			int c;
			for (c = 0; c + i < expression.Length; c++)
			{
				if (expression[i + c] == ' ' || expression[i + c] == '}')
					break;
			}

			if (i + c == expression.Length)
			{
				end = 6;
				match = null;
				return false;
			}

			end = i + c;
			match = expression.Substring(i, c);
			return true;
		}

		protected Property ParseProperty(IServiceProvider serviceProvider, ref string remaining)
		{
			object value = null;
			string str_value;
			string name;

			remaining = remaining.TrimStart();
			if (remaining[0] == '{')
				return ParsePropertyExpression(null, serviceProvider, ref remaining);

			str_value = GetNextPiece(serviceProvider, ref remaining, out var next);
			if (next == '=')
			{
				remaining = remaining.TrimStart();
				if (remaining[0] == '{')
					return ParsePropertyExpression(str_value, serviceProvider, ref remaining);

				name = str_value;
				str_value = GetNextPiece(serviceProvider, ref remaining, out next);
			}
			else
			{
				name = null;
			}

			return new Property { last = next == '}', name = name, strValue = str_value, value = value };
		}

		Property ParsePropertyExpression(string prop, IServiceProvider serviceProvider, ref string remaining)
		{
			bool last;
			var value = ParseExpression(ref remaining, serviceProvider);
			remaining = remaining.TrimStart();
			if (remaining.Length <= 0)
				throw new XamlParseException("Unexpected end of markup expression", serviceProvider);
			if (remaining[0] == ',')
				last = false;
			else if (remaining[0] == '}')
				last = true;
			else
				throw new XamlParseException("Unexpected character following value string", serviceProvider);

			remaining = remaining.Substring(1);
			return new Property { last = last, name = prop, strValue = value as string, value = value };
		}

		string GetNextPiece(IServiceProvider serviceProvider, ref string remaining, out char next)
		{
			bool inString = false;
			int end = 0;
			char stringTerminator = '\0';

			var piece = new StringBuilder();
			// If we're inside a quoted string we append all chars to our piece until we hit the ending quote.
			while (end < remaining.Length &&
				   (inString || (remaining[end] != '}' && remaining[end] != ',' && remaining[end] != '=')))
			{
				if (inString)
				{
					if (remaining[end] == stringTerminator)
					{
						inString = false;
						end++;
						while (remaining[end] == ' ')
							end++;
						break;
					}
				}
				else
				{
					if (remaining[end] == '\'' || remaining[end] == '"')
					{
						inString = true;
						stringTerminator = remaining[end];
						end++;
						continue;
					}
				}

				// If this is an escape char, consume it and append the next char to our piece.
				if (remaining[end] == '\\')
				{
					end++;
					if (end == remaining.Length)
						break;
				}
				piece.Append(remaining[end]);
				end++;
			}

			if (inString && end == remaining.Length)
				throw new XamlParseException("Unterminated quoted string", serviceProvider);

			if (end == 0)
				throw new XamlParseException("Empty value string in markup expression", serviceProvider);

			next = remaining[end];
			remaining = remaining.Substring(end + 1);

			// Whitespace is trimmed from the end of the piece before stripping
			// quote chars from the start/end of the string. 
			while (piece.Length > 0 && char.IsWhiteSpace(piece[piece.Length - 1]))
				piece.Length--;

			if (piece.Length >= 2)
			{
				char first = piece[0];
				char last = piece[piece.Length - 1];
				if ((first == '\'' && last == '\'') || (first == '"' && last == '"'))
				{
					piece.Remove(piece.Length - 1, 1);
					piece.Remove(0, 1);
				}
			}

			return piece.ToString();
		}

		protected static (string, string) ParseName(string name)
		{
			var split = name.Split(':');

			if (split.Length > 2)
				throw new ArgumentException();

			return split.Length == 2 ? (split[0], split[1]) : ("", split[0]);
		}
	}
}