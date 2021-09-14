using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

#nullable enable

namespace Microsoft.Maui.TestUtils.SourceGen
{
	public partial class AssemblyGenerator
	{
		class TestAnalyzerOptionsConfigProvider : AnalyzerConfigOptionsProvider
		{
			public TestAnalyzerOptionsConfigProvider(Dictionary<string, string> options)
				=> GlobalOptions = new TestAnalyzerConfigOptions(options.ToImmutableDictionary());

			public override AnalyzerConfigOptions GlobalOptions { get; }

			public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
				=> new TestAnalyzerConfigOptions();

			public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
			{
				if (textFile is MSBuildItemGroupAdditionalText msBuildItemAdditionalText)
					return new TestAnalyzerConfigOptions(msBuildItemAdditionalText.Metadata);

				return new TestAnalyzerConfigOptions();
			}

			sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
			{
				ImmutableDictionary<string, string> backing;

				public TestAnalyzerConfigOptions()
					=> backing = new Dictionary<string, string>().ToImmutableDictionary();

				public TestAnalyzerConfigOptions(ImmutableDictionary<string, string> options)
					=> backing = options;

				public override bool TryGetValue(string key, out string value)
				{
					var r = backing.TryGetValue(key, out var v);
					value = v ?? string.Empty;
					return r;
				}
			}
		}
	}
}
