using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;


namespace Microsoft.Maui.Controls.SourceGen;

class SourceGenContext (IndentedTextWriter writer, Compilation compilation, AssemblyCaches assemblyCaches, IDictionary<XmlType, string> typeCache, ITypeSymbol rootType)
{
    public IndentedTextWriter Writer => writer;
    public Compilation Compilation => compilation;
    public AssemblyCaches XmlnsCache => assemblyCaches;
    public ITypeSymbol RootType => rootType;
    public IDictionary<XmlType, string> TypeCache => typeCache;
    public IDictionary<INode, object> Values { get; } = new Dictionary<INode, object>();
    public IDictionary<INode, LocalVariable> Variables { get; } = new Dictionary<INode, LocalVariable>();
}
