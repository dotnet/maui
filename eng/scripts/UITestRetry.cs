using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

public static class UITestRetry
{
	static readonly XNamespace Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

	public static void Run(string resultsPath, string originalFilter, bool retryOnFailure, Func<string, string, int> runTests)
	{
		if (System.IO.File.Exists(resultsPath))
			System.IO.File.Delete(resultsPath);
		if (runTests(originalFilter, resultsPath) == 0)
			return;
		if (!retryOnFailure || !System.IO.File.Exists(resultsPath))
			throw new InvalidOperationException("The UI test command failed without a retryable test report.");

		var filter = GetFilter(resultsPath, originalFilter);
		var directory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(resultsPath));
		var diagnostics = System.IO.Path.Combine(directory, "TestResultsFailures", Guid.NewGuid().ToString("N"));
		System.IO.Directory.CreateDirectory(diagnostics);
		System.IO.File.Copy(resultsPath, System.IO.Path.Combine(diagnostics, "original.trx"));
		var retryPath = System.IO.Path.Combine(directory, System.IO.Path.GetFileNameWithoutExtension(resultsPath) + ".retry.trx");
		if (System.IO.File.Exists(retryPath))
			System.IO.File.Delete(retryPath);

		Console.WriteLine("Retrying failed UI fixtures only; preserving the original run and diagnostics.");
		try
		{
			var exitCode = runTests(filter, retryPath);
			var passed = Merge(resultsPath, retryPath, resultsPath, exitCode == 0);
			if (!passed || exitCode != 0)
				throw new InvalidOperationException("UI tests failed after retrying the failed fixtures.");
		}
		finally
		{
			if (System.IO.File.Exists(retryPath))
				System.IO.File.Move(retryPath, System.IO.Path.Combine(diagnostics, "retry.trx"));
		}
	}

	public static string GetFilter(string resultsPath, string originalFilter)
	{
		var results = ReadResults(Read(resultsPath));
		var fixtures = FailedFixtures(results);
		var filter = string.Join("|", fixtures.OrderBy(name => name, StringComparer.Ordinal).Select(name =>
		{
			if (!Regex.IsMatch(name, @"\A[A-Za-z_][A-Za-z0-9_.+`]*(?:\((?:Android|iOS|Mac|Windows)\))?\z"))
				throw new InvalidDataException("The failed UI fixture has an unsupported name.");

			var escapedName = name.Replace("(", @"\(").Replace(")", @"\)");
			return $"FullyQualifiedName~{escapedName}.";
		}));

		return string.IsNullOrWhiteSpace(originalFilter) ? filter : $"({originalFilter})&({filter})";
	}

	public static bool Merge(string originalPath, string retryPath, string outputPath, bool retryCommandSucceeded = true)
	{
		var original = Read(originalPath);
		var originalResults = ReadResults(original);
		var retry = Read(retryPath);
		var retryResults = ReadResults(retry);
		var fixtures = FailedFixtures(originalResults);
		var selectedResults = originalResults.Where(result => fixtures.Contains(result.Fixture)).ToArray();
		var expected = new HashSet<string>(selectedResults.Select(result => result.Key), StringComparer.Ordinal);
		var actual = new HashSet<string>(retryResults.Select(result => result.Key), StringComparer.Ordinal);

		if (!expected.SetEquals(actual))
			throw new InvalidDataException("Retry results must contain every selected fixture case, and no additional cases.");
		var retryByKey = retryResults.ToDictionary(result => result.Key, StringComparer.Ordinal);
		if (selectedResults.Any(result => result.Outcome != "NotExecuted" && retryByKey[result.Key].Outcome == "NotExecuted"))
			throw new InvalidDataException("A skipped retry cannot replace an executed test.");
		if (!retryCommandSucceeded && retryResults.All(result => result.Outcome != "Failed"))
			throw new InvalidDataException("The retry command failed despite reporting no failed tests.");

		foreach (var result in selectedResults)
		{
			result.Result.Remove();
			result.Definition.Remove();
			result.Entry.Remove();
		}

		foreach (var result in retryResults)
		{
			var replacement = new XElement(result.Result);
			foreach (var attachment in replacement.Descendants(Namespace + "ResultFile"))
			{
				var deployment = RequiredElement(RequiredElement(retry.Root, "TestSettings"), "Deployment");
				var root = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(retryPath));
				var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(root, RequiredAttribute(deployment, "runDeploymentRoot"), "In",
					RequiredAttribute(replacement, "relativeResultsDirectory"), RequiredAttribute(attachment, "path")));
				var relative = System.IO.Path.GetRelativePath(root, path);
				if (System.IO.Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith(".." + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal))
					throw new InvalidDataException("A retry attachment points outside the test results directory.");

				// Each TRX has its own deployment root. Absolute paths keep retry attachments resolvable.
				attachment.SetAttributeValue("path", path);
			}
			RequiredElement(original.Root, "Results").Add(replacement);
			RequiredElement(original.Root, "TestDefinitions").Add(new XElement(result.Definition));
			RequiredElement(original.Root, "TestEntries").Add(new XElement(result.Entry));
		}

		var results = ReadResults(original);
		var passed = results.Count(result => result.Outcome == "Passed");
		var failed = results.Count(result => result.Outcome == "Failed");
		var skipped = results.Count(result => result.Outcome == "NotExecuted");
		var summary = RequiredElement(original.Root, "ResultSummary");
		var counters = RequiredElement(summary, "Counters");
		foreach (var attribute in counters.Attributes())
			attribute.Value = "0";
		counters.SetAttributeValue("total", results.Count);
		counters.SetAttributeValue("executed", passed + failed);
		counters.SetAttributeValue("passed", passed);
		counters.SetAttributeValue("failed", failed);
		counters.SetAttributeValue("notExecuted", skipped);
		summary.SetAttributeValue("outcome", failed == 0 ? "Completed" : "Failed");
		summary.Element(Namespace + "Output")?.Remove();
		summary.Add(new XElement(Namespace + "Output", new XElement(Namespace + "StdOut",
			$"Retried {fixtures.Count} failed fixture(s). Final results: {passed} passed, {failed} failed, {skipped} skipped. Both attempt reports are retained in TestResultsFailures; attachments remain in their deployment directories.")));

		var finish = retry.Root.Element(Namespace + "Times")?.Attribute("finish");
		if (finish != null && original.Root.Element(Namespace + "Times") is XElement times)
			times.SetAttributeValue("finish", finish.Value);

		// Validate the complete report before atomically replacing the original failure.
		ValidateSummary(original, results);
		var temporaryPath = outputPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			original.Save(temporaryPath);
			System.IO.File.Move(temporaryPath, outputPath, overwrite: true);
		}
		finally
		{
			if (System.IO.File.Exists(temporaryPath))
				System.IO.File.Delete(temporaryPath);
		}

		return failed == 0;
	}

	static XDocument Read(string path)
	{
		using var reader = XmlReader.Create(path, new XmlReaderSettings
		{
			DtdProcessing = DtdProcessing.Prohibit,
			XmlResolver = null,
			MaxCharactersInDocument = 64 * 1024 * 1024
		});
		var document = XDocument.Load(reader);
		if (document.Root?.Name != Namespace + "TestRun")
			throw new InvalidDataException("Expected a TRX TestRun.");

		ValidateSummary(document, ReadResults(document));
		return document;
	}

	static void ValidateSummary(XDocument document, List<TestResult> results)
	{
		var summary = RequiredElement(document.Root, "ResultSummary");
		var outcome = RequiredAttribute(summary, "outcome");
		if (outcome != "Completed" && outcome != "Failed")
			throw new InvalidDataException("An incomplete test run cannot be retried or merged.");
		if (summary.Descendants(Namespace + "RunInfo").Any(info => (string)info.Attribute("outcome") == "Error"))
			throw new InvalidDataException("A test-host error cannot be replaced by a fixture retry.");
		if (results.Count == 0)
			throw new InvalidDataException("The test run reported no results.");

		var counters = RequiredElement(summary, "Counters");
		var expectedCounts = new Dictionary<string, int>
		{
			["total"] = results.Count,
			["passed"] = results.Count(result => result.Outcome == "Passed"),
			["failed"] = results.Count(result => result.Outcome == "Failed"),
			["notExecuted"] = results.Count(result => result.Outcome == "NotExecuted"),
			["executed"] = results.Count(result => result.Outcome != "NotExecuted")
		};
		foreach (var expected in expectedCounts)
		{
			// VSTest leaves notExecuted at zero for skips; total and executed still account for them.
			if (!int.TryParse((string)counters.Attribute(expected.Key), NumberStyles.None, CultureInfo.InvariantCulture, out var count) ||
				(count != expected.Value && !(expected.Key == "notExecuted" && count == 0)))
				throw new InvalidDataException("TRX counters do not match the reported test results.");
		}
		if (counters.Attributes().Any(attribute => !expectedCounts.ContainsKey(attribute.Name.LocalName) && attribute.Value != "0"))
			throw new InvalidDataException("The test run contains unsupported or incomplete outcome counters.");
		if ((outcome == "Failed") != (expectedCounts["failed"] != 0))
			throw new InvalidDataException("The test run outcome does not match its failed results.");
	}

	static HashSet<string> FailedFixtures(List<TestResult> results)
	{
		var fixtures = new HashSet<string>(results.Where(result => result.Outcome == "Failed").Select(result => result.Fixture), StringComparer.Ordinal);
		if (fixtures.Count == 0)
			throw new InvalidDataException("The unsuccessful test command did not report any failed fixtures.");
		return fixtures;
	}

	static List<TestResult> ReadResults(XDocument document)
	{
		var definitions = RequiredElement(document.Root, "TestDefinitions").Elements(Namespace + "UnitTest")
			.ToDictionary(element => RequiredAttribute(element, "id"), StringComparer.Ordinal);
		var entries = RequiredElement(document.Root, "TestEntries").Elements(Namespace + "TestEntry")
			.ToDictionary(element => RequiredAttribute(element, "executionId"), StringComparer.Ordinal);
		var keys = new HashSet<string>(StringComparer.Ordinal);
		var results = new List<TestResult>();
		foreach (var element in RequiredElement(document.Root, "Results").Elements())
		{
			if (element.Name != Namespace + "UnitTestResult")
				throw new InvalidDataException("Only individual UI test results can be retried.");
			var id = RequiredAttribute(element, "testId");
			var execution = RequiredAttribute(element, "executionId");
			if (!definitions.TryGetValue(id, out var definition) || !entries.TryGetValue(execution, out var entry) ||
				RequiredAttribute(entry, "testId") != id ||
				RequiredAttribute(RequiredElement(definition, "Execution"), "id") != execution)
				throw new InvalidDataException("TRX results have missing or inconsistent execution metadata.");

			var result = new TestResult
			{
				Fixture = RequiredAttribute(RequiredElement(definition, "TestMethod"), "className"),
				Outcome = RequiredAttribute(element, "outcome"),
				Result = element,
				Definition = definition,
				Entry = entry
			};
			if (result.Outcome != "Passed" && result.Outcome != "Failed" && result.Outcome != "NotExecuted")
				throw new InvalidDataException("The test run contains an incomplete or unsupported result.");
			if (!keys.Add(result.Key))
				throw new InvalidDataException("The test run contains duplicate test identities.");
			results.Add(result);
		}
		return results;
	}

	static XElement RequiredElement(XElement parent, string name) =>
		parent.Element(Namespace + name) ?? throw new InvalidDataException($"TRX is missing {name}.");

	static string RequiredAttribute(XElement element, string name) =>
		(string)element.Attribute(name) is string value && !string.IsNullOrWhiteSpace(value)
			? value : throw new InvalidDataException($"TRX is missing {name}.");

	sealed class TestResult
	{
		public string Fixture;
		public string Outcome;
		public XElement Result;
		public XElement Definition;
		public XElement Entry;
		public string Key => Fixture + "\n" + RequiredAttribute(Result, "testName");
	}
}
