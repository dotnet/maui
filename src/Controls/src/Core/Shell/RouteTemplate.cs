#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	// Parser / matcher for path-parameter route templates such as
	// "product/{sku}" or "files/{*path}". Templates are an additive,
	// opt-in extension of Routing.RegisterRoute — existing literal routes
	// are unchanged.
	//
	// Supported syntax:
	//   {name}          — required parameter, matches exactly one segment
	//   {name?}         — optional parameter, matches zero or one segment
	//   {name=default}  — default value when segment is absent
	//   {*name}         — catch-all, captures all remaining segments (must be last)
	//   {id:int}        — constrained parameter (int, guid, long, bool, double, alpha)
	//   product-{sku}   — mixed literal+parameter segment (prefix/suffix matching)
	//
	// Rules:
	//   * Parameter names follow C# identifier rules.
	//   * Duplicate parameter names are rejected.
	//   * Catch-all must be the last segment.
	//   * At most one constraint per parameter (no chaining).
	internal sealed class RouteTemplate
	{
		readonly TemplateSegment[] _segments;

		RouteTemplate(TemplateSegment[] segments)
		{
			_segments = segments;
		}

		public bool HasParameters { get; private set; }

		public IReadOnlyList<TemplateSegment> Segments => _segments;

		public static bool ContainsTemplateSyntax(string route)
		{
			if (string.IsNullOrEmpty(route))
				return false;

			return route.IndexOf("{", StringComparison.Ordinal) >= 0;
		}

		public static bool IsTemplateSegment(string segment)
		{
			if (string.IsNullOrEmpty(segment))
				return false;

			// Pure template token: {name}, {name?}, {*name}, {name:int}, {name=default}
			if (segment.Length >= 3
				&& segment[0] == '{'
				&& segment[segment.Length - 1] == '}')
				return true;

			// Mixed segment: contains { but doesn't start with it (e.g. product-{sku})
			// Require { appears before } to reject malformed strings like "foo}bar{"
			int openIdx = segment.IndexOf("{", StringComparison.Ordinal);
			int closeIdx = segment.IndexOf("}", StringComparison.Ordinal);
			if (openIdx >= 0 && closeIdx > openIdx)
				return true;

			return false;
		}

		/// <summary>
		/// Returns true if the segment is a pure parameter token (starts with { and ends with }).
		/// Mixed segments like "product-{sku}" return false.
		/// </summary>
		public static bool IsPureParameterSegment(string segment)
		{
			if (string.IsNullOrEmpty(segment) || segment.Length < 3)
				return false;

			return segment[0] == '{' && segment[segment.Length - 1] == '}';
		}

		public static string GetSegmentParameterName(string segment)
		{
			if (string.IsNullOrEmpty(segment))
				return null;

			// For pure parameter segments
			if (IsPureParameterSegment(segment))
			{
				var inner = segment.Substring(1, segment.Length - 2);

				// Strip catch-all marker
				if (inner.Length > 0 && inner[0] == '*')
					inner = inner.Substring(1);

				// Strip constraint (e.g. ":int")
				var colonIdx = inner.IndexOf(":", StringComparison.Ordinal);
				if (colonIdx >= 0)
					inner = inner.Substring(0, colonIdx);

				// Strip default value (e.g. "=default")
				var eqIdx = inner.IndexOf("=", StringComparison.Ordinal);
				if (eqIdx >= 0)
					inner = inner.Substring(0, eqIdx);

				// Strip optional marker
				if (inner.Length > 0 && inner[inner.Length - 1] == '?')
					inner = inner.Substring(0, inner.Length - 1);

				return inner.Length == 0 ? null : inner;
			}

			// For mixed segments, extract the parameter name from within braces
			var start = segment.IndexOf("{", StringComparison.Ordinal);
			var end = segment.IndexOf("}", StringComparison.Ordinal);
			if (start >= 0 && end > start)
			{
				var token = segment.Substring(start + 1, end - start - 1);
				// Strip constraint
				var ci = token.IndexOf(":", StringComparison.Ordinal);
				if (ci >= 0) token = token.Substring(0, ci);
				// Strip default
				var ei = token.IndexOf("=", StringComparison.Ordinal);
				if (ei >= 0) token = token.Substring(0, ei);
				return token.Length == 0 ? null : token;
			}

			return null;
		}

		public static RouteTemplate Parse(string route, out string error)
		{
			error = null;

			if (string.IsNullOrWhiteSpace(route))
			{
				error = "Route cannot be empty";
				return null;
			}

			var raw = route.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
			var segments = new TemplateSegment[raw.Length];
			bool hasParameters = false;
			var seen = new HashSet<string>(StringComparer.Ordinal);

			for (var i = 0; i < raw.Length; i++)
			{
				var s = raw[i];
				bool hasBrace = s.IndexOf("{", StringComparison.Ordinal) >= 0;

				if (!hasBrace)
				{
					segments[i] = TemplateSegment.Literal(s);
					continue;
				}

				// Mixed segment: literal text around a parameter token
				if (!IsPureParameterSegment(s))
				{
					var openIdx = s.IndexOf("{", StringComparison.Ordinal);
					var closeIdx = s.IndexOf("}", StringComparison.Ordinal);
					if (openIdx < 0 || closeIdx < 0 || closeIdx <= openIdx + 1)
					{
						error = $"Route template segment \"{s}\" has malformed braces.";
						return null;
					}

					var prefix = s.Substring(0, openIdx);
					var suffix = closeIdx + 1 < s.Length ? s.Substring(closeIdx + 1) : "";

					// Reject multiple parameter tokens in one segment (e.g. "a-{x}-{y}")
					if (suffix.IndexOf("{", StringComparison.Ordinal) >= 0
						|| suffix.IndexOf("}", StringComparison.Ordinal) >= 0)
					{
						error = $"Route template segment \"{s}\" contains multiple parameter tokens. Only one parameter per segment is supported.";
						return null;
					}

					var token = s.Substring(openIdx + 1, closeIdx - openIdx - 1);

					// Parse constraint from token
					string constraint = null;
					var colonIdx = token.IndexOf(":", StringComparison.Ordinal);
					if (colonIdx >= 0)
					{
						constraint = token.Substring(colonIdx + 1);
						token = token.Substring(0, colonIdx);
					}

					if (string.IsNullOrEmpty(token) || !IsValidParameterName(token))
					{
						error = $"Route template segment \"{s}\" has an invalid parameter name.";
						return null;
					}

					if (!seen.Add(token))
					{
						error = $"Route template parameter \"{token}\" appears more than once.";
						return null;
					}

					if (constraint != null && !IsValidConstraint(constraint))
					{
						error = $"Route template constraint \":{constraint}\" is not recognized. Supported: int, long, double, bool, guid, alpha.";
						return null;
					}

					segments[i] = TemplateSegment.Mixed(token, prefix, suffix, constraint);
					hasParameters = true;
					continue;
				}

				// Pure parameter token: {name}, {name?}, {*name}, {name:int}, {name=default}
				var inner = s.Substring(1, s.Length - 2);

				bool isCatchAll = inner.Length > 0 && inner[0] == '*';
				if (isCatchAll)
					inner = inner.Substring(1);

				// Parse constraint
				string paramConstraint = null;
				bool optionalFromConstraint = false;
				var cIdx = inner.IndexOf(":", StringComparison.Ordinal);
				if (cIdx >= 0)
				{
					var constraintAndRest = inner.Substring(cIdx + 1);
					inner = inner.Substring(0, cIdx);

					// Constraint may be followed by "=default" (e.g. :int=5)
					var eqInConstraint = constraintAndRest.IndexOf("=", StringComparison.Ordinal);
					if (eqInConstraint >= 0)
					{
						paramConstraint = constraintAndRest.Substring(0, eqInConstraint);
						// Put the default value part back into inner for the
						// default-value parser below
						inner = inner + "=" + constraintAndRest.Substring(eqInConstraint + 1);
					}
					else
					{
						paramConstraint = constraintAndRest;
					}

					// Strip optional marker from constraint if present (e.g. "int?" → "int")
					if (paramConstraint.Length > 0 && paramConstraint[paramConstraint.Length - 1] == '?')
					{
						paramConstraint = paramConstraint.Substring(0, paramConstraint.Length - 1);
						optionalFromConstraint = true;
					}
				}

				// Parse default value
				string defaultValue = null;
				var eIdx = inner.IndexOf("=", StringComparison.Ordinal);
				if (eIdx >= 0)
				{
					defaultValue = inner.Substring(eIdx + 1);
					inner = inner.Substring(0, eIdx);
				}

				bool isOptional = optionalFromConstraint
					|| (inner.Length > 0 && inner[inner.Length - 1] == '?');
				if (!optionalFromConstraint && isOptional)
					inner = inner.Substring(0, inner.Length - 1);

				var name = inner;
				if (string.IsNullOrEmpty(name))
				{
					error = $"Route template segment \"{s}\" has no parameter name.";
					return null;
				}

				if (!IsValidParameterName(name))
				{
					error = $"Route template parameter \"{name}\" is not a valid identifier.";
					return null;
				}

				if (!seen.Add(name))
				{
					error = $"Route template parameter \"{name}\" appears more than once.";
					return null;
				}

				if (isCatchAll && i != raw.Length - 1)
				{
					error = $"Catch-all parameter \"{{*{name}}}\" must be the last segment in the route.";
					return null;
				}

				if (paramConstraint != null && !IsValidConstraint(paramConstraint))
				{
					error = $"Route template constraint \":{paramConstraint}\" is not recognized. Supported: int, long, double, bool, guid, alpha.";
					return null;
				}

				// Default value implies optional
				if (defaultValue != null)
					isOptional = true;

				// Optional/default parameters must be the last segment
				// (same as ASP.NET Core). Middle-optional is unmatchable
				// because the greedy matcher would consume the wrong segment.
				if (isOptional && i != raw.Length - 1)
				{
					error = $"Optional parameter \"{{{name}}}\" must be the last segment in the route. Optional parameters in the middle are not supported.";
					return null;
				}

				// Validate default value against constraint at registration time
				if (defaultValue != null && paramConstraint != null
					&& !SatisfiesConstraint(paramConstraint, defaultValue))
				{
					error = $"Default value \"{defaultValue}\" for parameter \"{name}\" does not satisfy the :{paramConstraint} constraint.";
					return null;
				}

				segments[i] = TemplateSegment.Parameter(name, isOptional, isCatchAll, paramConstraint, defaultValue);
				hasParameters = true;
			}

			return new RouteTemplate(segments) { HasParameters = hasParameters };
		}

		/// <summary>
		/// Checks if a value satisfies the constraint. Returns true if no constraint or value matches.
		/// </summary>
		public static bool SatisfiesConstraint(string constraint, string value)
		{
			if (string.IsNullOrEmpty(constraint))
				return true;
			if (value == null)
				return true; // null means absent; optional/required decides, not constraint

			switch (constraint)
			{
				case "int":
					return int.TryParse(value, out _);
				case "long":
					return long.TryParse(value, out _);
				case "double":
					return double.TryParse(value, System.Globalization.NumberStyles.Any,
						System.Globalization.CultureInfo.InvariantCulture, out _);
				case "bool":
					return bool.TryParse(value, out _);
				case "guid":
					return Guid.TryParse(value, out _);
				case "alpha":
					for (int i = 0; i < value.Length; i++)
						if (!char.IsLetter(value[i]))
							return false;
					return value.Length > 0;
				default:
					return true; // unknown constraint, be permissive
			}
		}

		static bool IsValidConstraint(string constraint)
		{
			switch (constraint)
			{
				case "int":
				case "long":
				case "double":
				case "bool":
				case "guid":
				case "alpha":
					return true;
				default:
					return false;
			}
		}

		static bool IsValidParameterName(string name)
		{
			if (string.IsNullOrEmpty(name))
				return false;

			if (!char.IsLetter(name[0]) && name[0] != '_')
				return false;

			for (var i = 1; i < name.Length; i++)
			{
				var c = name[i];
				if (!char.IsLetterOrDigit(c) && c != '_')
					return false;
			}

			return true;
		}

		internal readonly struct TemplateSegment
		{
			public readonly bool IsParameter;
			public readonly bool IsOptional;
			public readonly bool IsCatchAll;
			public readonly bool IsMixed;
			public readonly string Value; // param name or literal text
			public readonly string Prefix; // for mixed segments: text before {param}
			public readonly string Suffix; // for mixed segments: text after {param}
			public readonly string Constraint; // e.g. "int", "guid", null if none
			public readonly string DefaultValue; // e.g. "5", null if none

			TemplateSegment(bool isParameter, string value, bool isOptional = false,
				bool isCatchAll = false, string constraint = null, string defaultValue = null,
				bool isMixed = false, string prefix = null, string suffix = null)
			{
				IsParameter = isParameter;
				Value = value;
				IsOptional = isOptional;
				IsCatchAll = isCatchAll;
				IsMixed = isMixed;
				Prefix = prefix ?? "";
				Suffix = suffix ?? "";
				Constraint = constraint;
				DefaultValue = defaultValue;
			}

			public static TemplateSegment Literal(string text) =>
				new TemplateSegment(false, text);

			public static TemplateSegment Parameter(string name, bool isOptional = false,
				bool isCatchAll = false, string constraint = null, string defaultValue = null) =>
				new TemplateSegment(true, name, isOptional, isCatchAll, constraint, defaultValue);

			public static TemplateSegment Mixed(string paramName, string prefix, string suffix, string constraint = null) =>
				new TemplateSegment(true, paramName, isMixed: true, prefix: prefix, suffix: suffix, constraint: constraint);
		}
	}
}
