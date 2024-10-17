#!/bin/bash
ROOT="$(dirname "${BASH_SOURCE}")"
FULLROOT="$(cd "${ROOT}"; pwd)"

if [[ ! -x "${ROOT}/.dotnet/dotnet" ]] ; then
    echo 'You must build MAUI first. Please see `.github/DEVELOPMENT.md` for details.' 1>&2
    exit 1
fi

export DOTNET_ROOT="${FULLROOT}/.dotnet"
export PATH="${DOTNET_ROOT}:${PATH}"
exec "${ROOT}/.dotnet/dotnet" "$@"
