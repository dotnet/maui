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
	public IDictionary<INode, ILocalVariable> Variables { get; } = new Dictionary<INode, ILocalVariable>();
	public void ReportDiagnostic(Diagnostic diagnostic) => sourceProductionContext.ReportDiagnostic(diagnostic);
	public IDictionary<INode, ILocalVariable> ServiceProviders { get; } = new Dictionary<INode, ILocalVariable>();
	public IDictionary<INode, (ILocalVariable namescope, IDictionary<string, ILocalVariable> namesInScope)> Scopes = new Dictionary<INode, (ILocalVariable, IDictionary<string, ILocalVariable>)>();
	public SourceGenContext? ParentContext { get; set; }
	public ITypeSymbol? BaseType { get; } = baseType;
	public IDictionary<INode, ITypeSymbol> Types { get; } = new Dictionary<INode, ITypeSymbol>();
	public IDictionary<ILocalVariable, HashSet<string>> KeysInRD { get; } = new Dictionary<ILocalVariable, HashSet<string>>();
	public IDictionary<(ILocalVariable, IFieldSymbol?, IPropertySymbol?), ILocalVariable> VariablesProperties { get; } = new Dictionary<(ILocalVariable, IFieldSymbol?, IPropertySymbol?), ILocalVariable>();
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
