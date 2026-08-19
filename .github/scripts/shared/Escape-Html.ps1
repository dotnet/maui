function Escape-Html {
    param([string]$Value)

    if ($null -eq $Value) {
        return ""
    }

    return $Value -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;'
}
