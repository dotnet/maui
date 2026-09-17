using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging.StructuredLogger;
using Record = Microsoft.Build.Logging.Record;

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
