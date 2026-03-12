#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.StyleSheets
{
	sealed class Style
	{
		internal Style()
		{
		}

		public IDictionary<string, string> Declarations { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);
		internal HashSet<string> ImportantDeclarations { get; set; }
		internal static Dictionary<KeyValuePair<string, string>, object> ConvertedValues = new Dictionary<KeyValuePair<string, string>, object>();
		Dictionary<KeyValuePair<string, string>, object> convertedValues => ConvertedValues;

		public static Style Parse(CssReader reader, char stopChar = '\0')
		{
			Style style = new Style();
			string propertyName = null, propertyValue = null;

			int p;
			reader.SkipWhiteSpaces();
			bool readingName = true;
			while ((p = reader.Peek()) > 0)
			{
				switch (unchecked((char)p))
				{
					case ':':
						reader.Read();
						readingName = false;
						reader.SkipWhiteSpaces();
						break;
					case ';':
						reader.Read();
						if (!string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(propertyValue))
							AddDeclaration(style, propertyName, propertyValue);
						propertyName = propertyValue = null;
						readingName = true;
						reader.SkipWhiteSpaces();
						break;
					default:
						if ((char)p == stopChar)
							return style;

						if (readingName)
						{
							propertyName = reader.ReadIdent();
							if (propertyName == null)
								throw new Exception();
						}
						else
							propertyValue = reader.ReadUntil(stopChar, ';', ':');
						break;
				}
			}
			return style;
		}

		internal static void AddDeclaration(Style style, string name, string value)
		{
			var trimmed = value.TrimEnd();
			bool important = false;
			if (trimmed.EndsWith("!important", StringComparison.OrdinalIgnoreCase))
			{
				trimmed = trimmed.Substring(0, trimmed.Length - "!important".Length).TrimEnd();
				important = true;
			}

			// Expand shorthand properties
			if (TryExpandShorthand(style, name, trimmed, important))
				return;

			if (important)
			{
				style.ImportantDeclarations ??= new HashSet<string>(StringComparer.Ordinal);
				style.ImportantDeclarations.Add(name);
			}
			style.Declarations[name] = trimmed;
		}

		static bool TryExpandShorthand(Style style, string name, string value, bool important)
		{
			switch (name)
			{
				case "border":
					return ExpandBorder(style, value, important);
				case "font":
					return ExpandFont(style, value, important);
				default:
					return false;
			}
		}

		static void SetExpanded(Style style, string name, string value, bool important)
		{
			style.Declarations[name] = value;
			if (important)
			{
				style.ImportantDeclarations ??= new HashSet<string>(StringComparer.Ordinal);
				style.ImportantDeclarations.Add(name);
			}
		}

		// border: [width] [style] [color]  e.g. "1 solid red", "2 #ff0000"
		static bool ExpandBorder(Style style, string value, bool important)
		{
			var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0)
				return false;

			foreach (var part in parts)
			{
				var p = part.Trim();
				// Skip CSS border-style keywords — MAUI has no border-style property
				if (IsBorderStyle(p))
					continue;

				// Try as number (border-width)
				if (double.TryParse(p, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _)
					|| p.EndsWith("px", StringComparison.OrdinalIgnoreCase)
					|| p.EndsWith("rem", StringComparison.OrdinalIgnoreCase)
					|| p.EndsWith("em", StringComparison.OrdinalIgnoreCase))
				{
					SetExpanded(style, "border-width", p, important);
				}
				else
				{
					// Assume color value
					SetExpanded(style, "border-color", p, important);
				}
			}
			return true;
		}

		static bool IsBorderStyle(string value)
			=> string.Equals(value, "none", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "solid", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "dashed", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "dotted", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "double", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "groove", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "ridge", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "inset", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "outset", StringComparison.OrdinalIgnoreCase);

		// font: [style] [weight] [size] [family]  e.g. "italic bold 16 Arial"
		static bool ExpandFont(Style style, string value, bool important)
		{
			var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0)
				return false;

			foreach (var part in parts)
			{
				var p = part.Trim();
				if (string.Equals(p, "italic", StringComparison.OrdinalIgnoreCase)
					|| string.Equals(p, "oblique", StringComparison.OrdinalIgnoreCase))
				{
					SetExpanded(style, "font-style", p, important);
				}
				else if (string.Equals(p, "bold", StringComparison.OrdinalIgnoreCase)
					|| string.Equals(p, "normal", StringComparison.OrdinalIgnoreCase)
					|| string.Equals(p, "bolder", StringComparison.OrdinalIgnoreCase)
					|| string.Equals(p, "lighter", StringComparison.OrdinalIgnoreCase))
				{
					SetExpanded(style, "font-weight", p, important);
				}
				else if (double.TryParse(p, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _)
					|| p.EndsWith("px", StringComparison.OrdinalIgnoreCase)
					|| p.EndsWith("rem", StringComparison.OrdinalIgnoreCase)
					|| p.EndsWith("em", StringComparison.OrdinalIgnoreCase))
				{
					SetExpanded(style, "font-size", p, important);
				}
				else
				{
					// Remaining part is font-family (may contain commas for fallback fonts)
					SetExpanded(style, "font-family", p, important);
				}
			}
			return true;
		}

		public void Apply(VisualElement styleable, Selector.SelectorSpecificity selectorSpecificity = default, bool inheriting = false, IDictionary<string, string> variables = null)
		{
			if (styleable == null)
				throw new ArgumentNullException(nameof(styleable));

			foreach (var decl in Declarations)
			{
				var property = ((IStylable)styleable).GetProperty(decl.Key, inheriting);
				if (property == null)
					continue;

				var resolvedValue = CssVariableResolver.Resolve(decl.Value, variables);

				// Per CSS spec, unresolved var() with no fallback makes the declaration invalid — skip it
				if (string.IsNullOrEmpty(resolvedValue))
					continue;

				// Resolve calc(), rem, em units to plain numeric values
				resolvedValue = CssValueResolver.ResolveUnits(resolvedValue);

				// !important boosts specificity to max CSS values
				bool isImportant = ImportantDeclarations != null && ImportantDeclarations.Contains(decl.Key);
				byte idSpec = isImportant ? (byte)0xFF : (byte)selectorSpecificity.Id;
				byte classSpec = isImportant ? (byte)0xFF : (byte)selectorSpecificity.Class;
				byte typeSpec = isImportant ? (byte)0xFF : (byte)selectorSpecificity.Type;

				if (string.Equals(resolvedValue, "initial", StringComparison.OrdinalIgnoreCase))
					styleable.SetValue(property, property.DefaultValue, new SetterSpecificity(SetterSpecificity.StyleImplicit, idSpec, classSpec, typeSpec));
				else if (string.Equals(resolvedValue, "inherit", StringComparison.OrdinalIgnoreCase))
				{
					// Inherit from parent: walk up the visual tree to find the current value
					var parent = (styleable as Element)?.Parent as VisualElement;
					var inheritedValue = parent != null ? parent.GetValue(property) : property.DefaultValue;
					styleable.SetValue(property, inheritedValue, new SetterSpecificity(SetterSpecificity.StyleImplicit, idSpec, classSpec, typeSpec));
				}
				else if (string.Equals(resolvedValue, "unset", StringComparison.OrdinalIgnoreCase))
				{
					// unset = inherit for inherited properties, initial for non-inherited
					// We approximate by checking if the property has the Inherited flag in the style registry
					styleable.ClearValue(property);
				}
				else
				{
					object value;
					var resolvedDecl = new KeyValuePair<string, string>(decl.Key, resolvedValue);
					if (!convertedValues.TryGetValue(resolvedDecl, out value))
						convertedValues[resolvedDecl] = (value = Convert(styleable, resolvedValue, property));
					styleable.SetValue(property, value, new SetterSpecificity(SetterSpecificity.StyleImplicit, idSpec, classSpec, typeSpec));
				}
			}

			foreach (var child in ((IVisualTreeElement)styleable).GetVisualChildren())
			{
				var ve = child as VisualElement;
				if (ve == null)
					continue;
				Apply(ve, inheriting: true, variables: variables);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static object Convert(object target, object value, BindableProperty property)
		{
			var serviceProvider = new StyleSheetServiceProvider(target, property);
			Func<MemberInfo> minforetriever =
				() =>
				{
					MemberInfo minfo = null;
					try
					{
						minfo = property.DeclaringType.GetRuntimeProperty(property.PropertyName);
					}
					catch (AmbiguousMatchException e)
					{
						throw new XamlParseException($"Multiple properties with name '{property.DeclaringType}.{property.PropertyName}' found.", serviceProvider, innerException: e);
					}
					if (minfo != null)
						return minfo;
					try
					{
						return property.DeclaringType.GetRuntimeMethod("Get" + property.PropertyName, new[] { typeof(BindableObject) });
					}
					catch (AmbiguousMatchException e)
					{
						throw new XamlParseException($"Multiple methods with name '{property.DeclaringType}.Get{property.PropertyName}' found.", serviceProvider, innerException: e);
					}
				};
			var ret = value.ConvertTo(property.ReturnType, minforetriever, serviceProvider, out Exception exception);
			if (exception != null)
				throw exception;
			return ret;
		}

		public void UnApply(IStylable styleable)
		{
			throw new NotImplementedException();
		}
	}
}
