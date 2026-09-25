namespace Microsoft.Maui.UiEvidence;

static class Program
{
	public static async Task<int> Main(string[] args)
	{
		if (args.Length == 0)
		{
			WriteUsage();
			return 2;
		}

		try
		{
			var options = CommandLineOptions.Parse(args.Skip(1));
			return args[0] switch
			{
				"run" => await UiEvidenceRunCommand.ExecuteAsync(options),
				"compare" => UiEvidenceCompareCommand.Execute(options),
				"capture-devflow" => await UiEvidenceDevFlowCommand.ExecuteAsync(options),
				_ => throw new ArgumentException($"Unknown command '{args[0]}'.")
			};
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"UI evidence runner failed: {ex.GetType().Name}: {ex.Message}");
			return 2;
		}
	}

	static void WriteUsage()
	{
		Console.Error.WriteLine("""
			Usage:
			  Controls.UiEvidence.Runner run --platform <android|windows> --app <path>
			    --scenario <id> --registry <path> --output <dir> --variant <base|head>
			    --run-ordinal <1|2> --sequence-ordinal <1..4> --commit-sha <sha>
			    --harness-sha <sha> --request-key <key> [--appium-url <url>]
			    [--device-id <id>] [--devflow-port <port>] [--devflow]

			  Controls.UiEvidence.Runner compare --request <path> --registry <path>
			    --payload-manifest <path> --runs-root <dir> --output <path>

			  Controls.UiEvidence.Runner capture-devflow --output <dir>
			    [--devflow-port <port>]
			""");
	}
}
