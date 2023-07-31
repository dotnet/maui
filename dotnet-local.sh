#!/bin/bash
ROOT="$(dirname "${BASH_SOURCE}")"
FULLROOT="$(cd "${ROOT}"; pwd)"

if [[ ! -x "${ROOT}/bin/dotnet/dotnet" ]] ; then
    echo "You need to run 'build.sh' first." 1>&2
    exit 1
fi

export DOTNET_ROOT="${FULLROOT}/bin/dotnet"
export PATH="${DOTNET_ROOT}:${PATH}"
exec "${ROOT}/bin/dotnet/dotnet" "$@"
