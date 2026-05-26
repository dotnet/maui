using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

// type name, method name, currrent object accessor (this, local, field, ...), current inflator accessor (inflator, ), parent inflator accessor (__parent, ...)
record ScopeInfo(string type, string? method, string currentObjectAccessor, string currentInflatorAccessor, string? parentAccessor);

interface ILocalValue
{
	ITypeSymbol Type { get; }
	string ValueAccessor { get; }
}

record LocalVariable(ITypeSymbol type, string name) : ILocalValue
{
	public ITypeSymbol Type => type;
	public string ValueAccessor => name;

	public ScopeInfo? Scope { get; internal set; }
}

record ScopedVariable(ITypeSymbol Type, string scope, string name) : ILocalValue
{
	public string ValueAccessor => $"{scope}.{name}";
}

record InflatorScopedVar(ITypeSymbol Type, string name) : ScopedVariable(Type, "inflator", name)
{
	public ScopeInfo? Scope { get; set; }
}

record DirectValue(ITypeSymbol Type, string ValueAccessor) : ILocalValue
{
}

static class ILocalValueExtensions
{
	public static ILocalValue AsInflatorScoped(this ILocalValue value, ScopeInfo? scope = null) =>
		value switch
		{
			LocalVariable lv => new InflatorScopedVar(lv.Type, lv.name) { Scope = scope },
			_ => value
		};

	public static ILocalValue Descoped(this ILocalValue value) =>
		value switch
		{
			ScopedVariable sv => new LocalVariable(sv.Type, sv.name),
			_ => value
		};
}