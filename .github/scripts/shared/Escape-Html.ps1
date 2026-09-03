function Escape-Html {
    param([string]$Value)

    if ($null -eq $Value) {
        return ""
    }

    # Escape ampersands first so entities introduced by later replacements are not re-escaped.
    return $Value -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;' -replace '"', '&quot;' -replace "'", '&#39;'
}
