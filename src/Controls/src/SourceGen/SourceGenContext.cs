using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.NodeSGExtensions;


namespace Microsoft.Maui.Controls.SourceGen;

class SourceGenContext(IndentedTextWriter writer, Compilation compilation, SourceProductionContext sourceProductionContext, AssemblyCaches assemblyCaches, IDictionary<XmlType, ITypeSymbol> typeCache, ITypeSymbol rootType, ITypeSymbol? baseType, ProjectItem projectItem)
{
	public SourceProductionContext SourceProductionContext => sourceProductionContext;
	public IndentedTextWriter Writer => writer;
	public IList<TextWriter> AddtitionalWriters { get; } = [];
	public Compilation Compilation => compilation;
	public AssemblyCaches XmlnsCache => assemblyCaches;
	public ITypeSymbol RootType => rootType;
	public IDictionary<XmlType, ITypeSymbol> TypeCache => typeCache;
	public IDictionary<INode, object> Values { get; } = new Dictionary<INode, object>();
	public IDictionary<INode, LocalVariable> Variables { get; } = new Dictionary<INode, LocalVariable>();
	public void ReportDiagnostic(Diagnostic diagnostic) => sourceProductionContext.ReportDiagnostic(diagnostic);
	public IDictionary<INode, LocalVariable> ServiceProviders { get; } = new Dictionary<INode, LocalVariable>();
	public IDictionary<INode, (LocalVariable namescope, IDictionary<string, LocalVariable> namesInScope)> Scopes = new Dictionary<INode, (LocalVariable, IDictionary<string, LocalVariable>)>();
	public SourceGenContext? ParentContext { get; set; }
	public ITypeSymbol? BaseType { get; } = baseType;
	public IDictionary<INode, ITypeSymbol> Types { get; } = new Dictionary<INode, ITypeSymbol>();
	public IDictionary<LocalVariable, HashSet<string>> KeysInRD { get; } = new Dictionary<LocalVariable, HashSet<string>>();
	public IDictionary<(LocalVariable, IFieldSymbol?, IPropertySymbol?), LocalVariable> VariablesProperties { get; } = new Dictionary<(LocalVariable, IFieldSymbol?, IPropertySymbol?), LocalVariable>();
	public IList<string> LocalMethods { get; } = new List<string>();
	public ProjectItem ProjectItem { get; } = projectItem;

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

	internal Dictionary<ITypeSymbol, (ConverterDelegate, ITypeSymbol)>? knownSGTypeConverters;
	internal Dictionary<ITypeSymbol, ProvideValueDelegate>? knownSGValueProviders;
	internal Dictionary<ITypeSymbol, ProvideValueDelegate>? knownSGEarlyMarkupExtensions;
	internal Dictionary<ITypeSymbol, ProvideValueDelegate>? knownSGLateMarkupExtensions;
}
