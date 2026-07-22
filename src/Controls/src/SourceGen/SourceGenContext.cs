using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.NodeSGExtensions;


namespace Microsoft.Maui.Controls.SourceGen;

class SourceGenContext(IndentedTextWriter writer, Compilation compilation, SourceProductionContext sourceProductionContext, AssemblyAttributes assemblyCaches, IDictionary<XmlType, INamedTypeSymbol> typeCache, ITypeSymbol rootType, ITypeSymbol? baseType, ProjectItem projectItem, Action<Diagnostic>? diagnosticReporter = null)
{
	List<Diagnostic>? _bufferedDiagnostics;

	internal static SourceGenContext CreateNewForTests(Action<Diagnostic>? diagnosticReporter = null) => new SourceGenContext(
		null!,
		null!,
		default,
		null!,
		new Dictionary<XmlType, INamedTypeSymbol>(),
		null!,
		null,
		null!,
		diagnosticReporter);

	public SourceProductionContext SourceProductionContext => sourceProductionContext;
	public IndentedTextWriter Writer => writer;

	public IndentedTextWriter? RefStructWriter { get; set; }

	public Compilation Compilation => compilation;
	public AssemblyAttributes XmlnsCache => assemblyCaches;
	public ITypeSymbol RootType => rootType;
	public IDictionary<XmlType, INamedTypeSymbol> TypeCache => typeCache;
	public IDictionary<INode, object> Values { get; } = new Dictionary<INode, object>();
	public IDictionary<INode, ILocalValue> Variables { get; } = new Dictionary<INode, ILocalValue>();
	public void ReportDiagnostic(Diagnostic diagnostic)
	{
		if (ParentContext is not null)
		{
			ParentContext.ReportDiagnostic(diagnostic);
			return;
		}

		// Check if this diagnostic should be suppressed based on NoWarn
		var noWarn = ProjectItem?.NoWarn;
		if (!string.IsNullOrEmpty(noWarn))
		{
			var suppressedIds = noWarn!.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var id in suppressedIds)
			{
				var code = id.Trim();
				// Match full ID (e.g., "MAUIX2015") or bare numeric suffix (e.g., "2015")
				if (code.Equals(diagnostic.Id, StringComparison.OrdinalIgnoreCase) ||
					(diagnostic.Id.StartsWith("MAUIX", StringComparison.OrdinalIgnoreCase) &&
					 code == diagnostic.Id.Substring("MAUIX".Length)))
				{
					return; // Suppress this diagnostic
				}
			}
		}

		if (_bufferedDiagnostics is not null)
		{
			_bufferedDiagnostics.Add(diagnostic);
			return;
		}

		ReportDiagnosticCore(diagnostic);
	}

	internal int BufferedDiagnosticCount => _bufferedDiagnostics?.Count ?? 0;

	internal void BeginDiagnosticBuffering()
	{
		if (_bufferedDiagnostics is not null)
			throw new InvalidOperationException("Diagnostic buffering is already active.");

		_bufferedDiagnostics = [];
	}

	internal void FlushBufferedDiagnostics()
	{
		if (_bufferedDiagnostics is null)
			throw new InvalidOperationException("Diagnostic buffering is not active.");

		var diagnostics = _bufferedDiagnostics;
		_bufferedDiagnostics = null;
		foreach (var diagnostic in diagnostics)
			ReportDiagnosticCore(diagnostic);
	}

	internal void DiscardBufferedDiagnostics() => _bufferedDiagnostics = null;

	void ReportDiagnosticCore(Diagnostic diagnostic)
	{
		if (diagnosticReporter is not null)
			diagnosticReporter(diagnostic);
		else
			sourceProductionContext.ReportDiagnostic(diagnostic);
	}

	public IDictionary<INode, ILocalValue> ServiceProviders { get; } = new Dictionary<INode, ILocalValue>();
	public IDictionary<INode, (ILocalValue namescope, IDictionary<string, ILocalValue> namesInScope)> Scopes = new Dictionary<INode, (ILocalValue, IDictionary<string, ILocalValue>)>();
	public SourceGenContext? ParentContext { get; set; }
	public ITypeSymbol? BaseType { get; } = baseType;
	public IDictionary<INode, ITypeSymbol> Types { get; } = new Dictionary<INode, ITypeSymbol>();
	public IDictionary<ILocalValue, HashSet<string>> KeysInRD { get; } = new Dictionary<ILocalValue, HashSet<string>>();
	public IDictionary<(ILocalValue, IFieldSymbol?, IPropertySymbol?), ILocalValue> VariablesProperties { get; } = new Dictionary<(ILocalValue, IFieldSymbol?, IPropertySymbol?), ILocalValue>();
	public IList<string> LocalMethods { get; } = new List<string>();
	public ProjectItem ProjectItem { get; } = projectItem;

	public Dictionary<string, int> lastIdForName = [];

	public void AddLocalMethod(string code)
	{
		if (ParentContext != null)
		{
			ParentContext.AddLocalMethod(code);
		}
		else
		{
			LocalMethods.Add(code);
		}
	}

	readonly HashSet<string> _emittedTemplateMethods = new HashSet<string>();

	// Reserves a generated DataTemplate LoadTemplate method name once per compilation unit, so a
	// template whose value is set more than once in the same scope (e.g. a `required` property set
	// in the object initializer AND as an assignment) emits the local function only once instead of
	// redeclaring it. Returns true the first time a name is seen, false afterwards. See dotnet/maui#36682.
	public bool TryReserveTemplateMethod(string name)
		=> ParentContext != null ? ParentContext.TryReserveTemplateMethod(name) : _emittedTemplateMethods.Add(name);

	internal Dictionary<ITypeSymbol, (ConverterDelegate, ITypeSymbol)>? knownSGTypeConverters;
	internal Dictionary<ITypeSymbol, IKnownMarkupValueProvider>? knownSGValueProviders;
	internal Dictionary<ITypeSymbol, ProvideValueDelegate>? knownSGEarlyMarkupExtensions;
	internal Dictionary<ITypeSymbol, ProvideValueDelegate>? knownSGLateMarkupExtensions;
}
