using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Text;
using System.Runtime.Versioning;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Factory for creating and ensuring readiness of Windows Copilot Runtime (Phi Silica) <see cref="LanguageModel"/> instances.
/// </summary>
[SupportedOSPlatform("windows10.0.26100.0")]
internal static class PhiSilicaModelFactory
{
	/// <summary>
	/// Creates a <see cref="LanguageModel"/> instance, ensuring the Windows Copilot Runtime (Phi Silica) is ready.
	/// </summary>
	/// <returns>A ready-to-use <see cref="LanguageModel"/> instance.</returns>
	/// <exception cref="NotSupportedException">
	/// Thrown when Phi Silica is not supported on the current system, disabled by the user, or not ready.
	/// </exception>
	public static async Task<LanguageModel> CreateModelAsync()
	{
		var readyState = LanguageModel.GetReadyState();

		if (readyState is AIFeatureReadyState.DisabledByUser or AIFeatureReadyState.NotSupportedOnCurrentSystem)
		{
			var message = readyState switch
			{
				AIFeatureReadyState.NotSupportedOnCurrentSystem => "Not supported on current system",
				AIFeatureReadyState.DisabledByUser => "Disabled by user",
				_ => "Unknown reason"
			};
			throw new NotSupportedException($"Phi Silica (Windows Copilot Runtime) is not available: {message}");
		}

		if (readyState is AIFeatureReadyState.NotReady)
		{
			var operation = await LanguageModel.EnsureReadyAsync();

			if (operation.Status is not AIFeatureReadyResultState.Success)
				throw new NotSupportedException("Phi Silica (Windows Copilot Runtime) is not available");
		}

		if (LanguageModel.GetReadyState() is not AIFeatureReadyState.Ready)
		{
			throw new NotSupportedException("Phi Silica (Windows Copilot Runtime) is not available");
		}

		var languageModel = await LanguageModel.CreateAsync();

		return languageModel;
	}
}
