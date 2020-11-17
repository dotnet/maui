using System.Collections.Generic;

namespace System.Graphics.Text
{
    public interface IAttributedText
    {
        string Text { get; }
        IReadOnlyList<IAttributedTextRun> Runs { get; }
    }
}