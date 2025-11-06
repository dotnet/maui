using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.BindingSourceGen;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

internal struct CompiledBindingMarkup
{
	private readonly SourceGenContext _context;
	private readonly ElementNode _node;
	private readonly string _path;
	private readonly ILocalValue _bindingExtension;

	public CompiledBindingMarkup(ElementNode node, string path, ILocalValue bindingExtension, SourceGenContext context)
	{
		_context = context;
		_node = node;
		_path = path;
		_bindingExtension = bindingExtension;
	}

	private Location GetLocation(INode node)
		=> LocationHelpers.LocationCreate(_context.ProjectItem.RelativePath!, (IXmlLineInfo)node, "x:DataType");

	public bool TryCompileBinding(ITypeSymbol sourceType, bool isTemplateBinding, out string? newBindingExpression)
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

		ILocalValue extVariable;
		if (!_context.Variables.TryGetValue(_node, out extVariable))
		{
			throw new Exception("BindingExtension not found"); // TODO report diagnostic
		}

		var methodName = $"CreateTypedBindingFrom_{_bindingExtension.ValueAccessor}";

		// TODO emit #line info?
		// TODO move ShouldUseSetter methods to shared code (public in MAUI?) + the same for the BindingSourceGen?

		var extensionTypeName = isTemplateBinding
			? "global::Microsoft.Maui.Controls.Xaml.TemplateBindingExtension"
			: "global::Microsoft.Maui.Controls.Xaml.BindingExtension";

		// Check which properties were actually set in XAML
		// Only generate code to pass properties that were explicitly set
		var hasMode = _node.Properties.TryGetValue("Mode", out _);
		var hasConverter = _node.Properties.TryGetValue("Converter", out _);
		var hasConverterParameter = _node.Properties.TryGetValue("ConverterParameter", out _);
		var hasStringFormat = _node.Properties.TryGetValue("StringFormat", out _);
		var hasSource = _node.Properties.TryGetValue("Source", out _) || isTemplateBinding;
		var hasFallbackValue = _node.Properties.TryGetValue("FallbackValue", out _);
		var hasTargetNullValue = _node.Properties.TryGetValue("TargetNullValue", out _);

		// Build the flags for which properties to set in the generated Create method
		var propertyFlags = BindingPropertyFlags.None;
		if (hasMode)
			propertyFlags |= BindingPropertyFlags.Mode;
		if (hasConverter)
			propertyFlags |= BindingPropertyFlags.Converter;
		if (hasConverterParameter)
			propertyFlags |= BindingPropertyFlags.ConverterParameter;
		if (hasStringFormat)
			propertyFlags |= BindingPropertyFlags.StringFormat;
		if (hasSource)
			propertyFlags |= BindingPropertyFlags.Source;
		if (hasFallbackValue)
			propertyFlags |= BindingPropertyFlags.FallbackValue;
		if (hasTargetNullValue)
			propertyFlags |= BindingPropertyFlags.TargetNullValue;

		// Build the parameter list for the Create call
		var createParams = new StringBuilder();
		createParams.AppendLine($"getter: {GenerateGetterLambda(binding.Path)},");
		
		if (hasMode)
			createParams.AppendLine("extension.Mode,");
		if (hasConverter)
			createParams.AppendLine("extension.Converter,");
		if (hasConverterParameter)
			createParams.AppendLine("extension.ConverterParameter,");
		if (hasStringFormat)
			createParams.AppendLine("extension.StringFormat,");
		if (hasSource)
		{
			if (isTemplateBinding)
				createParams.AppendLine("source: global::Microsoft.Maui.Controls.RelativeBindingSource.TemplatedParent,");
			else
				createParams.AppendLine("extension.Source,");
		}
		if (hasFallbackValue)
		{
			if (isTemplateBinding)
				createParams.AppendLine("fallbackValue: null,");
			else
				createParams.AppendLine("extension.FallbackValue,");
		}
		if (hasTargetNullValue)
		{
			if (isTemplateBinding)
				createParams.AppendLine("targetNullValue: null,");
			else
				createParams.AppendLine("extension.TargetNullValue,");
		}
		
		// Remove the trailing comma and newline from the last parameter
		var paramStr = createParams.ToString().TrimEnd();
		if (paramStr.EndsWith(","))
			paramStr = paramStr.Substring(0, paramStr.Length - 1);

		var createBindingLocalMethod = $$"""
				static global::Microsoft.Maui.Controls.BindingBase {{methodName}}({{extensionTypeName}} extension)
				{
					return Create(
						{{paramStr}});

				{{BindingCodeWriter.GenerateBindingMethod(binding, methodName: "Create", indent: 1, propertyFlags: propertyFlags)}}

					[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
					static bool ShouldUseSetter(global::Microsoft.Maui.Controls.BindingMode mode)
						=> mode == global::Microsoft.Maui.Controls.BindingMode.OneWayToSource
							|| mode == global::Microsoft.Maui.Controls.BindingMode.TwoWay
							|| mode == global::Microsoft.Maui.Controls.BindingMode.Default;
				}
				""";

		_context.AddLocalMethod(createBindingLocalMethod);
		newBindingExpression = $"{methodName}({extVariable.ValueAccessor})";

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

		var isNullable = false;
		var path = _path.Trim('.', ' ');
		var parts = path.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
		var bindingPathParts = new List<IPathPart>();

		var previousPartType = sourceType;
		var previousPartIsNullable = sourceType.IsTypeNullable(enabledNullable: true);

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

				// TODO: detect if the type is annotated or not
				var enabledNullable = true;
				// var enabledNullable = previousPartType.NullableAnnotation == NullableAnnotation.Annotated
				// 	|| previousPartType.GetAttributes().Any(a => a.AttributeClass?.ToFQDisplayString() == "global::System.Runtime.CompilerServices.NullableContextAttribute"
				// 		&& a.ConstructorArguments.Length == 1
				// 		&& a.ConstructorArguments[0].Value is int nullableContextValue
				// 		&& nullableContextValue > 0);
				var memberIsNullable = property.Type.IsTypeNullable(enabledNullable);
				isNullable |= memberIsNullable;

				IPathPart memberAccess = new MemberAccess(p, property.Type.IsValueType);
				if (previousPartIsNullable)
				{
					memberAccess = new ConditionalAccess(memberAccess);
				}

				bindingPathParts.Add(memberAccess);

				// TODO: do this only if it is the last part?
				setterOptions = new SetterOptions(
					IsWritable: property.SetMethod != null
						&& property.SetMethod.IsPublic()
						&& !property.SetMethod.IsInitOnly
						&& !property.SetMethod.IsStatic,
					AcceptsNullValue: memberIsNullable);

				previousPartType = property.Type;
				previousPartIsNullable = memberIsNullable;
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

		if (isNullable)
		{
			if (propertyType.IsValueType)
			{
				propertyType = _context.Compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(propertyType);
			}
			else
			{
				propertyType = propertyType.WithNullableAnnotation(NullableAnnotation.Annotated);
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
