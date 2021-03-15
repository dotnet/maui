using System.Collections.Generic;

namespace Microsoft.Maui.Graphics.Text
{
    public class AttributedTextRunComparer : IComparer<IAttributedTextRun>
    {
        public static readonly AttributedTextRunComparer Instance = new AttributedTextRunComparer();

        public int Compare(IAttributedTextRun first, IAttributedTextRun second)
        {
            if (first.Start < second.Start)
                return -1;

            if (first.Start == second.Start)
            {
                if (first.Length < second.Length)
                    return -1;

                if (first.Length == second.Length)
                    return 0;
            }

            return 1;
        }
    }
}