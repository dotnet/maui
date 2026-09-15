using System.Diagnostics;
using System.Text;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class BuildWarningsUtilitiesTests : IDisposable
{
	readonly string _directory = Directory.CreateTempSubdirectory("maui-binlog-tests-").FullName;
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

	string CreateBinlog()
	{
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

	public void Dispose() => Directory.Delete(_directory, recursive: true);

	sealed class RecordingOutput : ITestOutputHelper
	{
		readonly StringBuilder _text = new();
		public string Text => _text.ToString();
		public void WriteLine(string message) => _text.AppendLine(message);
		public void WriteLine(string format, params object[] args) => WriteLine(string.Format(format, args));
	}
}
