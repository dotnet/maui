using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

interface ILocalVariable
{
	ITypeSymbol Type { get; }
	string Name { get; }
}

record LocalVariable(ITypeSymbol type, string name) : ILocalVariable
{
	public ITypeSymbol Type => type;
	public string Name => name;
}