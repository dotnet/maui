using System;
using System.IO;
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
			string xamlOutputFile = Path.ChangeExtension (xamlInputFile, ".xaml.g.cs");
			var generator = new XamlGTask ();
			generator.BuildEngine = new DummyBuildEngine ();
			generator.AssemblyName = "Test";
			generator.Language = "C#";

			generator.Execute(new TaskItem(xamlInputFile), xamlOutputFile);
			File.Delete (xamlOutputFile);

			Assert.DoesNotThrow (() => File.Delete (xamlInputFile));
		}
	}
}
