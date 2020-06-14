using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Utilities;
using Xamarin.Forms;
using Xamarin.Forms.Build.Tasks;

using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;
using Xamarin.Forms.Controls;
using Xamarin.Forms.MSBuild.UnitTests;
using IOPath = System.IO.Path;

namespace Xamarin.Forms.Xaml.UnitTests
{	
	public partial class Gh2691 : ContentPage
	{
		public Gh2691()
		{
			InitializeComponent();
		}

		public Gh2691(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{			
			const string c_xaml = @"
				<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
							 xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							 xmlns:test=""http://xamarin.com/schemas/2014/forms/customurl1""
							 xmlns:test2=""http://xamarin.com/schemas/2014/forms/customurl2""
							 xmlns:test3=""clr-namespace:Xamarin.Forms.Controls.CustomNamespace1;assembly=Xamarin.Forms.Controls""
							 xmlns:test4=""using:Xamarin.Forms.Xaml.UnitTests""
							 x:Class=""Xamarin.Forms.Xaml.UnitTests.Gh2691"">
					<ContentPage.Content>
						<StackLayout>
							<test:CustomButton x:Name=""_testButton1""
											   Text=""customurl1 - CustomNamespace1 - CustomButton""/>
							<test:CustomLabel x:Name=""_testLabel1"" 
											  Text=""customurl1 - CustomNamespace2 - CustomLabel""/>
							<test2:CustomStackLayout x:Name=""_testStackLayout""
													 Orientation=""Vertical"">
								<test2:CustomLabel x:Name=""_testLabel2""
												   Text=""customurl2 - CustomNamespace3 - CustomLabel""/>
								<test3:CustomButton x:Name=""_testButton2""
													Text=""clr-namespace - CustomNamespace1 - CustomButton""/>
								<test4:Gh2691TestUsingSyntaxLabel x:Name=""_testLabel3""
																  Text=""using - UnitTests - Gh2691TestUsingSyntaxLabel""/>
							</test2:CustomStackLayout>
						</StackLayout>
					</ContentPage.Content>
				</ContentPage>";

			[SetUp]
			public void SetUp()
			{
				Device.PlatformServices = new MockPlatformServices();
				GH2691.Init();	// only to make sure compiler pulls in Controls assembly
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestXamlParserAndGenerator(bool useCompiledXaml)
			{
				Gh2691 issue2691 = new Gh2691(useCompiledXaml);

				// http://xamarin.com/schemas/2014/forms/customurl1 -> Xamarin.Forms.Controls.CustomNamespace1
				var button = issue2691.FindByName("_testButton1") as Controls.CustomNamespace1.CustomButton;
				Assert.IsNotNull(button);

				// http://xamarin.com/schemas/2014/forms/customurl1 -> Xamarin.Forms.Controls.CustomNamespace2
				var label1 = issue2691.FindByName("_testLabel1") as Controls.CustomNamespace2.CustomLabel;
				Assert.IsNotNull(label1);

				// http://xamarin.com/schemas/2014/forms/customurl2 -> Xamarin.Forms.Controls.CustomNamespace3
				var label2 = issue2691.FindByName("_testLabel2") as Controls.CustomNamespace3.CustomLabel;
				Assert.IsNotNull(label2);

				// http://xamarin.com/schemas/2014/forms/customurl2 -> Xamarin.Forms.Controls.CustomNamespace3
				var stack = issue2691.FindByName("_testStackLayout") as Controls.CustomNamespace3.CustomStackLayout;
				Assert.IsNotNull(stack);

				// clr-namespace:Xamarin.Forms.Controls.CustomNamespace1;assembly=Xamarin.Forms.Controls
				var button2 = issue2691.FindByName("_testButton2") as Controls.CustomNamespace1.CustomButton;
				Assert.IsNotNull(button2);

				// using:Xamarin.Forms.Xaml.UnitTests
				var label3 = issue2691.FindByName("_testLabel3") as Gh2691TestUsingSyntaxLabel;
				Assert.IsNotNull(label3);
			}

			[TestCase]
			public void TestXamlCompiler()
			{
				MockCompiler.Compile(typeof(Gh2691));
			}

			[TestCase]
			public void TestXamlGenerator()
			{
				string xamlInputFile = CreateXamlInputFile();
				string xamlOutputFile = IOPath.GetTempFileName();
				var item = new TaskItem(xamlInputFile);
				item.SetMetadata("TargetPath", xamlInputFile);

				string testAssemblyBinPath =
#if DEBUG
					"Debug";
#else
					"Release";
#endif

				var references = string.Join(";",
					IOPath.GetFullPath(
						IOPath.Combine(
							TestContext.CurrentContext.TestDirectory, "Xamarin.Forms.Controls.dll")),
					IOPath.GetFullPath(
						IOPath.Combine(
							TestContext.CurrentContext.TestDirectory, "Xamarin.Forms.Core.dll"))
					);
				var xamlg = new XamlGTask()
				{
					BuildEngine = new DummyBuildEngine(),
					AssemblyName = "test",
					Language = "C#",
					XamlFiles = new[] { item },
					OutputFiles = new[] { new TaskItem(xamlOutputFile) },
					References = references
				};

				var generator = new XamlGenerator(item, xamlg.Language, xamlg.AssemblyName, xamlOutputFile, xamlg.References, null);
				Assert.IsTrue(generator.Execute());

				Assert.IsTrue(xamlg.Execute());
			}

			string CreateXamlInputFile()
			{
				string fileName = IOPath.GetTempFileName();
				File.WriteAllText(fileName, c_xaml);
				return fileName;
			}
		}
	}

	public class Gh2691TestUsingSyntaxLabel : Label
	{
	}
}