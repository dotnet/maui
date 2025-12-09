#!/usr/bin/env bash

# This command sends helix test job from local machine

export BUILD_SOURCEBRANCH="main"
export BUILD_REPOSITORY_NAME="maui"
export SYSTEM_TEAMPROJECT="public"
export BUILD_REASON="test"

if [[ ! -f ".dotnet/dotnet" ]]; then
    dotnet build eng/helix.proj /restore /t:Test "$@"
else
    .dotnet/dotnet build eng/helix.proj /restore /t:Test "$@"
fi