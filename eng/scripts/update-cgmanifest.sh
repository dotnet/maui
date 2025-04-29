#!/bin/bash

# Generate cgmanifest.json with versions from Versions.props
#
# This script reads the Versions.props file to extract NuGet package versions
# and updates the cgmanifest.json file with these versions.

set -e

# Get the paths to the files
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
VERSIONS_PROPS_PATH="$REPO_ROOT/eng/Versions.props"
CG_MANIFEST_PATH="$REPO_ROOT/src/Templates/src/cgmanifest.json"

echo "Reading versions from: $VERSIONS_PROPS_PATH"

# Function to extract a property value from Versions.props
get_property_value() {
  local property_name="$1"
  grep -A1 "<$property_name>" "$VERSIONS_PROPS_PATH" | grep -v "<$property_name>" | sed -E 's/.*>([^<]+)<.*/\1/'
}

# Function to update JSON value using jq
update_package_version() {
  local package_name="$1"
  local package_version="$2"
  
  # Check if jq is installed
  if ! command -v jq &> /dev/null; then
    echo "Error: jq is required but not installed. Please install jq."
    exit 1
  fi
  
  if [ -f "$CG_MANIFEST_PATH" ]; then
    # Check if the package exists in the manifest
    if jq --arg name "$package_name" '.registrations[] | select(.component.type=="nuget" and .component.nuget.name==$name)' "$CG_MANIFEST_PATH" | grep -q .; then
      # Update existing entry
      jq --arg name "$package_name" --arg version "$package_version" '(.registrations[] | select(.component.type=="nuget" and .component.nuget.name==$name).component.nuget.version) = $version' "$CG_MANIFEST_PATH" > "$CG_MANIFEST_PATH.tmp"
      mv "$CG_MANIFEST_PATH.tmp" "$CG_MANIFEST_PATH"
      echo "Updated $package_name to version $package_version"
    else
      # Add new entry
      jq --arg name "$package_name" --arg version "$package_version" '.registrations += [{"component": {"type": "nuget", "nuget": {"name": $name, "version": $version}}}]' "$CG_MANIFEST_PATH" > "$CG_MANIFEST_PATH.tmp"
      mv "$CG_MANIFEST_PATH.tmp" "$CG_MANIFEST_PATH"
      echo "Added $package_name with version $package_version"
    fi
  else
    # Create a new file with initial content
    cat > "$CG_MANIFEST_PATH" << EOF
{
  "\$schema": "https://json.schemastore.org/component-detection-manifest.json",
  "version": 1,
  "registrations": [
    {
      "component": {
        "type": "nuget",
        "nuget": {
          "name": "$package_name",
          "version": "$package_version"
        }
      }
    }
  ]
}
EOF
    echo "Created new cgmanifest.json with $package_name version $package_version"
  fi
}

# Create the manifest file if it doesn't exist
if [ ! -f "$CG_MANIFEST_PATH" ]; then
  echo "Creating a new cgmanifest.json file"
  mkdir -p "$(dirname "$CG_MANIFEST_PATH")"
  echo '{
  "$schema": "https://json.schemastore.org/component-detection-manifest.json",
  "version": 1,
  "registrations": []
}' > "$CG_MANIFEST_PATH"
fi

# Define mappings of package names to property names in Versions.props
declare -A PACKAGE_MAPPINGS=(
  ["Microsoft.NET.Test.Sdk"]="MicrosoftNETTestSdkPackageVersion"
  ["xunit"]="XunitPackageVersion"
  ["xunit.runner.visualstudio"]="XunitRunnerVisualStudioPackageVersion"
  ["xunit.analyzer"]="XUnitAnalyzersPackageVersion"
  ["coverlet.collector"]="CoverletCollectorPackageVersion"
  ["Microsoft.Extensions.Logging.Debug"]="MicrosoftExtensionsLoggingDebugVersion"
  ["Microsoft.Extensions.Configuration"]="MicrosoftExtensionsConfigurationVersion"
  ["Microsoft.Extensions.DependencyInjection"]="MicrosoftExtensionsDependencyInjectionVersion"
  ["Microsoft.WindowsAppSDK"]="MicrosoftWindowsAppSDKPackageVersion"
  ["Microsoft.Graphics.Win2D"]="MicrosoftGraphicsWin2DPackageVersion"
  ["Microsoft.Windows.SDK.BuildTools"]="MicrosoftWindowsSDKBuildToolsPackageVersion"
)

# Update the manifest with versions from Versions.props
for package_name in "${!PACKAGE_MAPPINGS[@]}"; do
  property_name="${PACKAGE_MAPPINGS[$package_name]}"
  version=$(get_property_value "$property_name")
  
  if [ -n "$version" ]; then
    update_package_version "$package_name" "$version"
  else
    echo "Warning: Could not find version for $property_name in Versions.props"
  fi
done

echo "Updated cgmanifest.json saved to: $CG_MANIFEST_PATH"
