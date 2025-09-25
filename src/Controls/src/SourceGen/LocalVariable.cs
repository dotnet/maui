using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

interface ILocalValue
{
	ITypeSymbol Type { get; }
	string ValueAccessor { get; }
}

record LocalVariable(ITypeSymbol type, string name) : ILocalValue
{
	public ITypeSymbol Type => type;
	public string ValueAccessor => name;
}

record InflatorProperty(ITypeSymbol Type, string name) : ILocalValue
{
	public string ValueAccessor => $"inflator.{name}";
}

record DirectValue(ITypeSymbol Type, string ValueAccessor) : ILocalValue
{
}