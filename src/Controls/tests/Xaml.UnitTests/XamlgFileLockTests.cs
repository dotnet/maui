using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Utilities;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class XamlgFileLockTests : BaseTestFixture
	{
		string CreateXamlInputFile()
		{
			string xaml =
				@"<ContentPage xmlns='http://xamarin.com/schemas/2014/forms' xmlns:x='http://schemas.microsoft.com/winfx/2009/xaml' x:Class='Test.MyPage'>
					<ContentPage.Content></ContentPage.Content> 
				</ContentPage>";

			string fileName = IOPath.GetTempFileName();
			File.WriteAllText(fileName, xaml);

			return fileName;
		}

		[Test]
		public void XamlFileShouldNotBeLockedAfterFileIsGenerated()
		{
			string xamlInputFile = CreateXamlInputFile();
			var item = new TaskItem(xamlInputFile);
			item.SetMetadata("TargetPath", xamlInputFile);
			var generator = new XamlGTask()
			{
				BuildEngine = new MSBuild.UnitTests.DummyBuildEngine(),
				AssemblyName = "test",
				Language = "C#",
				XamlFiles = new[] { item },
				OutputFiles = new[] { new TaskItem(xamlInputFile + ".g.cs") }
			};

			generator.Execute();

			string xamlOutputFile = generator.OutputFiles.First().ItemSpec;
			File.Delete(xamlOutputFile);

			Assert.DoesNotThrow(() => File.Delete(xamlInputFile));
		}
	}
}
