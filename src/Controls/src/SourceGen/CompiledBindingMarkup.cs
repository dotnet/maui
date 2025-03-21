using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.BindingSourceGen;

namespace Microsoft.Maui.Controls.SourceGen;

internal struct CompiledBindingMarkup
{
	private readonly SourceGenContext _context;
	private readonly IElementNode _node;
	private readonly string _path;
	private readonly LocalVariable _bindingExtension;

	public CompiledBindingMarkup(IElementNode node, string path, LocalVariable bindingExtension, SourceGenContext context)
	{
		_context = context;
		_node = node;
		_path = path;
		_bindingExtension = bindingExtension;
	}

	private Location GetLocation(INode node)
		=> LocationHelpers.LocationCreate(_context.FilePath!, (IXmlLineInfo)node, "x:DataType");

	public bool TryCompileBinding(ITypeSymbol sourceType, out string? newBindingExpression)
	{
		newBindingExpression = null;

		if (!TryParsePath(
			sourceType,
			out ITypeSymbol? propertyType,
			out SetterOptions? setterOptions,
			out EquatableArray<IPathPart>? parsedPath)
			|| propertyType is null
			|| setterOptions is null
			|| !parsedPath.HasValue)
		{
			return false;
		}

		var binding = new BindingInvocationDescription(
			InterceptableLocation: null,
			SimpleLocation: null,
			SourceType: sourceType.CreateTypeDescription(enabledNullable: true),
			PropertyType: propertyType.CreateTypeDescription(enabledNullable: true),
			Path: parsedPath.Value,
			SetterOptions: setterOptions,
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.Create,
			IsPublic: false,
			RequiresAllUnsafeGetters: true);

		LocalVariable extVariable;
		if (!_context.Variables.TryGetValue(_node, out extVariable))
		{
			throw new Exception("BindingExtension not found"); // TODO report diagnostic
		}

		var methodName = $"CreateTypedBindingFrom_{_bindingExtension.Name}";

		// TODO emit #line info?
		// TODO move ShouldUseSetter methods to shared code (public in MAUI?) + the same for the BindingSourceGen?

		var createBindingLocalMethod = $$"""

			static global::Microsoft.Maui.Controls.BindingBase {{methodName}}(global::Microsoft.Maui.Controls.Xaml.BindingExtension extension)
			{
				global::System.Func<{{binding.SourceType}}, {{binding.PropertyType}}> getter = {{GenerateGetterLambda(binding.Path)}};

				return Create(getter, extension.Mode, extension.Converter, extension.ConverterParameter,
					extension.StringFormat, extension.Source, extension.FallbackValue, extension.TargetNullValue);

				{{BindingCodeWriter.GenerateBindingMethod(binding)}}

				[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
				static bool ShouldUseSetter(global::Microsoft.Maui.Controls.BindingMode mode)
					=> mode == global::Microsoft.Maui.Controls.BindingMode.OneWayToSource
						|| mode == global::Microsoft.Maui.Controls.BindingMode.TwoWay
						|| mode == global::Microsoft.Maui.Controls.BindingMode.Default;
			}
			""";

		_context.AddLocalMethod(createBindingLocalMethod);
		newBindingExpression = $"{methodName}({extVariable.Name})";

		return true;
	}

	bool TryParsePath(
		ITypeSymbol sourceType,
		out ITypeSymbol? propertyType,
		out SetterOptions? setterOptions,
		out EquatableArray<IPathPart>? bindingPath)
	{
		propertyType = sourceType;
		setterOptions = null;
		bindingPath = default;

		var path = _path.Trim('.', ' ');
		var parts = path.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
		var bindingPathParts = new List<IPathPart>();

		var previousPartType = sourceType;
		foreach (var part in parts)
		{
			var p = part;
			string? indexArg = null;
			object? index = null;

			var lbIndex = part.IndexOf('[');
			if (lbIndex != -1)
			{
				var rbIndex = p.IndexOf(']', lbIndex);
				if (rbIndex == -1)
				{
					return false; // TODO report diagnostic
				}

				var argLength = rbIndex - lbIndex - 1;
				if (argLength == 0)
				{
					return false; // TODO report diagnostic
				}

				indexArg = p.Substring(lbIndex + 1, argLength);
				if (indexArg.Length == 0)
				{
					return false; // TODO report diagnostic
				}

				p = p.Substring(0, lbIndex);
				p = p.Trim();
			}

			if (p.Length > 0)
			{
				var property = previousPartType.GetAllProperties(p, _context).FirstOrDefault(property => property.GetMethod != null && !property.GetMethod.IsStatic);
				if (property is null)
				{
					return false; // TODO report diagnostic
				}

				bindingPathParts.Add(new MemberAccess(p, property.Type.IsValueType));
				previousPartType = property.Type;

				setterOptions = new SetterOptions(
					IsWritable: property.SetMethod != null
						&& property.SetMethod.IsPublic()
						&& !property.SetMethod.IsInitOnly
						&& !property.SetMethod.IsStatic,
					AcceptsNullValue: previousPartType.IsTypeNullable(enabledNullable: true));
			}

			if (indexArg != null)
			{
				var defaultMemberAttribute = previousPartType.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToFQDisplayString() == "global::System.Reflection.DefaultMemberAttribute");
				var indexerName = defaultMemberAttribute?.ConstructorArguments.FirstOrDefault().Value as string ?? "Item";

				index = indexArg;

				IPropertySymbol? indexer = null;
				if (int.TryParse(indexArg, out int indexArgInt))
				{
					index = indexArgInt;
					indexer = previousPartType
						.GetAllProperties(indexerName, _context)
						.FirstOrDefault(property =>
							property.GetMethod != null
							&& !property.GetMethod.IsStatic
							&& property.Parameters.Length == 1
							&& property.Parameters[0].Type.SpecialType == SpecialType.System_Int32);
				}
				
				indexer ??= previousPartType
					.GetAllProperties(indexerName, _context)
					.FirstOrDefault(property =>
						property.GetMethod != null
						&& !property.GetMethod.IsStatic
						&& property.Parameters.Length == 1
						&& property.Parameters[0].Type.SpecialType == SpecialType.System_String);

				indexer ??= previousPartType
					.GetAllProperties(indexerName, _context)
					.FirstOrDefault(property =>
						property.GetMethod != null
						&& !property.GetMethod.IsStatic
						&& property.Parameters.Length == 1
						&& property.Parameters[0].Type.SpecialType == SpecialType.System_Object);

				if (indexer is not null)
				{
					var indexAccess = new IndexAccess(indexerName, index, indexer.Type.IsValueType);
					bindingPathParts.Add(indexAccess);

					previousPartType = indexer.Type;

					setterOptions = new SetterOptions(
						IsWritable: indexer.SetMethod != null && indexer.SetMethod.IsPublic() && !indexer.SetMethod.IsStatic,
						AcceptsNullValue: previousPartType.IsTypeNullable(enabledNullable: true));
				}
				else if (previousPartType is IArrayTypeSymbol arrayType)
				{
					var indexAccess = new IndexAccess("", index, arrayType.ElementType.IsValueType);
					bindingPathParts.Add(indexAccess);

					previousPartType = arrayType.ElementType;

					setterOptions = new SetterOptions(
						IsWritable: true, // TODO is this correct?
						AcceptsNullValue: previousPartType.IsTypeNullable(enabledNullable: true));
				}
				else
				{
					return false; // TODO report diagnostic
				}
			}
		}

		propertyType = previousPartType;
		bindingPath = new EquatableArray<IPathPart>(bindingPathParts.ToArray());
		return true;
	}

	string GenerateGetterLambda(EquatableArray<IPathPart> bindingPath)
	{
		string expression = "source";
		bool forceConditionalAccessToNextPart = false;

		foreach (var part in bindingPath)
		{
			// Note: AccessExpressionBuilder will happily call unsafe accessors and it expects them to be available.
			// By calling BindingCodeWriter.GenerateBindingMethod(...), we are ensuring that the unsafe accessors are available.
			expression = AccessExpressionBuilder.ExtendExpression(expression, MaybeWrapInConditionalAccess(part, forceConditionalAccessToNextPart));
			forceConditionalAccessToNextPart = part is Cast;
		}

		return $"static source => {expression}";

		static IPathPart MaybeWrapInConditionalAccess(IPathPart part, bool forceConditionalAccessToNextPart)
			=> (forceConditionalAccessToNextPart, part) switch
			{
				(true, MemberAccess memberAccess) => new ConditionalAccess(memberAccess),
				(true, IndexAccess indexAccess) => new ConditionalAccess(indexAccess),
				_ => part,
			};
	}
}
