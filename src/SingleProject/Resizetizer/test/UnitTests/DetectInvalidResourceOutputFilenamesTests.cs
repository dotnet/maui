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
	public class DetectInvalidResourceOutputFilenamesTests
	{
		public class ExecuteForApp : MSBuildTaskTestFixture<DetectInvalidResourceOutputFilenamesTask>
		{
			protected DetectInvalidResourceOutputFilenamesTask GetNewTask(params ITaskItem[] items) =>
				new DetectInvalidResourceOutputFilenamesTask
				{
					Items = items,
					ThrowsError = true,
					ErrorMessage = "Invalid Filenames: ",
					BuildEngine = this,
				};

			protected ITaskItem GetInvalidFilename(DetectInvalidResourceOutputFilenamesTask task, string path) =>
				task.InvalidItems.Single(c => c.ItemSpec.Replace("\\", "/").EndsWith(path));

			protected void AssertValidFilename(DetectInvalidResourceOutputFilenamesTask task, ITaskItem item)
				=> Assert.DoesNotContain(task.InvalidItems, c => c.ItemSpec == item.ItemSpec);

			protected void AssertInvalidFilename(DetectInvalidResourceOutputFilenamesTask task, ITaskItem item)
				=> Assert.Contains(task.InvalidItems, c => c.ItemSpec == item.ItemSpec);

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
			public void InvalidFileFails()
			{
				var i = new TaskItem("images/appiconfg-red-512.svg");
				var task = GetNewTask(i);

				var success = task.Execute();

				AssertInvalidFilename(task, i);
				Assert.False(success);
			}
		}
	}
}
