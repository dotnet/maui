#Requires -Version 7.0

$Script:PublicSafeConfusables = @{
    'ԁ'='d'; 'ɡ'='g'; 'г'='r'; 'ӏ'='l'
    'а'='a'; 'е'='e'; 'і'='i'; 'о'='o'; 'р'='p'; 'с'='c'; 'х'='x'; 'у'='y'
    'α'='a'; 'ε'='e'; 'ι'='i'; 'ο'='o'; 'ρ'='p'; 'χ'='x'; 'υ'='y'
}

function Get-PublicSafeCanonicalLiteralPattern {
    param(
        [Parameter(Mandatory)][string]$Literal,
        [string]$InterCharacterPattern = ''
    )

    $patterns = foreach ($character in $Literal.ToCharArray()) {
        $codePoints = @([int]$character)
        if ([char]::IsLetter($character)) {
            $codePoints += [int][char]::ToLowerInvariant($character)
            $codePoints += [int][char]::ToUpperInvariant($character)
        }

        $forms = [System.Collections.Generic.List[string]]::new()
        [void]$forms.Add([regex]::Escape([string]$character))
        foreach ($codePoint in @($codePoints | Sort-Object -Unique)) {
            [void]$forms.Add("&#0*$codePoint;")
            [void]$forms.Add(('&#x0*{0:x};' -f $codePoint))
            [void]$forms.Add(('%(?:25){{0,5}}{0:x2}' -f $codePoint))
            if ($codePoint -ge 0x21 -and $codePoint -le 0x7e) {
                [void]$forms.Add([regex]::Escape([string][char]($codePoint + 0xfee0)))
            }
        }
        if ($character -eq ' ') {
            [void]$forms.Add([regex]::Escape([string][char]0x3000))
        }
        $latinCharacter = [string][char]::ToLowerInvariant($character)
        foreach ($confusable in $Script:PublicSafeConfusables.Keys) {
            if ($Script:PublicSafeConfusables[$confusable] -eq $latinCharacter) {
                [void]$forms.Add([regex]::Escape([string]$confusable))
            }
        }
        '(?:' + (@($forms | Sort-Object -Unique) -join '|') + ')'
    }
    return ($patterns -join $InterCharacterPattern)
}

$Script:PublicSafePrivateToolPattern = @(
    Get-PublicSafeCanonicalLiteralPattern -Literal '.NET Release Tracker'
    Get-PublicSafeCanonicalLiteralPattern -Literal 'dotnet-release-tracker'
    Get-PublicSafeCanonicalLiteralPattern -Literal 'dotnet/release'
) -join '|'

$candidateNoise = '(?:[^A-Za-z0-9:/?._#&=%\s<>"''`|)]*)'
$Script:PublicSafeUrlCandidatePattern = @(
    'https?'
    'dev\.azure\.com'
    Get-PublicSafeCanonicalLiteralPattern -Literal 'dnceng' -InterCharacterPattern $candidateNoise
    Get-PublicSafeCanonicalLiteralPattern -Literal 'DevDiv' -InterCharacterPattern $candidateNoise
    '[\uFF01-\uFF5E]'
    '%(?:25)?[0-9a-f]{2}'
    '&#(?:x[0-9a-f]+|\d+);'
) -join '|'

function ConvertTo-PublicSafeMarkdown {
    param([AllowNull()][AllowEmptyString()][string]$Text)

    if ([string]::IsNullOrEmpty($Text)) { return $Text }

    $safe = $Text -replace '(?i)&#(?:x0*2f|0*47);', '/'
    $safe = $safe -replace '(?i)&sol;|&frasl;', '/'
    $safe = $safe -replace '(?i)&period;', '.'
    $safe = $safe -replace '(?i)&colon;', ':'
    $safe = $safe -replace '(?i)&bsol;|&setminus;', '\'
    $safe = $safe -replace '(?i)&Tab;|&NewLine;', ' '
    $safe = $safe -replace '(?i)&quest;|&#(?:x0*3f|0*63);', '?'
    $safe = $safe -replace '(?i)&num;|&hash;|&#(?:x0*23|0*35);', '#'
    $safe = [regex]::Replace($safe, '(?i)d[\s\-_.]*n[\s\-_.]*c[\s\-_.]*e[\s\-_.]*n[\s\-_.]*g(?=[./\\])', 'dnceng')
    $safe = [regex]::Replace($safe, '(?i)D[\s\-_.]*e[\s\-_.]*v[\s\-_.]*D[\s\-_.]*i[\s\-_.]*v(?=[/\\])', 'DevDiv')
    $safe = [regex]::Replace(
        $safe,
        '(?i)(?<host>(?:dev\.azure\.com/)?dnceng|dnceng\.visualstudio\.com)\s*(?=/)',
        '${host}')
    $safe = [regex]::Replace(
        $safe,
        '(?i)(?<prefix>(?:https?://|dev\.azure\.com/|dnceng(?:\.visualstudio\.com)?/)[^<>"''`|)]{0,512}?)i[\s\-_.]*n[\s\-_.]*t[\s\-_.]*e[\s\-_.]*r[\s\-_.]*n[\s\-_.]*a[\s\-_.]*l',
        '${prefix}internal')
    $safe = [regex]::Replace(
        $safe,
        '(?i)(?<prefix>(?:https?://)?(?:(?:dev\.azure\.com(?::\d+)?/dnceng)|(?:dnceng\.visualstudio\.com(?::\d+)?)|dnceng)/)\s*(?<collection>DefaultCollection\s*[/\\]\s*)?internal\s*(?=[/\\])',
        {
            param($match)
            $collection = if ($match.Groups['collection'].Success) { 'DefaultCollection/' } else { '' }
            "$($match.Groups['prefix'].Value)${collection}internal"
        })
    $safe = [regex]::Replace($safe, '(?i)(?<![A-Za-z0-9])(?:' + $Script:PublicSafeUrlCandidatePattern + ')[^\s<>"''`|]*', {
        param($match)
        $original = $match.Value
        $canonical = $original
        $decodeStabilized = $false
        for ($decodePass = 0; $decodePass -lt 12; $decodePass++) {
            try {
                $decoded = [System.Uri]::UnescapeDataString($canonical)
                if ($decoded -eq $canonical) {
                    $decodeStabilized = $true
                    break
                }
                $canonical = $decoded
            } catch { break }
        }
        if (-not $decodeStabilized -and $canonical -match '(?i)%(?:25)*[0-9a-f]{2}') {
            return '_encoded URL omitted_'
        }
        $canonical = [regex]::Replace($canonical, '(?i)&#(?:(?:x(?<hex>[0-9a-f]+))|(?<dec>\d+));', {
            param($entity)
            try {
                $value = if ($entity.Groups['hex'].Success) {
                    [Convert]::ToInt32($entity.Groups['hex'].Value, 16)
                } else {
                    [Convert]::ToInt32($entity.Groups['dec'].Value, 10)
                }
            } catch { return $entity.Value }
            if ($value -le 0xffff) { return [char]$value }
            return $entity.Value
        })
        $canonical = $canonical.Normalize([System.Text.NormalizationForm]::FormD)
        $canonical = $canonical -replace '\p{Mn}', ''
        $canonical = $canonical.Normalize([System.Text.NormalizationForm]::FormKC)
        $canonical = [regex]::Replace($canonical, '&[A-Za-z][A-Za-z0-9]+;', '')
        $canonical = $canonical -replace '[\p{Cf}\u00AD\u180E\uFE00-\uFE0F]', ''
        $canonical = $canonical -replace '[\u2044\u2215\uFF0F\\]', '/'
        $canonical = [regex]::Replace($canonical, '(?<!:)/{2,}', '/')
        $canonical = [regex]::Replace($canonical, '(?i)dnceng\s*\.\s*visualstudio\s*\.\s*com', 'dnceng.visualstudio.com')
        $canonical = [regex]::Replace($canonical, '(?i)i\s*n\s*t\s*e\s*r\s*n\s*a\s*l', 'internal')
        $canonical = $canonical.Replace('Т', 'T').Replace('Н', 'H').Replace('В', 'B').Replace('М', 'M').Replace('К', 'K')
        $canonical = $canonical.ToLowerInvariant()
        foreach ($key in $Script:PublicSafeConfusables.Keys) {
            $canonical = $canonical.Replace($key, $Script:PublicSafeConfusables[$key])
        }
        do {
            $beforeDots = $canonical
            $canonical = [regex]::Replace($canonical, '/\.(?=/)', '')
            $canonical = [regex]::Replace($canonical, '/(?!\.\.?/)[^/?#]+/\.\.(?=/)', '')
        } while ($canonical -ne $beforeDots)
        $canonical = [regex]::Replace(
            $canonical,
            '(?i)\b(dnceng|DefaultCollection)\s+(?=/|internal\b)',
            '$1')
        $detection = [regex]::Replace($canonical, '[^A-Za-z0-9@:/?._#&=%-]', '')
        $azdoAuthority = '(?:(?:dev\.azure\.com\.?(?::\d+)?/(?:dnceng|DevDiv))|(?:(?:dnceng|devdiv)\.visualstudio\.com\.?(?::\d+)?))'
        $hasAzdoUserInfo = $detection -match "(?i)https?://[^/@]+@$azdoAuthority(?:[/?:#]|$)"
        if ($hasAzdoUserInfo) {
            return '_credential-bearing URL omitted_'
        }
        $isInternal = $detection -match "(?i)^(?:(?:https?://)?(?:dev\.azure\.com\.?(?::\d+)?/dnceng|dnceng\.visualstudio\.com\.?(?::\d+)?)/(?:DefaultCollection/)?internal|(?:https?://)?(?:dev\.azure\.com\.?(?::\d+)?/DevDiv|devdiv\.visualstudio\.com\.?(?::\d+)?)|(?:dnceng(?:\.visualstudio\.com)?/(?:DefaultCollection/)?internal|DevDiv))(?:[/?:#]|$)"
        if ($isInternal) {
            return '_internal URL omitted_'
        }
        $isRecognizedAzdoHost = $detection -match "(?i)(?:(?:https?://)?$azdoAuthority|^(?:dnceng|DevDiv))(?=[/?:#]|$)"
        if ($isRecognizedAzdoHost -and $canonical.Contains('?')) {
            return [regex]::Replace(
                $original,
                '(?i)(?:\?|\uFF1F|&#(?:x0*3f|0*63);|%(?:25)*3f)[\s\S]*$',
                '?_query_omitted_')
        }
        if ($isRecognizedAzdoHost -and $canonical.Contains('#')) {
            return [regex]::Replace(
                $original,
                '(?i)(?:#|\uFF03|&#(?:x0*23|0*35);|%(?:25)*23)[\s\S]*$',
                '#_fragment_omitted_')
        }
        return $original
    })
    $safe = [regex]::Replace($safe, '(?i)dnceng\s*\.\s*visualstudio\s*\.\s*com', 'dnceng.visualstudio.com')
    $safe = $safe -replace '[\u2044\u2215\uFF0F]', '/'
    $safe = [regex]::Replace(
        $safe,
        '(?i)(?:https?://)?(?:dev\.azure\.com|dnceng\.visualstudio\.com|dnceng)[^\s<>"''`|)]*',
        { param($match) $match.Value -replace '\\', '/' })
    $safe = [regex]::Replace($safe, '(?i)https?://dev\.azure\.com/dnceng(?:/|%2f|%252f)(?:DefaultCollection(?:/|%2f|%252f))?internal[^\s<>"''`|)]*', '_internal URL omitted_')
    $safe = [regex]::Replace($safe, '(?i)https?://dnceng\.visualstudio\.com(?:/|%2f|%252f)(?:DefaultCollection(?:/|%2f|%252f))?internal[^\s<>"''`|)]*', '_internal URL omitted_')
    $safe = [regex]::Replace($safe, '(?i)https(?:%3a|%253a)(?:%2f|%252f){2}(?:(?:dev\.azure\.com(?:%2f|%252f)dnceng)|dnceng\.visualstudio\.com)(?:%2f|%252f)(?:DefaultCollection(?:%2f|%252f))?internal[^\s<>"''`|)]*', '_internal URL omitted_')
    $safe = [regex]::Replace($safe, '(?i)\b(?:dev\.azure\.com(?:/|%2f|%252f))?dnceng(?:/|%2f|%252f)(?:DefaultCollection(?:/|%2f|%252f))?internal[^\s<>"''`|)]*', 'internal source')
    $safe = [regex]::Replace($safe, '(?i)\bdnceng\.visualstudio\.com(?:/|%2f|%252f)(?:DefaultCollection(?:/|%2f|%252f))?internal[^\s<>"''`|)]*', 'internal source')
    $safe = [regex]::Replace($safe, '(?i)\bapi://[A-Za-z0-9._/-]+', '_internal identifier omitted_')
    $safe = [regex]::Replace(
        $safe,
        '(?i)(?:' + $Script:PublicSafePrivateToolPattern + ')',
        'official release source')
    $safe = [regex]::Replace(
        $safe,
        '(?i)(?<base>https?://(?:(?:dev\.azure\.com(?::\d+)?/(?:dnceng|DevDiv))|(?:(?:dnceng|devdiv)\.visualstudio\.com(?::\d+)?))(?:/[^\s<>"''`|)?#]*)?)\?[^\s<>"''`|)]*',
        '${base}?_query_omitted_')
    return $safe
}

function ConvertTo-PublicSafeValue {
    param([AllowNull()]$Value)

    if ($null -eq $Value) { return $null }
    if ($Value -is [string]) { return ConvertTo-PublicSafeMarkdown -Text $Value }
    if ($Value -is [System.Collections.IDictionary]) {
        $copy = [ordered]@{}
        foreach ($key in $Value.Keys) {
            $copy[$key] = ConvertTo-PublicSafeValue -Value $Value[$key]
        }
        return $copy
    }
    if ($Value -is [System.Collections.IEnumerable]) {
        $items = [System.Collections.Generic.List[object]]::new()
        foreach ($item in $Value) {
            [void]$items.Add((ConvertTo-PublicSafeValue -Value $item))
        }
        return ,$items.ToArray()
    }
    if ($Value.PSObject.BaseObject.GetType().FullName -eq 'System.Management.Automation.PSCustomObject') {
        $copy = [ordered]@{}
        foreach ($property in $Value.PSObject.Properties) {
            $copy[$property.Name] = ConvertTo-PublicSafeValue -Value $property.Value
        }
        return [PSCustomObject]$copy
    }
    return $Value
}
