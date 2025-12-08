#!/usr/bin/env bash

# .NET MAUI PR Build Applicator (Bash version)
#
# This script downloads and applies NuGet packages from a specific .NET MAUI pull request build
# to your local project. It automatically detects your project's target framework and updates
# the necessary package references.
#
# The script uses a hive-based approach, storing packages in: ~/.maui/hives/pr-<PR_NUMBER>/packages
#
# Usage:
#   curl -fsSL https://raw.githubusercontent.com/dotnet/maui/main/eng/scripts/get-maui-pr.sh | bash -s -- 33002
#   ./get-maui-pr.sh <PR_NUMBER> [PROJECT_PATH]
#
# Examples:
#   ./get-maui-pr.sh 33002
#   ./get-maui-pr.sh 33002 ./MyApp/MyApp.csproj
#
# Requirements:
#   - .NET SDK installed
#   - curl and jq installed
#   - unzip installed
#   - Internet connection to access GitHub and Azure DevOps APIs
#   - A valid .NET MAUI project
#
# Repository Override:
#   Set MAUI_REPO environment variable to point to a fork (e.g., 'myfork/maui')
#
# For more information about testing PR builds, visit:
# https://github.com/dotnet/maui/wiki/Testing-PR-Builds

set -e

# Error handler
trap 'handle_error $? $LINENO' ERR

handle_error() {
    local exit_code=$1
    local line_num=$2
    echo ""
    error "Failed to apply PR build (exit code: $exit_code at line $line_num)"
    echo ""
    info "Troubleshooting tips:"
    echo "  • Make sure you're in a directory containing a .NET MAUI project"
    echo "  • Verify that PR #${pr_number:-NUMBER} exists: https://github.com/dotnet/maui/pull/${pr_number:-NUMBER}"
    echo "  • Check if there's a completed build for this PR (look for green checkmarks)"
    echo "  • Check your internet connection"
    echo "  • Visit: https://github.com/dotnet/maui/wiki/Testing-PR-Builds"
    exit $exit_code
}

# Configuration - Allow override via environment variable
GITHUB_REPO="${MAUI_REPO:-dotnet/maui}"
AZURE_DEVOPS_ORG="xamarin"
AZURE_DEVOPS_PROJECT="public"
PACKAGE_NAME="Microsoft.Maui.Controls"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
WHITE='\033[1;37m'
GRAY='\033[0;37m'
DGRAY='\033[0;90m'
NC='\033[0m' # No Color

# Output functions
info() {
    echo -e "${CYAN}ℹ️  $1${NC}"
}

success() {
    echo -e "${GREEN}✅ $1${NC}"
}

warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

error() {
    echo -e "${RED}❌ $1${NC}"
}

step() {
    echo -e "\n${BLUE}▶️  $1${NC}"
}

# Check dependencies
check_dependencies() {
    local missing_deps=()
    
    if ! command -v curl &> /dev/null; then
        missing_deps+=("curl")
    fi
    
    if ! command -v jq &> /dev/null; then
        missing_deps+=("jq")
    fi
    
    if ! command -v unzip &> /dev/null; then
        missing_deps+=("unzip")
    fi
    
    if ! command -v dotnet &> /dev/null; then
        missing_deps+=("dotnet")
    fi
    
    if [ ${#missing_deps[@]} -gt 0 ]; then
        error "Missing required dependencies: ${missing_deps[*]}"
        echo ""
        info "Please install the missing dependencies:"
        for dep in "${missing_deps[@]}"; do
            echo "  - $dep"
        done
        exit 1
    fi
}

# Find MAUI project
find_maui_project() {
    local search_path="$1"
    
    if [ -z "$search_path" ]; then
        search_path="."
    fi
    
    # If it's a file and ends with .csproj
    if [ -f "$search_path" ] && [[ "$search_path" == *.csproj ]]; then
        echo "$search_path"
        return 0
    fi
    
    # Search for .csproj files with UseMaui
    for proj in "$search_path"/*.csproj; do
        if [ -f "$proj" ]; then
            if grep -q '<UseMaui>true</UseMaui>' "$proj"; then
                echo "$proj"
                return 0
            fi
        fi
    done
    
    error "No .NET MAUI project found in $search_path"
    info "Make sure you're in a directory containing a MAUI project (.csproj with <UseMaui>true</UseMaui>)"
    exit 1
}

# Get PR information from GitHub
get_pr_info() {
    local pr_number="$1"
    
    info "Fetching PR #$pr_number information from GitHub..."
    
    local pr_url="https://api.github.com/repos/$GITHUB_REPO/pulls/$pr_number"
    local pr_json=$(curl -s -H "User-Agent: MAUI-PR-Script" "$pr_url")
    
    if [ -z "$pr_json" ] || echo "$pr_json" | jq -e '.message' > /dev/null 2>&1; then
        error "Failed to fetch PR information. Make sure PR #$pr_number exists."
        exit 1
    fi
    
    echo "$pr_json"
}

# Get build information from GitHub Checks API
get_build_info() {
    local sha="$1"
    
    info "Looking for build artifacts for commit ${sha:0:7}..."
    
    local checks_url="https://api.github.com/repos/$GITHUB_REPO/commits/$sha/check-runs"
    local checks_json=$(curl -s -H "User-Agent: MAUI-PR-Script" -H "Accept: application/vnd.github.v3+json" "$checks_url")
    
    # Find the main MAUI build check
    local build_check=$(echo "$checks_json" | jq -r '.check_runs[] | select(.name == "MAUI-public" and .status == "completed") | @json' | head -n 1)
    
    if [ -z "$build_check" ] || [ "$build_check" == "null" ]; then
        error "No completed build found for this PR"
        info "The build may still be in progress or may have failed."
        exit 1
    fi
    
    local conclusion=$(echo "$build_check" | jq -r '.conclusion')
    if [ "$conclusion" != "success" ]; then
        warning "Build completed with status: $conclusion"
        read -p "Do you want to continue anyway? (y/N) " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            error "Build was not successful. Aborting."
            exit 1
        fi
    fi
    
    # Extract build ID from details URL
    local details_url=$(echo "$build_check" | jq -r '.details_url')
    if [[ "$details_url" =~ buildId=([0-9]+) ]]; then
        local build_id="${BASH_REMATCH[1]}"
        success "Found build ID: $build_id"
        echo "$build_id"
        return 0
    fi
    
    error "Could not extract build ID from check run details"
    exit 1
}

# Get artifacts from Azure DevOps
get_build_artifacts() {
    local build_id="$1"
    
    info "Fetching artifacts from Azure DevOps build $build_id..."
    
    local artifacts_url="https://dev.azure.com/$AZURE_DEVOPS_ORG/$AZURE_DEVOPS_PROJECT/_apis/build/builds/$build_id/artifacts?api-version=7.1"
    local artifacts_json=$(curl -s -H "User-Agent: MAUI-PR-Script" "$artifacts_url")
    
    # Look for nuget artifact
    local download_url=$(echo "$artifacts_json" | jq -r '.value[] | select(.name == "nuget") | .resource.downloadUrl' | head -n 1)
    
    if [ -z "$download_url" ] || [ "$download_url" == "null" ]; then
        error "No 'nuget' artifact found in build $build_id"
        exit 1
    fi
    
    echo "$download_url"
}

# Download and extract artifacts
get_artifacts() {
    local download_url="$1"
    local build_id="$2"
    
    # Use hive directory pattern like Aspire CLI
    local hive_dir="$HOME/.maui/hives/pr-$pr_number"
    local packages_dir="$hive_dir/packages"
    local temp_dir="$hive_dir"
    local zip_file="$temp_dir/artifacts.zip"
    local extract_dir="$packages_dir"
    
    if [ -d "$temp_dir" ]; then
        info "Cleaning up previous download..."
        rm -rf "$temp_dir"
    fi
    
    mkdir -p "$temp_dir"
    mkdir -p "$extract_dir"
    
    info "Downloading artifacts (this may take a moment)..."
    curl -L -o "$zip_file" "$download_url" 2>/dev/null
    success "Downloaded artifacts"
    
    info "Extracting artifacts..."
    unzip -q "$zip_file" -d "$extract_dir"
    
    # Find the NuGet packages directory
    local nupkg_dir=$(find "$extract_dir" -type f -name "*.nupkg" -not -name "*.symbols.nupkg" | head -n 1 | xargs dirname)
    
    if [ -z "$nupkg_dir" ]; then
        error "Could not find NuGet packages in the extracted artifacts"
        exit 1
    fi
    
    echo "$nupkg_dir"
}

# Get package version from directory
get_package_version() {
    local packages_dir="$1"
    
    local package_file=$(find "$packages_dir" -type f -name "$PACKAGE_NAME.*.nupkg" -not -name "*.symbols.nupkg" | head -n 1)
    
    if [ -z "$package_file" ]; then
        error "Could not find $PACKAGE_NAME package in artifacts"
        exit 1
    fi
    
    local filename=$(basename "$package_file")
    if [[ "$filename" =~ $PACKAGE_NAME\.(.+)\.nupkg ]]; then
        echo "${BASH_REMATCH[1]}"
        return 0
    fi
    
    error "Could not extract version from package filename: $filename"
    exit 1
}

# Detect target framework version
get_target_framework_version() {
    local project_path="$1"
    
    local content=$(cat "$project_path")
    
    if [[ "$content" =~ \<TargetFrameworks?\>([^\<]+)\</TargetFrameworks?\> ]]; then
        local tfms="${BASH_REMATCH[1]}"
        
        if [[ "$tfms" =~ net([0-9]+)\.0 ]]; then
            echo "${BASH_REMATCH[1]}"
            return 0
        fi
    fi
    
    error "Could not determine target framework version from project file"
    exit 1
}

# Extract .NET version from package version
get_package_dotnet_version() {
    local version="$1"
    
    # Extract major version from package (e.g., "10.0.20-ci..." -> 10)
    if [[ "$version" =~ ^([0-9]+)\. ]]; then
        echo "${BASH_REMATCH[1]}"
        return 0
    fi
    
    # Default to current stable if can't determine
    echo "9"
}

# Check if version matches target framework
test_version_compatibility() {
    local version="$1"
    local target_net_version="$2"
    local package_net_version="$3"
    
    if [[ "$version" =~ preview|ci\. ]]; then
        if [ "$target_net_version" -lt "$package_net_version" ]; then
            return 1
        fi
    fi
    
    return 0
}

# Update target frameworks
update_target_frameworks() {
    local project_path="$1"
    local new_net_version="$2"
    
    # Create a backup
    cp "$project_path" "$project_path.bak"
    
    # Update all netX.0-* references (including in conditional TargetFrameworks)
    sed -i.tmp "s/net[0-9]\+\.0-/net$new_net_version.0-/g" "$project_path"
    rm -f "$project_path.tmp"
    
    success "Updated target frameworks to .NET $new_net_version.0"
    warning "You may need to update other package dependencies to match .NET $new_net_version.0"
}

# Create or update NuGet.config
update_nuget_config() {
    local project_dir="$1"
    local packages_dir="$2"
    
    local nuget_config="$project_dir/NuGet.config"
    local source_name="maui-pr-build"
    
    if [ -f "$nuget_config" ]; then
        info "Updating existing NuGet.config..."
        
        # Remove existing source with same name if it exists
        sed -i.tmp "/<add key=\"$source_name\"/d" "$nuget_config"
        
        # Add new source before the closing packageSources tag
        sed -i.tmp "s|</packageSources>|  <add key=\"$source_name\" value=\"$packages_dir\" />\n  </packageSources>|" "$nuget_config"
        
        rm -f "$nuget_config.tmp"
    else
        info "Creating new NuGet.config..."
        cat > "$nuget_config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="$source_name" value="$packages_dir" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF
    fi
    
    success "NuGet.config configured with local package source"
}

# Update project package reference
update_package_reference() {
    local project_path="$1"
    local version="$2"
    
    # Create a backup
    cp "$project_path" "$project_path.bak"
    
    local content=$(cat "$project_path")
    
    # Check if using $(MauiVersion) variable
    if [[ "$content" =~ \<PackageReference[[:space:]]+Include=\"$PACKAGE_NAME\"[[:space:]]+Version=\"\$\(MauiVersion\)\" ]]; then
        info "Replacing \$(MauiVersion) with explicit version $version"
    fi
    
    # Replace the version in PackageReference
    sed -i.tmp "s|\(<PackageReference[[:space:]]\+Include=\"$PACKAGE_NAME\"[[:space:]]\+Version=\"\)[^\"]\+\(\"[[:space:]]*/*>\)|\1$version\2|g" "$project_path"
    
    rm -f "$project_path.tmp"
    
    success "Updated $PACKAGE_NAME to version $version"
}

# Main execution
main() {
    # Check arguments
    if [ $# -lt 1 ]; then
        error "Usage: $0 <PR_NUMBER> [PROJECT_PATH]"
        exit 1
    fi
    
    pr_number="$1"  # Global for error handler
    local project_path_arg="${2:-}"
    
    # Check dependencies
    check_dependencies
    
    # Display banner
    echo -e "${MAGENTA}"
    cat << "EOF"

╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║        .NET MAUI PR Build Applicator                     ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝

EOF
    echo -e "${NC}"
    
    step "Finding MAUI project"
    local project_path=$(find_maui_project "$project_path_arg")
    local project_dir=$(dirname "$project_path")
    local project_name=$(basename "$project_path")
    success "Found project: $project_name"
    
    step "Fetching PR information"
    local pr_json=$(get_pr_info "$pr_number")
    local pr_title=$(echo "$pr_json" | jq -r '.title')
    local pr_state=$(echo "$pr_json" | jq -r '.state')
    local pr_sha=$(echo "$pr_json" | jq -r '.head.sha')
    
    info "PR #$pr_number: $pr_title"
    info "State: $pr_state"
    
    step "Detecting target framework"
    local target_net_version=$(get_target_framework_version "$project_path")
    info "Current target framework: .NET $target_net_version.0"
    
    step "Finding build artifacts"
    local build_id=$(get_build_info "$pr_sha")
    
    step "Downloading artifacts"
    local download_url=$(get_build_artifacts "$build_id")
    local packages_dir=$(get_artifacts "$download_url" "$build_id")
    
    step "Extracting package information"
    local version=$(get_package_version "$packages_dir")
    success "Found package version: $version"
    
    # Extract .NET version from package version (e.g., 10.0.20-ci.main.25607.5 -> 10)
    local package_dotnet_version=""
    if [[ $version =~ ^([0-9]+)\. ]]; then
        package_dotnet_version="${BASH_REMATCH[1]}"
    fi
    
    # Get package .NET version
    local package_net_version
    package_net_version=$(get_package_dotnet_version "$version")
    
    # Check compatibility
    local will_update_tfm=false
    local target_version="$package_net_version.0"
    if ! test_version_compatibility "$version" "$target_net_version" "$package_net_version"; then
        warning "This PR build may target a newer .NET version than your project"
        info "Your project targets: .NET $target_net_version.0"
        if [[ -n "$package_dotnet_version" ]]; then
            info "This PR build targets: .NET $package_dotnet_version.0"
            target_version="$package_dotnet_version.0"
        else
            info "This PR build targets: .NET $package_net_version.0"
        fi
        
        read -p "Do you want to update your project to .NET $target_version? (y/N) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            will_update_tfm=true
            warning "Note: You may need to manually update other package dependencies to versions compatible with .NET $target_version"
        else
            warning "Continuing without updating target framework. The package may not be compatible."
        fi
    fi
    
    # Confirmation prompt
    echo ""
    echo -e "${YELLOW}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${YELLOW}  CONFIRMATION${NC}"
    echo -e "${YELLOW}═══════════════════════════════════════════════════════════${NC}"
    echo ""
    echo -e "${CYAN}By continuing, you will apply the PR artifacts to your project.${NC}"
    echo ""
    warning "This should NOT be used in production and is for testing purposes only."
    echo ""
    echo -e "${CYAN}TIP: Create a separate Git branch for testing!${NC}"
    echo -e "${GRAY}     git checkout -b test-pr-$pr_number${NC}"
    echo ""
    echo -e "${WHITE}Please test the changes you are looking for, check for any side-effects,${NC}"
    echo -e "${WHITE}and report your findings on:${NC}"
    echo -e "${BLUE}  https://github.com/dotnet/maui/pull/$pr_number${NC}"
    echo ""
    echo -e "${WHITE}Changes to be applied:${NC}"
    echo -e "${GRAY}  • Project: $project_name${NC}"
    echo -e "${GRAY}  • Package version: $version${NC}"
    if [ "$will_update_tfm" = true ]; then
        echo -e "${GRAY}  • Target framework: Will be updated to .NET $target_version${NC}"
    fi
    echo ""
    
    read -p "Do you want to continue? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        warning "Operation cancelled by user"
        exit 0
    fi
    echo ""
    
    if [ "$will_update_tfm" = true ]; then
        local target_net_version_to_apply=10
        if [[ -n "$package_dotnet_version" ]]; then
            target_net_version_to_apply="$package_dotnet_version"
        fi
        update_target_frameworks "$project_path" "$target_net_version_to_apply"
        target_net_version="$target_net_version_to_apply"
    fi
    
    step "Configuring NuGet sources"
    update_nuget_config "$project_dir" "$packages_dir"
    
    step "Updating package reference"
    update_package_reference "$project_path" "$version"
    
    # Get latest stable version for revert instructions
    local latest_stable=$(curl -s "https://api.nuget.org/v3-flatcontainer/microsoft.maui.controls/index.json" | \
        jq -r '.versions[]' | grep -v '-' | tail -1)
    if [ -z "$latest_stable" ]; then
        latest_stable="X.Y.Z"
    fi
    
    echo -e "${GREEN}"
    cat << EOF

╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║        ✅ Successfully applied PR #$pr_number!                  ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝

EOF
    echo -e "${NC}"
    
    info "Next steps:"
    echo "  1. Run 'dotnet restore' to download the packages"
    echo "  2. Build and test your project with the PR changes"
    echo -e "  3. Report your findings on: ${CYAN}https://github.com/dotnet/maui/pull/$pr_number${NC}"
    echo ""
    info "Package: $PACKAGE_NAME $version"
    info "Local package source: $packages_dir"
    echo ""
    
    echo -e "${YELLOW}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${YELLOW}  TO REVERT TO PRODUCTION VERSION${NC}"
    echo -e "${YELLOW}═══════════════════════════════════════════════════════════${NC}"
    echo ""
    echo -e "${WHITE}1. Edit $project_name and change the version:${NC}"
    echo -e "${GRAY}   From: Version=\"$version\"${NC}"
    echo -e "${GRAY}   To:   Version=\"X.Y.Z\"${NC}"
    echo -e "${DGRAY}   (Check https://www.nuget.org/packages/$PACKAGE_NAME for latest)${NC}"
    echo ""
    echo -e "${WHITE}2. In NuGet.config, remove or comment out the 'maui-pr-$pr_number' source${NC}"
    echo ""
    echo -e "${WHITE}3. Run: dotnet restore --force${NC}"
    echo ""
    echo -e "${CYAN}TIP: Use a separate Git branch for testing PR builds!${NC}"
    echo -e "${CYAN}     Then you can easily revert: git checkout main${NC}"
    echo ""
}

# Run main function
main "$@"
