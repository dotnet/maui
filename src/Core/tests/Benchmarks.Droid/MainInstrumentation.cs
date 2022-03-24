using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace Benchmarks.Droid;

[Instrumentation(Name = "com.microsoft.maui.MainInstrumentation")]
public class MainInstrumentation : Instrumentation
{
    const string Tag = "MAUI";

	public static MainInstrumentation? Instance { get; private set; }

    protected MainInstrumentation(IntPtr handle, JniHandleOwnership transfer)
        : base(handle, transfer) { }

    public override void OnCreate (Bundle? arguments)
    {
        base.OnCreate (arguments);

		Instance = this;

        Start ();
    }

    public async override void OnStart ()
    {
        base.OnStart ();

        var success = await Task.Factory.StartNew (Run);
        Log.Debug (Tag, $"Benchmark complete, success: {success}");
        Finish (success ? Result.Ok : Result.Canceled, new Bundle());
    }

    static bool Run()
    {
        bool success = false;
        try {
            // NOTE: this is mostly working around bugs in BenchmarkDotNet configuration
            var logger = new Logger();
            var baseConfig = new DebugInProcessConfig();

            var config = new ManualConfig();

            foreach (var e in baseConfig.GetExporters())
                config.AddExporter (e);
            foreach (var d in baseConfig.GetDiagnosers())
                config.AddDiagnoser (d);
            foreach (var a in baseConfig.GetAnalysers())
                config.AddAnalyser (a);
            foreach (var v in baseConfig.GetValidators())
                config.AddValidator (v);
            foreach (var p in baseConfig.GetColumnProviders())
                config.AddColumnProvider(p);
            config.AddJob(JobMode<Job>.Default.WithToolchain(new InProcessEmitToolchain(TimeSpan.FromMinutes(10), logOutput: true)));
            config.UnionRule = ConfigUnionRule.AlwaysUseGlobal; // Overriding the default
            config.AddLogger(logger);

			// ImageBenchmark class is hardcoded here for now
			BenchmarkRunner.Run<ImageBenchmark>(config.WithOptions(ConfigOptions.DisableLogFile));
			BenchmarkRunner.Run<ViewHandlerBenchmark>(config.WithOptions(ConfigOptions.DisableLogFile));

            success = true;
        } catch (Exception ex) {
            Log.Error (Tag, $"Error: {ex}");
        }
        return success;
    }

    // NOTE: the built-in ConsoleLogger throws PlatformNotSupportedException
    class Logger : ILogger
    {
        public string Id => "AndroidLogger";

        public int Priority => 0;

        public void Flush() { }

        public void Write(LogKind logKind, string text) => Console.Write(text);

        public void WriteLine() => Console.WriteLine();

        public void WriteLine(LogKind logKind, string text) => Console.WriteLine(text);
    }
}