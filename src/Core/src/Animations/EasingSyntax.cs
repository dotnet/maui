#nullable enable
using System;
using System.Globalization;

namespace Microsoft.Maui
{
	/// <summary>
	/// The text grammar for <see cref="Easing"/> shared by the runtime type converter and the XAML source
	/// generator (which links this file), so a literal that builds is a literal that loads.
	/// </summary>
	/// <remarks>
	/// <para>Accepts the named curves (<c>Linear</c>, <c>CubicOut</c>, <c>Hold</c>, …, optionally prefixed with
	/// <c>Easing.</c>) and the functions <c>CubicBezier(x1, y1, x2, y2)</c>, <c>Spring(bounce)</c> and
	/// <c>Steps(count[, jumpAtStart])</c>.</para>
	/// <para>The CSS spellings parse too, so a curve copied from a stylesheet, a design tool or browser
	/// developer tools works verbatim: <c>cubic-bezier(0.34, 1.56, 0.64, 1)</c>, <c>steps(4, jump-start)</c>,
	/// <c>ease</c>, <c>ease-in</c>, <c>ease-out</c>, <c>ease-in-out</c>, <c>step-start</c> and <c>step-end</c>.
	/// Function names match case-insensitively, with or without dashes.</para>
	/// </remarks>
	internal static class EasingSyntax
	{
		/// <summary>The bounce of <c>Spring()</c> with no argument.</summary>
		internal const double DefaultSpringBounce = 0.2;

		internal enum Kind
		{
			Invalid,
			Named,
			CubicBezier,
			Spring,
			Steps,
		}

		internal readonly struct Parsed
		{
			public Parsed(Kind kind, string? name, double a = 0, double b = 0, double c = 0, double d = 0, bool flag = false)
			{
				Kind = kind;
				Name = name;
				A = a;
				B = b;
				C = c;
				D = d;
				Flag = flag;
			}

			public Kind Kind { get; }

			/// <summary>For <see cref="Kind.Named"/>, the canonical static member name on <see cref="Easing"/>.</summary>
			public string? Name { get; }
			public double A { get; }
			public double B { get; }
			public double C { get; }
			public double D { get; }
			public bool Flag { get; }

			/// <summary>Why the text was rejected, for <see cref="Kind.Invalid"/>.</summary>
			public string? Error => Kind == Kind.Invalid ? Name : null;
		}

		/// <summary>The static <see cref="Easing"/> members a name can resolve to, in canonical spelling.</summary>
		internal static readonly string[] NamedCurves =
		{
			"Linear", "SinIn", "SinOut", "SinInOut", "CubicIn", "CubicOut", "CubicInOut",
			"BounceIn", "BounceOut", "SpringIn", "SpringOut", "Hold",
		};

		public static Parsed Parse(string? text)
		{
			if (text is null)
				return Invalid("An easing can't be empty.");

			var value = text.Trim();
			if (value.Length == 0)
				return Invalid("An easing can't be empty.");

			if (value.StartsWith("Easing.", StringComparison.OrdinalIgnoreCase))
				value = value.Substring("Easing.".Length);

			foreach (var name in NamedCurves)
			{
				if (string.Equals(name, value, StringComparison.OrdinalIgnoreCase))
					return new Parsed(Kind.Named, name);
			}

			// CSS keywords, as defined by CSS Easing Functions Level 1.
			switch (Normalize(value))
			{
				case "ease":
					return new Parsed(Kind.CubicBezier, null, 0.25, 0.1, 0.25, 1);
				case "easein":
					return new Parsed(Kind.CubicBezier, null, 0.42, 0, 1, 1);
				case "easeout":
					return new Parsed(Kind.CubicBezier, null, 0, 0, 0.58, 1);
				case "easeinout":
					return new Parsed(Kind.CubicBezier, null, 0.42, 0, 0.58, 1);
				case "stepstart":
					return new Parsed(Kind.Steps, null, 1, flag: true);
				case "stepend":
					return new Parsed(Kind.Steps, null, 1);
			}

			var open = IndexOfOpenParenthesis(value);
			if (open <= 0 || value[value.Length - 1] != ')')
				return Invalid($"\"{text}\" is not a known easing. Use a name such as CubicOut, or CubicBezier(x1, y1, x2, y2), Spring(bounce) or Steps(count).");

			var function = Normalize(value.Substring(0, open));
			var arguments = value.Substring(open + 1, value.Length - open - 2).Split(',');

			switch (function)
			{
				case "cubicbezier":
					return ParseCubicBezier(text, arguments);
				case "spring":
					return ParseSpring(text, arguments);
				case "steps":
					return ParseSteps(text, arguments);
				default:
					return Invalid($"\"{value.Substring(0, open).Trim()}\" is not a known easing function. Use CubicBezier, Spring or Steps.");
			}
		}

		static Parsed ParseCubicBezier(string text, string[] arguments)
		{
			if (arguments.Length != 4)
				return Invalid($"CubicBezier takes four numbers (x1, y1, x2, y2): \"{text}\".");

			if (!TryNumber(arguments[0], out var x1) || !TryNumber(arguments[1], out var y1)
				|| !TryNumber(arguments[2], out var x2) || !TryNumber(arguments[3], out var y2))
				return Invalid($"CubicBezier takes four numbers (x1, y1, x2, y2): \"{text}\".");

			if (x1 < 0 || x1 > 1 || x2 < 0 || x2 > 1)
				return Invalid($"CubicBezier x1 and x2 must be between 0 and 1: \"{text}\".");

			return new Parsed(Kind.CubicBezier, null, x1, y1, x2, y2);
		}

		static Parsed ParseSpring(string text, string[] arguments)
		{
			var bounce = DefaultSpringBounce;
			var empty = arguments.Length == 1 && arguments[0].Trim().Length == 0;
			if (!empty)
			{
				if (arguments.Length != 1 || !TryNumber(arguments[0], out bounce))
					return Invalid($"Spring takes one number, its bounce between 0 and 0.95: \"{text}\".");
			}

			if (bounce < 0 || bounce > 0.95)
				return Invalid($"Spring bounce must be between 0 and 0.95: \"{text}\".");

			return new Parsed(Kind.Spring, null, bounce);
		}

		static Parsed ParseSteps(string text, string[] arguments)
		{
			if (arguments.Length < 1 || arguments.Length > 2 || !TryNumber(arguments[0], out var count)
				|| count < 1 || count != Math.Floor(count) || count > int.MaxValue)
				return Invalid($"Steps takes a whole number of steps of at least 1, and optionally jump-start or jump-end: \"{text}\".");

			var jumpAtStart = false;
			if (arguments.Length == 2)
			{
				switch (Normalize(arguments[1]))
				{
					case "jumpstart":
					case "start":
					case "true":
						jumpAtStart = true;
						break;
					case "jumpend":
					case "end":
					case "false":
						break;
					default:
						return Invalid($"Steps position must be jump-start or jump-end: \"{text}\".");
				}
			}

			return new Parsed(Kind.Steps, null, count, flag: jumpAtStart);
		}

		// A loop rather than IndexOf: the file also builds for netstandard2.0 (the source generator), where the
		// char overload has no StringComparison and the analyzers disagree about the alternatives.
		static int IndexOfOpenParenthesis(string text)
		{
			for (var i = 0; i < text.Length; i++)
			{
				if (text[i] == '(')
					return i;
			}
			return -1;
		}

		static bool TryNumber(string text, out double value)
			=> double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
				&& !double.IsNaN(value) && !double.IsInfinity(value);

		// Lower-cased, trimmed and without dashes, so cubic-bezier, CubicBezier and cubicbezier all match.
		static string Normalize(string text)
		{
			var trimmed = text.Trim();
			var builder = new System.Text.StringBuilder(trimmed.Length);
			foreach (var c in trimmed)
			{
				if (c != '-')
					builder.Append(char.ToLowerInvariant(c));
			}
			return builder.ToString();
		}

		static Parsed Invalid(string message) => new Parsed(Kind.Invalid, message);

		/// <summary>The invariant text that parses back to the same curve.</summary>
		public static string FormatCubicBezier(double x1, double y1, double x2, double y2)
			=> string.Format(CultureInfo.InvariantCulture, "CubicBezier({0}, {1}, {2}, {3})", x1, y1, x2, y2);

		public static string FormatSpring(double bounce)
			=> string.Format(CultureInfo.InvariantCulture, "Spring({0})", bounce);

		public static string FormatSteps(int count, bool jumpAtStart)
			=> jumpAtStart
				? string.Format(CultureInfo.InvariantCulture, "Steps({0}, jump-start)", count)
				: string.Format(CultureInfo.InvariantCulture, "Steps({0})", count);
	}
}
