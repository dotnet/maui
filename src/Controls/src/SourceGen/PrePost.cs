using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Xml;


namespace Microsoft.Maui.Controls.SourceGen;

class PrePost : IDisposable
{
	/// <summary>
	/// Adds a #line directive to the code writer, reverts to default afterwards. No ident, no tabs
	/// </summary>
	/// <param name="codeWriter"></param>
	/// <param name="iXmlLineInfo"></param>
	/// <param name="fileName"></param>
	/// <returns></returns>
	public static PrePost NewLineInfo(IndentedTextWriter codeWriter, IXmlLineInfo iXmlLineInfo, ProjectItem? projectItem)
	{
		// Emit #line with an absolute path since relative paths have undefined behavior (https://github.com/dotnet/roslyn/issues/71202#issuecomment-1874649780)
		static void LineInfo(IndentedTextWriter codeWriter, IXmlLineInfo iXmlLineInfo, ProjectItem? projectItem)
		{
			var lineNumber = iXmlLineInfo.LineNumber != -1 ? iXmlLineInfo.LineNumber : 1;
			codeWriter.WriteLineNoTabs($"#line {lineNumber} \"{projectItem?.TargetPath}\"");
		}

		static void LineDefault(IndentedTextWriter codeWriter, IXmlLineInfo iXmlLineInfo)
			=> codeWriter.WriteLineNoTabs("#line default");

		return new(() => LineInfo(codeWriter, iXmlLineInfo, projectItem), () => LineDefault(codeWriter, iXmlLineInfo));
	}

	public static PrePost NoBlock() =>
			new(() => { }, () => { });

	public static PrePost NewConditional(IndentedTextWriter codeWriter, string condition, Action? orElse = null)
	{
		return new(() => codeWriter.WriteLineNoTabs($"#if {condition}"), () =>
		{
			if (orElse != null)
			{
				codeWriter.WriteLineNoTabs("#else");
				orElse();
			}
			codeWriter.WriteLineNoTabs("#endif");
		});
	}

	public static PrePost NewDisableWarning(IndentedTextWriter codeWriter, string warning)
		=> new(() => codeWriter.WriteLineNoTabs($"#pragma warning disable {warning}"), () => codeWriter.WriteLineNoTabs($"#pragma warning restore {warning}"));

	readonly Action post;
	PrePost(Action pre, Action post)
	{
		this.post = post;
		pre();
	}

	void IDisposable.Dispose() => post();

	/// <summary>
	/// Adds a new idented block between curly braces to the code writer
	/// </summary>
	/// <param name="codeWriter"></param>
	/// <param name="begin"></param>
	/// <param name="end"></param>
	public static PrePost NewBlock(IndentedTextWriter codeWriter, string begin = "{", string end = "}", int ident = 1, bool noTab = false) =>
		new(
			() =>
			{
				if (noTab)
					codeWriter.WriteLineNoTabs(begin);
				else
					codeWriter.WriteLine(begin);
				codeWriter.Indent += ident;
			},
			() =>
			{
				codeWriter.Indent -= ident;
				if (noTab)
					codeWriter.WriteLineNoTabs(end);
				else
					codeWriter.WriteLine(end);
			});
}
