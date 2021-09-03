using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen
{
	//[Generator]
	public class MauiFontRegistrationGenerator : ISourceGenerator
	{
		public void Initialize(GeneratorInitializationContext context)
		{
		}

		public void Execute(GeneratorExecutionContext context)
		{
			var src = new StringBuilder();

			src.Append(@"
namespace Maui.Generated
{
    [global::System.CodeDom.Compiler.GeneratedCode(""Microsoft.Maui.Controls.SourceGen"", ""1.0.0.0"")]
    internal static class MauiGeneratedRegistrar
    {
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        internal static void Register()
        {
");
			var g = new MauiFontRegistrarSourceGenerator();
			g.GenerateRegisterBodyCode(context, src);

			src.Append(
		@"}
    }
}");
			context.AddSource("Maui_Generated_MauiGeneratedRegistrar.cs", src.ToString());
		}
	}

	public interface IMauiRegistrarSourceGenerator
	{
		void GenerateRegisterBodyCode(GeneratorExecutionContext context, StringBuilder sourceBuilder);
	}

	public class MauiFontRegistrarSourceGenerator : IMauiRegistrarSourceGenerator
	{
		public void GenerateRegisterBodyCode(GeneratorExecutionContext context, StringBuilder sourceBuilder)
		{
			foreach (var file in context.AdditionalFiles)
			{
				var extension = Path.GetExtension(file.Path);

				if (extension.Equals(".ttf", StringComparison.InvariantCultureIgnoreCase)
					|| extension.Equals(".otf", StringComparison.InvariantCultureIgnoreCase))
				{
					var alias = context.GetMSBuildItemMetadata(file, "FontAlias");

					var filename = Path.GetFileName(file.Path);

					var srcAlias = string.IsNullOrWhiteSpace(alias) ? "null" : $"\"{alias}\"";

					sourceBuilder.AppendLine(
						@"global::Xamarin.Forms.Internals.FontRegistrar.Register(
                            new global::Xamarin.Forms.ExportFontAttribute(""" + filename + @""")
                            { Alias = " + srcAlias + @" },
                            global::System.Reflection.Assembly.GetExecutingAssembly());");
				}
			}
		}
	}
}
