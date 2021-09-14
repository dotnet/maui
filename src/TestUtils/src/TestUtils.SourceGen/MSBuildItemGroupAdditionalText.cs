using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

#nullable enable

namespace Microsoft.Maui.TestUtils.SourceGen
{
	public partial class AssemblyGenerator
	{
		class MSBuildItemGroupAdditionalText : AdditionalText
		{
			public MSBuildItemGroupAdditionalText(ITaskItem2 taskItem)
			{
				Path = taskItem.ItemSpec;

				var metadata = new Dictionary<string, string>();

				foreach (string name in taskItem.MetadataNames)
					metadata.Add($"build_metadata.AdditionalFiles.{name}", taskItem.GetMetadata(name));

				Metadata = metadata.ToImmutableDictionary();
			}

			public MSBuildItemGroupAdditionalText(string path, ImmutableDictionary<string, string> metadata)
			{
				Path = path;
				Metadata = metadata;
			}

			public override string Path { get; }

			public ImmutableDictionary<string, string> Metadata { get; }

			public override SourceText GetText(CancellationToken cancellationToken = default)
				=> SourceText.From(JsonConvert.SerializeObject(Metadata));
		}
	}
}
