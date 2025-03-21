using System.CodeDom.Compiler;
using System.Globalization;
using System.Runtime.InteropServices;

using static Microsoft.Maui.Controls.BindingSourceGen.UnsafeAccessorsMethodName;

namespace Microsoft.Maui.Controls.BindingSourceGen;

public static class BindingCodeWriter
{
	private static readonly string NewLine = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\n" : "\n";

	public static string GeneratedCodeAttribute => $"[GeneratedCodeAttribute(\"{typeof(BindingCodeWriter).Assembly.FullName}\", \"{typeof(BindingCodeWriter).Assembly.GetName().Version}\")]";

	public static string GenerateCommonCode() => $$"""
		//------------------------------------------------------------------------------
		// <auto-generated>
		//     This code was generated by a .NET MAUI source generator.
		//
		//     Changes to this file may cause incorrect behavior and will be lost if
		//     the code is regenerated.
		// </auto-generated>
		//------------------------------------------------------------------------------
		#nullable enable

		namespace Microsoft.Maui.Controls.Generated
		{
			using System.CodeDom.Compiler;

			{{GeneratedCodeAttribute}}
			internal static partial class GeneratedBindingInterceptors
			{
				private static bool ShouldUseSetter(BindingMode mode, BindableProperty bindableProperty)
					=> mode == BindingMode.OneWayToSource
						|| mode == BindingMode.TwoWay
						|| (mode == BindingMode.Default
							&& (bindableProperty.DefaultBindingMode == BindingMode.OneWayToSource
								|| bindableProperty.DefaultBindingMode == BindingMode.TwoWay));

				private static bool ShouldUseSetter(BindingMode mode)
					=> mode == BindingMode.OneWayToSource
						|| mode == BindingMode.TwoWay
						|| mode == BindingMode.Default;
			}
		}
		""";

	public static string GenerateBinding(BindingInvocationDescription binding, uint id) => $$"""
		//------------------------------------------------------------------------------
		// <auto-generated>
		//     This code was generated by a .NET MAUI source generator.
		//
		//     Changes to this file may cause incorrect behavior and will be lost if
		//     the code is regenerated.
		// </auto-generated>
		//------------------------------------------------------------------------------
		#nullable enable

		namespace System.Runtime.CompilerServices
		{
			using System;
			using System.CodeDom.Compiler;
		
			{{GeneratedCodeAttribute}}
			[global::System.Diagnostics.Conditional("DEBUG")]
			[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
			file sealed class InterceptsLocationAttribute : Attribute
			{
				public InterceptsLocationAttribute(int version, string data)
				{
					_ = version;
					_ = data;
				}
			}
		}

		namespace Microsoft.Maui.Controls.Generated
		{
			using System;
			using System.CodeDom.Compiler;
			using System.Runtime.CompilerServices;
			using Microsoft.Maui.Controls.Internals;

			internal static partial class GeneratedBindingInterceptors
			{
				{{GenerateBindingMethod(binding, methodNameSuffix: id.ToString())}}
			}
		}
		""";

	public static string GenerateBindingMethod(BindingInvocationDescription binding, string methodNameSuffix = "")
	{
		if (!binding.NullableContextEnabled)
		{
			var referenceTypesConditionalAccessTransformer = new ReferenceTypesConditionalAccessTransformer();
			binding = referenceTypesConditionalAccessTransformer.Transform(binding);
		}

		using var builder = new BindingInterceptorCodeBuilder(indent: 2);
		builder.AppendBindingFactoryMethod(binding, methodNameSuffix);
		return builder.ToString();
	}

	public sealed class BindingInterceptorCodeBuilder : IDisposable
	{
		private StringWriter _stringWriter;
		private IndentedTextWriter _indentedTextWriter;

		public override string ToString()
		{
			_indentedTextWriter.Flush();
			return _stringWriter.ToString();
		}

		public BindingInterceptorCodeBuilder(int indent = 0)
		{
			_stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			_indentedTextWriter = new IndentedTextWriter(_stringWriter, "\t") { Indent = indent };
		}

		public void AppendBindingFactoryMethod(BindingInvocationDescription binding, string methodNameSuffix = "")
		{
			AppendLine(GeneratedCodeAttribute);
			if (binding.InterceptableLocation is not null)
			{
				AppendInterceptorAttribute(binding.InterceptableLocation);
			}
			AppendMethodName(binding, methodNameSuffix);
			if (binding.SourceType.IsGenericParameter && binding.PropertyType.IsGenericParameter)
			{
				Append($"<{binding.SourceType}, {binding.PropertyType}>");
			}
			else if (binding.SourceType.IsGenericParameter)
			{
				Append($"<{binding.SourceType}>");
			}
			else if (binding.PropertyType.IsGenericParameter)
			{
				Append($"<{binding.PropertyType}>");
			}
			AppendFunctionArguments(binding);

			AppendLine('{');
			Indent();

			// Initialize setter
			AppendLines($$"""
				Action<{{binding.SourceType}}, {{binding.PropertyType}}>? setter = null;
				if ({{GetShouldUseSetterCall(binding.MethodType)}})
				{
				""");

			Indent();
			Indent();

			if (binding.SetterOptions.IsWritable)
			{
				Append("setter = ");
				AppendSetterLambda(binding);
			}
			else
			{
				AppendLine("throw new InvalidOperationException(\"Cannot set value on the source object.\");");
			}

			Unindent();
			Unindent();

			AppendLine('}');
			AppendBlankLine();

			// Create instance of TypedBinding
			AppendLines($$"""
				var binding = new TypedBinding<{{binding.SourceType}}, {{binding.PropertyType}}>(
					getter: source => (getter(source), true),
					setter,
				""");
			Append("    handlers: ");

			AppendHandlersArray(binding);
			AppendLine(')');
			AppendLines($$"""
				{
					Mode = mode,
					Converter = converter,
					ConverterParameter = converterParameter,
					StringFormat = stringFormat,
					Source = source,
					FallbackValue = fallbackValue,
					TargetNullValue = targetNullValue
				};
				""");

			AppendBlankLine();

			// Set binding
			if (binding.MethodType == InterceptedMethodType.SetBinding)
			{
				AppendLine("bindableObject.SetBinding(bindableProperty, binding);");
			}
			else if (binding.MethodType == InterceptedMethodType.Create)
			{
				AppendLine("return binding;");
			}
			else
			{
				throw new ArgumentOutOfRangeException(nameof(binding.MethodType));
			}

			AppendUnsafeAccessors(binding);

			Unindent();
			AppendLine('}');
		}

		private void AppendFunctionArguments(BindingInvocationDescription binding)
		{
			AppendLine('(');
			Indent();

			if (binding.MethodType == InterceptedMethodType.SetBinding)
			{
				AppendLines($$"""
					this BindableObject bindableObject,
					BindableProperty bindableProperty,
					""");
			}

			AppendLines($$"""
				Func<{{binding.SourceType}}, {{binding.PropertyType}}> getter,
				BindingMode mode = BindingMode.Default,
				IValueConverter? converter = null,
				object? converterParameter = null,
				string? stringFormat = null,
				object? source = null,
				object? fallbackValue = null,
				object? targetNullValue = null)
				""");

			Unindent();
		}

		private static string GetShouldUseSetterCall(InterceptedMethodType interceptedMethodType) =>
			interceptedMethodType switch
			{
				InterceptedMethodType.SetBinding => "ShouldUseSetter(mode, bindableProperty)",
				InterceptedMethodType.Create => "ShouldUseSetter(mode)",
				_ => throw new ArgumentOutOfRangeException(nameof(interceptedMethodType))
			};


		private void AppendMethodName(BindingInvocationDescription binding, string methodNameSuffix)
		{
			var visibility = binding.IsPublic ? "public" : "private";
			Append(binding.MethodType switch
			{
				InterceptedMethodType.SetBinding => $"{visibility} static void SetBinding{methodNameSuffix}",
				InterceptedMethodType.Create => $"{visibility} static global::Microsoft.Maui.Controls.BindingBase Create{methodNameSuffix}",
				_ => throw new ArgumentOutOfRangeException(nameof(binding.MethodType))
			});
		}

		private void AppendInterceptorAttribute(InterceptableLocationRecord location)
		{
			AppendLine($"[InterceptsLocationAttribute({location.Version}, @\"{location.Data}\")]");
		}

		private void AppendSetterLambda(BindingInvocationDescription binding, string sourceVariableName = "source", string valueVariableName = "value")
		{
			AppendLine($"static ({sourceVariableName}, {valueVariableName}) =>");
			AppendLine('{');
			Indent();

			var assignedValueExpression = valueVariableName;

			// early return for nullable values if the setter doesn't accept them
			if (binding.PropertyType.IsNullable && !binding.SetterOptions.AcceptsNullValue)
			{
				if (binding.PropertyType.IsValueType)
				{
					AppendLine($"if (!{valueVariableName}.HasValue)");
					assignedValueExpression = $"{valueVariableName}.Value";
				}
				else
				{
					AppendLine($"if ({valueVariableName} is null)");
				}
				AppendLine('{');
				Indent();
				AppendLine("return;");
				Unindent();
				AppendLine('}');
			}

			var setter = Setter.From(binding.Path, sourceVariableName, assignedValueExpression);
			if (setter.PatternMatchingExpressions.Length > 0)
			{
				Append("if (");

				for (int i = 0; i < setter.PatternMatchingExpressions.Length; i++)
				{
					if (i == 1)
					{
						Indent();
					}

					if (i > 0)
					{
						AppendBlankLine();
						Append("&& ");
					}

					Append(setter.PatternMatchingExpressions[i]);
				}

				AppendLine(')');
				if (setter.PatternMatchingExpressions.Length > 1)
				{
					Unindent();
				}

				AppendLine('{');
				Indent();
			}

			AppendLine(setter.AssignmentStatement);

			if (setter.PatternMatchingExpressions.Length > 0)
			{
				Unindent();
				AppendLine('}');
			}

			Unindent();
			AppendLine("};");
		}

		private void AppendHandlersArray(BindingInvocationDescription binding)
		{
			AppendLine($"new Tuple<Func<{binding.SourceType}, object?>, string>[]");
			AppendLine('{');

			Indent();

			string nextExpression = "source";
			bool forceConditonalAccessToNextPart = false;
			foreach (var part in binding.Path)
			{
				var previousExpression = nextExpression;
				nextExpression = AccessExpressionBuilder.ExtendExpression(previousExpression, MaybeWrapInConditionalAccess(part, forceConditonalAccessToNextPart));
				forceConditonalAccessToNextPart = part is Cast;

				// Make binding react for PropertyChanged events on indexer itself
				if (part is IndexAccess indexAccess)
				{
					AppendLine($"new(static source => {previousExpression}, \"{indexAccess.DefaultMemberName}\"),");
				}
				else if (part is ConditionalAccess conditionalAccess && conditionalAccess.Part is IndexAccess innerIndexAccess)
				{
					AppendLine($"new(static source => {previousExpression}, \"{innerIndexAccess.DefaultMemberName}\"),");
				}

				// Some parts don't have a property name, so we can't generate a handler for them (for example casts)
				if (part.PropertyName is string propertyName)
				{
					AppendLine($"new(static source => {previousExpression}, \"{propertyName}\"),");
				}
			}
			Unindent();

			Append('}');

			static IPathPart MaybeWrapInConditionalAccess(IPathPart part, bool forceConditonalAccess)
			{
				if (!forceConditonalAccess)
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

		private void AppendUnsafeAccessors(BindingInvocationDescription binding)
		{
			// Append unsafe accessors as local methods
			var unsafeAccessors = binding.Path.OfType<InaccessibleMemberAccess>();

			foreach (var unsafeAccessor in unsafeAccessors)
			{
				AppendBlankLine();

				if (unsafeAccessor.Kind == AccessorKind.Field)
				{
					AppendUnsafeFieldAccessor(unsafeAccessor.MemberName, unsafeAccessor.memberType.GlobalName, unsafeAccessor.ContainingType.GlobalName);
				}
				else if (unsafeAccessor.Kind == AccessorKind.Property)
				{
					bool isLastPart = unsafeAccessor.Equals(binding.Path.Last());
					bool needsGetterForLastPart = binding.RequiresAllUnsafeGetters;

					if (!isLastPart || needsGetterForLastPart)
					{
						// we don't need the unsafe getter if the item is the very last part of the path
						// because we don't need to access its value while constructing the handlers array
						AppendUnsafePropertyGetAccessors(unsafeAccessor.MemberName, unsafeAccessor.memberType.GlobalName, unsafeAccessor.ContainingType.GlobalName);
					}

					if (isLastPart && binding.SetterOptions.IsWritable)
					{
						// We only need the unsafe setter if the item is the very last part of the path
						AppendUnsafePropertySetAccessors(unsafeAccessor.MemberName, unsafeAccessor.memberType.GlobalName, unsafeAccessor.ContainingType.GlobalName);
					}
				}
				else
				{
					throw new ArgumentException(nameof(unsafeAccessor.Kind));
				}
			}

		}

		private void AppendUnsafeFieldAccessor(string fieldName, string memberType, string containingType)
			=> AppendLines($$"""
				[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "{{fieldName}}")]
				static extern ref {{memberType}} {{CreateUnsafeFieldAccessorMethodName(fieldName)}}({{containingType}} source);
				""");

		private void AppendUnsafePropertyGetAccessors(string propertyName, string memberType, string containingType)
			=> AppendLines($$"""
				[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_{{propertyName}}")]
				static extern {{memberType}} {{CreateUnsafePropertyAccessorGetMethodName(propertyName)}}({{containingType}} source);
				""");

		private void AppendUnsafePropertySetAccessors(string propertyName, string memberType, string containingType)
			=> AppendLines($$"""
				[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_{{propertyName}}")]
				static extern void {{CreateUnsafePropertyAccessorSetMethodName(propertyName)}}({{containingType}} source, {{memberType}} value);
				""");

		public void Dispose()
		{
			_indentedTextWriter.Dispose();
			_stringWriter.Dispose();
		}

		private void AppendBlankLine() => _indentedTextWriter.WriteLine();
		private void AppendLine(string line) => _indentedTextWriter.WriteLine(line);
		private void AppendLine(char character) => _indentedTextWriter.WriteLine(character);
		private void Append(string str) => _indentedTextWriter.Write(str);
		private void Append(char character) => _indentedTextWriter.Write(character);

		private readonly char[] LineSeparators = ['\n', '\r'];
		private void AppendLines(string lines)
		{
			foreach (var line in lines.Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries))
			{
				AppendLine(line);
			}
		}

		private void Indent() => _indentedTextWriter.Indent++;
		private void Unindent() => _indentedTextWriter.Indent--;
	}
}
