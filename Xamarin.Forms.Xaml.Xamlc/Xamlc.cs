using System;
using System.Collections.Generic;
using Mono.Options;
using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Xaml
{
	class Xamlc
	{
		static readonly string help_string = "xamlc.exe - a utility for compiling XAML into IL.\n" +
		                                     "xamlc.exe assembly\n\n";

		public static void Main(string[] args)
		{
			bool help = false;
			int verbosity = 1;
			bool keep = false;
			bool optimize = false;
			string paths = null;
			string refs = null;
			List<string> extra = null;

			var p = new OptionSet
			{
				{ "h|?|help", "Print this help message", v => help = true },
				{ "v=|verbosity=", "0 is quiet, 1 is normal, 2 is verbose", v => verbosity = Int32.Parse(v) },
				{ "o|optimize", "Optimize generated IL", v => optimize = true },
				{ "keep", "do not strip compiled embedded xaml", v => keep = true },
				{ "p=|paths=|dependencypaths=", "look for dependencies in (comma separated) list of paths", v => paths = v },
				{ "r=", "referencepath", v => refs = v },
			};

			if (help || args.Length < 1)
			{
				ShowHelp(p);
				Environment.Exit(0);
			}
			try
			{
				extra = p.Parse(args);
			}
			catch (OptionException)
			{
				Console.WriteLine("Type `xamlc --help' for more information.");
				return;
			}

			if (extra.Count == 0)
			{
				if (verbosity > 0)
				{
					Console.WriteLine("assembly missing");
					ShowHelp(p);
				}
				Environment.Exit(0);
			}

			var assembly = extra[0];
			var xamlc = new XamlCTask {
				Assembly = assembly,
				KeepXamlResources = keep,
				OptimizeIL = optimize,
				DependencyPaths = paths,
				ReferencePath = refs,
				DebugSymbols = true,
			};
			IList<Exception> _;
			xamlc.Execute(out _);
		}

		static void ShowHelp(OptionSet ops)
		{
			Console.WriteLine(help_string);
			ops.WriteOptionDescriptions(Console.Out);
		}
	}
}