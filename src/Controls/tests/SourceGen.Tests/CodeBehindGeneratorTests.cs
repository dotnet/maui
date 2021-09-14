using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.TestUtils.SourceGen;

namespace Microsoft.Maui.Controls.SourceGen.Tests
{
	public class CodeBehindGeneratorTests : BaseSourceGeneratorTests<CodeBehindGenerator>
	{
		public CodeBehindGeneratorTests(ITestOutputHelper output) : base(output)
		{
			Generator.AddReferences(
				"Microsoft.Extensions.DependencyInjection.Abstractions",
				"Microsoft.Maui",
				"Microsoft.Maui.Graphics",
				"Microsoft.Maui.Controls",
				"Microsoft.Maui.Controls.Xaml");

			Generator.AddMSBuildProperty("RootNamespace", "CodeBehindGenTestApp");
			Generator.AddMSBuildProperty("TargetPlatformIdentifier", "ios");
			Generator.AddMSBuildProperty("OutputType", "Exe");
			Generator.AddMSBuildProperty("UseMaui", "true");

			Generator.AddMSBuildItems(new TaskItem("CompilerVisibleItemMetadata",
				new Dictionary<string, string>
				{
					{ "Include", "AdditionalFiles" },
					{ "MetadataName", "SourceItemGroup" },
					{ "Visible", "False" }
				}));
		}

		[Fact]
		public void SimpleXaml()
		{
			Generator.AddMSBuildItems(
				new TaskItem("TestData\\SimpleTest.xaml", new Dictionary<string, string> {
					{ "ManifestResourceName", "TestData_SimpleTestXaml" },
					{ "TargetPath", "TestData\\SimpleTest.xaml" },
					{ "GenKind", "Xaml" }
				}));

			RunGenerator();
			Compilation.AssertContent("label");
		}
	}
}
