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

    public static PrePost NewBlock(IndentedTextWriter codeWriter) =>
        new(
            () => {
                codeWriter.WriteLine("{");
                codeWriter.Indent++;
            },
            () => {
                codeWriter.Indent--;
                codeWriter.WriteLine("}");
            });

    public void Dispose() => post();
}