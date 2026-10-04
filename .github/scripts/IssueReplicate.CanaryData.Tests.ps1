#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'IssueReplicate.CanaryData.ps1')
}

Describe 'Bounded canary publication data' {
    BeforeEach {
        $source = Join-Path $TestDrive ([guid]::NewGuid().ToString())
        $directories = @{}
        foreach ($kind in @('Input', 'Sample', 'Candidate', 'Verified')) {
            $directories[$kind] = Join-Path $source $kind
            New-Item -ItemType Directory -Path $directories[$kind] | Out-Null
        }
        [IO.File]::WriteAllText((Join-Path $directories.Input 'manifest.json'), '{"issueNumber":12345}')
        [IO.File]::WriteAllText((Join-Path $directories.Sample 'sample-result.json'), '{"buildSucceeded":true}')
        [IO.File]::WriteAllText((Join-Path $directories.Candidate 'candidate.json'), '{"kind":"ui","files":[]}')
        $bytes = [byte[]]::new(24)
        [Array]::Copy([Text.Encoding]::ASCII.GetBytes('ftypisom'), 0, $bytes, 4, 8)
        $recording = @{
            status = 'available'; bytes = 24; diagnostic = ''
            sha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        }
        @{ testKind = 'ui'; recording = $recording } | ConvertTo-Json -Depth 5 |
        Set-Content (Join-Path $directories.Verified 'result.json')
        [IO.File]::WriteAllBytes((Join-Path $directories.Verified 'test.patch'),
            [Text.Encoding]::UTF8.GetBytes("candidate`r`nno final newline"))
        $saved = @{}
        for ($index = 0; $index -lt 8; $index++) {
            $name = "REPRO_VIDEO_$index"
            $saved[$name] = [Environment]::GetEnvironmentVariable($name)
            [Environment]::SetEnvironmentVariable($name, $null)
        }
        $env:REPRO_VIDEO_0 = [Convert]::ToBase64String($bytes)
        $lines = @(Export-IssueReplicateCanaryPublicationData -Directories $directories `
                -BuildId 1622596 -PipelineSha ('a' * 40) -Recording $recording)
        $output = Join-Path $TestDrive ([guid]::NewGuid().ToString())
    }

    AfterEach {
        foreach ($name in $saved.Keys) { [Environment]::SetEnvironmentVariable($name, $saved[$name]) }
    }

    It 'imports timestamped normal-log data and preserves exact patch and video bytes' {
        $text = "Unrelated log line`n" + (($lines | ForEach-Object { "2026-10-04T12:20:00.1234567Z $_`r" }) -join "`n")
        $import = Import-IssueReplicateCanaryPublicationData -Text $text -Directory $output `
            -BuildId 1622596 -PipelineSha ('a' * 40)
        [IO.File]::ReadAllText((Join-Path $output 'Verified/test.patch')) |
        Should -BeExactly "candidate`r`nno final newline"
        [Convert]::ToBase64String($import.RecordingBytes) | Should -BeExactly ([Convert]::ToBase64String($bytes))
        [Convert]::ToBase64String([IO.File]::ReadAllBytes((Join-Path $output 'recording.mp4'))) |
        Should -BeExactly ([Convert]::ToBase64String($bytes))
        $env:REPRO_VIDEO_0 | Should -BeExactly ([Convert]::ToBase64String($bytes))
    }

    It 'rejects a different build or infrastructure revision' -ForEach @(
        @{ Build = 1; Sha = ('a' * 40) }, @{ Build = 1622596; Sha = ('b' * 40) }
    ) {
        { Import-IssueReplicateCanaryPublicationData -Text ($lines -join "`n") -Directory $output `
                -BuildId $Build -PipelineSha $Sha } | Should -Throw '*selected build and source*'
        Test-Path $output | Should -BeFalse
    }

    It 'rejects missing, duplicate, corrupt, unknown and oversized packet fields' -ForEach @(
        @{ Fault = 'missing' }, @{ Fault = 'duplicate' }, @{ Fault = 'corrupt' },
        @{ Fault = 'unknown' }, @{ Fault = 'oversized' }, @{ Fault = 'video' }
    ) {
        $altered = @($lines)
        switch ($Fault) {
            missing { $altered = @($altered | Where-Object { -not $_.StartsWith('CANARY_PUBLICATION_Input=') }) }
            duplicate { $altered += $altered[0] }
            corrupt { $altered = @($altered | ForEach-Object { $_ -replace '^CANARY_PUBLICATION_Verified=.+$', 'CANARY_PUBLICATION_Verified=AAAA' }) }
            unknown { $altered += 'CANARY_PUBLICATION_Script=AAAA' }
            oversized { $altered += 'CANARY_PUBLICATION_Recording1=' + ('A' * 87388) }
            video { $altered = @($altered | ForEach-Object { $_ -replace '^CANARY_PUBLICATION_Recording0=.+$', ('CANARY_PUBLICATION_Recording0=' + [Convert]::ToBase64String([byte[]]::new(24))) }) }
        }
        { Import-IssueReplicateCanaryPublicationData -Text ($altered -join "`n") -Directory $output `
                -BuildId 1622596 -PipelineSha ('a' * 40) } | Should -Throw
    }

    It 'refuses existing output paths and logs above two MiB' {
        New-Item -ItemType Directory $output | Out-Null
        { Import-IssueReplicateCanaryPublicationData -Text ($lines -join "`n") -Directory $output `
                -BuildId 1622596 -PipelineSha ('a' * 40) } | Should -Throw '*overwrite*'
        { Import-IssueReplicateCanaryPublicationData -Text ('x' * (2MB + 1)) -Directory ($output + '-large') `
                -BuildId 1622596 -PipelineSha ('a' * 40) } | Should -Throw '*bound*'
    }
}
