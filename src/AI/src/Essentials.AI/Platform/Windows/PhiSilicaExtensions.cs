using Microsoft.Maui.Essentials.AI;
using Microsoft.Windows.AI.Text;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Provides extension methods for converting Windows Copilot Runtime (Phi Silica) types
/// to Microsoft.Extensions.AI abstractions.
/// </summary>
public static class PhiSilicaExtensions
{
	/// <summary>
	/// Wraps an existing <see cref="LanguageModel"/> instance as an
	/// <see cref="IChatClient"/> for use with Microsoft.Extensions.AI abstractions.
	/// </summary>
	/// <param name="model">The <see cref="LanguageModel"/> instance to wrap.</param>
	/// <returns>
	/// An <see cref="IChatClient"/> that uses the provided <see cref="LanguageModel"/>.
	/// The returned client does not own the underlying model and will not dispose it
	/// when the client is disposed.
	/// </returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <see langword="null"/>.</exception>
	public static IChatClient AsIChatClient(this LanguageModel model)
	{
		return new PhiSilicaChatClient(model);
	}
}
