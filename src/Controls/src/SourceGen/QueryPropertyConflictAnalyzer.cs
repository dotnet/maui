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
			if (queryAttributable is null || queryPropertyAttribute is null)
				return;

			context.RegisterSymbolAction(context =>
			{
				var type = (INamedTypeSymbol)context.Symbol;
				if (type.TypeKind != TypeKind.Class || !type.Implements(queryAttributable))
					return;

				// Shell applies inherited attributes as well as inherited interface implementations,
				// so the conflict is checked on the whole hierarchy. It is only reported on types that
				// introduce one of the mechanisms themselves, so a single conflict isn't reported on
				// every derived type.
				var declaresInterface = type.Interfaces.Any(i =>
					SymbolEqualityComparer.Default.Equals(i, queryAttributable) || i.Implements(queryAttributable));
				var declaresAttribute = type.GetAttributes().Any(IsQueryProperty);
				if (!declaresInterface && !declaresAttribute)
					return;

				if (declaresAttribute || type.GetAllAttributes(context: null).Any(IsQueryProperty))
				{
					context.ReportDiagnostic(Diagnostic.Create(
						Descriptors.QueryPropertyConflictsWithIQueryAttributable,
						type.Locations[0],
						type.Name));
				}

				bool IsQueryProperty(AttributeData attribute)
					=> attribute.AttributeClass?.InheritsFrom(queryPropertyAttribute, context: null) == true;
			}, SymbolKind.NamedType);
		});
	}
}
