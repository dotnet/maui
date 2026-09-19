namespace Microsoft.Maui.UiEvidence;

static class UiEvidenceDevFlowCommand
{
	public static async Task<int> ExecuteAsync(CommandLineOptions options)
	{
		var output = Path.GetFullPath(options.Required("output"));
		Directory.CreateDirectory(output);
		var result = await DevFlowEvidenceCollector.CaptureAsync(
			output,
			options.OptionalInt("devflow-port", 9223));
		UiEvidenceJson.Write(Path.Combine(output, "devflow-capture.json"), result);
		return result.Status == "captured" ? 0 : 3;
	}
}
