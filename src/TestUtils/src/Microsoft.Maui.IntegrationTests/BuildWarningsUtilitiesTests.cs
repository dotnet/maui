using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging.StructuredLogger;
using Xunit.Sdk;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class BuildWarningsUtilitiesTests : IDisposable
{
	readonly string _directory = Path.Combine(TestEnvironment.GetTestDirectoryRoot(), nameof(BuildWarningsUtilitiesTests), Guid.NewGuid().ToString("N"));
	const string Framework = "net11.0-android";
	string ProjectFile => Path.Combine(_directory, "App.csproj");

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

	[Theory]
	[InlineData(nameof(BuildWarningsUtilities.AssertProjectProperties))]
	[InlineData(nameof(BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog))]
	[InlineData(nameof(BuildWarningsUtilities.OutputBuildErrorsFromBinLog))]
	[InlineData(nameof(BuildWarningsUtilities.AssertTaskSucceeded))]
	[InlineData(nameof(BuildWarningsUtilities.AssertTargetSucceeded))]
	public void ReadsInvariantCultureReassignmentsWithoutLosingDiagnostics(string operation)
	{
		string binlog;
		var originalCulture = CultureInfo.CurrentUICulture;
		try
		{
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
			binlog = WriteBinlog(
				new PropertyReassignmentEventArgs("TrimMode", "partial", "full", $"{ProjectFile} (1,1)", "Property reassigned", "", "MSBuild", MessageImportance.Low),
				Evaluation(ProjectFile, 1, 19, Properties()),
				Started(ProjectFile, 1, 19, null),
				new BuildWarningEventArgs("", "TESTWARN001", ProjectFile, 1, 1, 1, 1, "Warning preserved", "", "Test"),
				new BuildErrorEventArgs("", "TESTERR001", ProjectFile, 2, 1, 2, 1, "Error preserved", "", "Test"),
				new TaskFinishedEventArgs("Task finished", "", ProjectFile, "App.targets", "ILLink", true),
				new TargetFinishedEventArgs("Target finished", "", "Publish", ProjectFile, "App.targets", true));
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
}
