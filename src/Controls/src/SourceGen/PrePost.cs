using System;
using System.CodeDom.Compiler;
using System.Xml;


namespace Microsoft.Maui.Controls.SourceGen;

class PrePost : IDisposable
{
    readonly Action post;
    public PrePost(Action pre, Action post)
    {
        this.post = post;
        pre();
    }
    /// <summary>
    /// Adds a new idented block between curly braces to the code writer
    /// </summary>
    /// <param name="codeWriter"></param>
    /// <returns></returns>
    public static PrePost NewBlock(IndentedTextWriter codeWriter)
        => NewBlock(codeWriter, "{", "}");
    public static PrePost NewBlock(IndentedTextWriter codeWriter, string begin, string end, int ident = 1, bool noTab = false) =>
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
    
    /// <summary>
    /// Adds a #line directive to the code writer   
    /// </summary>
    /// <param name="codeWriter"></param>
    /// <param name="iXmlLineInfo"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static PrePost NewLineInfo(IndentedTextWriter codeWriter, IXmlLineInfo iXmlLineInfo, string? fileName)
        => NewBlock(codeWriter, $"#line {(iXmlLineInfo.LineNumber != -1 ? iXmlLineInfo.LineNumber : 1)} \"{fileName}\"", "#line default", ident: 0, noTab: true);
        
    public void Dispose() => post();
}