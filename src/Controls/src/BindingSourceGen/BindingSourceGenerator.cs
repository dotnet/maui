using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.BindingSourceGen;

[Generator(LanguageNames.CSharp)]
public class BindingSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext initializationContext)
	{
		var bindingsWithDiagnostics = context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (node, _) => IsSetBindingMethod(node),
			transform: static (context, token) =>
			{
				// TODO
				return null;
			})
			.Where(static endpoint => endpoint != null)
			.Select((endpoint, _) =>
			{
				AnalyzerDebug.Assert(endpoint != null, "Invalid endpoints should not be processed.");
				return endpoint;
			})
			.WithTrackingName(GeneratorSteps.EndpointModelStep);

		context.RegisterSourceOutput(bindingsWithDiagnostics, (context, endpoint) =>
		{
			foreach (var diagnostic in endpoint.Diagnostics)
			{
				context.ReportDiagnostic(diagnostic);
			}
		});

		var endpoints = bindingsWithDiagnostics
			.Where(endpoint => endpoint.Diagnostics.Count == 0)
			.WithTrackingName(GeneratorSteps.EndpointsWithoutDiagnosicsStep);

		context.RegisterSourceOutput(bindings, (context, bindings) =>
		{
			using var codeWriter = new BindingCodeWriter();
			foreach (var binding in bindings)
			{
				codeWriter.AddBinding(binding);
			}

			context.AddSource("GeneratedBindableObjectExtensions.g.cs", codeWriter.GenerateCode());
		});
	}

	private bool IsSetBindingMethod(SyntaxNode node)
	{
		
	}
}

public sealed 

public sealed record Binding(
	int Id,
	SourceCodeLocation Location,
	TypeName SourceType,
	TypeName PropertyType,
	PathPart[] Path,
	bool GenerateSetter);

public sealed record SourceCodeLocation(string FilePath, int Line, int Column);

public sealed record TypeName(string GlobalName, bool IsNullable, bool IsGenericParameter)
{
	public override string ToString()
		=> IsNullable
			? $"{GlobalName}?"
			: GlobalName;
}

public sealed record PathPart(string Member, bool IsNullable, object? Index = null)
{
    public string MemberName
        => Index is not null
            ? $"{Member}[{Index}]"
            : Member;
    
	public string PartGetter
        => Index switch
        {
                string str => $"[\"{str}\"]",
                int num => $"[{num}]",
                null => $".{MemberName}",
                _ => throw new NotSupportedException(),
        };
}
