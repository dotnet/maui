using System;
using System.CodeDom.Compiler;

namespace Microsoft.Maui.Controls.SourceGen;

static class IndentedTextWriterExtensions
{
	public static void Append(this IndentedTextWriter writer, IndentedTextWriter other, bool noTabs = false)
	{
		other.Flush();
		foreach (var line in other.InnerWriter.ToString().Split([other.InnerWriter.NewLine], StringSplitOptions.None))
		{
			if (noTabs)
				writer.WriteLineNoTabs(line);
			else
				writer.WriteLine(line);
		}
	}
}
