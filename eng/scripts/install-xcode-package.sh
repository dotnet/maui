#!/bin/bash

# Installs the Xcode Universal Package that dotnet/macios pins and publishes to the
# 'macios-tools-internal' feed (dotnet/macios tools/devops/automation/templates/common/provision-xcode.yml).
# The package holds xcode-metadata.json and the Apple-signed Xcode .xip it describes.
#
# Usage: install-xcode-package.sh <package-directory>
# Sets the XCODE pipeline variable to the installed major.minor version.

set -euo pipefail

PACKAGE_DIRECTORY=${1:?Usage: $0 <package-directory>}
PACKAGE_DIRECTORY=$(cd -- "$PACKAGE_DIRECTORY" && pwd -P)
MANIFEST="$PACKAGE_DIRECTORY/xcode-metadata.json"

if [[ "$(sysctl -n hw.optional.arm64 2>/dev/null || echo 0)" != "1" ]]; then
	echo "The Xcode Universal Package requires an Apple silicon agent." >&2
	exit 1
fi

PACKAGE_VERSION=$(jq -er '.package.version' "$MANIFEST")
BUILD_VERSION=$(jq -er '.xcode.buildVersion' "$MANIFEST")
ARCHIVE="$PACKAGE_DIRECTORY/$(jq -er '.artifacts[0].fileName' "$MANIFEST")"
EXPECTED_SHA256=$(jq -er '.artifacts[0].sha256' "$MANIFEST")
XCODE_APP="/Applications/Xcode_${PACKAGE_VERSION}.app"
XCODEBUILD="$XCODE_APP/Contents/Developer/usr/bin/xcodebuild"

installed_build ()
{
	/usr/libexec/PlistBuddy -c 'Print :ProductBuildVersion' "$XCODE_APP/Contents/version.plist" 2>/dev/null || true
}

if [[ "$(installed_build)" == "$BUILD_VERSION" ]]; then
	echo "Xcode $PACKAGE_VERSION ($BUILD_VERSION) is already installed at $XCODE_APP."
else
	actual_sha256=$(shasum -a 256 "$ARCHIVE" | cut -d ' ' -f 1)
	if [[ "$actual_sha256" != "$EXPECTED_SHA256" ]]; then
		echo "SHA-256 mismatch for $ARCHIVE: expected $EXPECTED_SHA256, got $actual_sha256." >&2
		exit 1
	fi
	if ! pkgutil --check-signature "$ARCHIVE" | grep -q 'signed Apple Software'; then
		echo "$ARCHIVE is not signed Apple Software." >&2
		exit 1
	fi

	staging=$(mktemp -d "${AGENT_TEMPDIRECTORY:-${TMPDIR:-/tmp}}/xcode-expand.XXXXXX")
	echo "Expanding $ARCHIVE into $staging"
	(cd "$staging" && /usr/bin/xip --expand "$ARCHIVE")
	expanded_app=$(find "$staging" -maxdepth 1 -name 'Xcode*.app' -type d | head -n 1)
	if [[ -z "$expanded_app" ]]; then
		echo "No Xcode application found in $ARCHIVE." >&2
		exit 1
	fi

	sudo rm -rf -- "$XCODE_APP"
	sudo mv "$expanded_app" "$XCODE_APP"
	rm -rf -- "$staging"

	if [[ "$(installed_build)" != "$BUILD_VERSION" ]]; then
		echo "$XCODE_APP has build '$(installed_build)'; expected '$BUILD_VERSION'." >&2
		exit 1
	fi
fi

sudo "$XCODEBUILD" -license accept
sudo "$XCODEBUILD" -runFirstLaunch

# The 'Select Xcode Version' step resolves XCODE (major.minor) to /Applications/Xcode_<major.minor>.*.app.
xcode_major_minor=$(cut -d . -f 1-2 <<< "$PACKAGE_VERSION")
echo "Installed Xcode $PACKAGE_VERSION ($BUILD_VERSION) at $XCODE_APP."
echo "##vso[task.setvariable variable=XCODE]$xcode_major_minor"
