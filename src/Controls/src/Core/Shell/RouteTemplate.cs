#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	// Lightweight parser / matcher for path-parameter route templates such as
	// "product/{sku}" or "product/{sku}/review". Templates are an additive,
	// opt-in extension of <see cref="Routing.RegisterRoute(string, Type)"/> —
	// existing literal routes are unchanged.
	//
	// Rules (v1):
	//   * Each segment is either fully literal (e.g. "product") or a single
	//     parameter token of the form "{name}". Mixed segments such as
	//     "product-{sku}" are rejected.
	//   * Parameter names follow C# identifier rules.
	//   * Optional '?' (e.g. "{sku?}") and catch-all '*' (e.g. "{*path}") are
	//     parsed but are not yet honored by the matcher; they are reserved
	//     for a follow-up.
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

			return segment.Length >= 3
				&& segment[0] == '{'
				&& segment[segment.Length - 1] == '}';
		}

		public static string GetSegmentParameterName(string segment)
		{
			if (!IsTemplateSegment(segment))
				return null;

			var inner = segment.Substring(1, segment.Length - 2);

			// Strip catch-all marker
			if (inner.Length > 0 && inner[0] == '*')
				inner = inner.Substring(1);

			// Strip optional marker
			if (inner.Length > 0 && inner[inner.Length - 1] == '?')
				inner = inner.Substring(0, inner.Length - 1);

			return inner.Length == 0 ? null : inner;
		}

		// Parses a route string (e.g. "product/{sku}"). Returns null and sets
		// <paramref name="error"/> if the template is malformed. A route with
		// no '{' returns a template with HasParameters=false; callers may
		// still treat that route as a plain literal.
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
				bool startsWithBrace = s.IndexOf("{", StringComparison.Ordinal) >= 0;
				bool endsWithBrace = s.IndexOf("}", StringComparison.Ordinal) >= 0;

				if (!startsWithBrace && !endsWithBrace)
				{
					segments[i] = TemplateSegment.Literal(s);
					continue;
				}

				if (!IsTemplateSegment(s))
				{
					error = $"Route template segment \"{s}\" must be a single parameter token (e.g. \"{{name}}\") or a pure literal. Mixed segments are not supported.";
					return null;
				}

				var name = GetSegmentParameterName(s);
				if (string.IsNullOrEmpty(name))
				{
					error = $"Route template segment \"{s}\" has no parameter name.";
					return null;
				}

				// Reject optional and catch-all syntax — not yet implemented.
				var inner = s.Substring(1, s.Length - 2);
				if (inner.Length > 0 && inner[0] == '*')
				{
					error = $"Catch-all route parameters (\"{{*{name}}}\") are not yet supported.";
					return null;
				}
				if (inner.Length > 0 && inner[inner.Length - 1] == '?')
				{
					error = $"Optional route parameters (\"{{{name}?}}\") are not yet supported.";
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

				segments[i] = TemplateSegment.Parameter(name);
				hasParameters = true;
			}

			return new RouteTemplate(segments) { HasParameters = hasParameters };
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
			public readonly string Value;

			TemplateSegment(bool isParameter, string value)
			{
				IsParameter = isParameter;
				Value = value;
			}

			public static TemplateSegment Literal(string text) => new TemplateSegment(false, text);
			public static TemplateSegment Parameter(string name) => new TemplateSegment(true, name);
		}
	}
}
