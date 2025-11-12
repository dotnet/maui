using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.Maui.Controls.SourceGen;


interface ILocalValue
{
	ITypeSymbol Type { get; }
	string ValueAccessor { get; }
}

//FIXME: all variable should be scoped
record LocalVariable(ITypeSymbol Type, string Name) : ILocalValue
{
	public string ValueAccessor => Name;
}

record ScopedVariable(ITypeSymbol Type, string Name, Scope Scope) : ILocalValue
{
	public string ValueAccessor
	{
		get
		{
			if (this is ThisValue)
				return "this";
			throw new InvalidOperationException("ScopedVariable should be AccessedFrom(scope).ValueAccessor");
		}
	}
}

record ThisValue(ITypeSymbol Type, Scope Scope) : ScopedVariable(Type, "this", Scope)
{
}

record DirectValue(ITypeSymbol Type, string ValueAccessor) : ILocalValue
{
}


static class ILocalValueExtensions
{
	public static ILocalValue Descope(this ILocalValue value) =>
		value switch
		{
			ScopedVariable sv => new DirectValue(sv.Type, sv.Name),
			_ => value
		};

	public static ILocalValue Scope(this ILocalValue value, Scope scope) =>
	value switch
	{
		ScopedVariable sv => throw new InvalidOperationException("Already scoped"),
		ILocalValue lv => new ScopedVariable(lv.Type, lv.ValueAccessor, scope),
	};

	public static ILocalValue AccessedFrom(this ILocalValue value, ImmutableArray<Scope>? scopes) => value switch
	{
		ScopedVariable sv => scopes != null ? sv.AccessedFromInternal(scopes.Value) : value,
		_ => value
	};

	static ILocalValue AccessedFromInternal(this ScopedVariable value, ImmutableArray<Scope> scopes)
	{
		if (value.TryAccessFrom(scopes, out var accessed))
			return accessed!;
		throw new NotImplementedException();
	}

	static bool TryAccessFrom(this ScopedVariable value, ImmutableArray<Scope> scopes, out ILocalValue? accessed)
	{
		var scope = scopes[scopes.Length - 1];
		if (value.Scope == scope)
		{
			accessed = new DirectValue(value.Type, value.Name);
			return true;
		}

		if (scope is InitializeComponentScope ics && ics.InflatorScope.scope == value.Scope)
		{
			accessed = new DirectValue(value.Type, $"{ics.InflatorScope.accessor}.{value.Name}");
			return true;
		}

		if (scope is StaticMethodScope sms && sms.InflatorScope.scope == value.Scope)
		{
			accessed = new DirectValue(value.Type, $"{sms.InflatorScope.accessor}.{value.Name}");
			return true;
		}
    
		if (scope is InitializeComponentScope || scope is PropertyScope)
			return value.TryAccessFrom(scopes.RemoveAt(scopes.Length - 1), out accessed);

		accessed = null;
		return false;		
	}
}