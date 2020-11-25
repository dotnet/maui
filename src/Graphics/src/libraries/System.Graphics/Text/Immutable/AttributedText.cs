using System.Collections.Generic;

namespace System.Graphics.Text.Immutable
{
    public class AttributedText : AbstractAttributedText
    {
        public AttributedText(
            string text,
            IReadOnlyList<IAttributedTextRun> runs,
            bool optimal = false)
        {
            Text = text;
            Runs = runs;
            Optimal = optimal;
        }

        public override string Text { get; }

        public override IReadOnlyList<IAttributedTextRun> Runs { get; }
    }
}