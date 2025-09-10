using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

class DependencyFirstInflator
{
	record LazyLocalVariable(string name, ITypeSymbol Type, ITypeSymbol? LazyType = null) : ILocalVariable
	{
		public bool IsLazy => LazyType != null;

		public string Name => IsLazy ? $"{name}.Value" : name;
	}

	static readonly IList<XmlName> skips = [
		XmlName.xArguments,
		XmlName.xClass,
		XmlName.xDataType,
		XmlName.xFactoryMethod,
		XmlName.xFieldModifier,
		XmlName.xKey,
		XmlName.xName,
		XmlName.xTypeArguments,
	];

	public void Inflate(SourceGenContext context, IElementNode root, IndentedTextWriter writer)
	{
		Dictionary<IElementNode, LazyLocalVariable> deferredVariables = [];
		var thisVar = new LazyLocalVariable("this", (INamedTypeSymbol)context.RootType);

		//where we declare the lazies
		var declarationWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writer.Indent };

		//where we create the lazies, and set he creator func
		var assignmentWriters = new List<IndentedTextWriter>();

		var setXNamesWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writer.Indent };

		//where we set the values on the current object
		var setValueWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writer.Indent };

		//collects all the variables to create, collect declaration, assignment, and usage
		//if it needs to creates lazy variables, it will declare them on the declarationWriter, and assign them on a new assignmentWriter
		SetValuesForNode(root, (declarationWriter, assignmentWriters, setValueWriter, writer.Indent), thisVar, context, deferredVariables);

		//write declarations
		writer.Append(declarationWriter, noTabs: true);

		//write the SetXName calls
		writer.Append(setXNamesWriter, noTabs: true);

		//write the assignments
		foreach (var assignmentWriter in assignmentWriters)
			writer.Append(assignmentWriter, noTabs: true);

		//write the SetValue calls
		writer.Append(setValueWriter, noTabs: true);
	}

	void SetValuesForNode(IElementNode node, (IndentedTextWriter Declaration, IList<IndentedTextWriter> Assignments, IndentedTextWriter SetValue, int Indent) writers, ILocalVariable parentVar, SourceGenContext context, Dictionary<IElementNode, LazyLocalVariable> deferredVariables)
	{
		foreach (var prop in node.Properties)
		{
			if (skips.Contains(prop.Key))
				continue;

			SetPropertyValue(prop, writers, parentVar, context, deferredVariables);
		}

		var contentPropertyName = node.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache)?.GetContentPropertyName(context);
		if (node.CollectionItems != null && node.CollectionItems.Count > 0 && contentPropertyName != null)
		{
			foreach (var child in node.CollectionItems)
			{
				var prop = new KeyValuePair<XmlName, INode>(new XmlName(null, contentPropertyName), child);

				SetPropertyValue(prop, writers, parentVar, context, deferredVariables);
			}
		}
	}

	void SetPropertyValue(KeyValuePair<XmlName, INode> prop, (IndentedTextWriter Declaration, IList<IndentedTextWriter> Assignments, IndentedTextWriter SetValue, int Indent) writers, ILocalVariable parentVar, SourceGenContext context, Dictionary<IElementNode, LazyLocalVariable> deferredVariables)
	{
		if (!CanBeSetDirectly(prop, context))
		{
			if (prop.Value is not IElementNode elementNode)
				throw new NotImplementedException();
			if (elementNode.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache) is not INamedTypeSymbol type)
				throw new NotImplementedException();
			var name = NamingHelpers.CreateUniqueVariableName(context, type!);
			var localVariable = new LazyLocalVariable(name, type!, context.Compilation.GetTypeByMetadataName("System.Lazy`1")!.Construct(type!));
			context.Variables.Add(elementNode, localVariable);
			deferredVariables.Add(elementNode, localVariable);

			writers.Declaration.WriteLine($"{localVariable.LazyType!.ToFQDisplayString()} {name};");

			var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writers.Indent };
			using (PrePost.NewBlock(writer, $"{name} = new {localVariable.LazyType!.ToFQDisplayString()}(() => {{", "});"))
			{

				CreateValuesVisitor.CreateValue((ElementNode)elementNode, writer, context.Variables, context.Compilation, context.XmlnsCache, context);
				var localVar = context.Variables[elementNode];

				//set properties of the created object
				SetValuesForNode(elementNode, (writers.Declaration, writers.Assignments, writer, writers.Indent), localVar, context, deferredVariables);
				writer.WriteLine($"return {localVar.Name};");
			}
			writers.Assignments.Add(writer);
		}
		SetPropertyHelpers.SetPropertyValue(writers.SetValue, parentVar, prop.Key, prop.Value, context);

	}

	bool CanBeSetDirectly(KeyValuePair<XmlName, INode> prop, SourceGenContext context)
	{
		if (prop.Value is ValueNode)
			return true;

		return false;
	}
}
