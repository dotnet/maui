#Requires -Modules Pester

BeforeAll {
    $script:RepoRoot = (Resolve-Path "$PSScriptRoot/../..").Path
}

Describe 'Direct Cake native execution' {
    It 'has no executable XHarness dependency in any platform script' {
        foreach ($platform in @('ios', 'android', 'catalyst')) {
            $source = Get-Content -Raw (Join-Path $PSScriptRoot "$platform.cake")
            $source | Should -Not -Match '(?i)run\s+xharness'
            $source | Should -Match 'RunNativeTestWithLocalDotNet\(project, config, tfm, rid,'
        }
    }

    It 'uses the common native builder and propagates failing process exits' {
        $source = Get-Content -Raw (Join-Path $RepoRoot 'eng/cake/dotnet.cake')
        $source | Should -Match 'MtpTestArguments.Native'
        $source | Should -Match 'Native dotnet test failed with exit code'
        $source | Should -Not -Match 'ignore-exit-code'
    }

    It 'retains Appium preparation and NUnit retry execution' {
        foreach ($platform in @('ios', 'android', 'catalyst')) {
            $source = Get-Content -Raw (Join-Path $PSScriptRoot "$platform.cake")
            $source | Should -Match 'RunUITestSetupWithRetry'
            $source | Should -Match 'RunUITestsWithRetry'
        }
        (Get-Content -Raw (Join-Path $PSScriptRoot 'ios.cake')) | Should -Match 'InstallIpa\(testApp, "", device,'
        (Get-Content -Raw (Join-Path $PSScriptRoot 'android.cake')) | Should -Match 'InstallApk\(testApp, appPackageName,'
    }

    It 'installs UI apps with native commands and never suppresses installation failures' {
        $ios = Get-Content -Raw (Join-Path $PSScriptRoot 'ios.cake')
        $android = Get-Content -Raw (Join-Path $PSScriptRoot 'android.cake')
        $ios | Should -Match '"devicectl", "device", "install", "app"'
        $ios | Should -Match '"simctl", "install"'
        $ios | Should -Match 'xcrun .*failed with exit code'
        $android | Should -Match 'Append\("install"\).Append\("-r"\)'
        $android | Should -Match 'APK installation failed with exit code'
    }
}
