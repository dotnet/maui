using System.Collections.Generic;

namespace Microsoft.Maui.Graphics.Text
{
    public abstract class AbstractAttributedText : IAttributedText
    {
        public abstract string Text { get; }
        public abstract IReadOnlyList<IAttributedTextRun> Runs { get; }
        
        public bool Optimal { get; protected set;  }
    }
}
