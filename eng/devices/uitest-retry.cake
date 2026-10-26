using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

public static class UITestRetry
{
    public static void RunSetup(Action setup, bool retry, Action<Exception> onRetry)
    {
        var maxAttempts = retry ? 5 : 1;
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                setup();
                return;
            }
            catch (Exception ex)
            {
                if (attempt + 1 >= maxAttempts)
                    throw;
                onRetry(ex);
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds((attempt + 1) * (attempt + 1)));
            }
        }
    }

    public static string GetFilter(string originalFilter, IEnumerable<XDocument> reports)
    {
        // Only narrow the pipeline's ORed category groups, not arbitrary user filters.
        if (string.IsNullOrEmpty(originalFilter) ||
            !Regex.IsMatch(originalFilter, @"^TestCategory=[A-Za-z0-9_]+(\|TestCategory=[A-Za-z0-9_]+)*$"))
            throw new InvalidOperationException("The original filter is not an exact category group.");

        var categories = originalFilter.Split('|').Select(term => term.Substring("TestCategory=".Length)).ToArray();
        var originalCategoryFilter = "(" + string.Join(" or ", categories.Select(category => $"cat == {QuoteFilterValue(category)}")) + ")";
        var filters = new HashSet<string>(StringComparer.Ordinal);
        var reportList = reports.ToList();
        var reportCount = 0;
        foreach (var report in reportList)
        {
            reportCount++;
            var run = report.Root;
            var cases = run?.Descendants("test-case").ToList();
            if (run?.Name != "test-run" || (string)run.Attribute("result") != "Failed" ||
                (string)run.Attribute("label") == "Cancelled" || run.Element("failure") != null || cases.Count == 0 ||
                (int?)run.Attribute("total") != cases.Count ||
                cases.Any(test => (string)test.Attribute("label") == "Cancelled") ||
                cases.Any(test => !new[] { "Passed", "Failed", "Skipped", "Inconclusive", "Warning" }.Contains((string)test.Attribute("result"))))
                throw new InvalidOperationException("NUnit did not produce a complete failed test run.");

            var failures = run.Descendants().Where(node =>
                (string)node.Attribute("result") == "Failed" &&
                (node.Name == "test-case" || (node.Name == "test-suite" && (string)node.Attribute("site") != "Child"))).ToList();
            if (failures.Count == 0)
                throw new InvalidOperationException("No failed tests or fixtures were found.");

            foreach (var failure in failures)
            {
                // Feature-matrix fixtures use ordered tests for navigation and shared state.
                var fixture = failure.AncestorsAndSelf("test-suite")
                    .FirstOrDefault(suite => (string)suite.Attribute("type") == "TestFixture");
                var fixtureClass = (string)fixture?.Attribute("classname");
                if (fixture != null && string.IsNullOrWhiteSpace(fixtureClass))
                    throw new InvalidOperationException("A failed fixture is missing its NUnit classname.");
                var featureMatrix = fixtureClass?.EndsWith("FeatureTests", StringComparison.Ordinal) == true;
                var categoryNodes = featureMatrix ? fixture.DescendantsAndSelf().Concat(fixture.Ancestors()) : failure.AncestorsAndSelf();
                var matches = categoryNodes
                    .Elements("properties").Elements("property")
                    .Where(property => (string)property.Attribute("name") == "Category")
                    .Select(property => (string)property.Attribute("value"))
                    .Intersect(categories, StringComparer.OrdinalIgnoreCase).ToList();
                if (matches.Count == 0)
                    throw new InvalidOperationException("A failure could not be mapped to the original category group.");

                if (featureMatrix)
                {
                    var fixtureName = (string)fixture.Attribute("fullname");
                    if (string.IsNullOrWhiteSpace(fixtureName) ||
                        reportList.SelectMany(item => item.Descendants("test-suite"))
                            .Count(suite => (string)suite.Attribute("type") == "TestFixture" &&
                                (string)suite.Attribute("fullname") == fixtureName) != 1)
                        throw new InvalidOperationException("A feature-matrix fixture does not have a unique NUnit fullname.");
                    filters.Add($"({originalCategoryFilter} and test == {QuoteFilterValue(fixtureName)})");
                    continue;
                }

                var categoryFilter = "(" + string.Join(" or ", matches.Select(category => $"cat == {QuoteFilterValue(category)}")) + ")";
                if (failure.Name == "test-case")
                {
                    var fullName = (string)failure.Attribute("fullname");
                    if (string.IsNullOrWhiteSpace(fullName) ||
                        reportList.SelectMany(item => item.Descendants("test-case"))
                            .Count(test => (string)test.Attribute("fullname") == fullName) != 1)
                        throw new InvalidOperationException("A failed test does not have a unique NUnit fullname.");
                    filters.Add($"({categoryFilter} and test == {QuoteFilterValue(fullName)})");
                }
                else
                {
                    // A fixture error needs category recovery, not just its failed child tests.
                    filters.Add(categoryFilter);
                }
            }
        }

        if (reportCount == 0)
            throw new InvalidOperationException("No NUnit result files were found.");

        return string.Join(" or ", filters);
    }

    static string QuoteFilterValue(string value)
    {
        return "'" + value.Replace("\\", "\\\\").Replace("'", "\\'") + "'";
    }

    public static XDocument CreateRunSettings(string filter)
    {
        return new XDocument(new XElement("RunSettings",
            new XElement("NUnit", new XElement("Where", filter))));
    }

    public static bool ApplyNUnitFailures(XDocument trx, IEnumerable<XDocument> reports)
    {
        var runs = reports.Select(report => report.Root).ToList();
        if (runs.Any(run => run?.Name != "test-run"))
            throw new InvalidOperationException("NUnit XML is missing a test-run.");

        var failedRuns = runs.Where(run => (string)run.Attribute("result") == "Failed").ToList();
        if (failedRuns.Count == 0)
            return false;

        XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var summary = trx.Root.Element(ns + "ResultSummary");
        if (summary == null)
            throw new InvalidOperationException("TRX is missing ResultSummary.");

        foreach (var run in failedRuns)
            AddNUnitRunError(summary, run);
        return true;
    }

    static void AddNUnitRunError(XElement summary, XElement failure)
    {
        XNamespace ns = summary.Name.Namespace;
        summary.SetAttributeValue("outcome", "Failed");
        var runInfos = summary.Element(ns + "RunInfos");
        if (runInfos == null)
        {
            runInfos = new XElement(ns + "RunInfos");
            summary.Add(runInfos);
        }

        var messages = failure.Descendants("failure").Elements("message").Select(message => message.Value);
        runInfos.Add(new XElement(ns + "RunInfo",
            new XAttribute("computerName", Environment.MachineName),
            new XAttribute("outcome", "Error"),
            new XAttribute("timestamp", DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture)),
            new XElement(ns + "Text", "NUnit reported failed tests or fixtures. " + string.Join(Environment.NewLine, messages))));
    }

    static bool FailureCompletedSuccessfully(XElement failure, IEnumerable<XDocument> retryReports)
    {
        var fullName = (string)failure.Attribute("fullname");
        var type = (string)failure.Attribute("type");
        var assembly = (string)failure.AncestorsAndSelf("test-suite")
            .FirstOrDefault(suite => (string)suite.Attribute("type") == "Assembly")?.Attribute("fullname");
        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(assembly) ||
            (failure.Name == "test-suite" && string.IsNullOrEmpty(type)))
            return false;

        // NUnit IDs are attempt-specific; match the fully qualified name within its assembly.
        var matches = retryReports.Where(report => report.Root?.Name == "test-run" &&
            new[] { "Passed", "Failed", "Skipped", "Inconclusive", "Warning" }.Contains((string)report.Root.Attribute("result")) &&
            int.TryParse((string)report.Root.Attribute("total"), out var total) && total == report.Descendants("test-case").Count())
            .SelectMany(report => report.Descendants(failure.Name)).Where(suite =>
            (string)suite.Attribute("fullname") == fullName &&
            (string)suite.Attribute("type") == type &&
            (string)suite.AncestorsAndSelf("test-suite")
                .FirstOrDefault(parent => (string)parent.Attribute("type") == "Assembly")?.Attribute("fullname") == assembly).ToList();
        return matches.Count > 0 && matches.All(suite =>
        {
            var cases = suite.DescendantsAndSelf("test-case").ToList();
            return (string)suite.Attribute("result") == "Passed" &&
                !suite.AncestorsAndSelf().Concat(cases).Any(node => (string)node.Attribute("label") == "Cancelled") &&
                (suite.Name == "test-case" || (int.TryParse((string)suite.Attribute("total"), out var total) && total == cases.Count)) &&
                cases.Any(test => (string)test.Attribute("result") == "Passed") &&
                cases.All(test => (string)test.Attribute("result") == "Passed" || (string)test.Attribute("result") == "Skipped");
        });
    }

    public static void MergeResults(XDocument original, XDocument retry, IEnumerable<XDocument> originalReports, IEnumerable<XDocument> retryReports)
    {
        XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var failedIds = new HashSet<string>(original.Root.Element(ns + "Results")?.Elements()
            .Where(result => (string)result.Attribute("outcome") == "Failed")
            .Select(result => (string)result.Attribute("testId")) ?? Enumerable.Empty<string>());
        var unresolvedIds = new HashSet<string>(retry.Root.Element(ns + "Results")?.Elements()
            .Where(result => (string)result.Attribute("outcome") != "Passed" && (string)result.Attribute("outcome") != "Failed")
            .Select(result => (string)result.Attribute("testId")).Where(failedIds.Contains) ?? Enumerable.Empty<string>());
        foreach (var section in new[] { "Results", "TestDefinitions", "TestEntries" })
        {
            var destination = original.Root.Element(ns + section);
            var source = retry.Root.Element(ns + section);
            if (destination == null || source == null)
                throw new InvalidOperationException($"TRX is missing {section}.");

            var key = section == "TestDefinitions" ? "id" : "testId";
            var replacements = source.Elements().Where(node => !unresolvedIds.Contains((string)node.Attribute(key))).ToList();
            var replacedIds = new HashSet<string>(replacements.Select(node => (string)node.Attribute(key)));
            destination.Elements().Where(node => replacedIds.Contains((string)node.Attribute(key))).Remove();
            destination.Add(replacements.Select(node => new XElement(node)));
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

        // TRX alone may omit NUnit failures or report their cases as passed.
        foreach (var run in originalReports.Select(report => report.Root).Where(run => (string)run?.Attribute("result") == "Failed"))
        {
            var failures = run.Descendants().Where(node =>
                (string)node.Attribute("result") == "Failed" &&
                (node.Name == "test-case" || (node.Name == "test-suite" && (string)node.Attribute("site") != "Child"))).ToList();
            if (run.Element("failure") != null ||
                (failures.Count == 0 && !run.Descendants("test-case").Any(test => (string)test.Attribute("result") == "Failed")))
                failures.Add(run);

            foreach (var failure in failures.Where(failure => !FailureCompletedSuccessfully(failure, retryReports)))
                AddNUnitRunError(mergedSummary, failure);
        }

        summary.ReplaceWith(mergedSummary);
        original.Root.Element(ns + "Times")?.SetAttributeValue("finish", (string)retry.Root.Element(ns + "Times")?.Attribute("finish"));
    }
}
