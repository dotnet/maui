#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Build-TemplateApp.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
        'Assert-EnvironmentValue',
        'Get-NewestBuildOutput',
        'Invoke-DotNetPublish',
        'Test-IsNet11OrLater',
        'Add-NativeAotArguments',
        'Get-BinlogConfiguration',
        'Invoke-MacNotarization',
        'New-MacCatalystDeveloperIdSideload',
        'New-IosAdHocSideload'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)

        if (-not $function) {
            throw "Function '$functionName' not found"
        }

        Invoke-Expression $function.Extent.Text
    }

    $testRoot = Join-Path $PSScriptRoot '.test-results'
    New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
    $projectPath = Join-Path $testRoot 'TestApp.csproj'
    Set-Content -Path $projectPath -Value '<Project Sdk="Microsoft.NET.Sdk" />'
    $projectFile = Get-Item $projectPath
}

AfterAll {
    Remove-Item -Path $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'optional Apple sideload signing' {
    BeforeEach {
        $env:IOS_ADHOC_CODESIGN_PROVISION = $null
        $env:IOS_CODESIGN_KEY = $null
        $env:APPLE_DEVELOPERID_CODESIGN_KEY = $null
        $env:APPLE_DEVELOPERID_CODESIGN_PROVISION = $null
    }

    It 'preserves the no-secret iOS fallback' {
        $result = New-IosAdHocSideload `
            -ProjectFile $projectFile `
            -TargetFramework 'net11.0-ios' `
            -Configuration 'Release' `
            -RuntimeIdentifier 'ios-arm64' `
            -OutputPath $testRoot `
            -AppDisplayVersion '11.0' `
            -AppBuildNumber '1'

        $result | Should -BeNullOrEmpty
    }

    It 'fails when a configured iOS ad-hoc publish fails' {
        $env:IOS_ADHOC_CODESIGN_PROVISION = 'AdHoc Profile'
        $env:IOS_CODESIGN_KEY = 'Apple Distribution'
        Mock Invoke-DotNetPublish { throw 'simulated ad-hoc publish failure' }

        {
            New-IosAdHocSideload `
                -ProjectFile $projectFile `
                -TargetFramework 'net11.0-ios' `
                -Configuration 'Release' `
                -RuntimeIdentifier 'ios-arm64' `
                -OutputPath $testRoot `
                -AppDisplayVersion '11.0' `
                -AppBuildNumber '1'
        } | Should -Throw '*simulated ad-hoc publish failure*'
    }

    It 'fails when Developer ID signing is only partially configured' {
        $env:APPLE_DEVELOPERID_CODESIGN_KEY = 'Developer ID Application'

        {
            New-MacCatalystDeveloperIdSideload `
                -ProjectFile $projectFile `
                -TargetFramework 'net11.0-maccatalyst' `
                -Configuration 'Release' `
                -OutputPath $testRoot `
                -AppDisplayVersion '11.0' `
                -AppBuildNumber '1' `
                -RuntimeIdentifier 'maccatalyst-arm64'
        } | Should -Throw '*partially configured*'
    }

    It 'fails when a configured Developer ID publish fails' {
        $env:APPLE_DEVELOPERID_CODESIGN_KEY = 'Developer ID Application'
        $env:APPLE_DEVELOPERID_CODESIGN_PROVISION = 'Developer ID Profile'
        Mock Invoke-DotNetPublish { throw 'simulated Developer ID publish failure' }

        {
            New-MacCatalystDeveloperIdSideload `
                -ProjectFile $projectFile `
                -TargetFramework 'net11.0-maccatalyst' `
                -Configuration 'Release' `
                -OutputPath $testRoot `
                -AppDisplayVersion '11.0' `
                -AppBuildNumber '1' `
                -RuntimeIdentifier 'maccatalyst-arm64'
        } | Should -Throw '*simulated Developer ID publish failure*'
    }
}

Describe 'Android artifact safety' {
    It 'requires an APK before assigning the sideload output' {
        $scriptText = Get-Content -Path $scriptPath -Raw

        $scriptText | Should -Match 'Android APK publish completed but no APK artifact was found'
        $scriptText.IndexOf('Android APK publish completed but no APK artifact was found') |
            Should -BeLessThan $scriptText.IndexOf('$sideloadPackage = $apkPackage')
    }
}

Describe 'iOS dry-run artifact safety' {
    It 'keeps a device IPA when simulator artifact discovery fails' {
        $scriptText = Get-Content -Path $scriptPath -Raw
        $deviceIpaBlock = [regex]::Match(
            $scriptText,
            '(?s)if \(\$deviceIpa\) \{(?<Body>.*?)\r?\n\s*\}'
        )

        $deviceIpaBlock.Success | Should -BeTrue
        $deviceIpaBlock.Groups['Body'].Value | Should -Match '\$package = \$deviceIpa'
        $deviceIpaBlock.Groups['Body'].Value | Should -Match '\$sideloadPackage = \$deviceIpa'
    }
}

Describe 'publish binlogs' {
    It 'uses a distinct store binlog for an Android publish' {
        $configuration = Get-BinlogConfiguration `
            -OutputPath $testRoot `
            -Platform 'android' `
            -Publish `
            -CreateBinlog

        $configuration.BuildPath | Should -Be (Join-Path $testRoot 'build.binlog')
        $configuration.StorePath | Should -Be (Join-Path $testRoot 'store-build.binlog')
        $configuration.BuildArguments | Should -Contain "/bl:$($configuration.BuildPath)"
        $configuration.StoreArguments | Should -Contain "/bl:$($configuration.StorePath)"
    }

    It 'wires the store binlog into the AAB publish and artifact upload' {
        $scriptText = Get-Content -Path $scriptPath -Raw
        $githubDirectory = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
        $workflowPath = Join-Path $githubDirectory 'workflows/template-app-distribution.yml'
        $workflowText = Get-Content -Path $workflowPath -Raw

        $scriptText | Should -Match '\$aabArgs = .*\+ \$storeBinlogArguments'
        $scriptText | Should -Match '"store_binlog_path=\$storeBinlogPath"'
        $workflowText | Should -Match '\$\{\{ steps\.build\.outputs\.store_binlog_path \}\}'
    }
}

Describe 'template metadata replacement' {
    BeforeAll {
        $newTemplateScriptPath = Join-Path $PSScriptRoot 'New-TemplateApp.ps1'
        $newTemplateTokens = $null
        $newTemplateParseErrors = $null
        $newTemplateAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $newTemplateScriptPath,
            [ref]$newTemplateTokens,
            [ref]$newTemplateParseErrors
        )
        if ($newTemplateParseErrors -and $newTemplateParseErrors.Count -gt 0) {
            throw ($newTemplateParseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
        }

        foreach ($functionName in @('ConvertTo-XmlEscaped', 'Set-ProjectElementValue')) {
            $function = $newTemplateAst.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)

            if (-not $function) {
                throw "Function '$functionName' not found"
            }

            Invoke-Expression $function.Extent.Text
        }
    }

    It 'treats dollar signs as literal replacement text' {
        $content = '<ApplicationTitle>Old</ApplicationTitle>'

        Set-ProjectElementValue $content 'ApplicationTitle' 'Cash $$ App $&' |
            Should -Be '<ApplicationTitle>Cash $$ App $&amp;</ApplicationTitle>'
    }
}

Describe 'TestFlight timeout handling' {
    It 'fails instead of reporting incomplete distribution as successful' {
        $fastfilePath = Join-Path $PSScriptRoot 'fastlane/Fastfile'
        $fastfileText = Get-Content -Path $fastfilePath -Raw
        $timeoutBranch = [regex]::Match(
            $fastfileText,
            '(?s)elsif testflight_processing_timeout\?\(error\)(?<Body>.*?)\r?\n\s*else'
        )

        $timeoutBranch.Success | Should -BeTrue
        $timeoutBranch.Groups['Body'].Value | Should -Match '\braise\b'
        $timeoutBranch.Groups['Body'].Value | Should -Not -Match 'Treating this as a successful upload'
    }
}
