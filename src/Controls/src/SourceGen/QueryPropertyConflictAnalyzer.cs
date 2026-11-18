using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Maui.Controls.SourceGen;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class QueryPropertyConflictAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		=> ImmutableArray.Create(Descriptors.QueryPropertyConflictsWithIQueryAttributable);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterCompilationStartAction(static context =>
		{
			var queryAttributable = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.IQueryAttributable");
			var queryPropertyAttribute = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.QueryPropertyAttribute");
			var applyQueryAttributes = queryAttributable?.GetMembers("ApplyQueryAttributes").OfType<IMethodSymbol>().FirstOrDefault();
			if (queryAttributable is null || queryPropertyAttribute is null || applyQueryAttributes is null)
				return;

			context.RegisterSymbolAction(context =>
			{
				var type = (INamedTypeSymbol)context.Symbol;
				if (type.TypeKind != TypeKind.Class || !type.Implements(queryAttributable))
					return;

				if (!type.GetAllAttributes(context: null).Any(attribute =>
					attribute.AttributeClass?.InheritsFrom(queryPropertyAttribute, context: null) == true))
					return;

				if (type.FindImplementationForInterfaceMember(applyQueryAttributes) is not IMethodSymbol implementation ||
					QueryPropertyGenerator.IsGeneratedQueryPropertyImplementation(implementation))
					return;

				context.ReportDiagnostic(Diagnostic.Create(
					Descriptors.QueryPropertyConflictsWithIQueryAttributable, type.Locations[0], type.Name));
			}, SymbolKind.NamedType);
		});
	}
}
