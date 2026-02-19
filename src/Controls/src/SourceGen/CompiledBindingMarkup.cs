using System;
using System.CodeDom.Compiler;
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

	private Location GetLocation(INode node, string? context = null)
		=> LocationHelpers.LocationCreate(_context.ProjectItem.RelativePath!, (IXmlLineInfo)node, context ?? "Binding");

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
			// This should not happen normally, but report a diagnostic instead of throwing
			_context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, GetLocation(_node), "BindingExtension not found"));
			return false;
		}

		var methodName = $"CreateTypedBindingFrom_{_bindingExtension.ValueAccessor}";

		// TODO emit #line info?

		var extensionTypeName = isTemplateBinding
			? "global::Microsoft.Maui.Controls.Xaml.TemplateBindingExtension"
			: "global::Microsoft.Maui.Controls.Xaml.BindingExtension";

		// Determine which properties were set in XAML
		var propertyFlags = BindingPropertyFlags.None;

		if (_node.HasProperty("Mode"))
			propertyFlags |= BindingPropertyFlags.Mode;
		if (_node.HasProperty("Converter"))
			propertyFlags |= BindingPropertyFlags.Converter;
		if (_node.HasProperty("ConverterParameter"))
			propertyFlags |= BindingPropertyFlags.ConverterParameter;
		if (_node.HasProperty("StringFormat"))
			propertyFlags |= BindingPropertyFlags.StringFormat;
		if (_node.HasProperty("Source") || isTemplateBinding)
			propertyFlags |= BindingPropertyFlags.Source;
		if (_node.HasProperty("FallbackValue"))
			propertyFlags |= BindingPropertyFlags.FallbackValue;
		if (_node.HasProperty("TargetNullValue"))
			propertyFlags |= BindingPropertyFlags.TargetNullValue;

		//Generate the complete inline binding creation method
		using var stringWriter = new StringWriter();
		using var code = new IndentedTextWriter(stringWriter, "\t");

		code.WriteLine($"static global::Microsoft.Maui.Controls.BindingBase {methodName}({extensionTypeName} extension)");
		code.WriteLine("{");
		code.Indent++;

		// Setter initialization
		// If we can determine the exact binding mode at compile time, we can avoid generating the setter or avoid the ShouldUseSetter method.
		// If we cannot, we need to generate a ShouldUseSetter helper method.
		bool generateSetter = false;
		bool generateShouldUseSetter = false;
		if (_node.Properties.TryGetValue("Mode", out INode modeNode))
		{
			if (modeNode is ValueNode { Value: string mode })
			{
				generateSetter = mode switch
				{
					"OneWayToSource" => true,
					"TwoWay" => true,
					"Default" => true,
					"OneWay" => false,
					"OneTime" => false,
					_ => true, // unknown mode, be safe and generate the setter
				};
			}
			else
			{
				// There is Mono, but it doesn't have a static value -- check `extension.Mode` at runtime
				generateSetter = true;
				generateShouldUseSetter = true;
			}
		}
		else
		{
			// The user did not set the Mode property, so the binding mode is determined by the target bindable property and its default binding mode.
			// TODO: Ideally, we should be able to figure out what the default binding mode of the target property is and use that information here.
			//       Unfortunately, this seems rather difficult at this time. We might be able to do this for built-in MAUI controls at some later point,
			//       although it is unclear if that could be done in a reliable and future-proof way.
			//
			// Skip generating the binding if the property or field at the end of the path is read-only and a propagating the value from target to source
			// does not make any sense.
			generateSetter = binding.SetterOptions.IsWritable;
			generateShouldUseSetter = false;
		}

		code.Write($"global::System.Action<{binding.SourceType}, {binding.PropertyType}>? setter = ");
		if (generateSetter)
		{
			if (generateShouldUseSetter)
			{
				code.WriteLine("null;");
				code.WriteLine("if (ShouldUseSetter(extension.Mode))");
				code.WriteLine("{");
				code.Indent++;
				code.Write("setter = ");
			}

			if (binding.SetterOptions.IsWritable)
			{
				AppendSetterLambda(code, binding);
			}
			else
			{
				code.WriteLine("static (source, value) =>");
				code.WriteLine("{");
				code.Indent++;
				code.WriteLine("throw new global::System.InvalidOperationException(\"Cannot set value on the source object.\");");
				code.Indent--;
				code.WriteLine("};");
			}

			if (generateShouldUseSetter)
			{
				code.Indent--;
				code.WriteLine("}");
			}

			code.WriteLine();
		}
		else
		{
			code.WriteLine("null;");
		}

		// TypedBinding creation
		var (handlersCount, localFunctionCode) = GenerateHandlersLocalFunction(binding);
		code.WriteLine($"var handlersCount = {handlersCount};");
		if (localFunctionCode is not null)
		{
			code.WriteLine();
			code.Write(localFunctionCode);
			code.WriteLine();
		}
		code.WriteLine($"return new global::Microsoft.Maui.Controls.Internals.TypedBinding<{binding.SourceType}, {binding.PropertyType}>(");
		code.Indent++;
		var targetNullValueExpression = isTemplateBinding || !propertyFlags.HasFlag(BindingPropertyFlags.TargetNullValue) ? null : "extension.TargetNullValue";
		code.WriteLine($"getter: source => ({GenerateGetterExpression(binding, sourceVariableName: "source", targetNullValueExpression)}, true),");
		code.WriteLine("setter,");
		code.WriteLine("handlersCount,");
		code.Write(localFunctionCode is not null ? "GetHandlers" : "null");
		code.Write(")");;
		code.Indent--;

		// Object initializer if any properties are set
		if (propertyFlags != BindingPropertyFlags.None)
		{
			code.WriteLine();
			code.WriteLine("{");
			code.Indent++;

			if (propertyFlags.HasFlag(BindingPropertyFlags.Mode))
				code.WriteLine("Mode = extension.Mode,");
			if (propertyFlags.HasFlag(BindingPropertyFlags.Converter))
				code.WriteLine("Converter = extension.Converter,");
			if (propertyFlags.HasFlag(BindingPropertyFlags.ConverterParameter))
				code.WriteLine("ConverterParameter = extension.ConverterParameter,");
			if (propertyFlags.HasFlag(BindingPropertyFlags.StringFormat))
				code.WriteLine("StringFormat = extension.StringFormat,");
			if (propertyFlags.HasFlag(BindingPropertyFlags.Source))
			{
				if (isTemplateBinding)
					code.WriteLine("Source = global::Microsoft.Maui.Controls.RelativeBindingSource.TemplatedParent,");
				else
					code.WriteLine("Source = extension.Source,");
			}
			if (propertyFlags.HasFlag(BindingPropertyFlags.FallbackValue))
			{
				if (isTemplateBinding)
					code.WriteLine("FallbackValue = null,");
				else
					code.WriteLine("FallbackValue = extension.FallbackValue,");
			}
			if (propertyFlags.HasFlag(BindingPropertyFlags.TargetNullValue))
			{
				if (isTemplateBinding)
					code.WriteLine("TargetNullValue = null,");
				else
					code.WriteLine("TargetNullValue = extension.TargetNullValue,");
			}

			code.Indent--;
			code.WriteLine("};");
		}
		else
		{
			code.WriteLine(";");
		}

		if (generateShouldUseSetter)
		{
			code.WriteLine();

			// ShouldUseSetter helper
			code.WriteLine("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
			code.WriteLine("static bool ShouldUseSetter(global::Microsoft.Maui.Controls.BindingMode mode)");
			code.Indent++;
			code.WriteLine("=> mode == global::Microsoft.Maui.Controls.BindingMode.OneWayToSource");
			code.Indent++;
			code.WriteLine("|| mode == global::Microsoft.Maui.Controls.BindingMode.TwoWay");
			code.WriteLine("|| mode == global::Microsoft.Maui.Controls.BindingMode.Default;");
			code.Indent -= 2;
		}

		code.Indent--;
		code.WriteLine("}");

		_context.AddLocalMethod(stringWriter.ToString());
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
					_context.ReportDiagnostic(Diagnostic.Create(Descriptors.BindingIndexerNotClosed, GetLocation(_node)));
					return false;
				}

				var argLength = rbIndex - lbIndex - 1;
				if (argLength == 0)
				{
					_context.ReportDiagnostic(Diagnostic.Create(Descriptors.BindingIndexerEmpty, GetLocation(_node)));
					return false;
				}

				indexArg = p.Substring(lbIndex + 1, argLength);

				p = p.Substring(0, lbIndex);
				p = p.Trim();
			}

			if (p.Length > 0)
			{
				// Try to find property or infer from RelayCommand method
				if (!previousPartType.TryGetProperty(p, _context, out var property, out var currentPropertyType) 
					|| currentPropertyType is null)
				{
					// Report as WARNING (not Error) because the property might be generated by another source generator
					// that runs after the MAUI XAML source generator. The binding will fall back to reflection at runtime.
					// This allows scenarios like CommunityToolkit.Mvvm's [ObservableProperty] to work correctly.
					_context.ReportDiagnostic(Diagnostic.Create(Descriptors.BindingPropertyNotFound, GetLocation(_node), p, previousPartType.ToFQDisplayString()));
					return false;
				}

				// TODO: detect if the type is annotated or not
				var enabledNullable = true;
				// var enabledNullable = previousPartType.NullableAnnotation == NullableAnnotation.Annotated
				// 	|| previousPartType.GetAttributes().Any(a => a.AttributeClass?.ToFQDisplayString() == "global::System.Runtime.CompilerServices.NullableContextAttribute"
				// 		&& a.ConstructorArguments.Length == 1
				// 		&& a.ConstructorArguments[0].Value is int nullableContextValue
				// 		&& nullableContextValue > 0);
				var memberIsNullable = currentPropertyType.IsTypeNullable(enabledNullable);
				isNullable |= memberIsNullable;

				IPathPart memberAccess = new MemberAccess(p, currentPropertyType.IsValueType,
					DefinitelyImplementsINPC: DefinitelyImplementsINPC(previousPartType),
					MaybeImplementsINPC: MaybeImplementsINPC(previousPartType));
				if (previousPartIsNullable)
				{
					memberAccess = new ConditionalAccess(memberAccess);
				}

				bindingPathParts.Add(memberAccess);

				// TODO: do this only if it is the last part?
				setterOptions = new SetterOptions(
					IsWritable: property is not null
						&& property.SetMethod is not null
						&& property.SetMethod.IsPublic()
						&& !property.SetMethod.IsInitOnly
						&& !property.SetMethod.IsStatic,
					AcceptsNullValue: memberIsNullable);

				previousPartType = currentPropertyType;
				previousPartIsNullable = memberIsNullable;
			}

			if (indexArg != null)
			{
				index = indexArg;

				// For arrays, use the element type directly without trying to find an indexer
				if (previousPartType is IArrayTypeSymbol arrayType)
				{
					if (!int.TryParse(indexArg, out int indexArgInt))
					{
						_context.ReportDiagnostic(Diagnostic.Create(Descriptors.BindingIndexerTypeUnsupported, GetLocation(_node), previousPartType.ToFQDisplayString()));
						return false;
					}
					index = indexArgInt;

					IPathPart indexAccess = new IndexAccess("", index, arrayType.ElementType.IsValueType);
					if (previousPartIsNullable)
					{
						indexAccess = new ConditionalAccess(indexAccess);
					}
					bindingPathParts.Add(indexAccess);

					previousPartType = arrayType.ElementType;

					setterOptions = new SetterOptions(
						IsWritable: true, // arrays are writable by index
						AcceptsNullValue: previousPartType.IsTypeNullable(enabledNullable: true));
				}
				else
				{
					// For constructed generic types, we need to check the OriginalDefinition for the DefaultMemberAttribute
					var typeToCheckForAttribute = previousPartType is INamedTypeSymbol namedType && namedType.IsGenericType 
						? namedType.OriginalDefinition 
						: previousPartType;
					var defaultMemberAttribute = typeToCheckForAttribute.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToFQDisplayString() == "global::System.Reflection.DefaultMemberAttribute");
					var indexerName = defaultMemberAttribute?.ConstructorArguments.FirstOrDefault().Value as string ?? "Item";

					// For indexer lookup, we also need to search properties that are indexers (have parameters)
					// The indexer may be declared directly on the type or inherited from interfaces
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

						// If not found by name, try to find any indexer with int parameter (for cases where DefaultMemberAttribute is missing or different)
						// But prefer indexers that are not from non-generic interfaces like IList (which returns object)
						indexer ??= previousPartType
							.GetAllProperties(_context)
							.Where(property =>
								property.IsIndexer
								&& property.GetMethod != null
								&& !property.GetMethod.IsStatic
								&& property.Parameters.Length == 1
								&& property.Parameters[0].Type.SpecialType == SpecialType.System_Int32)
							.OrderByDescending(p => p.Type.SpecialType != SpecialType.System_Object) // Prefer non-object return types
							.FirstOrDefault();
					}

					indexer ??= previousPartType
						.GetAllProperties(indexerName, _context)
						.FirstOrDefault(property =>
							property.GetMethod != null
							&& !property.GetMethod.IsStatic
							&& property.Parameters.Length == 1
							&& property.Parameters[0].Type.SpecialType == SpecialType.System_String);

					// Fallback: try to find any indexer with string parameter, preferring non-object return types
					indexer ??= previousPartType
						.GetAllProperties(_context)
						.Where(property =>
							property.IsIndexer
							&& property.GetMethod != null
							&& !property.GetMethod.IsStatic
							&& property.Parameters.Length == 1
							&& property.Parameters[0].Type.SpecialType == SpecialType.System_String)
						.OrderByDescending(p => p.Type.SpecialType != SpecialType.System_Object)
						.FirstOrDefault();

					indexer ??= previousPartType
						.GetAllProperties(indexerName, _context)
						.FirstOrDefault(property =>
							property.GetMethod != null
							&& !property.GetMethod.IsStatic
							&& property.Parameters.Length == 1
							&& property.Parameters[0].Type.SpecialType == SpecialType.System_Object);

					// Fallback: try to find any indexer with object parameter
					indexer ??= previousPartType
						.GetAllProperties(_context)
						.FirstOrDefault(property =>
							property.IsIndexer
							&& property.GetMethod != null
							&& !property.GetMethod.IsStatic
							&& property.Parameters.Length == 1
							&& property.Parameters[0].Type.SpecialType == SpecialType.System_Object);

					if (indexer is not null)
					{
						// Use MetadataName because for indexers with [IndexerName("CustomName")], the
						// Name property is "this[]" but MetadataName is "CustomName" which is what
						// PropertyChanged events use (e.g., "CustomName[3]" not "this[][3]")
						var actualIndexerName = indexer.MetadataName;
						IPathPart indexAccess = new IndexAccess(actualIndexerName, index, indexer.Type.IsValueType,
							DefinitelyImplementsINPC: DefinitelyImplementsINPC(previousPartType),
							MaybeImplementsINPC: MaybeImplementsINPC(previousPartType));
						if (previousPartIsNullable)
						{
							indexAccess = new ConditionalAccess(indexAccess);
						}
						bindingPathParts.Add(indexAccess);

						previousPartType = indexer.Type;

						setterOptions = new SetterOptions(
							IsWritable: indexer.SetMethod != null && indexer.SetMethod.IsPublic() && !indexer.SetMethod.IsStatic,
							AcceptsNullValue: previousPartType.IsTypeNullable(enabledNullable: true));
					}
					else
					{
						_context.ReportDiagnostic(Diagnostic.Create(Descriptors.BindingIndexerTypeUnsupported, GetLocation(_node), previousPartType.ToFQDisplayString()));
						return false;
					}
				}
			}
		}

		if (bindingPathParts.Count == 0)
		{
			// Handle self-binding case (path is "." or empty after splitting)
			// For self-bindings, the property type is the source type itself
			// and there's no property to set, so we mark it as not writable
			setterOptions = new SetterOptions(
				IsWritable: false,
				AcceptsNullValue: sourceType.IsTypeNullable(enabledNullable: true));
		}

		// After the loop, update propertyType to the type of the last property in the path
		// For self-bindings (empty path), propertyType remains as sourceType
		propertyType = previousPartType;

		// Apply nullable annotation if any part of the path introduces nullability
		// For reference types, mark as nullable so the TypedBinding signature is correct
		// For value types, we don't mark as nullable here because GenerateGetterExpression
		// will add ?? default fallback for non-nullable value types with conditional access
		if (isNullable && !propertyType.IsValueType)
		{
			propertyType = propertyType.WithNullableAnnotation(NullableAnnotation.Annotated);
		}

		bindingPath = new EquatableArray<IPathPart>(bindingPathParts.ToArray());
		return true;
	}

	string GenerateGetterLambda(BindingInvocationDescription binding, string? targetNullValueExpression)
	{
		return $"static source => {GenerateGetterExpression(binding, sourceVariableName: "source", targetNullValueExpression)}";
	}

	string GenerateGetterExpression(
		BindingInvocationDescription binding,
		string sourceVariableName,
		string? targetNullValueExpression = null)
	{
		string expression = sourceVariableName;
		bool forceConditionalAccessToNextPart = false;
		bool hasConditionalAccess = false;

		foreach (var part in binding.Path)
		{
			// Note: AccessExpressionBuilder will happily call unsafe accessors and it expects them to be available.
			// By calling BindingCodeWriter.GenerateBindingMethod(...), we are ensuring that the unsafe accessors are available.
			expression = AccessExpressionBuilder.ExtendExpression(expression, MaybeWrapInConditionalAccess(part, forceConditionalAccessToNextPart));
			forceConditionalAccessToNextPart = part is Cast;
			hasConditionalAccess |= forceConditionalAccessToNextPart || part is ConditionalAccess;
		}

		if (hasConditionalAccess && binding.PropertyType is { IsValueType: true, IsNullable: false })
		{
			// for non-nullable value types with conditional access in the path, we need to unwrap the getter result
			// with fallback to either the target null value or default
			if (targetNullValueExpression is not null)
			{
				var nullablePropertyType = binding.PropertyType with { IsNullable = true };
				expression = $"{expression} ?? {targetNullValueExpression} as {nullablePropertyType}";
			}

			expression = $"{expression} ?? default";
		}

		return expression;

		static IPathPart MaybeWrapInConditionalAccess(IPathPart part, bool forceConditionalAccessToNextPart)
			=> (forceConditionalAccessToNextPart, part) switch
			{
				(true, MemberAccess memberAccess) => new ConditionalAccess(memberAccess),
				(true, IndexAccess indexAccess) => new ConditionalAccess(indexAccess),
				_ => part,
			};
	}

	void AppendSetterLambda(IndentedTextWriter code, BindingInvocationDescription binding)
	{
		code.Write("static (source, value) =>");
		code.WriteLine();
		code.WriteLine("{");
		code.Indent++;
		
		var assignedValueExpression = "value";

		// early return for nullable values if the setter doesn't accept them
		if (binding.PropertyType.IsNullable && !binding.SetterOptions.AcceptsNullValue)
		{
			if (binding.PropertyType.IsValueType)
			{
				code.WriteLine("if (!value.HasValue)");
				assignedValueExpression = "value.Value";
			}
			else
			{
				code.WriteLine("if (value is null)");
			}
			using (PrePost.NewBlock(code))
			{
				code.WriteLine("return;");
			}
		}

		var setter = Setter.From(binding.Path, "source", assignedValueExpression);
		if (setter.PatternMatchingExpressions.Length > 0)
		{
			code.Write("if (");

			for (int i = 0; i < setter.PatternMatchingExpressions.Length; i++)
			{
				if (i > 0)
				{
					code.WriteLine();
					code.Write("&& ");
				}
				code.Write(setter.PatternMatchingExpressions[i]);
			}

			code.WriteLine(")");
			code.WriteLine("{");
			code.Indent++;
			code.WriteLine(setter.AssignmentStatement);
			code.Indent--;
			code.WriteLine("}");
		}
		else
		{
			code.WriteLine(setter.AssignmentStatement);
		}

		code.Indent--;
		code.WriteLine("};");
	}

	const string INPC = "global::System.ComponentModel.INotifyPropertyChanged";
	const string IEnumerableOfINPCTuple = $"global::System.Collections.Generic.IEnumerable<global::System.ValueTuple<{INPC}?, string>>";

	/// <summary>
	/// Generates the GetHandlers local function and returns the handlersCount and the local function code.
	/// </summary>
	(int handlersCount, string? localFunctionCode) GenerateHandlersLocalFunction(BindingInvocationDescription binding)
	{
		if (!binding.Path.Any())
		{
			return (0, null);
		}

		int handlersCount = 0;
		var statements = new List<string>();

		string currentVar = "source";
		int varIndex = 0;
		bool forceConditionalAccessToNextPart = false;
		
		foreach (var part in binding.Path)
		{
			var previousVar = currentVar;
			var wrappedPart = MaybeWrapInConditionalAccess(part, forceConditionalAccessToNextPart);
			var nextExpression = AccessExpressionBuilder.ExtendExpression(previousVar, wrappedPart);
			forceConditionalAccessToNextPart = part is Cast;

			// Create a variable for the next expression to avoid re-evaluating
			var nextVar = $"p{varIndex++}";
			statements.Add($"var {nextVar} = {nextExpression};");

			// Make binding react for PropertyChanged events on indexer itself
			// If we know for certain it implements INPC, generate the yield directly
			// If it might implement INPC at runtime, generate a runtime check
			// Generate two handlers: one for the base indexer name (e.g., "Item") and one for the
			// specific index (e.g., "Item[2]") so the binding reacts to both general and specific changes
			if (part is IndexAccess indexAccess)
			{
				if (indexAccess.DefinitelyImplementsINPC)
				{
					if (!string.IsNullOrEmpty(indexAccess.DefaultMemberName))
					{
						statements.Add($"yield return ({previousVar}, \"{indexAccess.DefaultMemberName}\");");
						handlersCount++;
					}
					statements.Add($"yield return ({previousVar}, \"{indexAccess.PropertyName}\");");
					handlersCount++;
				}
				else if (indexAccess.MaybeImplementsINPC)
				{
					var nextVarLocal = $"p{varIndex++}";
					if (!string.IsNullOrEmpty(indexAccess.DefaultMemberName))
					{
						statements.Add(
							$"if ({previousVar} is {INPC} {nextVarLocal})\n" +
							$"{{\n" +
							$"\tyield return ({nextVarLocal}, \"{indexAccess.DefaultMemberName}\");\n" +
							$"\tyield return ({nextVarLocal}, \"{indexAccess.PropertyName}\");\n" +
							$"}}");
					}
					else
					{
						statements.Add($"""
							if ({previousVar} is {INPC} {nextVarLocal})
								yield return ({nextVarLocal}, "{indexAccess.PropertyName}");
							""");
					}
					handlersCount++;
				}
			}
			else if (part is ConditionalAccess conditionalAccess && conditionalAccess.Part is IndexAccess innerIndexAccess)
			{
				if (innerIndexAccess.DefinitelyImplementsINPC)
				{
					if (!string.IsNullOrEmpty(innerIndexAccess.DefaultMemberName))
					{
						statements.Add($"yield return ({previousVar}, \"{innerIndexAccess.DefaultMemberName}\");");
						handlersCount++;
					}
					statements.Add($"yield return ({previousVar}, \"{innerIndexAccess.PropertyName}\");");
					handlersCount++;
				}
				else if (innerIndexAccess.MaybeImplementsINPC)
				{
					var nextVarLocal = $"p{varIndex++}";
					if (!string.IsNullOrEmpty(innerIndexAccess.DefaultMemberName))
					{
						statements.Add(
							$"if ({previousVar} is {INPC} {nextVarLocal})\n" +
							$"{{\n" +
							$"\tyield return ({nextVarLocal}, \"{innerIndexAccess.DefaultMemberName}\");\n" +
							$"\tyield return ({nextVarLocal}, \"{innerIndexAccess.PropertyName}\");\n" +
							$"}}");
					}
					else
					{
						statements.Add($"""
							if ({previousVar} is {INPC} {nextVarLocal})
								yield return ({nextVarLocal}, "{innerIndexAccess.PropertyName}");
							""");
					}
					handlersCount++;
				}
			}

			// Some parts don't have a property name, so we can't generate a handler for them (for example casts)
			// If we know for certain it implements INPC, generate the yield directly
			// If it might implement INPC at runtime, generate a runtime check
			if (part is MemberAccess memberAccess)
			{
				if (memberAccess.DefinitelyImplementsINPC)
				{
					statements.Add($"yield return ({previousVar}, \"{memberAccess.PropertyName}\");");
					handlersCount++;
				}
				else if (memberAccess.MaybeImplementsINPC)
				{
					var nextVarLocal = $"p{varIndex++}";
					statements.Add($"""
						if ({previousVar} is {INPC} {nextVarLocal})
							yield return ({nextVarLocal}, "{memberAccess.PropertyName}");
						""");
					handlersCount++;
				}
			}
			else if (part is ConditionalAccess condAccess && condAccess.Part is MemberAccess innerMemberAccess)
			{
				if (innerMemberAccess.DefinitelyImplementsINPC)
				{
					statements.Add($"yield return ({previousVar}, \"{innerMemberAccess.PropertyName}\");");
					handlersCount++;
				}
				else if (innerMemberAccess.MaybeImplementsINPC)
				{
					var nextVarLocal = $"p{varIndex++}";
					statements.Add($"""
						if ({previousVar} is {INPC} {nextVarLocal})
							yield return ({nextVarLocal}, "{innerMemberAccess.PropertyName}");
						""");
					handlersCount++;
				}
			}

			currentVar = nextVar;
		}

		// If no handlers were generated, don't generate the function
		if (handlersCount == 0)
		{
			return (0, null);
		}

		// Generate the local function
		using var localFunctionWriter = new StringWriter();
		using var localCode = new IndentedTextWriter(localFunctionWriter, "\t");
		
		localCode.WriteLine($"static {IEnumerableOfINPCTuple} GetHandlers({binding.SourceType} source)");
		localCode.WriteLine("{");
		localCode.Indent++;

		foreach (var statement in statements)
		{
			localCode.WriteLine(statement);
		}

		localCode.Indent--;
		localCode.WriteLine("}");

		return (handlersCount, localFunctionWriter.ToString());

		static IPathPart MaybeWrapInConditionalAccess(IPathPart part, bool forceConditionalAccess)
		{
			if (!forceConditionalAccess)
			{
				return part;
			}

			return part switch
			{
				MemberAccess memberAccess => new ConditionalAccess(memberAccess),
				IndexAccess indexAccess => new ConditionalAccess(indexAccess),
				_ => part,
			};
		}
	}

	static bool DefinitelyImplementsINPC(ITypeSymbol? typeSymbol)
	{
		if (typeSymbol == null)
			return false;
		return typeSymbol.AllInterfaces.Any(i => i.ToFQDisplayString() == "global::System.ComponentModel.INotifyPropertyChanged");
	}

	static bool MaybeImplementsINPC(ITypeSymbol? typeSymbol)
	{
		if (typeSymbol == null)
			return true;
		if (DefinitelyImplementsINPC(typeSymbol))
			return false;
		if (typeSymbol.TypeKind == TypeKind.TypeParameter)
			return true;
		if (typeSymbol.TypeKind == TypeKind.Interface)
			return true;
		if (typeSymbol.TypeKind == TypeKind.Class && !typeSymbol.IsSealed)
			return true;
		return false;
	}
}

internal static class ElementNodeExtensions
{
	public static bool HasProperty(this ElementNode node, string propertyName)
		=> node.Properties.TryGetValue(propertyName, out _);
}
