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
// Original Author for Moonlight:
//   Jackson Harper (jackson@ximian.com)
//
// Copyright 2007 Novell, Inc.
//
// Author:
//   Stephane Delcroix (stephane@mi8.be)
//
// Copyright 2013 Mobile Inception

using System;
using System.Collections.Generic;
using System.IO;
using Mono.Options;
using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Xaml
{
	public class Xamlg
	{
		static readonly string HelpString = "xamlg.exe - a utility for generating partial classes from XAML.\n" +
		                                    "xamlg.exe xamlfile[,outputfile]...\n\n" +
		                                    "If an outputfile is not specified one will be created using the format <xamlfile>.g.cs\n\n";

		public static void Main(string[] args)
		{
			bool help = false;
			var p = new OptionSet
			{
				{ "h|?|help", "Print this help message", v => help = true }
			};

			if (help || args.Length < 1)
			{
				ShowHelp(p);
				Environment.Exit(0);
			}
			List<string> extra = null;
			try
			{
				extra = p.Parse(args);
			}
			catch (OptionException)
			{
				Console.WriteLine("Type `xamlg --help' for more information.");
				return;
			}

			foreach (var file in extra)
			{
				var f = file;
				var n = "";

				var sub = file.IndexOf(",", StringComparison.InvariantCulture);
				if (sub > 0)
				{
					n = f.Substring(sub + 1);
					f = f.Substring(0, sub);
				}
				else
					n = string.Concat(Path.GetFileName(f), ".g.", XamlGTask.Provider.FileExtension);

				XamlGTask.GenerateFile(f, f, f, n);
			}
		}

		static void ShowHelp(OptionSet ops)
		{
			Console.WriteLine(HelpString);
			ops.WriteOptionDescriptions(Console.Out);
		}
	}
}