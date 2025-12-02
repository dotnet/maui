namespace Microsoft.Maui;

/// <summary>
/// Configuration interface for Material Design theming options.
/// </summary>
public interface IMaterialConfiguration
{
    /// <summary>
    /// Gets or sets whether to use Material Design 3.
    /// Default is false (Material Design 2).
    /// </summary>
    bool UseMaterial3 { get; set; }

    /// <summary>
    /// Gets the theme resource ID based on Material version.
    /// </summary>
    /// <returns>Resource ID for the appropriate Material theme.</returns>
    int GetThemeResourceId();

    /// <summary>
    /// Gets whether Material 3 theming is enabled.
    /// </summary>
    /// <returns>True if Material 3 is enabled, false for Material 2.</returns>
    bool IsMaterial3Enabled();
}
