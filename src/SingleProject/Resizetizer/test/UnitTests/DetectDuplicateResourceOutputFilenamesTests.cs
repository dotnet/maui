using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class DetectDuplicateResourceOutputFilenamesTests
	{
		public class ExecuteForApp : MSBuildTaskTestFixture<DetectDuplicateResourceOutputFilenamesTask>
		{
			protected DetectDuplicateResourceOutputFilenamesTask GetNewTask(params ITaskItem[] items) =>
				new DetectDuplicateResourceOutputFilenamesTask
				{
					Items = items,
					Message = "Duplicate Filenames: ",
					BuildEngine = this,
				};

			protected string GetInvalidFilename(DetectDuplicateResourceOutputFilenamesTask task, string path) =>
				task.InvalidItems.Single(c => c.Replace('\\', '/').EndsWith(path, StringComparison.Ordinal));

			protected void AssertValidFilename(DetectDuplicateResourceOutputFilenamesTask task, ITaskItem item)
				=> Assert.DoesNotContain(task.InvalidItems ?? Enumerable.Empty<string>(), c => c == item.ItemSpec);

			protected void AssertInvalidFilename(DetectDuplicateResourceOutputFilenamesTask task, ITaskItem item)
				=> Assert.Contains(task.InvalidItems ?? Enumerable.Empty<string>(), c => c == item.ItemSpec);

			[Fact]
			public void NoItemsSucceed()
			{
				var task = GetNewTask();

				var success = task.Execute();

				Assert.True(success);
			}

			[Fact]
			public void NullItemsSucceed()
			{
				var task = GetNewTask(null);

				var success = task.Execute();

				Assert.True(success);
			}

			[Fact]
			public void NonExistantFileFails()
			{
				var items = new[]
				{
					new TaskItem("non-existant.png"),
				};

				var task = GetNewTask(items);

				var success = task.Execute();

				Assert.False(success);
			}

			[Fact]
			public void ValidFileSucceeds()
			{
				var i = new TaskItem("images/camera.png");
				var task = GetNewTask(i);

				var success = task.Execute();

				AssertValidFilename(task, i);
				Assert.True(success);
			}

			[Fact]
			public void SameMultipleFilesFailsWithCorrectMessage()
			{
				var i = new TaskItem("images/camera.png");
				var j = new TaskItem("images/camera.png");
				var task = GetNewTask(i, j);

				var success = task.Execute();
				Assert.True(success);

				Assert.Equal("Duplicate Filenames: camera", LogWarningEvents[0].Message);
			}

			[Fact]
			public void DifferentMultipleFilesFailsWithCorrectMessage()
			{
				var i = new TaskItem("images/camera.png");
				var j = new TaskItem("images/camera.svg");
				var task = GetNewTask(i, j);

				var success = task.Execute();
				Assert.True(success);

				Assert.Equal("Duplicate Filenames: camera", LogWarningEvents[0].Message);
			}

			[Fact]
			public void MultipleMultipleFilesFailsWithCorrectMessage()
			{
				var i = new TaskItem("images/camera.png");
				var j = new TaskItem("images/camera.svg");
				var k = new TaskItem("images/camera_color.png");
				var l = new TaskItem("images/camera_color.svg");
				var task = GetNewTask(i, j, k, l);

				var success = task.Execute();
				Assert.True(success);

				Assert.Equal("Duplicate Filenames: camera, camera_color", LogWarningEvents[0].Message);
			}
		}
	}
}
