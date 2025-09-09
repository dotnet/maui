using System.Collections.Generic;
using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Microsoft.Maui.Controls.SourceGen;

abstract record Scope(IndentedTextWriter Writer);

record TypeScope(IndentedTextWriter Writer, INamedTypeSymbol Type) : Scope(Writer);
record InitializeComponentScope(IndentedTextWriter Writer, (string accessor, InflatorScope scope) InflatorScope) : Scope(Writer);
record InflatorScope(IndentedTextWriter Writer, string Type) : Scope(Writer)
{
	public InflatorScope(IndentedTextWriter Writer, string Type, (string, ImmutableArray<Scope>) InitializeComponentScope) : this(Writer, Type) 
		=> this.InitializeComponentScope = InitializeComponentScope;

	public (string accessor, ImmutableArray<Scope> scopes) InitializeComponentScope { get; set; }
}

record PropertyScope(IndentedTextWriter Writer, ITypeSymbol PropertyType, string PropertyName) : Scope(Writer);
record StaticMethodScope(IndentedTextWriter Writer, (string accessor, InflatorScope scope) InflatorScope) : Scope(Writer);

static class ScopeExtensions
{
	public static InflatorScope GetInflatorScope(this ImmutableArray<Scope> scopes)
	{
		if (scopes.Length > 0 && scopes[scopes.Length - 1] is InflatorScope inflatorScope)
			return inflatorScope;
		if (scopes.Length > 1 && scopes[scopes.Length - 2] is InflatorScope inflatorScope1)
			return inflatorScope1;
		if (scopes.Length > 2 && scopes[scopes.Length - 3] is InflatorScope inflatorScope2)
			return inflatorScope2;
		throw new InvalidOperationException("No parent scope for inflator scoped var");
	}
}