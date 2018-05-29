using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.Forms.Build.Tasks;
using Xamarin.Forms.Core.UnitTests;
using Microsoft.Build.Utilities;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class XamlgFileLockTests : BaseTestFixture
	{
		string CreateXamlInputFile ()
		{
			string xaml = 
				@"<ContentPage xmlns='http://xamarin.com/schemas/2014/forms' xmlns:x='http://schemas.microsoft.com/winfx/2009/xaml' x:Class='Test.MyPage'>
					<ContentPage.Content></ContentPage.Content> 
				</ContentPage>";

			string fileName = Path.GetTempFileName ();
			File.WriteAllText (fileName, xaml);

			return fileName;
		}

		[Test]
		public void XamlFileShouldNotBeLockedAfterFileIsGenerated ()
		{
			string xamlInputFile = CreateXamlInputFile ();
			var item = new TaskItem(xamlInputFile);
			item.SetMetadata("TargetPath", xamlInputFile);
			var generator = new XamlGTask() {
				BuildEngine= new DummyBuildEngine(),
				AssemblyName = "test",
				Language = "C#",
				XamlFiles = new[] { item },
				OutputFiles = new[] { new TaskItem(xamlInputFile + ".g.cs") }
			};

			generator.Execute();

			string xamlOutputFile = generator.OutputFiles.First().ItemSpec;
			File.Delete (xamlOutputFile);

			Assert.DoesNotThrow (() => File.Delete (xamlInputFile));
		}
	}
}
