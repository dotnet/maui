using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;


namespace Microsoft.Maui.Controls.SourceGen;

class SourceGenContext (IndentedTextWriter writer, Compilation compilation, SourceProductionContext sourceProductionContext, AssemblyCaches assemblyCaches, IDictionary<XmlType, ITypeSymbol> typeCache, ITypeSymbol rootType, ITypeSymbol? baseType)
{
	public  SourceProductionContext SourceProductionContext => sourceProductionContext;
	public IndentedTextWriter Writer => writer;
    public IList<TextWriter> AddtitionalWriters { get; } = [];
    public Compilation Compilation => compilation;
    public AssemblyCaches XmlnsCache => assemblyCaches;
    public ITypeSymbol RootType => rootType;
    public IDictionary<XmlType, ITypeSymbol> TypeCache => typeCache;
    public IDictionary<INode, object> Values { get; } = new Dictionary<INode, object>();
    public IDictionary<INode, LocalVariable> Variables { get; } = new Dictionary<INode, LocalVariable>();
    public void ReportDiagnostic(Diagnostic diagnostic) => sourceProductionContext.ReportDiagnostic(diagnostic);
    public string? FilePath {get; set;}
	public IDictionary<INode, LocalVariable> ServiceProviders { get; } = new Dictionary<INode, LocalVariable>();
	public IDictionary<INode, (LocalVariable namescope, IList<string> namesInScope)> Scopes = new Dictionary<INode, (LocalVariable, IList<string>)>(); 
    public SourceGenContext? ParentContext {get;set;}
	public ITypeSymbol? BaseType { get; } = baseType;
	public IDictionary<INode, ITypeSymbol> Types { get; } = new Dictionary<INode, ITypeSymbol>();
	public IDictionary<LocalVariable, HashSet<string>> KeysInRD { get; } = new Dictionary<LocalVariable, HashSet<string>>();
	public IDictionary<(LocalVariable, IFieldSymbol?, IPropertySymbol?), LocalVariable> VariablesProperties { get; } = new Dictionary<(LocalVariable, IFieldSymbol?, IPropertySymbol?), LocalVariable>();
	public IList<string> LocalMethods { get; } = new List<string>();
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
}
