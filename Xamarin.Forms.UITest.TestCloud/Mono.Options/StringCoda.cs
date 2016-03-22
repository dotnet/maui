//
// Options.cs
//
// Authors:
//  Jonathan Pryor <jpryor@novell.com>
//  Federico Di Gregorio <fog@initd.org>
//  Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (C) 2008 Novell (http://www.novell.com)
// Copyright (C) 2009 Federico Di Gregorio.
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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

// Compile With:
//   gmcs -debug+ -r:System.Core Options.cs -o:NDesk.Options.dll
//   gmcs -debug+ -d:LINQ -r:System.Core Options.cs -o:NDesk.Options.dll
//
// The LINQ version just changes the implementation of
// OptionSet.Parse(IEnumerable<string>), and confers no semantic changes.

//
// A Getopt::Long-inspired option parsing library for C#.
//
// NDesk.Options.OptionSet is built upon a key/value table, where the
// key is a option format string and the value is a delegate that is 
// invoked when the format string is matched.
//
// Option format strings:
//  Regex-like BNF Grammar: 
//    name: .+
//    type: [=:]
//    sep: ( [^{}]+ | '{' .+ '}' )?
//    aliases: ( name type sep ) ( '|' name type sep )*
// 
// Each '|'-delimited name is an alias for the associated action.  If the
// format string ends in a '=', it has a required value.  If the format
// string ends in a ':', it has an optional value.  If neither '=' or ':'
// is present, no value is supported.  `=' or `:' need only be defined on one
// alias, but if they are provided on more than one they must be consistent.
//
// Each alias portion may also end with a "key/value separator", which is used
// to split option values if the option accepts > 1 value.  If not specified,
// it defaults to '=' and ':'.  If specified, it can be any character except
// '{' and '}' OR the *string* between '{' and '}'.  If no separator should be
// used (i.e. the separate values should be distinct arguments), then "{}"
// should be used as the separator.
//
// Options are extracted either from the current option by looking for
// the option name followed by an '=' or ':', or is taken from the
// following option IFF:
//  - The current option does not contain a '=' or a ':'
//  - The current option requires a value (i.e. not a Option type of ':')
//
// The `name' used in the option format string does NOT include any leading
// option indicator, such as '-', '--', or '/'.  All three of these are
// permitted/required on any named option.
//
// Option bundling is permitted so long as:
//   - '-' is used to start the option group
//   - all of the bundled options are a single character
//   - at most one of the bundled options accepts a value, and the value
//     provided starts from the next character to the end of the string.
//
// This allows specifying '-a -b -c' as '-abc', and specifying '-D name=value'
// as '-Dname=value'.
//
// Option processing is disabled by specifying "--".  All options after "--"
// are returned by OptionSet.Parse() unchanged and unprocessed.
//
// Unprocessed options are returned from OptionSet.Parse().
//
// Examples:
//  int verbose = 0;
//  OptionSet p = new OptionSet ()
//    .Add ("v", v => ++verbose)
//    .Add ("name=|value=", v => Console.WriteLine (v));
//  p.Parse (new string[]{"-v", "--v", "/v", "-name=A", "/name", "B", "extra"});
//
// The above would parse the argument string array, and would invoke the
// lambda expression three times, setting `verbose' to 3 when complete.  
// It would also print out "A" and "B" to standard output.
// The returned array would contain the string "extra".
//
// C# 3.0 collection initializers are supported and encouraged:
//  var p = new OptionSet () {
//    { "h|?|help", v => ShowHelp () },
//  };
//
// System.ComponentModel.TypeConverter is also supported, allowing the use of
// custom data types in the callback type; TypeConverter.ConvertFromString()
// is used to convert the value option to an instance of the specified
// type:
//
//  var p = new OptionSet () {
//    { "foo=", (Foo f) => Console.WriteLine (f.ToString ()) },
//  };
//
// Random other tidbits:
//  - Boolean options (those w/o '=' or ':' in the option format string)
//    are explicitly enabled if they are followed with '+', and explicitly
//    disabled if they are followed with '-':
//      string a = null;
//      var p = new OptionSet () {
//        { "a", s => a = s },
//      };
//      p.Parse (new string[]{"-a"});   // sets v != null
//      p.Parse (new string[]{"-a+"});  // sets v != null
//      p.Parse (new string[]{"-a-"});  // sets v == null
//

using System;
using System.Collections.Generic;

#if LINQ
using System.Linq;
#endif

#if TEST
using NDesk.Options;
#endif

#if NDESK_OPTIONS
namespace NDesk.Options
#else

namespace Mono.Options
#endif
{
	internal static class StringCoda
	{
		public static IEnumerable<string> WrappedLines(string self, params int[] widths)
		{
			IEnumerable<int> w = widths;
			return WrappedLines(self, w);
		}

		public static IEnumerable<string> WrappedLines(string self, IEnumerable<int> widths)
		{
			if (widths == null)
				throw new ArgumentNullException("widths");
			return CreateWrappedLinesIterator(self, widths);
		}

		static IEnumerable<string> CreateWrappedLinesIterator(string self, IEnumerable<int> widths)
		{
			if (string.IsNullOrEmpty(self))
			{
				yield return string.Empty;
				yield break;
			}
			using(IEnumerator<int> ewidths = widths.GetEnumerator())
			{
				bool? hw = null;
				int width = GetNextWidth(ewidths, int.MaxValue, ref hw);
				int start = 0, end;
				do
				{
					end = GetLineEnd(start, width, self);
					char c = self[end - 1];
					if (char.IsWhiteSpace(c))
						--end;
					bool needContinuation = end != self.Length && !IsEolChar(c);
					string continuation = "";
					if (needContinuation)
					{
						--end;
						continuation = "-";
					}
					string line = self.Substring(start, end - start) + continuation;
					yield return line;
					start = end;
					if (char.IsWhiteSpace(c))
						++start;
					width = GetNextWidth(ewidths, width, ref hw);
				} while (start < self.Length);
			}
		}

		static int GetNextWidth(IEnumerator<int> ewidths, int curWidth, ref bool? eValid)
		{
			if (!eValid.HasValue || (eValid.HasValue && eValid.Value))
			{
				curWidth = (eValid = ewidths.MoveNext()).Value ? ewidths.Current : curWidth;
				// '.' is any character, - is for a continuation
				const string minWidth = ".-";
				if (curWidth < minWidth.Length)
				{
					throw new ArgumentOutOfRangeException("widths",
						string.Format("Element must be >= {0}, was {1}.", minWidth.Length, curWidth));
				}
				return curWidth;
			}
			// no more elements, use the last element.
			return curWidth;
		}

		static bool IsEolChar(char c)
		{
			return !char.IsLetterOrDigit(c);
		}

		static int GetLineEnd(int start, int length, string description)
		{
			int end = System.Math.Min(start + length, description.Length);
			int sep = -1;
			for (int i = start; i < end; ++i)
			{
				if (description[i] == '\n')
					return i + 1;
				if (IsEolChar(description[i]))
					sep = i + 1;
			}
			if (sep == -1 || end == description.Length)
				return end;
			return sep;
		}
	}
}