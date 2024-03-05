using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.BindingSourceGen;

internal class DiagnosticFactory
{
    // public static Diagnostic ClassIsNotPartial(ClassDeclarationSyntax classDeclaration)
    //     => Diagnostic.Create(
    //         new DiagnosticDescriptor(
    //             "MAUIG2001",
    //             "Class is not partial",
    //             "The class '{0}' is not partial. The generated code will not be able to extend it.",
    //             "SourceGeneration",
    //             DiagnosticSeverity.Error,
    //             isEnabledByDefault: true),
    //         classDeclaration.Keyword.GetLocation(),
    //         classDeclaration.Identifier.Text);
}
