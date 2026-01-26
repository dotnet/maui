using System.Runtime.Versioning;
using Microsoft.Extensions.AI;
using Google.MLKit.GenAI.Prompt;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Extension methods for <see cref="IGenerativeModel"/>
/// </summary>
[SupportedOSPlatform("android26.0")]
public static class GeminiNanoExtensions
{
	/// <summary>
	/// Converts the <see cref="IGenerativeModel"/> to an <see cref="IChatClient"/>.
	/// </summary>
	/// <param name="model">The <see cref="IGenerativeModel"/> instance.</param>
	/// <returns>An <see cref="IChatClient"/> wrapping the model.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <see langword="null"/>.</exception>
	public static IChatClient AsIChatClient(this IGenerativeModel model) =>
		new GeminiNanoChatClient(model);
}
