using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Maui.Graphics.Text
{
    public static class TextAttributesExtensions
    {
        public static string GetAttribute(
            this ITextAttributes attributes,
            TextAttribute type,
            string defaultValue = null)
        {
            if (attributes != null)
            {
                if (attributes.TryGetValue(type, out var value))
                    return value;
            }

            return defaultValue;
        }

        public static void SetAttribute(
            this Dictionary<TextAttribute, string> attributes,
            TextAttribute type,
            string value)
        {
            if (attributes != null)
            {
                if (value == null)
                    attributes.Remove(type);
                else
                    attributes[type] = value;
            }
        }

        public static void RemoveAttribute(
            this Dictionary<TextAttribute, string> attributes,
            TextAttribute type)
        {
            attributes?.Remove(type);
        }

        public static int GetIntAttribute(
            this ITextAttributes attributes,
            TextAttribute type,
            int defaultValue)
        {
            var value = attributes.GetAttribute(type);
            if (value != null)
            {
                if (int.TryParse(value, out var intValue))
                    return intValue;
            }

            return defaultValue;
        }

        public static void SetIntAttribute(
            this Dictionary<TextAttribute, string> attributes,
            TextAttribute type,
            int value,
            int defaultValue)
        {
            if (value == defaultValue)
                attributes.RemoveAttribute(type);
            else
                attributes.SetAttribute(type, value.ToString(CultureInfo.InvariantCulture));
        }

        public static float GetFloatAttribute(
            this ITextAttributes attributes,
            TextAttribute type,
            float defaultValue)
        {
            var value = attributes.GetAttribute(type);
            if (value != null)
            {
                if (float.TryParse(value, out var floatValue))
                    return floatValue;
            }

            return defaultValue;
        }

        public static void SetFloatAttribute(
            this Dictionary<TextAttribute, string> attributes,
            TextAttribute type,
            float value,
            float defaultValue)
        {
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if (value == defaultValue)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                attributes.RemoveAttribute(type);
            else
                attributes.SetAttribute(type, value.ToString(CultureInfo.InvariantCulture));
        }

        public static bool GetBoolAttribute(
            this ITextAttributes attributes,
            TextAttribute type,
            bool defaultValue = false)
        {
            var value = attributes.GetAttribute(type);
            if (value != null)
            {
                if (bool.TryParse(value, out var boolValue))
                    return boolValue;
            }

            return defaultValue;
        }

        public static void SetBoolAttribute(
            this Dictionary<TextAttribute, string> attributes,
            TextAttribute type,
            bool value,
            bool defaultValue = false)
        {
            if (value == defaultValue)
                attributes.RemoveAttribute(type);
            else
                attributes.SetAttribute(type, value.ToString());
        }

        public static T GetEnumAttribute<T>(
            this ITextAttributes attributes,
            TextAttribute type,
            T defaultValue) where T : struct
        {
            var value = attributes.GetAttribute(type);
            if (value != null)
            {
                if (Enum.TryParse(value, out T enumValue))
                    return enumValue;
            }

            return defaultValue;
        }

        public static void SetEnumAttribute<T>(
            this Dictionary<TextAttribute, string> attributes,
            TextAttribute type,
            T value,
            T defaultValue) where T : struct
        {
            if (Equals(value, defaultValue))
                attributes.RemoveAttribute(type);
            else
                attributes.SetAttribute(type, value.ToString());
        }
    }
}