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
    public static PrePost NewLineInfo(IndentedTextWriter codeWriter, IXmlLineInfo iXmlLineInfo, string? fileName)
        => new(() => LineInfo(codeWriter, iXmlLineInfo, fileName), () => LineDefault(codeWriter, iXmlLineInfo));

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
    public static PrePost NewBlock(IndentedTextWriter codeWriter, string begin="{", string end="}", int ident = 1, bool noTab = false) =>
        new(
            () => {
                if (noTab)
                    codeWriter.WriteLineNoTabs(begin);
                else
                    codeWriter.WriteLine(begin);
                codeWriter.Indent+=ident;
            },
            () => {
                codeWriter.Indent-=ident;
                if (noTab)
                    codeWriter.WriteLineNoTabs(end);
                else
                    codeWriter.WriteLine(end);
            });    
 
    [Conditional("_MAUIXAML_SG_LINEINFO")]
    static void LineInfo(IndentedTextWriter codeWriter, IXmlLineInfo iXmlLineInfo, string? fileName)
        => codeWriter.WriteLineNoTabs($"#line {(iXmlLineInfo.LineNumber != -1 ? iXmlLineInfo.LineNumber : 1)} \"{fileName}\"");

    [Conditional("_MAUIXAML_SG_LINEINFO")]
    static void LineDefault(IndentedTextWriter codeWriter, IXmlLineInfo iXmlLineInfo)
        => codeWriter.WriteLineNoTabs("#line default");

}