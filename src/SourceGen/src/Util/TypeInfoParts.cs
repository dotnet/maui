namespace Microsoft.Maui.SourceGen
{
	public class TypeInfoParts
	{
		public string[] NamespaceParts { get; set; } = new string[0];
		public string[] TypeNameParts { get; set; } = new string[0];

		public int TypeArgsCount { get; set; } = 0;

		public string GetNamespace(string separator = ".")
			=> NamespaceParts is null ? string.Empty : string.Join(separator, NamespaceParts);

		public string GetTypeName(string separator = ".")
			=> TypeNameParts is null ? string.Empty : string.Join(separator, TypeNameParts);

		public string GetFullyQualifiedName(string nsSeparator = ".", string typeSeparator = ".", bool includeGlobalPrefix = true)
			=> includeGlobalPrefix ? $"global::{GetNamespace(nsSeparator)}.{GetTypeName(typeSeparator)}"
			: $"{GetNamespace(nsSeparator)}.{GetTypeName(typeSeparator)}";
	}
}
