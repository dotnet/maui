using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging.StructuredLogger;
using Xunit.Sdk;
using Record = Microsoft.Build.Logging.Record;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class BuildWarningsUtilitiesTests : IDisposable
{
	readonly string _directory = Path.Combine(TestEnvironment.GetTestDirectoryRoot(), nameof(BuildWarningsUtilitiesTests), Guid.NewGuid().ToString("N"));
	const string Framework = "net11.0-android";
	string ProjectFile => Path.Combine(_directory, "App.csproj");
	readonly ITestOutputHelper _output;

	public BuildWarningsUtilitiesTests(ITestOutputHelper output) => _output = output;

	[Fact]
	public void ReadsErrorsAndWarningsAfterPropertyReassignment()
	{
		var binlog = CreateBinlog();
		var output = new RecordingOutput();

		BuildWarningsUtilities.OutputBuildErrorsFromBinLog(binlog, output: output);

		Assert.Contains("TEST0002: test build error", output.Text, StringComparison.Ordinal);
		Assert.DoesNotContain("Could not completely read", output.Text, StringComparison.Ordinal);
		var warning = Assert.Single(Assert.Single(BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binlog)).WarningsPerCode);
		Assert.Equal("TEST0001", warning.Code);
		Assert.Contains("test build warning", warning.Messages);
	}

	[Fact]
	public void ReportsTruncatedBinlogWithoutReplacingBuildFailure()
	{
		var binlog = CreateBinlog();
		using (var stream = File.OpenWrite(binlog))
			stream.SetLength(new FileInfo(binlog).Length / 2);

		var output = new RecordingOutput();
		BuildWarningsUtilities.OutputBuildErrorsFromBinLog(binlog, output: output);

		Assert.Contains("Could not completely read binlog", output.Text, StringComparison.Ordinal);
		Assert.Contains(binlog, output.Text, StringComparison.Ordinal);
		Assert.ThrowsAny<Exception>(() => BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binlog));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void UsesMatchingEvaluationWhenStartedPropertiesAreOmitted(bool evaluationAfterStart)
	{
		var evaluation = Evaluation(ProjectFile, 1, 19, Properties());
		var started = Started(ProjectFile, 1, 19, null);
		var events = evaluationAfterStart ? new BuildEventArgs[] { started, evaluation } : new BuildEventArgs[] { evaluation, started };
		var binlog = WriteBinlog(events);

		AssertProperties(binlog);
	}

	[Fact]
	public void ReadsPropertiesStoredOnProjectStarted()
	{
		var properties = new ArrayList
		{
			new DictionaryEntry("TargetFramework", Framework),
			new DictionaryEntry("PublishTrimmed", "true"),
			new DictionaryEntry("TrimMode", "full"),
		};
		var binlog = WriteBinlog(Started(ProjectFile, 1, BuildEventContext.InvalidEvaluationId, properties));

		AssertProperties(binlog);
	}

	[Fact]
	public void StartedPropertiesOverrideEvaluatedValues()
	{
		var binlog = WriteBinlog(
			Evaluation(ProjectFile, 1, 19, Properties()),
			Started(ProjectFile, 1, 19, Properties(trimMode: "partial")));

		Assert.ThrowsAny<XunitException>(() => AssertProperties(binlog));
	}

	[Theory]
	[InlineData(2, 19)]
	[InlineData(1, 20)]
	public void DoesNotUseAnotherNodeOrEvaluation(int nodeId, int evaluationId)
	{
		var binlog = WriteBinlog(
			Evaluation(ProjectFile, nodeId, evaluationId, Properties()),
			Evaluation(ProjectFile, 1, 19, Properties(trimMode: "partial")),
			Started(ProjectFile, 1, 19, null));

		Assert.ThrowsAny<XunitException>(() => AssertProperties(binlog));
	}

	[Fact]
	public void DoesNotUseAnotherProjectWithTheSameFileName()
	{
		var otherProject = Path.Combine(_directory, "other", "App.csproj");
		var binlog = WriteBinlog(
			Evaluation(otherProject, 1, 19, Properties()),
			Started(ProjectFile, 1, 19, null));

		Assert.ThrowsAny<XunitException>(() => AssertProperties(binlog));
	}

	[Fact]
	public void DoesNotUseAnotherTargetFramework()
	{
		var binlog = WriteBinlog(
			Evaluation(ProjectFile, 1, 19, Properties(framework: "net11.0-ios")),
			Started(ProjectFile, 1, 19, null));

		Assert.ThrowsAny<XunitException>(() => AssertProperties(binlog));
	}

	[Fact]
	public void DoesNotAcceptEvaluationWithoutAStartedProject()
	{
		var binlog = WriteBinlog(Evaluation(ProjectFile, 1, 19, Properties()));

		Assert.ThrowsAny<XunitException>(() => AssertProperties(binlog));
	}

	[Fact]
	public void DoesNotAssociateInvalidEvaluationIdentifiers()
	{
		var binlog = WriteBinlog(
			Evaluation(ProjectFile, 1, BuildEventContext.InvalidEvaluationId, Properties()),
			Started(ProjectFile, 1, BuildEventContext.InvalidEvaluationId, null));

		Assert.ThrowsAny<XunitException>(() => AssertProperties(binlog));
	}

	[Fact]
	public void DoesNotCombinePropertiesAcrossProjectInstances()
	{
		var binlog = WriteBinlog(
			Evaluation(ProjectFile, 1, 19, Properties(trimMode: "partial")),
			Started(ProjectFile, 1, 19, null),
			Evaluation(ProjectFile, 1, 20, Properties(publishTrimmed: "false")),
			Started(ProjectFile, 1, 20, null));

		Assert.ThrowsAny<XunitException>(() => AssertProperties(binlog));
	}

	[Fact]
	public void MissingPropertiesFailWithAnExplicitDiagnostic()
	{
		var binlog = WriteBinlog(Started(ProjectFile, 1, 19, null));

		var error = Assert.ThrowsAny<XunitException>(() => AssertProperties(binlog));
		Assert.Contains("missing", error.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void MatchesNormalizedProjectPaths()
	{
		var binlog = WriteBinlog(
			Evaluation(ProjectFile, 1, 19, Properties()),
			Started(ProjectFile, 1, 19, null));

		BuildWarningsUtilities.AssertProjectProperties(binlog, Path.Combine(_directory, "unused", "..", "App.csproj"), Framework,
			("PublishTrimmed", "true"), ("TrimMode", "full"));
	}

	[Fact]
	public void ReadsFullTrimTaskInputsWithoutEvaluatedPublishTrimmed()
	{
		var binlog = WriteBinlog(ExecutionEvents());

		AssertFullTrimTask(binlog);
	}

	[Fact]
	public void RequiresNativeCompilerExecutionInIlcCompile()
	{
		var binlog = WriteBinlog(ExecutionEvents(targetName: "IlcCompile", taskName: "Exec", trimMode: null));

		BuildWarningsUtilities.AssertTaskSucceededInTarget(binlog, ProjectFile, "IlcCompile", "Exec");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("partial")]
	[InlineData("copy")]
	public void RejectsMissingOrNonFullTrimTaskInput(string? trimMode)
	{
		var binlog = WriteBinlog(ExecutionEvents(trimMode: trimMode));

		Assert.ThrowsAny<XunitException>(() => AssertFullTrimTask(binlog));
	}

	[Fact]
	public void DoesNotUseTaskOutputsAsInputs()
	{
		var events = ExecutionEvents();
		var parameter = Array.FindIndex(events, e => e is TaskParameterEventArgs);
		events[parameter] = new TaskParameterEventArgs(TaskParameterMessageKind.TaskOutput, "TrimMode",
			new[] { "full" }, false, DateTime.UtcNow)
		{ BuildEventContext = events[parameter].BuildEventContext };

		Assert.ThrowsAny<XunitException>(() => AssertFullTrimTask(WriteBinlog(events)));
	}

	[Theory]
	[InlineData(false, true, true)]
	[InlineData(true, false, true)]
	[InlineData(true, true, false)]
	public void RejectsFailedOrSkippedExecution(bool taskSucceeded, bool targetSucceeded, bool executeTask)
	{
		var binlog = WriteBinlog(ExecutionEvents(targetName: "IlcCompile", taskName: "Exec",
			taskSucceeded: taskSucceeded, targetSucceeded: targetSucceeded, executeTask: executeTask));

		Assert.ThrowsAny<XunitException>(() =>
			BuildWarningsUtilities.AssertTaskSucceededInTarget(binlog, ProjectFile, "IlcCompile", "Exec"));
	}

	[Theory]
	[InlineData(2, 1, 5, 7)]
	[InlineData(1, 2, 5, 7)]
	[InlineData(1, 1, 6, 7)]
	[InlineData(1, 1, 5, 8)]
	public void DoesNotCombineTaskInputsAcrossBuildContexts(int node, int project, int target, int task)
	{
		var events = ExecutionEvents();
		var parameter = Assert.Single(events.OfType<TaskParameterEventArgs>());
		parameter.BuildEventContext = new BuildEventContext(1, node, -1, 1, project, target, task);

		Assert.ThrowsAny<XunitException>(() => AssertFullTrimTask(WriteBinlog(events)));
	}

	[Theory]
	[InlineData("_OtherTarget", "ILLink", false)]
	[InlineData("_RunILLink", "OtherTask", false)]
	[InlineData("_RunILLink", "ILLink", true)]
	public void RejectsExecutionInAnotherTargetTaskOrProject(string target, string task, bool otherProject)
	{
		var binlog = WriteBinlog(ExecutionEvents(targetName: target, taskName: task,
			projectFile: otherProject ? Path.Combine(_directory, "other", "App.csproj") : ProjectFile));

		Assert.ThrowsAny<XunitException>(() => AssertFullTrimTask(binlog));
	}

	[Fact]
	public void DoesNotCombineInputsFromFailedTaskWithSuccessfulRetry()
	{
		var failed = ExecutionEvents(taskSucceeded: false);
		var retry = ExecutionEvents(trimMode: "partial");
		foreach (var e in retry.Where(e => e.BuildEventContext!.TaskId == 7))
			e.BuildEventContext = new BuildEventContext(1, 1, -1, 1, 1, 5, 8);
		var events = failed[..^1].Concat(retry[1..]).ToArray();

		Assert.ThrowsAny<XunitException>(() => AssertFullTrimTask(WriteBinlog(events)));
	}

	[Fact]
	public void SuccessfulExecutionDoesNotHideLaterWarnings()
	{
		var binlog = WriteBinlog(ExecutionEvents().Append(new BuildWarningEventArgs("", "IL2070",
			ProjectFile, 1, 1, 1, 1, "Real trimming warning", "", "ILC")).ToArray());

		AssertFullTrimTask(binlog);
		Assert.ThrowsAny<XunitException>(() => BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binlog).AssertNoWarnings());
	}

	void AssertFullTrimTask(string binlog) =>
		BuildWarningsUtilities.AssertTaskSucceededInTarget(binlog, ProjectFile, "_RunILLink", "ILLink", ("TrimMode", "full"));

	BuildEventArgs[] ExecutionEvents(string targetName = "_RunILLink", string taskName = "ILLink", string? trimMode = "full",
		bool taskSucceeded = true, bool targetSucceeded = true, bool executeTask = true, string? projectFile = null)
	{
		projectFile ??= ProjectFile;
		var targetContext = new BuildEventContext(1, 1, -1, 1, 1, 5, -1);
		var taskContext = new BuildEventContext(1, 1, -1, 1, 1, 5, 7);
		var events = new List<BuildEventArgs>
		{
			new TargetStartedEventArgs("Target started", "", targetName, projectFile, "App.targets")
				{ BuildEventContext = targetContext },
		};
		if (executeTask)
		{
			events.Add(new TaskStartedEventArgs("Task started", "", projectFile, "App.targets", taskName)
			{ BuildEventContext = taskContext });
			if (trimMode is not null)
				events.Add(new TaskParameterEventArgs(TaskParameterMessageKind.TaskInput, "TrimMode",
					new[] { trimMode }, false, DateTime.UtcNow)
				{ BuildEventContext = taskContext });
			events.Add(new TaskFinishedEventArgs("Task finished", "", projectFile, "App.targets", taskName, taskSucceeded)
			{ BuildEventContext = taskContext });
		}
		events.Add(new TargetFinishedEventArgs("Target finished", "", targetName, projectFile, "App.targets", targetSucceeded)
		{ BuildEventContext = targetContext });
		return events.ToArray();
	}

	[Theory]
	[InlineData(nameof(BuildWarningsUtilities.AssertProjectProperties))]
	[InlineData(nameof(BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog))]
	[InlineData(nameof(BuildWarningsUtilities.OutputBuildErrorsFromBinLog))]
	[InlineData(nameof(BuildWarningsUtilities.AssertTaskSucceeded))]
	[InlineData(nameof(BuildWarningsUtilities.AssertTaskSucceededInTarget))]
	[InlineData(nameof(BuildWarningsUtilities.AssertTargetSucceeded))]
	public void ReadsInvariantCultureReassignmentsWithoutLosingDiagnostics(string operation)
	{
		string binlog;
		var originalCulture = CultureInfo.CurrentUICulture;
		try
		{
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
			binlog = WriteBinlog([
				new PropertyReassignmentEventArgs("TrimMode", "partial", "full", $"{ProjectFile} (1,1)", "Property reassigned", "", "MSBuild", MessageImportance.Low),
				Evaluation(ProjectFile, 1, 19, Properties()),
				Started(ProjectFile, 1, 19, null),
				new BuildWarningEventArgs("", "TESTWARN001", ProjectFile, 1, 1, 1, 1, "Warning preserved", "", "Test"),
				new BuildErrorEventArgs("", "TESTERR001", ProjectFile, 2, 1, 2, 1, "Error preserved", "", "Test"),
				new TaskFinishedEventArgs("Task finished", "", ProjectFile, "App.targets", "ILLink", true),
				new TargetFinishedEventArgs("Target finished", "", "Publish", ProjectFile, "App.targets", true),
				.. ExecutionEvents()]);
		}
		finally
		{
			CultureInfo.CurrentUICulture = originalCulture;
		}

		// Isolate static resources so another test cannot initialize them and hide the first-read failure.
		var context = new ColdReaderLoadContext();
		try
		{
			var loggerAssembly = context.LoadFromAssemblyPath(typeof(BinLogReader).Assembly.Location);
			Assert.Null(loggerAssembly.GetType(typeof(Strings).FullName!)!.GetProperty(nameof(Strings.PropertyReassignment))!.GetValue(null));
			var helperAssembly = context.LoadFromAssemblyPath(typeof(BuildWarningsUtilities).Assembly.Location);
			var helperType = helperAssembly.GetType(typeof(BuildWarningsUtilities).FullName!)!;
			var output = new RecordingOutput();
			var arguments = operation switch
			{
				nameof(BuildWarningsUtilities.AssertProjectProperties) =>
					new object[] { binlog, ProjectFile, Framework, new (string, string)[] { ("PublishTrimmed", "true"), ("TrimMode", "full") } },
				nameof(BuildWarningsUtilities.OutputBuildErrorsFromBinLog) => new object[] { binlog, 50, output },
				nameof(BuildWarningsUtilities.AssertTaskSucceeded) => new object[] { binlog, ProjectFile, "ILLink" },
				nameof(BuildWarningsUtilities.AssertTaskSucceededInTarget) =>
					new object[] { binlog, ProjectFile, "_RunILLink", "ILLink", new (string, string)[] { ("TrimMode", "full") } },
				nameof(BuildWarningsUtilities.AssertTargetSucceeded) => new object[] { binlog, ProjectFile, "Publish" },
				_ => new object[] { binlog },
			};
			var result = helperType.GetMethod(operation)!.Invoke(null, arguments);

			if (operation == nameof(BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog))
				Assert.Contains("TESTWARN001", string.Join("\n", ((IEnumerable)result!).Cast<object>()), StringComparison.Ordinal);
			if (operation == nameof(BuildWarningsUtilities.OutputBuildErrorsFromBinLog))
				Assert.Contains(output.Lines, line => line.Contains("TESTERR001", StringComparison.Ordinal));
			Assert.Same(originalCulture, CultureInfo.CurrentUICulture);
			using var exclusiveRead = File.Open(binlog, FileMode.Open, FileAccess.Read, FileShare.None);
			Assert.True(exclusiveRead.CanRead);
		}
		finally
		{
			context.Unload();
		}
	}

	void AssertProperties(string binlog) =>
		BuildWarningsUtilities.AssertProjectProperties(binlog, ProjectFile, Framework, ("PublishTrimmed", "true"), ("TrimMode", "full"));

	static Dictionary<string, string> Properties(string framework = Framework, string trimMode = "full", string publishTrimmed = "true") =>
		new()
		{
			["TargetFramework"] = framework,
			["PublishTrimmed"] = publishTrimmed,
			["TrimMode"] = trimMode,
		};

	static BuildEventContext Context(int nodeId, int evaluationId) =>
		new(1, nodeId, evaluationId, 1, 1, -1, -1);

	static ProjectEvaluationFinishedEventArgs Evaluation(string projectFile, int nodeId, int evaluationId, IEnumerable properties) =>
		new()
		{
			ProjectFile = projectFile,
			BuildEventContext = Context(nodeId, evaluationId),
			Properties = properties,
		};

	static ProjectStartedEventArgs Started(string projectFile, int nodeId, int evaluationId, IEnumerable? properties) =>
		// Real binlogs omit these properties despite the constructor's non-nullable annotation.
		new("Project started", "", projectFile, "Build", properties!, Array.Empty<DictionaryEntry>())
		{
			BuildEventContext = Context(nodeId, evaluationId),
		};

	string WriteBinlog(params BuildEventArgs[] events)
	{
		Directory.CreateDirectory(_directory);
		var binlog = Path.Combine(_directory, "properties.binlog");
		var source = new LogEventSource();
		var logger = new BinaryLogger { Parameters = $"{binlog};ProjectImports=None" };
		logger.Initialize(source);
		try
		{
			source.Emit(new BuildStartedEventArgs("Build started", ""));
			foreach (var buildEvent in events)
				source.Emit(buildEvent);
			source.Emit(new BuildFinishedEventArgs("Build finished", "", true));
		}
		finally
		{
			logger.Shutdown();
		}
		return binlog;
	}

	public void Dispose()
	{
		if (Directory.Exists(_directory))
			Directory.Delete(_directory, recursive: true);
	}

	sealed class ColdReaderLoadContext() : AssemblyLoadContext(isCollectible: true)
	{
		protected override Assembly? Load(AssemblyName assemblyName) =>
			assemblyName.Name == typeof(BinLogReader).Assembly.GetName().Name
				? LoadFromAssemblyPath(typeof(BinLogReader).Assembly.Location)
				: null;
	}

	sealed class RecordingOutput : ITestOutputHelper
	{
		public List<string> Lines { get; } = new();
		public string Text => string.Join(Environment.NewLine, Lines);
		public void WriteLine(string message) => Lines.Add(message);
		public void WriteLine(string format, params object[] args) => Lines.Add(string.Format(CultureInfo.InvariantCulture, format, args));
	}

	sealed class LogEventSource : IEventSource
	{
		public event AnyEventHandler? AnyEventRaised;
		public void Emit(BuildEventArgs buildEvent) => AnyEventRaised?.Invoke(this, buildEvent);

		public event BuildMessageEventHandler MessageRaised { add { } remove { } }
		public event BuildErrorEventHandler ErrorRaised { add { } remove { } }
		public event BuildWarningEventHandler WarningRaised { add { } remove { } }
		public event BuildStartedEventHandler BuildStarted { add { } remove { } }
		public event BuildFinishedEventHandler BuildFinished { add { } remove { } }
		public event ProjectStartedEventHandler ProjectStarted { add { } remove { } }
		public event ProjectFinishedEventHandler ProjectFinished { add { } remove { } }
		public event TargetStartedEventHandler TargetStarted { add { } remove { } }
		public event TargetFinishedEventHandler TargetFinished { add { } remove { } }
		public event TaskStartedEventHandler TaskStarted { add { } remove { } }
		public event TaskFinishedEventHandler TaskFinished { add { } remove { } }
		public event CustomBuildEventHandler CustomEventRaised { add { } remove { } }
		public event BuildStatusEventHandler StatusEventRaised { add { } remove { } }
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void RejectsTruncationAfterBuildFinished(bool completeImportArchive)
	{
		var binlog = CreateBinlog();
		List<Record> records;
		using (var stream = File.OpenRead(binlog))
			records = new BinLogReader().ReadRecords(stream).ToList();

		var finished = Assert.Single(records, record => record.Args is BuildFinishedEventArgs);
		var archive = Assert.Single(records, record => record.Kind == BinaryLogRecordKind.ProjectImportArchive);
		Assert.True(archive.Start >= finished.Start + finished.Length, "Imports must be written after BuildFinished.");
		Assert.True(archive.Length > 0, "The fixture must contain embedded imports.");

		using var decompressed = new MemoryStream();
		using (var stream = File.OpenRead(binlog))
		using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
			gzip.CopyTo(decompressed);

		var archiveStart = decompressed.GetBuffer().AsSpan(0, checked((int)decompressed.Length)).IndexOf(archive.Bytes);
		Assert.True(archiveStart >= 0, "The decompressed log must contain the import archive.");
		Assert.Equal(decompressed.Length - 1, archiveStart + archive.Length);

		// Keep all build events, but cut during the import archive or just before the end marker.
		decompressed.SetLength(archiveStart + (completeImportArchive ? archive.Length : archive.Length / 2));
		decompressed.Position = 0;
		using (var stream = File.Create(binlog))
		using (var gzip = new GZipStream(stream, CompressionMode.Compress))
			decompressed.CopyTo(gzip);

		using (var stream = File.OpenRead(binlog))
			Assert.Contains(new BinLogReader().ReadRecords(stream), record => record.Args is BuildFinishedEventArgs);

		var output = new RecordingOutput();
		BuildWarningsUtilities.OutputBuildErrorsFromBinLog(binlog, output: output);

		Assert.Contains("Could not completely read binlog", output.Text, StringComparison.Ordinal);
		Assert.Contains("TEST0002: test build error", output.Text, StringComparison.Ordinal);
		Assert.Throws<InvalidDataException>(() => BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binlog));
		var project = Path.Combine(_directory, "test.proj");
		Assert.Throws<InvalidDataException>(() =>
			BuildWarningsUtilities.AssertProjectProperties(binlog, project, Framework, ("TrimMode", "full")));
		Assert.Throws<InvalidDataException>(() => BuildWarningsUtilities.AssertTaskSucceeded(binlog, project, "Warning"));
		Assert.Throws<InvalidDataException>(() => BuildWarningsUtilities.AssertTaskSucceededInTarget(binlog, project, "Build", "Warning"));
		Assert.Throws<InvalidDataException>(() => BuildWarningsUtilities.AssertTargetSucceeded(binlog, project, "Build"));
	}

	string CreateBinlog()
	{
		Directory.CreateDirectory(_directory);
		var project = Path.Combine(_directory, "test.proj");
		var binlog = Path.Combine(_directory, "test.binlog");
		File.WriteAllText(project, """
			<Project>
			  <PropertyGroup><Reassigned>before</Reassigned></PropertyGroup>
			  <PropertyGroup><Reassigned>after</Reassigned></PropertyGroup>
			  <Target Name="Build">
			    <Warning Code="TEST0001" Text="test build warning" File="Test.cs" />
			    <Error Code="TEST0002" Text="test build error" File="Test.cs" />
			  </Target>
			</Project>
			""");
		var info = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
		{
			WorkingDirectory = _directory
		};
		info.ArgumentList.Add("msbuild");
		info.ArgumentList.Add(project);
		info.ArgumentList.Add("-v:diag");
		info.ArgumentList.Add($"-bl:{binlog}");
		var output = ToolRunner.Run(info, out var exitCode, timeoutInSeconds: 60, output: _output);
		Assert.Equal(1, exitCode);
		Assert.True(File.Exists(binlog), output);
		return binlog;
	}

}
