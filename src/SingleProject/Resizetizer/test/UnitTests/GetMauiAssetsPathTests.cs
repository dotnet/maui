using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class GetMauiAssetsPathTests : MSBuildTaskTestFixture<GetMauiAssetPath>
	{
		static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		static string ProjectDirectory => IsWindows ? @"C:\src\code\MyProject" : @"/usr/code/MyProject";
		static string LibraryProjectDirectory => ProjectDirectory + (IsWindows ? @"\ClassLibrary1" : @"/ClassLibrary1");
		static string Sep => IsWindows ? @"\" : "/";

		public GetMauiAssetsPathTests(ITestOutputHelper output)
			: base(output)
		{
		}

		protected GetMauiAssetPath GetNewTask(string folderName, params ITaskItem[] input) => new()
		{
			ProjectDirectory = ProjectDirectory,
			FolderName = folderName,
			Input = input
		};

		public static IEnumerable<object[]> LinkMetadataIsBlankData()
		{
			yield return new object[] { "foo.mp3", "foo.mp3", null };
			yield return new object[] { ProjectDirectory + @"\foo.mp3", "foo.mp3", null };
			yield return new object[] { "foo.mp3", $"Assets{Sep}foo.mp3", "Assets" };
			yield return new object[] { "Resources/Assets/foo.mp3", $"Resources{Sep}Assets{Sep}foo.mp3", null };
			yield return new object[] { @"Resources\Assets\foo.mp3", $"Resources{Sep}Assets{Sep}foo.mp3", null };
			yield return new object[] { ProjectDirectory + @"\foo.mp3", $"Assets{Sep}foo.mp3", "Assets" };
		}

		[Theory]
		[MemberData(nameof(LinkMetadataIsBlankData))]
		public void LinkMetadataIsBlank(string input, string output, string folderName)
		{
			var item = new TaskItem(input);
			var task = GetNewTask(folderName, item);
			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			Assert.Equal(output, task.Output[0].GetMetadata("Link"));
		}

		public static IEnumerable<object[]> UseLinkMetadataData()
		{
			if (IsWindows)
			{
				yield return new object[] { @"C:\Program Files\foo.mp3", "foo.mp3", "foo.mp3", null };
				yield return new object[] { "foo.mp3", ProjectDirectory + @"\foo.mp3", "foo.mp3", null };
				yield return new object[] { @"\Resources\Assets\foo.mp3", @"Resources\Assets\foo.mp3", @"Resources\Assets\foo.mp3", null };
				yield return new object[] { @"/Resources/Assets/foo.mp3", @"Resources\Assets\foo.mp3", @"Resources\Assets\foo.mp3", null };
				yield return new object[] { @"C:\Program Files\foo.mp3", "foo.mp3", @"Assets\foo.mp3", "Assets" };
				yield return new object[] { @"C:/Program Files/foo.mp3", "foo.mp3", @"Assets\foo.mp3", "Assets" };
				yield return new object[] { "foo.mp3", ProjectDirectory + @"\foo.mp3", @"Assets\foo.mp3", "Assets" };
			}
			else
			{
				yield return new object[] { @"/Program Files/foo.mp3", "foo.mp3", "foo.mp3", null };
				yield return new object[] { "foo.mp3", ProjectDirectory + @"/foo.mp3", "foo.mp3", null };
				yield return new object[] { @"\Resources\Assets\foo.mp3", @"Resources\Assets\foo.mp3", @"Resources/Assets/foo.mp3", null };
				yield return new object[] { @"/Resources/Assets/foo.mp3", "Resources/Assets/foo.mp3", "Resources/Assets/foo.mp3", null };
				yield return new object[] { @"\Program Files\foo.mp3", "foo.mp3", @"Assets/foo.mp3", "Assets" };
				yield return new object[] { @"/Program Files/foo.mp3", "foo.mp3", @"Assets/foo.mp3", "Assets" };
				yield return new object[] { "foo.mp3", ProjectDirectory + @"/foo.mp3", @"Assets/foo.mp3", "Assets" };
			}
		}

		[Theory]
		[MemberData(nameof(UseLinkMetadataData))]
		public void UseLinkMetadata(string input, string link, string output, string folderName)
		{
			var item = new TaskItem(input);
			item.SetMetadata("Link", link);
			var task = GetNewTask(folderName, item);
			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			Assert.Equal(output, task.Output[0].GetMetadata("Link"));
		}

		public static IEnumerable<object[]> UseTargetPathData()
		{
			yield return new object[] { "foo.mp3", "foo.mp3", null };
			yield return new object[] { "foo.mp3", $"Assets{Sep}foo.mp3", "Assets" };
		}

		[Theory]
		[MemberData(nameof(UseTargetPathData))]
		public void UseTargetPath(string input, string output, string folderName)
		{
			var item = new TaskItem(input);
			var task = GetNewTask(folderName, item);
			task.ItemMetadata = "TargetPath";
			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			Assert.Equal(output, task.Output[0].GetMetadata("TargetPath"));
		}

		public static IEnumerable<object[]> UseProjectDirectoryData()
		{
			yield return new object[] { LibraryProjectDirectory + @"\foo.mp3", "foo.mp3", null, null };
			yield return new object[] { LibraryProjectDirectory + @"\foo.mp3", "foo.mp3", null, @"\" };
			yield return new object[] { LibraryProjectDirectory + @"\foo.mp3", "foo.mp3", null, @"/" };
			yield return new object[] { LibraryProjectDirectory + (IsWindows ? @"\foo.mp3" : "/foo.mp3"), $"Assets{Sep}foo.mp3", "Assets", null };
			yield return new object[] { LibraryProjectDirectory + (IsWindows ? @"\foo.mp3" : "/foo.mp3"), $"Assets{Sep}foo.mp3", "Assets", @"\" };
			yield return new object[] { LibraryProjectDirectory + (IsWindows ? @"\foo.mp3" : "/foo.mp3"), $"Assets{Sep}foo.mp3", "Assets", @"/" };
		}

		[Theory]
		[MemberData(nameof(UseProjectDirectoryData))]
		public void UseProjectDirectory(string input, string output, string folderName, string suffix)
		{
			var item = new TaskItem(input);
			item.SetMetadata("ProjectDirectory", LibraryProjectDirectory + suffix);
			var task = GetNewTask(folderName, item);
			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			Assert.Equal(output, task.Output[0].GetMetadata("Link"));
		}
	}
}
