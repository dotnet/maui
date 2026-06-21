using System;
using System.CodeDom.Compiler;
using System.Linq;

namespace Microsoft.Maui.Controls.SourceGen;

static class IndentedTextWriterExtensions
{
	public static void Append(this IndentedTextWriter writer, IndentedTextWriter other, bool noTabs = false)
	{
		other.Flush();
		var lines = other.InnerWriter.ToString().Split([other.InnerWriter.NewLine], StringSplitOptions.None).ToArray();
		for (var i = 0; i < lines.Length - 1; i++)
		{
			var line = lines[i];
			if (noTabs)
				writer.WriteLineNoTabs(line);
			else
				writer.WriteLine(line);
		}
	}
	public static void WriteLineNoTabs(this IndentedTextWriter writer)
		=> writer.WriteLineNoTabs("");
}
