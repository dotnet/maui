using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.SourceGen
{
	[Generator]
	public class CodeBehindGenerator : ISourceGenerator
	{
		static string[] _csharpKeywords = new[] {
			"abstract", "as",
			"base", "bool", "break", "byte",
			"case", "catch", "char", "checked", "class", "const", "continue",
			"decimal", "default", "delegate", "do", "double",
			"else", "enum", "event", "explicit", "extern",
			"false", "finally", "fixed", "float", "for", "foreach",
			"goto",
			"if", "implicit", "in", "int", "interface", "internal", "is",
			"lock", "long", "namespace", "new", "null",
			"object", "operator", "out", "override",
			"params", "private", "protected", "public",
			"readonly", "ref", "return",
			"sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
			"this", "throw", "true", "try", "typeof",
			"uint", "ulong", "unchecked", "unsafe", "ushort", "using",
			"virtual", "void", "volatile",
			"while",
		};

		public void Initialize(GeneratorInitializationContext context)
		{
			//#if DEBUG
			//if (!Debugger.IsAttached)
			//	Debugger.Launch();
			//#endif
		}

		public void Execute(GeneratorExecutionContext context)
		{
			if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.targetframework", out var targetframework))
				return;

			if (targetframework.IndexOf("-android", StringComparison.OrdinalIgnoreCase) != -1)
			{
				var code = GenerateAndroidSource(context);
				var name = "Android.sg.cs";
				context.AddSource(name, SourceText.From(code, Encoding.UTF8));
			}
		}

		string GenerateAndroidSource(GeneratorExecutionContext context)
		{
			return "Test";
		}
	}
}
