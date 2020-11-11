#!/usr/bin/env bash

echo "Variables:"

# Updating manifest
sed -i '' "s/AC_IOS/$AC_IOS/g" $BUILD_REPOSITORY_LOCALPATH/Samples/Samples/Helpers/CommonConstants

sed -i '' "s/APP-SECRET/$APP_SECRET/g" $BUILD_REPOSITORY_LOCALPATH/Samples/Samples.iOS/Info.plist

echo "Manifest updated!"
