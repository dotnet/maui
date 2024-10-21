﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using SkiaSharp;
using SkiaSharp.Extended;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class BaseTest : IDisposable
	{
		private const string TestFolderName = "Microsoft.Maui.Resizetizer.Tests";
		private const string TestImagesFolderName = "imageresults";
		private const double ImageErrorThreshold = 0.0027;

		private readonly string DeleteDirectory;
		protected readonly string DestinationDirectory;

		public BaseTest(string testContextDirectory = null)
		{
			if (!string.IsNullOrEmpty(testContextDirectory))
			{
				DestinationDirectory = testContextDirectory;
				DeleteDirectory = testContextDirectory;
			}
			else
			{
				var name = GetType().FullName;
				if (name.StartsWith(TestFolderName + ".", StringComparison.OrdinalIgnoreCase))
					name = name.Substring(TestFolderName.Length + 1);

				var dir = Path.Combine(Path.GetTempPath(), TestFolderName, name, Path.GetRandomFileName());

				DestinationDirectory = dir;
				DeleteDirectory = dir;
			}
		}

		public virtual void Dispose()
		{
			if (Directory.Exists(DeleteDirectory))
				Directory.Delete(DeleteDirectory, true);
		}

		protected void AssertFileSize(string file, int width, int height)
		{
			file = Path.Combine(DestinationDirectory, file);

			Assert.True(File.Exists(file), $"File did not exist: {file}");

			using var codec = SKCodec.Create(file);
			Assert.Equal(new SKSizeI(width, height), codec.Info.Size);
		}

		protected void AssertFileExists(string file)
		{
			file = Path.Combine(DestinationDirectory, file);

			Assert.True(File.Exists(file), $"File did not exist: {file}");
		}

		protected void AssertFileNotExists(string file)
		{
			file = Path.Combine(DestinationDirectory, file);

			Assert.False(File.Exists(file), $"File existed: {file}");
		}

		protected void AssertFileContains(string file, params string[] snippet)
		{
			file = Path.Combine(DestinationDirectory, file);

			var content = File.ReadAllText(file);

			foreach (var snip in snippet)
				Assert.Contains(snip, content, StringComparison.Ordinal);
		}

		protected void AssertFileContains(string file, SKColor color, int x, int y)
		{
			file = Path.Combine(DestinationDirectory, file);

			using var resultImage = SKBitmap.Decode(file);
			Assert.Equal(color, resultImage.GetPixel(x, y));
		}

		protected void AssertFileDoesNotContain(string file, SKColor color, int x, int y)
		{
			file = Path.Combine(DestinationDirectory, file);

			using var resultImage = SKBitmap.Decode(file);
			Assert.NotEqual(color, resultImage.GetPixel(x, y));
		}

		protected void AssertFileContains(string file, params SKColor[] colors)
		{
			file = Path.Combine(DestinationDirectory, file);

			using var resultImage = SKBitmap.Decode(file);
			var pixels = resultImage.Pixels;
			foreach (var color in colors)
				Assert.Contains(color, pixels);
		}

		protected void AssertFileDoesNotContain(string file, params SKColor[] colors)
		{
			file = Path.Combine(DestinationDirectory, file);

			using var resultImage = SKBitmap.Decode(file);
			var pixels = resultImage.Pixels;
			foreach (var color in colors)
				Assert.DoesNotContain(color, pixels);
		}

		protected void AssertFileMatches(string actualFilename, object[] args = null, [CallerMemberName] string methodName = null) =>
			AssertFileMatchesReal(actualFilename, args, methodName);

		protected void SaveImageResultFile(string actualFilename, object[] args = null, [CallerMemberName] string methodName = null) =>
			SaveImageResultFileReal(actualFilename, args, methodName);

		void AssertFileMatchesReal(string actualFilename, object[] args = null, [CallerMemberName] string methodName = null)
		{
			actualFilename = Path.IsPathRooted(actualFilename)
				? actualFilename
				: Path.Combine(DestinationDirectory, actualFilename);

			var expectedFilename = GetTestImageFileName(args, methodName, Path.GetExtension(actualFilename));

			if (Path.GetExtension(actualFilename).Equals(".json", StringComparison.OrdinalIgnoreCase))
			{
				var actual = File.ReadAllText(actualFilename);
				var expected = File.ReadAllText(expectedFilename);

				Assert.Equal(expected, actual);
			}
			else
			{
				using var actual = SKImage.FromEncodedData(actualFilename);
				using var expected = SKImage.FromEncodedData(expectedFilename);

				var similarity = SKPixelComparer.Compare(actual, expected);

				var isSimilar = similarity.ErrorPixelPercentage <= ImageErrorThreshold;

				if (!isSimilar)
				{
					var root = GetTestProjectRoot();

					var maskFilename = Path.Combine(root, "errors", expectedFilename);
					maskFilename = Path.ChangeExtension(maskFilename, ".mask.png");

					Directory.CreateDirectory(Path.GetDirectoryName(maskFilename));

					using (var mask = SKPixelComparer.GenerateDifferenceMask(actual, expected))
					using (var data = mask.Encode(SKEncodedImageFormat.Png, 100))
					using (var maskFile = File.Create(maskFilename))
					{
						data.SaveTo(maskFile);
					}

					Assert.True(
						isSimilar,
						$"Image was not equal. Error was {similarity.ErrorPixelPercentage}% ({similarity.AbsoluteError} pixels). See {maskFilename}");
				}
			}
		}

		void SaveImageResultFileReal(string destinationFilename, object[] args = null, [CallerMemberName] string methodName = null)
		{
			var root = GetTestProjectRoot();
			var imagePath = GetTestImageFileName(args, methodName, Path.GetExtension(destinationFilename));
			var path = Path.Combine(root, imagePath);

			var dir = Path.GetDirectoryName(path);
			Directory.CreateDirectory(dir);

			var src = Path.IsPathRooted(destinationFilename)
				? destinationFilename
				: Path.Combine(DestinationDirectory, destinationFilename);

			File.Copy(src, path, true);
		}

		private string GetTestImageFileName(object[] args, string methodName, string extension)
		{
			var strsSelect = args?.Select(a => string
				.Format(CultureInfo.InvariantCulture, "{0}", a ?? "NULL")
				.Replace("\\", "SLASH", StringComparison.OrdinalIgnoreCase)
				.Replace("/", "SLASH", StringComparison.OrdinalIgnoreCase)
				.Replace("#", "", StringComparison.OrdinalIgnoreCase));

			var strs = strsSelect?.ToArray();
			var sepStr = strs?.Length > 0 ? "-" : "";
			var argStr = strs?.Length > 0 ? string.Join("-", strs) : "";

			var filename = $"output{sepStr}{argStr}{extension}";

			var name = GetType().FullName;
			if (name.StartsWith(TestFolderName + ".", StringComparison.OrdinalIgnoreCase))
				name = name.Substring(TestFolderName.Length + 1);

			return Path.Combine(TestImagesFolderName, name, methodName, filename);
		}

		private static string GetTestProjectRoot()
		{
			var cwd = Directory.GetCurrentDirectory();

			var root = Path.Combine(cwd, "../../../../../src/SingleProject/Resizetizer/test/UnitTests/");

			return root;
		}
	}
}
