#!/bin/bash
#
# Script to create GitHub issues for iOS 26 skipped tests
# 
# Usage: ./create-ios26-issues.sh
#
# Requirements:
# - GitHub CLI (gh) must be installed and authenticated
# - Must have access to dotnet/maui repository
#

set -e

REPO="dotnet/maui"
ISSUE_DIR="docs/ios26-issues"

echo "=========================================="
echo "Creating GitHub Issues for iOS 26 Skipped Tests"
echo "Repository: $REPO"
echo "=========================================="
echo ""

# Function to create an issue
create_issue() {
    local title="$1"
    local labels="$2"
    local body_file="$3"
    
    echo "Creating issue: $title"
    echo "Labels: $labels"
    echo "Body file: $body_file"
    echo ""
    
    if [ ! -f "$body_file" ]; then
        echo "ERROR: Body file not found: $body_file"
        return 1
    fi
    
    # Create the issue and capture the URL
    issue_url=$(gh issue create \
        --repo "$REPO" \
        --title "$title" \
        --label "$labels" \
        --body-file "$body_file")
    
    echo "✅ Created: $issue_url"
    echo ""
    
    # Extract issue number from URL
    issue_number=$(echo "$issue_url" | grep -oE '[0-9]+$')
    echo "$issue_number"
}

# Array to store created issue numbers
declare -a issue_numbers

echo "Creating Issue 1: Shell Flyout Header/Footer resize tests"
issue1=$(create_issue \
    "Enable Shell Flyout Header/Footer resize tests on iOS/Catalyst" \
    "area-shell,platform/iOS 🍎,platform/macOS 🍏,t/bug,s/triaged" \
    "$ISSUE_DIR/issue1-shell-flyout-resize.md")
issue_numbers+=("$issue1")
sleep 2

echo "Creating Issue 2: TableView context actions"
issue2=$(create_issue \
    "Re-enable Issue2954 test on iOS/Catalyst - TableView cell becomes empty after adding with context actions" \
    "area-controls-tableview,platform/iOS 🍎,platform/macOS 🍏,t/bug,s/triaged" \
    "$ISSUE_DIR/issue2-tableview-context-actions.md")
issue_numbers+=("$issue2")
sleep 2

echo "Creating Issue 3: FlyoutPage RTL"
issue3=$(create_issue \
    "Re-enable Issue2818 test on iOS/Catalyst - FlyoutPage RTL hamburger icon and flyout items not displaying" \
    "area-controls-flyoutpage,platform/iOS 🍎,platform/macOS 🍏,t/bug,s/triaged,area-layout" \
    "$ISSUE_DIR/issue3-flyoutpage-rtl.md")
issue_numbers+=("$issue3")
sleep 2

echo "Creating Issue 4: CollectionView scroll position"
issue4=$(create_issue \
    "Re-enable Issue7993 test on iOS/Catalyst - CollectionView scroll position not reset when updating ItemsSource" \
    "area-controls-collectionview,platform/iOS 🍎,platform/macOS 🍏,t/bug,s/triaged" \
    "$ISSUE_DIR/issue4-collectionview-scroll.md")
issue_numbers+=("$issue4")

echo ""
echo "=========================================="
echo "✅ All issues created successfully!"
echo "=========================================="
echo ""
echo "Created issue numbers:"
for num in "${issue_numbers[@]}"; do
    echo "  - #$num"
done
echo ""
echo "To view the issues:"
for num in "${issue_numbers[@]}"; do
    echo "  https://github.com/$REPO/issues/$num"
done
echo ""
echo "Next steps:"
echo "1. Review each issue and adjust labels/milestones as needed"
echo "2. Consider adding 'version/iOS-26' label to relevant issues"
echo "3. Link issues to any existing related PRs"
echo "4. Consider creating a tracking epic/milestone for iOS 26 test re-enablement"
echo ""
