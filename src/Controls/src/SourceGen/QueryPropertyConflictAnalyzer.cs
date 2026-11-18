using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Maui.Controls.SourceGen;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class QueryPropertyConflictAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		=> ImmutableArray.Create(Descriptors.QueryPropertyConflictsWithIQueryPropertyAttributable);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterCompilationStartAction(static context =>
		{
			var queryAttributable = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.IQueryPropertyAttributable");
			var queryPropertyAttribute = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.QueryPropertyAttribute");
			var applyQueryProperties = queryAttributable?.GetMembers("ApplyQueryProperties").OfType<IMethodSymbol>().FirstOrDefault();
			if (queryAttributable is null || queryPropertyAttribute is null || applyQueryProperties is null)
				return;

			context.RegisterSymbolAction(context =>
			{
				var type = (INamedTypeSymbol)context.Symbol;
				if (type.TypeKind != TypeKind.Class || !type.Implements(queryAttributable))
					return;

				if (!type.GetAllAttributes(context: null).Any(attribute =>
					attribute.AttributeClass?.InheritsFrom(queryPropertyAttribute, context: null) == true))
					return;

				if (type.FindImplementationForInterfaceMember(applyQueryProperties) is not IMethodSymbol implementation ||
					QueryPropertyGenerator.IsGeneratedQueryPropertyImplementation(implementation))
					return;

				context.ReportDiagnostic(Diagnostic.Create(
					Descriptors.QueryPropertyConflictsWithIQueryPropertyAttributable, type.Locations[0], type.Name));
			}, SymbolKind.NamedType);
		});
	}
}
