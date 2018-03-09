#!/usr/bin/env bash

##########################################################################
# This is the Cake bootstrapper script for Linux and OS X.
# This file was downloaded from https://github.com/cake-build/resources
# Feel free to change this file to fit your needs.
##########################################################################

# Define directories.
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
ADDINS_DIR=$TOOLS_DIR/Addins
MODULES_DIR=$TOOLS_DIR/Modules
NUGET_EXE=$TOOLS_DIR/nuget.exe
CAKE_EXE=$TOOLS_DIR/Cake/Cake.exe
PACKAGES_CONFIG=$TOOLS_DIR/packages.config
PACKAGES_CONFIG_MD5=$TOOLS_DIR/packages.config.md5sum
ADDINS_PACKAGES_CONFIG=$ADDINS_DIR/packages.config
MODULES_PACKAGES_CONFIG=$MODULES_DIR/packages.config

# Define md5sum or md5 depending on Linux/OSX
MD5_EXE=
if [[ "$(uname -s)" == "Darwin" ]]; then
    MD5_EXE="md5 -r"
else
    MD5_EXE="md5sum"
fi

# Define default arguments.
SCRIPT="build.cake"
CAKE_ARGUMENTS=()

# Parse arguments.
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        --) shift; CAKE_ARGUMENTS+=("$@"); break ;;
        *) CAKE_ARGUMENTS+=("$1") ;;
    esac
    shift
done

# Make sure the tools folder exist.
if [ ! -d "$TOOLS_DIR" ]; then
  mkdir "$TOOLS_DIR"
fi

# Make sure that packages.config exist.
if [ ! -f "$TOOLS_DIR/packages.config" ]; then
    echo "Downloading packages.config..."
    curl -Lsfo "$TOOLS_DIR/packages.config" https://cakebuild.net/download/bootstrapper/packages
    if [ $? -ne 0 ]; then
        echo "An error occurred while downloading packages.config."
        exit 1
    fi
fi

# Download NuGet if it does not exist.
if [ ! -f "$NUGET_EXE" ]; then
    echo "Downloading NuGet..."
    curl -Lsfo "$NUGET_EXE" https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
    if [ $? -ne 0 ]; then
        echo "An error occurred while downloading nuget.exe."
        exit 1
    fi
fi

# Restore tools from NuGet.
pushd "$TOOLS_DIR" >/dev/null
if [ ! -f "$PACKAGES_CONFIG_MD5" ] || [ "$( cat "$PACKAGES_CONFIG_MD5" | sed 's/\r$//' )" != "$( $MD5_EXE "$PACKAGES_CONFIG" | awk '{ print $1 }' )" ]; then
    find . -type d ! -name . ! -name 'Cake.Bakery' | xargs rm -rf
fi

mono "$NUGET_EXE" install -ExcludeVersion
if [ $? -ne 0 ]; then
    echo "Could not restore NuGet tools."
    exit 1
fi

$MD5_EXE "$PACKAGES_CONFIG" | awk '{ print $1 }' >| "$PACKAGES_CONFIG_MD5"

popd >/dev/null

# Restore addins from NuGet.
if [ -f "$ADDINS_PACKAGES_CONFIG" ]; then
    pushd "$ADDINS_DIR" >/dev/null

    mono "$NUGET_EXE" install -ExcludeVersion
    if [ $? -ne 0 ]; then
        echo "Could not restore NuGet addins."
        exit 1
    fi

    popd >/dev/null
fi

# Restore modules from NuGet.
if [ -f "$MODULES_PACKAGES_CONFIG" ]; then
    pushd "$MODULES_DIR" >/dev/null

    mono "$NUGET_EXE" install -ExcludeVersion
    if [ $? -ne 0 ]; then
        echo "Could not restore NuGet modules."
        exit 1
    fi

    popd >/dev/null
fi

# Make sure that Cake has been installed.
if [ ! -f "$CAKE_EXE" ]; then
    echo "Could not find Cake.exe at '$CAKE_EXE'."
    exit 1
fi

# Start Cake
exec mono "$CAKE_EXE" $SCRIPT "${CAKE_ARGUMENTS[@]}"
