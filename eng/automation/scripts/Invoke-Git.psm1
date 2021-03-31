<#
Copyright (c) Microsoft Corporation.  All rights reserved.
 #>
<#
.SYNOPSIS
Module for invoking git in a safe way that allows for stderr to be written to stdout.
#>
function Invoke-Git {
    <#
    .Synopsis
    Wrapper function that deals with Powershell's peculiar error output when Git uses the error stream.
    .Example
    Invoke-Git ThrowError
    $LASTEXITCODE
    #>
    [CmdletBinding()]
    param(
        [parameter(ValueFromRemainingArguments=$true)]
        [string[]]$Arguments
    )
    & {
        [CmdletBinding()]
        param(
            [parameter(ValueFromRemainingArguments=$true)]
            [string[]]$InnerArgs
        )
        git.exe $InnerArgs
    } -ErrorAction SilentlyContinue -ErrorVariable fail @Arguments
    if ($fail) {
        $fail.Exception
    }
}
#Exports
Export-ModuleMember -Function Invoke-Git