
using System.Collections;
using Newtonsoft.Json;

namespace Microsoft.Maui.IntegrationTests
{
	[Category(Categories.Samples)]
	public class SampleTests : BaseBuildTest
	{
		public static IEnumerable SampleTestMatrix
		{
			get
			{
				// Parse individual project files from `Microsoft.Maui.Samples.slnf` to generate a set of test cases
				var sampleSln = Path.Combine(TestEnvironment.GetMauiDirectory(), "eng", "Microsoft.Maui.Samples.slnf");
				var slnFile = JsonConvert.DeserializeObject<SolutionFile>(File.ReadAllText(sampleSln));
				foreach (var projectFile in slnFile?.solution.projects ?? new List<string> { sampleSln })
				{
					foreach (var config in new[] { "Debug", "Release" })
					{
						yield return new TestCaseData(projectFile, config);
					}
				}
			}
		}

		[Test]
		[TestCaseSource(nameof(SampleTestMatrix))]
		public void Build(string relativeProj, string config)
		{
			var projectFile = Path.GetFullPath(Path.Combine(TestEnvironment.GetMauiDirectory(), relativeProj));
			var binlog = Path.Combine(LogDirectory, Path.Combine("sample.binlog"));
			var sampleProps = new[]
			{
				"UseWorkload=true",
				$"RestoreConfigFile={TestNuGetConfig}",
				// Surface warnings as build errors
				"TreatWarningsAsErrors=true",
				// Detailed trimmer warnings, if present
				"TrimmerSingleWarn=false",
			};

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: sampleProps, binlogPath: binlog),
					$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

	}


	public class SolutionFile
	{
		public SolutionElement solution { get; set; } = new SolutionElement();
	}

	public class SolutionElement
	{
		public string path { get; set; } = "";
		public List<string> projects { get; set; } = new List<string>();
	}
}
