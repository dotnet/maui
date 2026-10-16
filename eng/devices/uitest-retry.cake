using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

public static class UITestRetry
{
    public static string GetFilter(string originalFilter, IEnumerable<XDocument> reports)
    {
        // Only narrow the pipeline's ORed category groups, not arbitrary user filters.
        if (string.IsNullOrEmpty(originalFilter) ||
            !Regex.IsMatch(originalFilter, @"^TestCategory=[A-Za-z0-9_]+(\|TestCategory=[A-Za-z0-9_]+)*$"))
            throw new InvalidOperationException("The original filter is not an exact category group.");

        var categories = originalFilter.Split('|').Select(term => term.Substring("TestCategory=".Length)).ToArray();
        var failedCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var reportCount = 0;
        foreach (var report in reports)
        {
            reportCount++;
            var run = report.Root;
            var cases = run?.Descendants("test-case").ToList();
            if (run?.Name != "test-run" || (string)run.Attribute("result") != "Failed" ||
                (string)run.Attribute("label") == "Cancelled" || cases.Count == 0 ||
                (int?)run.Attribute("total") != cases.Count ||
                cases.Any(test => !new[] { "Passed", "Failed", "Skipped", "Inconclusive", "Warning" }.Contains((string)test.Attribute("result"))))
                throw new InvalidOperationException("NUnit did not produce a complete failed test run.");

            var failures = run.Descendants().Where(node =>
                (string)node.Attribute("result") == "Failed" &&
                (node.Name == "test-case" || (node.Name == "test-suite" && (string)node.Attribute("site") != "Child"))).ToList();
            if (failures.Count == 0)
                throw new InvalidOperationException("No failed tests or fixtures were found.");

            foreach (var failure in failures)
            {
                var matches = failure.AncestorsAndSelf()
                    .Elements("properties").Elements("property")
                    .Where(property => (string)property.Attribute("name") == "Category")
                    .Select(property => (string)property.Attribute("value"))
                    .Intersect(categories, StringComparer.OrdinalIgnoreCase).ToList();
                if (matches.Count == 0)
                    throw new InvalidOperationException("A failure could not be mapped to the original category group.");
                failedCategories.UnionWith(matches);
            }
        }

        if (reportCount == 0)
            throw new InvalidOperationException("No NUnit result files were found.");

        return string.Join("|", categories.Where(failedCategories.Contains).Select(category => $"TestCategory={category}"));
    }

    public static void MergeResults(XDocument original, XDocument retry)
    {
        XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        foreach (var section in new[] { "Results", "TestDefinitions", "TestEntries" })
        {
            var destination = original.Root.Element(ns + section);
            var source = retry.Root.Element(ns + section);
            if (destination == null || source == null)
                throw new InvalidOperationException($"TRX is missing {section}.");

            var key = section == "TestDefinitions" ? "id" : "testId";
            var replacedIds = new HashSet<string>(source.Elements().Select(node => (string)node.Attribute(key)));
            destination.Elements().Where(node => replacedIds.Contains((string)node.Attribute(key))).Remove();
            destination.Add(source.Elements().Select(node => new XElement(node)));
        }

        var results = original.Root.Element(ns + "Results").Elements().ToList();
        var summary = original.Root.Element(ns + "ResultSummary");
        var retrySummary = retry.Root.Element(ns + "ResultSummary");
        if (summary == null || retrySummary == null)
            throw new InvalidOperationException("TRX is missing ResultSummary.");

        var mergedSummary = new XElement(retrySummary);
        var counters = mergedSummary.Element(ns + "Counters");
        if (counters == null)
            throw new InvalidOperationException("TRX is missing result counters.");

        foreach (var counter in counters.Attributes().ToList())
        {
            var count = counter.Name.LocalName == "total" ? results.Count :
                counter.Name.LocalName == "executed" ? results.Count(result => (string)result.Attribute("outcome") != "NotExecuted") :
                results.Count(result => string.Equals((string)result.Attribute("outcome"), counter.Name.LocalName, StringComparison.OrdinalIgnoreCase));
            counter.Value = count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (results.Any(result => (string)result.Attribute("outcome") == "Failed"))
            mergedSummary.SetAttributeValue("outcome", "Failed");
        summary.ReplaceWith(mergedSummary);
        original.Root.Element(ns + "Times")?.SetAttributeValue("finish", (string)retry.Root.Element(ns + "Times")?.Attribute("finish"));
    }
}
