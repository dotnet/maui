using System.Graphics.Text.Immutable;
using Markdig.Syntax.Inlines;

namespace System.Graphics.Text.Renderer
{
    public class EmphasisInlineRenderer : AttributedTextObjectRenderer<EmphasisInline>
    {
        protected override void Write(AttributedTextRenderer renderer, EmphasisInline obj)
        {
            var start = renderer.Count;
            renderer.WriteChildren(obj);
            var length = renderer.Count - start;

            var bold = IsBold(obj);
            var underline = IsUnderline(obj);
            var italic = IsItalic(obj);
            var strikethrough = IsStrikethrough(obj);
            var subscript = IsSubscript(obj);
            var superscript = IsSuperscript(obj);

            if (length > 0 && (bold || italic || strikethrough || subscript || superscript || underline))
            {
                var attributes = new TextAttributes();
                attributes.SetBold(bold);
                attributes.SetItalic(italic);
                attributes.SetStrikethrough(strikethrough);
                attributes.SetSubscript(subscript);
                attributes.SetSuperscript(superscript);
                attributes.SetUnderline(underline);
                renderer.AddTextRun(start, length, attributes);
            }
        }

        public bool IsUnderline(EmphasisInline obj)
        {
            if (obj.DelimiterChar == '_')
                return obj.DelimiterCount == 2;

            return false;
        }

        public bool IsBold(EmphasisInline obj)
        {
            if (obj.DelimiterChar == '*')
                return obj.DelimiterCount == 2;

            return false;
        }

        public bool IsItalic(EmphasisInline obj)
        {
            if (obj.DelimiterChar == '*')
                return obj.DelimiterCount != 2;

            return false;
        }

        public bool IsStrikethrough(EmphasisInline obj)
        {
            if (obj.DelimiterChar == '~')
                return obj.DelimiterCount == 2;

            return false;
        }

        public bool IsSubscript(EmphasisInline obj)
        {
            if (obj.DelimiterChar == '~')
                return obj.DelimiterCount != 2;

            return false;
        }

        public bool IsSuperscript(EmphasisInline obj)
        {
            if (obj.DelimiterChar == '^')
                return obj.DelimiterCount != 2;

            return false;
        }
    }
}