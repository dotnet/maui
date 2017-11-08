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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mono.Options;
using Xamarin.Forms.Build.Tasks;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace Xamarin.Forms.Xaml
{
	public class Xamlg
	{
		static readonly string HelpString = "xamlg.exe - a utility for generating partial classes from XAML.\n" +
		                                    "xamlg.exe xamlfile...\n\n" +
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

				var item = new TaskItem(f);
				item.SetMetadata("TargetPath", f);
				var generator = new XamlGTask() {
					BuildEngine = new DummyBuildEngine(),
					AssemblyName = "test",
					Language = "C#",
					XamlFiles = new[] { item },
					OutputPath = Path.GetDirectoryName(f),
				};


				new XamlGTask { 
					XamlFiles = new[] { new TaskItem(f)}
				}.Execute();

			}
		}

		static void ShowHelp(OptionSet ops)
		{
			Console.WriteLine(HelpString);
			ops.WriteOptionDescriptions(Console.Out);
		}
	}

	public class DummyBuildEngine : IBuildEngine
	{
		public void LogErrorEvent(BuildErrorEventArgs e)
		{
		}

		public void LogWarningEvent(BuildWarningEventArgs e)
		{
		}

		public void LogMessageEvent(BuildMessageEventArgs e)
		{
		}

		public void LogCustomEvent(CustomBuildEventArgs e)
		{
		}

		public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
		{
			return false;
		}

		public bool ContinueOnError {
			get { return false; }
		}

		public int LineNumberOfTaskNode {
			get { return 1; }
		}

		public int ColumnNumberOfTaskNode {
			get { return 1; }
		}

		public string ProjectFileOfTaskNode {
			get { return String.Empty; }
		}
	}
}