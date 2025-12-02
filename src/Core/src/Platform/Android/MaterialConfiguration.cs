
namespace Microsoft.Maui.Platform;

/// <summary>
/// Default implementation of Material Design configuration.
/// </summary>
public class MaterialConfiguration : IMaterialConfiguration
{
    /// <summary>
    /// Gets or sets whether to use Material Design 3.
    /// Default is false (Material Design 2).
    /// </summary>
    public bool UseMaterial3 { get; set; }

    /// <summary>
    /// Gets the theme resource ID based on Material version.
    /// </summary>
    /// <returns>Resource ID for the appropriate Material theme.</returns>
    public int GetThemeResourceId()
    {
        return UseMaterial3
            ? Resource.Style.Maui_Material3_Theme_Base
            : Resource.Style.Maui_MainTheme_Base;
    }

    /// <summary>
    /// Gets whether Material 3 theming is enabled.
    /// </summary>
    /// <returns>True if Material 3 is enabled, false for Material 2.</returns>
    public bool IsMaterial3Enabled()
    {
        return UseMaterial3;
    }
}