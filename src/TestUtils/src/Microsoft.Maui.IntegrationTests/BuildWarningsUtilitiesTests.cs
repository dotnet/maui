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
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(15)]
	public void ReportsTruncatedAssemblyGuidWithoutReplacingBuildFailure(int guidBytes)
	{
		var binlog = CreateAssemblyGuidBinlog(guidBytes);

		// Verify this cut exercises the short GUID read, rather than another truncation path.
		var reader = new BinLogReader();
		Exception? readException = null;
		reader.OnException += exception => readException = exception;
		reader.Replay(binlog);
		var original = Assert.IsType<ArgumentException>(readException);
		Assert.Equal("b", original.ParamName);
		Assert.Contains("ReadGuid", original.StackTrace, StringComparison.Ordinal);

		var output = new RecordingOutput();
		BuildWarningsUtilities.OutputBuildErrorsFromBinLog(binlog, output: output);

		Assert.Contains("Could not completely read binlog", output.Text, StringComparison.Ordinal);
		Assert.Contains(binlog, output.Text, StringComparison.Ordinal);
		Assert.Contains(original.Message, output.Text, StringComparison.Ordinal);
		Assert.Contains("TEST0002: test build error", output.Text, StringComparison.Ordinal);
		var strict = Assert.Throws<InvalidDataException>(() => BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binlog));
		var inner = Assert.IsType<ArgumentException>(strict.InnerException);
		Assert.Equal(original.Message, inner.Message);
		Assert.Contains("ReadGuid", inner.StackTrace, StringComparison.Ordinal);
	}

	string CreateAssemblyGuidBinlog(int guidBytes)
	{
		var binlog = CreateBinlog();
		using var decompressed = new MemoryStream();
		using (var stream = File.OpenRead(binlog))
		using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
			gzip.CopyTo(decompressed);

		using var readerForHeader = new BinaryReader(decompressed, Encoding.UTF8, leaveOpen: true);
		decompressed.Position = 0;
		Assert.True(readerForHeader.ReadInt32() >= 18, "The fixture must use length-framed records.");
		Assert.Equal((byte)BinaryLogRecordKind.EndOfFile, decompressed.GetBuffer()[checked((int)decompressed.Length - 1)]);
		decompressed.SetLength(decompressed.Length - 1);
		decompressed.Position = decompressed.Length;
		using var writer = new BinaryWriter(decompressed, Encoding.UTF8, leaveOpen: true);
		// Assembly-load events are incidental to task loading and need not occur in every MSBuild process.
		// Append a length-framed record explicitly: flags, context, three string indices, GUID, app-domain index.
		writer.Write7BitEncodedInt((int)BinaryLogRecordKind.AssemblyLoad);
		writer.Write7BitEncodedInt(22);
		writer.Write7BitEncodedInt(0); // No optional common fields.
		writer.Write7BitEncodedInt(0); // Assembly-loading context.
		writer.Write7BitEncodedInt(1); // Empty loading initiator.
		writer.Write7BitEncodedInt(1); // Empty assembly name.
		writer.Write7BitEncodedInt(1); // Empty assembly path.
		var guidStart = decompressed.Position;
		writer.Write(new Guid("01234567-89ab-cdef-0123-456789abcdef").ToByteArray());
		writer.Write7BitEncodedInt(1); // Empty app-domain descriptor.
		writer.Write7BitEncodedInt((int)BinaryLogRecordKind.EndOfFile);
		WriteBinlog();

		using (var stream = File.OpenRead(binlog))
			Assert.Equal(BinaryLogRecordKind.AssemblyLoad, new BinLogReader().ReadRecords(stream).Last(record => record.Args is not null).Kind);
		var validOutput = new RecordingOutput();
		BuildWarningsUtilities.OutputBuildErrorsFromBinLog(binlog, output: validOutput);
		Assert.Contains("TEST0002: test build error", validOutput.Text, StringComparison.Ordinal);
		Assert.DoesNotContain("Could not completely read", validOutput.Text, StringComparison.Ordinal);
		Assert.Equal("TEST0001", Assert.Single(Assert.Single(BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binlog)).WarningsPerCode).Code);

		decompressed.SetLength(guidStart + guidBytes);
		WriteBinlog();
		return binlog;

		void WriteBinlog()
		{
			decompressed.Position = 0;
			using var stream = File.Create(binlog);
			using var gzip = new GZipStream(stream, CompressionMode.Compress);
			decompressed.CopyTo(gzip);
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void DoesNotNormalizeEventCallbackFailures(bool shortGuid)
	{
		var binlog = CreateBinlog();
		Exception? callbackFailure = null;

		var exception = Assert.Throws<AggregateException>(() => BuildWarningsUtilities.ReadBuildEvents(binlog, args =>
		{
			try
			{
				if (shortGuid)
					_ = new Guid(new byte[1]);
				throw new ArgumentException("Callback failure", "b");
			}
			catch (ArgumentException failure)
			{
				callbackFailure = failure;
				throw;
			}
		}));

		Assert.NotNull(callbackFailure);
		Assert.Same(callbackFailure, Assert.Single(exception.Flatten().InnerExceptions));
	}

	[Fact]
	public void DoesNotNormalizeUnrelatedReadFailures()
	{
		Exception[] failures =
		[
			new ArgumentException("Unrelated argument", "b"),
			new ArgumentNullException("b"),
			new ArgumentOutOfRangeException("b"),
			new IOException("Read failure"),
			new InvalidDataException("Invalid data"),
			new OutOfMemoryException("Out of memory")
		];
		foreach (var failure in failures)
			Assert.Same(failure, BuildWarningsUtilities.NormalizeReadException(failure));

		Assert.Throws<ArgumentNullException>(() => BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(null!));
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
