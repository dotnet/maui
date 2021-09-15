using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.SourceGen
{
	public static class SyntaxNodeExtensions
	{
		public static bool TryGetParentSyntax<T>(this SyntaxNode? syntaxNode, out T? result)
			where T : SyntaxNode
		{
			// set defaults
			result = null;

			if (syntaxNode == null)
				return false;

			try
			{
				syntaxNode = syntaxNode.Parent;

				if (syntaxNode == null)
					return false;

				if (syntaxNode.GetType() == typeof(T))
				{
					result = syntaxNode as T;
					return true;
				}

				return TryGetParentSyntax<T>(syntaxNode, out result);
			}
			catch
			{
				return false;
			}
		}
	}

	public static class TypeDeclarationSyntaxExtensions
	{
		const char NESTED_CLASS_DELIMITER = '+';
		const char NAMESPACE_CLASS_DELIMITER = '.';
		const char TYPEPARAMETER_CLASS_DELIMITER = '`';

		public static TypeInfoParts GetTypeInfoParts(this TypeDeclarationSyntax source)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));

			var namespaces = new LinkedList<NamespaceDeclarationSyntax>();
			var types = new LinkedList<TypeDeclarationSyntax>();
			for (var parent = source.Parent; parent is object; parent = parent.Parent)
			{
				if (parent is NamespaceDeclarationSyntax @namespace)
					namespaces.AddFirst(@namespace);
				else if (parent is TypeDeclarationSyntax type)
					types.AddFirst(type);
			}

			var info = new TypeInfoParts();
			info.NamespaceParts = namespaces.Select(n => n.Name.ToString()).ToArray();
			info.TypeNameParts = types.Select(n => n.Identifier.Text)
				.Concat(new[] { source.Identifier.Text }).ToArray();

			var typeArguments = source.TypeParameterList?.ChildNodes()
				.Count(node => node is TypeParameterSyntax) ?? 0;
			if (typeArguments != 0)
				info.TypeArgsCount = typeArguments;

			return info;
		}

		public static string GetNamespace(this TypeDeclarationSyntax source)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));

			var namespaces = new LinkedList<NamespaceDeclarationSyntax>();
			for (var parent = source.Parent; parent is object; parent = parent.Parent)
			{
				if (parent is NamespaceDeclarationSyntax @namespace)
				{
					namespaces.AddFirst(@namespace);
				}
			}

			var result = new StringBuilder();
			for (var item = namespaces.First; item is object; item = item.Next)
			{
				result.Append(item.Value.Name).Append(NAMESPACE_CLASS_DELIMITER);
			}

			return result.ToString().TrimEnd('.');
		}

		public static string GetFullName(this TypeDeclarationSyntax source)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));

			var namespaces = new LinkedList<NamespaceDeclarationSyntax>();
			var types = new LinkedList<TypeDeclarationSyntax>();
			for (var parent = source.Parent; parent is object; parent = parent.Parent)
			{
				if (parent is NamespaceDeclarationSyntax @namespace)
				{
					namespaces.AddFirst(@namespace);
				}
				else if (parent is TypeDeclarationSyntax type)
				{
					types.AddFirst(type);
				}
			}

			var result = new StringBuilder();
			for (var item = namespaces.First; item is object; item = item.Next)
			{
				result.Append(item.Value.Name).Append(NAMESPACE_CLASS_DELIMITER);
			}
			for (var item = types.First; item is object; item = item.Next)
			{
				var type = item.Value;
				AppendName(result, type);
				result.Append(NESTED_CLASS_DELIMITER);
			}
			AppendName(result, source);

			return result.ToString();
		}

		static void AppendName(StringBuilder builder, TypeDeclarationSyntax type)
		{
			builder.Append(type.Identifier.Text);
			var typeArguments = type.TypeParameterList?.ChildNodes()
				.Count(node => node is TypeParameterSyntax) ?? 0;
			if (typeArguments != 0)
				builder.Append(TYPEPARAMETER_CLASS_DELIMITER).Append(typeArguments);
		}
	}
}
