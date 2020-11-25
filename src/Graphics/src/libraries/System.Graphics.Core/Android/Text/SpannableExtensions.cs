using System.Collections.Generic;
using System.Graphics.Text.Immutable;
using System.IO;
using Android.Text;

namespace System.Graphics.Android.Text
{
    public static class SpannableExtensions
    {
        public static AttributedText AsAttributedText(this ISpannable target)
        {
            if (target != null)
            {
                using (var textWriter = new StringWriter())
                {
                    var runs = CreateRuns(target, textWriter);
                    return new AttributedText(textWriter.ToString(), runs);
                }
            }

            return null;
        }

        private static List<AttributedTextRun> CreateRuns(
            ISpannable target,
            TextWriter writer)
        {
            var runs = new List<AttributedTextRun>();

            var spans = target.GetSpans(0, target.Length(), null);
            if (spans != null)
            {
                foreach (var span in spans)
                {
                    System.Console.WriteLine(span);
                }
            }

            writer.Write(target.ToString());
            return runs;
        }
    }
}