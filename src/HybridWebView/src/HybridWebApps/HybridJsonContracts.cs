using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>Validates consumer-supplied source metadata without enabling runtime reflection.</summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class HybridJsonContracts
{
	/// <summary>Gets one explicitly rooted, strict, typed JSON contract.</summary>
	/// <typeparam name="T">The declared contract.</typeparam><param name="context">The declared source context.</param>
	/// <returns>Validated typed metadata.</returns>
	public static JsonTypeInfo<T> Get<T>(JsonSerializerContext context)
	{
		if (context.GetTypeInfo(typeof(T)) is not JsonTypeInfo<T> info)
			throw new InvalidOperationException("The declared source context does not root the exact action contract.");
		CommandRegistry.ValidateMetadata(info);
		if (info.Options.AllowDuplicateProperties)
			throw new InvalidOperationException("Explicit action contexts must reject duplicate JSON properties.");
		return info;
	}
}
