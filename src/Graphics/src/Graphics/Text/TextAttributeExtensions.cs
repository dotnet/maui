using System.Collections.Generic;
using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics.Text
{
	public static class TextAttributeExtensions
	{
		public const float DefaultFontSize = 12f;

		public static string GetFontName(this ITextAttributes attributes)
		{
			return attributes.GetAttribute(TextAttribute.FontName);
		}

		public static void SetFontName(
			this Dictionary<TextAttribute, string> attributes,
			string value)
		{
			attributes.SetAttribute(TextAttribute.FontName, value);
		}

		public static float GetFontSize(
			this ITextAttributes attributes,
			float? fontSize = null)
		{
			return attributes.GetFloatAttribute(TextAttribute.FontSize, fontSize ?? DefaultFontSize);
		}

		public static void SetFontSize(
			this Dictionary<TextAttribute, string> attributes,
			float value)
		{
			attributes.SetFloatAttribute(TextAttribute.FontSize, value, DefaultFontSize);
		}

		public static bool GetUnderline(this ITextAttributes attributes)
		{
			return attributes.GetBoolAttribute(TextAttribute.Underline);
		}

		public static void SetUnderline(
			this Dictionary<TextAttribute, string> attributes,
			bool value)
		{
			attributes.SetBoolAttribute(TextAttribute.Underline, value);
		}

		public static bool GetBold(this ITextAttributes attributes)
		{
			return attributes.GetBoolAttribute(TextAttribute.Bold);
		}

		public static void SetBold(
			this Dictionary<TextAttribute, string> attributes,
			bool value)
		{
			attributes.SetBoolAttribute(TextAttribute.Bold, value);
		}

		public static bool GetItalic(this ITextAttributes attributes)
		{
			return attributes.GetBoolAttribute(TextAttribute.Italic);
		}

		public static void SetItalic(
			this Dictionary<TextAttribute, string> attributes,
			bool value)
		{
			attributes.SetBoolAttribute(TextAttribute.Italic, value);
		}

		public static bool GetUnorderedList(this ITextAttributes attributes)
		{
			return attributes.GetBoolAttribute(TextAttribute.UnorderedList);
		}

		public static void SetUnorderedList(
			this Dictionary<TextAttribute, string> attributes,
			bool value)
		{
			attributes.SetBoolAttribute(TextAttribute.UnorderedList, value);
		}

		public static MarkerType GetMarker(this ITextAttributes attributes)
		{
			return attributes.GetEnumAttribute(TextAttribute.UnorderedList, MarkerType.ClosedCircle);
		}

		public static void SetMarker(
			this Dictionary<TextAttribute, string> attributes,
			MarkerType value)
		{
			attributes.SetEnumAttribute(TextAttribute.UnorderedList, value, MarkerType.ClosedCircle);
		}

		public static bool GetStrikethrough(this ITextAttributes attributes)
		{
			return attributes.GetBoolAttribute(TextAttribute.Strikethrough);
		}

		public static void SetStrikethrough(
			this Dictionary<TextAttribute, string> attributes,
			bool value)
		{
			attributes.SetBoolAttribute(TextAttribute.Strikethrough, value);
		}

		public static bool GetSuperscript(this ITextAttributes attributes)
			=> attributes.GetBoolAttribute(TextAttribute.Superscript);

		public static void SetSuperscript(this Dictionary<TextAttribute, string> attributes, bool value)
			=> attributes.SetBoolAttribute(TextAttribute.Superscript, value);

		public static bool GetSubscript(this ITextAttributes attributes)
			=> attributes.GetBoolAttribute(TextAttribute.Subscript);

		public static void SetSubscript(this Dictionary<TextAttribute, string> attributes, bool value)
			=> attributes.SetBoolAttribute(TextAttribute.Subscript, value);

		public static string GetForegroundColor(this ITextAttributes attributes)
			=> attributes.GetAttribute(TextAttribute.Color);

		public static void SetForegroundColor(this Dictionary<TextAttribute, string> attributes, string value)
			=> attributes.SetAttribute(TextAttribute.Color, value);

		public static string GetBackgroundColor(this ITextAttributes attributes)
			=> attributes.GetAttribute(TextAttribute.Background);

		public static void SetBackgroundColor(this Dictionary<TextAttribute, string> attributes, string value)
			=> attributes.SetAttribute(TextAttribute.Background, value);

		public static ITextAttributes Union(
			this IReadOnlyDictionary<TextAttribute, string> first,
			IReadOnlyDictionary<TextAttribute, string> second)
		{
			return new TextAttributes(first, second);
		}
	}
}
