using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class GetMauiAssetsPathTests : MSBuildTaskTestFixture<GetMauiAssetPath>
	{
#if WINDOWS
		const string ProjectDirectory = @"C:\src\code\MyProject";
		const string LibraryProjectDirectory = ProjectDirectory + @"\ClassLibrary1";
#else
		const string ProjectDirectory = @"/usr/code/MyProject";
		const string LibraryProjectDirectory = ProjectDirectory + @"/ClassLibrary1";
#endif

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

		[Theory]
		[InlineData("foo.mp3", "foo.mp3")]
		[InlineData(ProjectDirectory + @"\foo.mp3", "foo.mp3")]
#if WINDOWS
		[InlineData("foo.mp3", @"Assets\foo.mp3", "Assets")]
		[InlineData("Resources/Assets/foo.mp3", @"Resources\Assets\foo.mp3")]
		[InlineData(@"Resources\Assets\foo.mp3", @"Resources\Assets\foo.mp3")]
		[InlineData(ProjectDirectory + @"\foo.mp3", @"Assets\foo.mp3", "Assets")]
#else
		[InlineData("foo.mp3", @"Assets/foo.mp3", "Assets")]
		[InlineData("Resources/Assets/foo.mp3", @"Resources/Assets/foo.mp3")]
		[InlineData(@"Resources\Assets\foo.mp3", @"Resources/Assets/foo.mp3")]
		[InlineData(ProjectDirectory + @"\foo.mp3", @"Assets/foo.mp3", "Assets")]
#endif
		public void LinkMetadataIsBlank(string input, string output, string folderName = null)
		{
			var item = new TaskItem(input);
			var task = GetNewTask(folderName, item);
			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			Assert.Equal(output, task.Output[0].GetMetadata("Link"));
		}

		[Theory]
		[InlineData(@"C:\Program Files\foo.mp3", "foo.mp3", "foo.mp3")]
		[InlineData("foo.mp3", ProjectDirectory + @"\foo.mp3", "foo.mp3")]
#if WINDOWS
		[InlineData(@"\Resources\Assets\foo.mp3", @"Resources\Assets\foo.mp3", @"Resources\Assets\foo.mp3")]
		[InlineData(@"/Resources/Assets/foo.mp3", @"Resources\Assets\foo.mp3", @"Resources\Assets\foo.mp3")]
		[InlineData(@"C:\Program Files\foo.mp3", "foo.mp3", @"Assets\foo.mp3", "Assets")]
		[InlineData(@"C:/Program Files/foo.mp3", "foo.mp3", @"Assets\foo.mp3", "Assets")]
		[InlineData("foo.mp3", ProjectDirectory + @"\foo.mp3", @"Assets\foo.mp3", "Assets")]
#else
		[InlineData(@"\Resources\Assets\foo.mp3", @"Resources\Assets\foo.mp3", @"Resources/Assets/foo.mp3")]
		[InlineData(@"/Resources/Assets/foo.mp3", "Resources/Assets/foo.mp3", "Resources/Assets/foo.mp3")]
		[InlineData(@"\Program Files\foo.mp3", "foo.mp3", @"Assets/foo.mp3", "Assets")]
		[InlineData(@"/Program Files/foo.mp3", "foo.mp3", @"Assets/foo.mp3", "Assets")]
		[InlineData("foo.mp3", ProjectDirectory + @"\foo.mp3", @"Assets/foo.mp3", "Assets")]
#endif
		public void UseLinkMetadata(string input, string link, string output, string folderName = null)
		{
			var item = new TaskItem(input);
			item.SetMetadata("Link", link);
			var task = GetNewTask(folderName, item);
			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			Assert.Equal(output, task.Output[0].GetMetadata("Link"));
		}

		[Theory]
		[InlineData("foo.mp3", "foo.mp3")]
#if WINDOWS
		[InlineData("foo.mp3", @"Assets\foo.mp3", "Assets")]
#else
		[InlineData("foo.mp3", @"Assets/foo.mp3", "Assets")]
#endif
		public void UseTargetPath(string input, string output, string folderName = null)
		{
			var item = new TaskItem(input);
			var task = GetNewTask(folderName, item);
			task.ItemMetadata = "TargetPath";
			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			Assert.Equal(output, task.Output[0].GetMetadata("TargetPath"));
		}

		[Theory]
		[InlineData(LibraryProjectDirectory + @"\foo.mp3", "foo.mp3")]
		[InlineData(LibraryProjectDirectory + @"\foo.mp3", "foo.mp3", null, @"\")]
		[InlineData(LibraryProjectDirectory + @"\foo.mp3", "foo.mp3", null, @"/")]
#if WINDOWS
		[InlineData(LibraryProjectDirectory + @"\foo.mp3", @"Assets\foo.mp3", "Assets")]
		[InlineData(LibraryProjectDirectory + @"\foo.mp3", @"Assets\foo.mp3", "Assets", @"\")]
		[InlineData(LibraryProjectDirectory + @"\foo.mp3", @"Assets\foo.mp3", "Assets", @"/")]
#else
		[InlineData(LibraryProjectDirectory + @"\foo.mp3", @"Assets/foo.mp3", "Assets")]
		[InlineData(LibraryProjectDirectory + @"\foo.mp3", @"Assets/foo.mp3", "Assets", @"\")]
		[InlineData(LibraryProjectDirectory + @"\foo.mp3", @"Assets/foo.mp3", "Assets", @"/")]
#endif
		public void UseProjectDirectory(string input, string output, string folderName = null, string suffix = null)
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
