#!/usr/bin/env bash

echo "Variables:"

# Updating manifest
sed -i '' "s/AC_ANDROID/$AC_ANDROID/g" $BUILD_REPOSITORY_LOCALPATH/Samples/Samples/Helpers/CommonConstants.cs

echo "Manifest updated!"