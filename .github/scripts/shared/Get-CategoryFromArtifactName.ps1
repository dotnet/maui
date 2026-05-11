function Get-CategoryFromArtifactName {
    param([string]$ArtifactName)

    # Pattern: drop-<stagename>-<jobname>-<attempt>
    $stagePrefixes = @(
        'android_ui_tests', 'android_ui_tests_coreclr', 'android_ui_tests_material3',
        'ios_ui_tests_mono', 'ios_ui_tests_mono_cv1', 'ios_ui_tests_mono_carv1',
        'ios_ui_tests_nativeaot',
        'winui_ui_tests', 'mac_ui_tests'
    )

    $name = $ArtifactName -replace '^drop-', '' -replace '-\d+$', ''

    foreach ($sp in $stagePrefixes | Sort-Object Length -Descending) {
        if ($name -match "^${sp}-(.+)$") {
            return $Matches[1].Trim()
        }
    }
    return $name
}
