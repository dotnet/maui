using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Compares compilations by their public API signatures (types, members, method signatures).
/// Implementation changes (method bodies) are ignored, so editing code inside a method
/// won't trigger XAML regeneration.
/// 
/// This is slower than a references-only comparer (~1ms for 100 files vs ~0.001ms)
/// but avoids regenerating all XAML files on every C# keystroke while still detecting
/// signature changes that could affect XAML (new types, changed members, etc.).
/// 
/// Performance characteristics:
/// - 10 files: ~0.1ms per comparison
/// - 50 files: ~0.9ms per comparison  
/// - 100 files: ~1.1ms per comparison
/// 
/// The comparer triggers XAML regeneration when:
/// - A new type is added or removed
/// - A type's base class or interfaces change
/// - A public/internal member is added, removed, or has its signature changed
/// - External references change
/// 
/// The comparer does NOT trigger regeneration for:
/// - Method body changes (implementation details)
/// - Comment changes
/// - Whitespace changes
/// - Private member changes
/// </summary>
class CompilationSignaturesComparer(bool includePrivateMembers = false) : IEqualityComparer<Compilation>
{
	public bool Equals(Compilation x, Compilation y)
	{
		if (ReferenceEquals(x, y))
			return true;

		if (x.AssemblyName != y.AssemblyName
			|| x.ExternalReferences.Length != y.ExternalReferences.Length)
			return false;

		if (!x.ExternalReferences.SequenceEqual(y.ExternalReferences))
			return false;

		// Compare type signatures (ignoring method implementations)
		return GetSignatureString(x, includePrivateMembers) == GetSignatureString(y, includePrivateMembers);
	}

	private static string GetSignatureString(Compilation compilation, bool includePrivateMembers)
	{
		var sb = new StringBuilder();
		AppendAttributes(sb, compilation.Assembly.GetAttributes());
		AppendNamespace(sb, compilation.Assembly.GlobalNamespace, includePrivateMembers);
		return sb.ToString();
	}

	private static void AppendAttributes(StringBuilder sb, IEnumerable<AttributeData> attributes)
	{
		foreach (var attribute in attributes.Select(a => a.ToString()).OrderBy(a => a, StringComparer.Ordinal))
			sb.Append('[').Append(attribute).Append(']');
	}

	private static void AppendNamespace(StringBuilder sb, INamespaceSymbol ns, bool includePrivateMembers)
	{
		foreach (var type in ns.GetTypeMembers().OrderBy(t => t.Name))
			AppendType(sb, type, includePrivateMembers);
		foreach (var child in ns.GetNamespaceMembers().OrderBy(n => n.Name))
			AppendNamespace(sb, child, includePrivateMembers);
	}

	private static void AppendType(StringBuilder sb, INamedTypeSymbol type, bool includePrivateMembers)
	{
		sb.Append(type.DeclaredAccessibility).Append(' ');
		sb.Append(type.TypeKind).Append(' ');
		if (type.IsStatic)
			sb.Append("static ");
		sb.Append(type.ToFQDisplayString());
		AppendAttributes(sb, type.GetAttributes());

		if (type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object)
			sb.Append(':').Append(type.BaseType.ToFQDisplayString());

		foreach (var iface in type.Interfaces.OrderBy(i => i.ToFQDisplayString()))
			sb.Append(',').Append(iface.ToFQDisplayString());

		sb.Append('{');

		foreach (var member in type.GetMembers()
			.Where(m => (includePrivateMembers || m.DeclaredAccessibility != Accessibility.Private) && !m.IsImplicitlyDeclared)
			.OrderBy(m => m.Name)
			.ThenBy(m => m.Kind))
		{
			switch (member)
			{
				case IFieldSymbol f:
					sb.Append(f.DeclaredAccessibility).Append(' ');
					if (f.IsStatic)
						sb.Append("static ");
					sb.Append(f.Type.ToFQDisplayString()).Append(' ').Append(f.Name).Append(';');
					break;

				case IPropertySymbol p:
					sb.Append(p.DeclaredAccessibility).Append(' ');
					if (p.IsStatic)
						sb.Append("static ");
					sb.Append(p.Type.ToFQDisplayString()).Append(' ').Append(p.Name);
					AppendAttributes(sb, p.GetAttributes());
					if (p.GetMethod != null)
						sb.Append("{get;}");
					if (p.SetMethod != null)
						sb.Append("{set;}");
					break;

				case IMethodSymbol m when m.MethodKind == MethodKind.Ordinary:
					sb.Append(m.DeclaredAccessibility).Append(' ');
					if (m.IsStatic)
						sb.Append("static ");
					sb.Append(m.ReturnType.ToFQDisplayString()).Append(' ').Append(m.Name);
					AppendAttributes(sb, m.GetAttributes());
					sb.Append('(');
					sb.Append(string.Join(",", m.Parameters.Select(p => p.Type.ToFQDisplayString())));
					sb.Append(')').Append(';');
					break;

				case IMethodSymbol m when m.MethodKind == MethodKind.Constructor:
					sb.Append(m.DeclaredAccessibility).Append(" .ctor(");
					foreach (var parameter in m.Parameters)
					{
						sb.Append(parameter.Type.ToFQDisplayString()).Append(' ').Append(parameter.Name);
						AppendAttributes(sb, parameter.GetAttributes());
						sb.Append(',');
					}
					sb.Append(");");
					break;

				case INamedTypeSymbol nested:
					AppendType(sb, nested, includePrivateMembers);
					break;
			}
		}

		sb.Append('}');
	}

	public int GetHashCode(Compilation obj) => obj.References.GetHashCode();
}
