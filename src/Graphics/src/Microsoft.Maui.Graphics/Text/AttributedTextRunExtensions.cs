using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics.Text.Immutable;

namespace Microsoft.Maui.Graphics.Text
{
    public static class AttributedTextRunExtensions
    {
        public static int GetEnd(this IAttributedTextRun run)
        {
            if (run == null)
                return 0;

            return run.Start + run.Length;
        }

        public static bool Intersects(
            this IAttributedTextRun first,
            IAttributedTextRun second)
        {
            if (first == null || second == null)
                return false;

            if (first.Start < second.GetEnd())
                return first.GetEnd() > second.Start;

            return false;
        }

        public static bool Intersects(
            this IAttributedTextRun first,
            int start,
            int length)
        {
            if (first == null)
                return false;

            var end = start + length;
            if (first.Start < end)
                return first.GetEnd() > start;

            return false;
        }

        public static bool IntersectsExactly(
            this IAttributedTextRun first,
            IAttributedTextRun second)
        {
            if (first == null || second == null)
                return false;

            return first.Start == second.Start && first.Length == second.Length;
        }

        public static bool IntersectsExactly(
            this IAttributedTextRun first,
            int start,
            int length)
        {
            if (first == null)
                return false;

            return first.Start == start && first.Length == length;
        }

        public static IList<IAttributedTextRun> CalculatedIntersections(
            this IAttributedTextRun first,
            IAttributedTextRun second)
        {
            List<IAttributedTextRun> intersections = new List<IAttributedTextRun>();

            var combined = first.Attributes.Union(second.Attributes);
            if (first.Start == second.Start)
            {
                if (first.Length == second.Length)
                {
                    intersections.Add(new AttributedTextRun(first.Start, first.Length, combined));
                }
                else if (first.Length > second.Length)
                {
                    var start1 = first.Start;
                    var length1 = Math.Min(first.Length, second.Length);
                    var start2 = start1 + length1;
                    var length2 = Math.Max(first.Length, second.Length) - length1;

                    intersections.Add(new AttributedTextRun(start1, length1, combined));
                    intersections.Add(new AttributedTextRun(start2, length2, first.Attributes));
                }
                else if (first.Length < second.Length)
                {
                    var start1 = first.Start;
                    var length1 = Math.Min(first.Length, second.Length);
                    var start2 = start1 + length1;
                    var length2 = Math.Max(first.Length, second.Length) - length1;

                    intersections.Add(new AttributedTextRun(start1, length1, combined));
                    intersections.Add(new AttributedTextRun(start2, length2, second.Attributes));
                }
            }
            else if (first.GetEnd() == second.GetEnd())
            {
                if (first.Start < second.Start)
                {
                    var start1 = first.Start;
                    var length1 = second.Start - first.Start;
                    var start2 = start1 + length1;
                    var length2 = Math.Max(first.Length, second.Length) - length1;

                    intersections.Add(new AttributedTextRun(start1, length1, first.Attributes));
                    intersections.Add(new AttributedTextRun(start2, length2, combined));
                }
                else
                {
                    var start1 = second.Start;
                    var length1 = first.Start - second.Start;
                    var start2 = start1 + length1;
                    var length2 = Math.Max(first.Length, second.Length) - length1;

                    intersections.Add(new AttributedTextRun(start1, length1, second.Attributes));
                    intersections.Add(new AttributedTextRun(start2, length2, combined));
                }
            }
            else
            {
                if (first.Start < second.Start)
                {
                    var start1 = first.Start;
                    var length1 = second.Start - first.Start;
                    var start2 = start1 + length1;
                    var length2 = second.Length;
                    var start3 = start2 + length2;
                    var length3 = Math.Max(first.Length, second.Length) - (length1 + length2);

                    intersections.Add(new AttributedTextRun(start1, length1, first.Attributes));
                    intersections.Add(new AttributedTextRun(start2, length2, combined));
                    intersections.Add(new AttributedTextRun(start3, length3, first.GetEnd() > second.GetEnd() ? first.Attributes : second.Attributes));
                }
                else
                {
                    var start1 = second.Start;
                    var length1 = first.Start - second.Start;
                    var start2 = start1 + length1;
                    var length2 = first.Length;
                    var start3 = start2 + length2;
                    var length3 = Math.Max(first.Length, second.Length) - (length1 + length2);

                    intersections.Add(new AttributedTextRun(start1, length1, second.Attributes));
                    intersections.Add(new AttributedTextRun(start2, length2, combined));
                    intersections.Add(new AttributedTextRun(start3, length3, first.GetEnd() > second.GetEnd() ? first.Attributes : second.Attributes));
                }
            }

            return intersections;
        }

        public static void Optimize(this List<IAttributedTextRun> runs, int textLength)
        {
            // Loop through the runs and make sure that they don't extend beyond the bounds of the text.
            for (int i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                var end = run.GetEnd();

                if (run.Start < 0 || end > textLength)
                {
                    var start = Math.Max(run.Start, 0);
                    var maxLength = textLength - start;
                    var length = Math.Min(run.Length, maxLength);
                    if (length > 0)
                        runs[i] = new AttributedTextRun(start, length, run.Attributes);
                    else
                        runs.RemoveAt(i--);
                }
            }

            runs.Sort(AttributedTextRunComparer.Instance);

            // Loop through the runs and join the ones that overlap.
            IAttributedTextRun previous = null;
            for (int i = 0; i < runs.Count; i++)
            {
                var run = runs[i];

                if (previous != null)
                {
                    if (previous.IntersectsExactly(run))
                    {
                        var combined = previous.Attributes.Union(run.Attributes);
                        run = runs[i - 1] = new AttributedTextRun(run.Start, run.Length, combined);
                        runs.RemoveAt(i--);
                    }
                    else if (previous.Intersects(run))
                    {
                        var intersections = previous.CalculatedIntersections(run);
                        runs.RemoveAt(i--);
                        runs.RemoveAt(i);
                        runs.InsertRange(i++, intersections);
                        run = runs[i];
                        runs.Sort(AttributedTextRunComparer.Instance);
                    }
                }

                previous = run;
            }
        }
    }
}
