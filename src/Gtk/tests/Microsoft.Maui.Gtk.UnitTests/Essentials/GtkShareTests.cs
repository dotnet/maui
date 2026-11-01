using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Essentials.Gtk;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests.Essentials;

public class GtkShareTests
{
	sealed class FakeBackend : IShareBackend
	{
		public List<string> Calls { get; } = new();
		public HashSet<string> Unhandled { get; } = new();
		public HashSet<string> Throwing { get; } = new();
		public bool UriResult { get; set; } = true;
		public bool FolderResult { get; set; } = true;

		public Task SetClipboardTextAsync(string text)
		{
			Calls.Add("clip:" + text);
			return Task.CompletedTask;
		}

		public Task<bool> OpenUriAsync(string uri)
		{
			Calls.Add("uri:" + uri);
			return Task.FromResult(UriResult);
		}

		public Task<bool> OpenFileAsync(string path)
		{
			Calls.Add("file:" + path);
			if (Throwing.Contains(path))
				throw new InvalidOperationException("no handler");
			return Task.FromResult(!Unhandled.Contains(path));
		}

		public Task<bool> OpenContainingFolderAsync(string path)
		{
			Calls.Add("folder:" + path);
			return Task.FromResult(FolderResult);
		}
	}

	[Fact]
	public async Task TextGoesToTheClipboardOnly()
	{
		var backend = new FakeBackend();
		await new GtkShare(backend).RequestAsync(new ShareTextRequest { Text = "hello" });
		Assert.Equal(new[] { "clip:hello" }, backend.Calls);
	}

	[Fact]
	public async Task UriIsOpenedAndTextCopied()
	{
		var backend = new FakeBackend();
		await new GtkShare(backend).RequestAsync(new ShareTextRequest { Text = "t", Uri = "https://example.org" });
		Assert.Equal(new[] { "clip:t", "uri:https://example.org" }, backend.Calls);

		backend.Calls.Clear();
		await new GtkShare(backend).RequestAsync(new ShareTextRequest { Uri = "https://example.org" });
		Assert.Equal(new[] { "uri:https://example.org" }, backend.Calls);
	}

	[Fact]
	public async Task UriNobodyHandlesFails()
	{
		var backend = new FakeBackend { UriResult = false };
		await Assert.ThrowsAsync<InvalidOperationException>(() => new GtkShare(backend).RequestAsync(new ShareTextRequest { Uri = "x://y" }));
	}

	[Fact]
	public async Task InvalidRequestsAreRejected()
	{
		var share = new GtkShare(new FakeBackend());
		await Assert.ThrowsAsync<ArgumentNullException>(() => share.RequestAsync((ShareTextRequest)null!));
		await Assert.ThrowsAsync<ArgumentNullException>(() => share.RequestAsync((ShareFileRequest)null!));
		await Assert.ThrowsAsync<ArgumentNullException>(() => share.RequestAsync((ShareMultipleFilesRequest)null!));
		await Assert.ThrowsAsync<ArgumentException>(() => share.RequestAsync(new ShareTextRequest()));
		await Assert.ThrowsAsync<ArgumentException>(() => share.RequestAsync(new ShareMultipleFilesRequest(new List<ShareFile>())));
	}

	[Fact]
	public async Task FileIsOpenedWithItsDefaultApplication()
	{
		using var dir = new TempDir();
		var path = dir.File("a.txt", "x");
		var backend = new FakeBackend();

		await new GtkShare(backend).RequestAsync(new ShareFileRequest("t", new ShareFile(path)));

		Assert.Equal(new[] { "file:" + path }, backend.Calls);
	}

	[Fact]
	public async Task MissingFileFailsBeforeAnythingIsOpened()
	{
		using var dir = new TempDir();
		var good = dir.File("a.txt", "x");
		var backend = new FakeBackend();

		await Assert.ThrowsAsync<FileNotFoundException>(() => new GtkShare(backend).RequestAsync(
			new ShareMultipleFilesRequest(new[] { new ShareFile(good), new ShareFile(Path.Combine(dir.Path, "none.txt")) })));

		Assert.Empty(backend.Calls);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task FileWithoutHandlerRevealsItsFolder(bool handlerThrows)
	{
		using var dir = new TempDir();
		var path = dir.File("a.bin", "x");
		var backend = new FakeBackend();
		(handlerThrows ? backend.Throwing : backend.Unhandled).Add(path);

		await new GtkShare(backend).RequestAsync(new ShareFileRequest(new ShareFile(path)));

		Assert.Equal(new[] { "file:" + path, "folder:" + path }, backend.Calls);
	}

	[Fact]
	public async Task NoHandlerAndNoFileManagerFails()
	{
		using var dir = new TempDir();
		var path = dir.File("a.bin", "x");
		var backend = new FakeBackend { FolderResult = false };
		backend.Unhandled.Add(path);

		await Assert.ThrowsAsync<InvalidOperationException>(() => new GtkShare(backend).RequestAsync(new ShareFileRequest(new ShareFile(path))));
	}

	[Fact]
	public async Task MultipleFilesAreAllAttemptedAndFailuresReported()
	{
		using var dir = new TempDir();
		var a = dir.File("a.txt", "1");
		var b = dir.File("b.txt", "2");
		var c = dir.File("c.txt", "3");
		var backend = new FakeBackend { FolderResult = false };
		backend.Unhandled.Add(a);
		backend.Unhandled.Add(b);

		var ex = await Assert.ThrowsAsync<AggregateException>(() => new GtkShare(backend).RequestAsync(
			new ShareMultipleFilesRequest(new[] { new ShareFile(a), new ShareFile(b), new ShareFile(c) })));

		Assert.Equal(2, ex.InnerExceptions.Count);
		Assert.Contains("file:" + c, backend.Calls);
		Assert.Equal(5, backend.Calls.Count);
	}

	[Fact]
	public async Task MultipleFilesOpenInOrder()
	{
		using var dir = new TempDir();
		var paths = new[] { dir.File("1.txt", "1"), dir.File("2.txt", "2") };

		var backend = new FakeBackend();
		await new GtkShare(backend).RequestAsync(new ShareMultipleFilesRequest("t", paths.Select(p => new ShareFile(p)).ToList()));

		Assert.Equal(paths.Select(p => "file:" + p), backend.Calls);
	}
}
