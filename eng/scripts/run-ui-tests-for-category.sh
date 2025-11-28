#!/bin/bash

# Wrapper script for run-ui-tests-for-category.ps1
# Allows running UI tests for specific categories from bash

# Check if pwsh is installed
if ! command -v pwsh &> /dev/null; then
    echo "❌ PowerShell (pwsh) is not installed."
    echo "Please install PowerShell Core: https://github.com/PowerShell/PowerShell"
    exit 1
fi

# Get the directory of this script
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Forward all arguments to the PowerShell script
pwsh "$SCRIPT_DIR/run-ui-tests-for-category.ps1" "$@"
