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
	
	public IndentedTextWriter? RefStructWriter { get; set; }

	public Compilation Compilation => compilation;
	public AssemblyCaches XmlnsCache => assemblyCaches;
	public ITypeSymbol RootType => rootType;
	public IDictionary<XmlType, ITypeSymbol> TypeCache => typeCache;
	public IDictionary<INode, object> Values { get; } = new Dictionary<INode, object>();
	public IDictionary<INode, ILocalValue> Variables { get; } = new Dictionary<INode, ILocalValue>();
	public void ReportDiagnostic(Diagnostic diagnostic) => sourceProductionContext.ReportDiagnostic(diagnostic);
	public IDictionary<INode, ILocalValue> ServiceProviders { get; } = new Dictionary<INode, ILocalValue>();
	public IDictionary<INode, (ILocalValue namescope, IDictionary<string, ILocalValue> namesInScope)> Scopes = new Dictionary<INode, (ILocalValue, IDictionary<string, ILocalValue>)>();
	public SourceGenContext? ParentContext { get; set; }
	public ITypeSymbol? BaseType { get; } = baseType;
	public IDictionary<INode, ITypeSymbol> Types { get; } = new Dictionary<INode, ITypeSymbol>();
	public IDictionary<ILocalValue, HashSet<string>> KeysInRD { get; } = new Dictionary<ILocalValue, HashSet<string>>();
	public IDictionary<(ILocalValue, IFieldSymbol?, IPropertySymbol?), ILocalValue> VariablesProperties { get; } = new Dictionary<(ILocalValue, IFieldSymbol?, IPropertySymbol?), ILocalValue>();
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
