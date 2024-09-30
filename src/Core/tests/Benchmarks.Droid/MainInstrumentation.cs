using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace Benchmarks.Droid;

[Instrumentation(Name = "com.microsoft.maui.MainInstrumentation")]
public class MainInstrumentation : Instrumentation
{
	const string Tag = "MAUI";

	public static MainInstrumentation? Instance { get; private set; }
	public static string ExternalDataDirectory { get; private set; } = string.Empty;

	protected MainInstrumentation(IntPtr handle, JniHandleOwnership transfer)
		: base(handle, transfer) { }

	public override void OnCreate(Bundle? arguments)
	{
		base.OnCreate(arguments);

		Instance = this;
		ExternalDataDirectory = Context?.GetExternalFilesDir(null)?.ToString() ?? string.Empty;
		if (string.IsNullOrEmpty(ExternalDataDirectory))
		{
			Log.Error(Tag, "ExternalDataDirectory is failed to be set");
			return;
		}
		Log.Debug(Tag, $"ExternalDataDirectory: {ExternalDataDirectory}");

		Start();
	}

	public async override void OnStart()
	{
		base.OnStart();
#if PERFLAB_INLAB
		Environment.SetEnvironmentVariable("PERFLAB_INLAB", "1");
		Environment.SetEnvironmentVariable("PERFLAB_BUILDTIMESTAMP", "0001-01-01T00:00:00.0000000Z");
		Environment.SetEnvironmentVariable("PERFLAB_BUILDNUM", "REPLACE_BUILDNUM");
		Environment.SetEnvironmentVariable("DOTNET_VERSION", "REPLACE_DOTNET_VERSION");
		Environment.SetEnvironmentVariable("PERFLAB_HASH", "REPLACE_HASH");
		Environment.SetEnvironmentVariable("HELIX_CORRELATION_ID", "REPLACE_HELIX_CORRELATION_ID");
		Environment.SetEnvironmentVariable("PERFLAB_PERFHASH", "REPLACE_PERFLAB_PERFHASH");
		Environment.SetEnvironmentVariable("PERFLAB_RUNNAME", "REPLACE_PERFLAB_RUNNAME");
		Environment.SetEnvironmentVariable("HELIX_WORKITEM_FRIENDLYNAME", "REPLACE_HELIX_WORKITEM_FRIENDLYNAME");
		Environment.SetEnvironmentVariable("PERFLAB_REPO", "REPLACE_PERFLAB_REPO");
		Environment.SetEnvironmentVariable("PERFLAB_BRANCH", "REPLACE_PERFLAB_BRANCH");
		Environment.SetEnvironmentVariable("PERFLAB_QUEUE", "REPLACE_PERFLAB_QUEUE");
		Environment.SetEnvironmentVariable("PERFLAB_BUILDARCH", "REPLACE_PERFLAB_BUILDARCH");
		Environment.SetEnvironmentVariable("PERFLAB_LOCALE", "REPLACE_PERFLAB_LOCALE");
		Directory.CreateDirectory(ExternalDataDirectory);
#endif

		var success = await Task.Factory.StartNew(Run);
		Log.Debug(Tag, $"Benchmark complete, success: {success}");
		Finish(success ? Result.Ok : Result.Canceled, new Bundle());
	}

	static bool Run()
	{
		bool success = false;
		try
		{
			var config = ManualConfig.CreateMinimumViable()
				.AddJob(Job.Default.WithToolchain(new InProcessEmitToolchain(TimeSpan.FromMinutes(10), logOutput: true)))
				.AddDiagnoser(MemoryDiagnoser.Default)
				.WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical));

			// ImageBenchmark class is hardcoded here for now
			BenchmarkRunner.Run<ImageBenchmark>(config);
			BenchmarkRunner.Run<ViewHandlerBenchmark>(config);

			success = true;
		}
		catch (Exception ex)
		{
			Log.Error(Tag, $"Error: {ex}");
		}
		return success;
	}
}