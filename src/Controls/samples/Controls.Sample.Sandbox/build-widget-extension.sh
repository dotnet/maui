#!/bin/bash
# Builds the MauiWidgetExtension for iOS Simulator and packages it as an .appex bundle.
# Uses proper 2-step compile+link with _NSExtensionMain entry point.
# Usage: ./build-widget-extension.sh [iossimulator-arm64|iossimulator-x64]
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
EXTENSION_DIR="$SCRIPT_DIR/Platforms/iOS/Extensions/MauiWidgetExtension"
ARCH="${1:-iossimulator-arm64}"

case "$ARCH" in
    iossimulator-arm64) TARGET="arm64-apple-ios17.0-simulator"; CLANG_ARCH="arm64" ;;
    iossimulator-x64)   TARGET="x86_64-apple-ios17.0-simulator"; CLANG_ARCH="x86_64" ;;
    ios-arm64)          TARGET="arm64-apple-ios17.0"; CLANG_ARCH="arm64" ;;
    *)                  echo "Unknown arch: $ARCH"; exit 1 ;;
esac

SDK_NAME="iphonesimulator"
[[ "$ARCH" == "ios-arm64" ]] && SDK_NAME="iphoneos"

SDK_PATH="$(xcrun --sdk "$SDK_NAME" --show-sdk-path)"
OUTPUT_DIR="$SCRIPT_DIR/bin/widget/$ARCH"
APPEX_DIR="$OUTPUT_DIR/MauiWidgetExtension.appex"
OBJ_DIR="$OUTPUT_DIR/obj"

echo "Building MauiWidgetExtension for $ARCH..."

rm -rf "$APPEX_DIR" "$OBJ_DIR"
mkdir -p "$APPEX_DIR" "$OBJ_DIR"

# Step 1: Compile Swift source to object file
xcrun swiftc \
    -sdk "$SDK_PATH" \
    -target "$TARGET" \
    -O \
    -module-name MauiWidgetExtension \
    -parse-as-library \
    -application-extension \
    -c \
    -o "$OBJ_DIR/MauiWidgetExtension.o" \
    "$EXTENSION_DIR/MauiWidgetExtension.swift"

# Step 2: Link with _NSExtensionMain entry point (like Xcode does)
xcrun clang \
    -target "$TARGET" \
    -isysroot "$SDK_PATH" \
    -fapplication-extension \
    -e _NSExtensionMain \
    -dead_strip \
    -fobjc-link-runtime \
    -L"$(xcrun --sdk "$SDK_NAME" --show-sdk-platform-path)/Developer/usr/lib" \
    -L/usr/lib/swift \
    -Xlinker -rpath -Xlinker /usr/lib/swift \
    -Xlinker -rpath -Xlinker @executable_path/Frameworks \
    -Xlinker -rpath -Xlinker @executable_path/../../Frameworks \
    "$OBJ_DIR/MauiWidgetExtension.o" \
    -o "$APPEX_DIR/MauiWidgetExtension"

# Copy Info.plist
cp "$EXTENSION_DIR/Info.plist" "$APPEX_DIR/Info.plist"

# Sign with entitlements if available
ENTITLEMENTS="$EXTENSION_DIR/MauiWidgetExtension.entitlements"
if [[ -f "$ENTITLEMENTS" ]]; then
    codesign --force --sign - --entitlements "$ENTITLEMENTS" "$APPEX_DIR"
else
    codesign --force --sign - "$APPEX_DIR"
fi

# Cleanup
rm -rf "$OBJ_DIR"

echo "✅ Built: $APPEX_DIR"
