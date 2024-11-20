using System;
using System.CodeDom.Compiler;


namespace Microsoft.Maui.Controls.SourceGen;

class PrePost : IDisposable
{
    readonly Action post;
    public PrePost(Action pre, Action post)
    {
        this.post = post;
        pre();
    }

    public static PrePost NewBlock(IndentedTextWriter codeWriter, string begin = "{", string end = "}", int ident = 1, bool noTab = false) =>
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

    public void Dispose() => post();
}