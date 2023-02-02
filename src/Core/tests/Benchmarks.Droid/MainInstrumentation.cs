using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace Benchmarks.Droid;

[Instrumentation(Name = "com.microsoft.maui.MainInstrumentation")]
public class MainInstrumentation : Instrumentation
{
	const string Tag = "MAUI";

	public static MainInstrumentation? Instance { get; private set; }

	protected MainInstrumentation(IntPtr handle, JniHandleOwnership transfer)
		: base(handle, transfer) { }

	public override void OnCreate(Bundle? arguments)
	{
		base.OnCreate(arguments);

		Instance = this;

		Start();
	}

	public async override void OnStart()
	{
		base.OnStart();

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