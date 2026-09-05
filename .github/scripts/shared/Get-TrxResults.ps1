function Get-TrxResults {
    param([string]$TrxPath)

    if (-not $TrxPath -or -not (Test-Path $TrxPath)) {
        return $null
    }

    try {
        [xml]$trx = Get-Content -Path $TrxPath -Raw -Encoding UTF8
    } catch {
        Write-Host "    ⚠️ Failed to parse TRX $TrxPath : $_" -ForegroundColor Yellow
        return $null
    }

    # The TRX is in the VSTest namespace. Set up an XmlNamespaceManager so we
    # can address nodes regardless of prefix.
    $ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
    $ns.AddNamespace('t', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010')

    # Counters live on <ResultSummary><Counters .../></ResultSummary>
    $countersNode = $trx.SelectSingleNode('//t:ResultSummary/t:Counters', $ns)
    $total = 0; $passed = 0; $failed = 0; $skipped = 0
    if ($countersNode) {
        $total   = [int]($countersNode.GetAttribute('total'))
        $passed  = [int]($countersNode.GetAttribute('passed'))
        $failed  = [int]($countersNode.GetAttribute('failed'))
        # Skipped is "executed - passed - failed" if not separately tracked.
        $executed = [int]($countersNode.GetAttribute('executed'))
        $skipped  = [Math]::Max(0, $total - $executed)
    }

    $entries = New-Object System.Collections.ArrayList
    $resultNodes = $trx.SelectNodes('//t:UnitTestResult', $ns)
    foreach ($r in $resultNodes) {
        $rawName = $r.GetAttribute('testName')
        # Use the raw test name as-is from TRX.
        $name = $rawName

        $outcomeAttr = $r.GetAttribute('outcome')
        $status = switch ($outcomeAttr) {
            'Passed'           { 'Passed' }
            'Failed'           { 'Failed' }
            'NotExecuted'      { 'Skipped' }
            'Inconclusive'     { 'Skipped' }
            # Map all other outcomes (Aborted, Timeout, Error, Disconnected,
            # Warning, Pending) to Failed so they appear in failure disclosures
            # and match the TRX Counters/failed count.
            default            { 'Failed' }
        }
        $duration = $r.GetAttribute('duration')

        $err = ''; $stack = ''
        $errInfo = $r.SelectSingleNode('t:Output/t:ErrorInfo', $ns)
        if ($errInfo) {
            $msgNode   = $errInfo.SelectSingleNode('t:Message', $ns)
            $stackNode = $errInfo.SelectSingleNode('t:StackTrace', $ns)
            if ($msgNode)   { $err   = $msgNode.InnerText.Trim() }
            if ($stackNode) { $stack = $stackNode.InnerText.Trim() }
        }

        [void]$entries.Add([ordered]@{
            status   = $status
            name     = $name
            duration = $duration
            error    = $err
            stack    = $stack
        })
    }

    return @{
        Total   = $total
        Passed  = $passed
        Failed  = $failed
        Skipped = $skipped
        Results = @($entries.ToArray())
        TrxPath = $TrxPath
    }
}
