#!/bin/bash
# Hot Reload Diagnostics Script for .NET MAUI
# Run this to collect diagnostic information for hot reload issues

set -e

OUTPUT_DIR="${1:-hot-reload-diagnostics}"
mkdir -p "$OUTPUT_DIR"

echo "🔍 Collecting Hot Reload Diagnostics..."
echo "   Output directory: $OUTPUT_DIR"
echo ""

# 1. Environment info
echo "📋 Collecting .NET info..."
dotnet --info > "$OUTPUT_DIR/dotnet-info.txt" 2>&1 || echo "Failed to get dotnet info"

echo "📋 Collecting workload list..."
dotnet workload list > "$OUTPUT_DIR/workloads.txt" 2>&1 || echo "Failed to get workloads"

# 2. Check hot reload environment variables
echo "🔧 Checking environment variables..."
{
    echo "=== Hot Reload Environment Variables ==="
    echo ""
    echo "Microsoft_CodeAnalysis_EditAndContinue_LogDir: ${Microsoft_CodeAnalysis_EditAndContinue_LogDir:-NOT SET}"
    echo "HOTRELOAD_XAML_LOG_MESSAGES: ${HOTRELOAD_XAML_LOG_MESSAGES:-NOT SET}"
    echo "XAMARIN_HOT_RELOAD_SHOW_DEBUG_LOGGING: ${XAMARIN_HOT_RELOAD_SHOW_DEBUG_LOGGING:-NOT SET}"
    echo ""
    echo "=== All Related Env Vars ==="
    env | grep -iE "(hotreload|editandcontinue|xamarin.*debug)" || echo "No matching environment variables found"
} > "$OUTPUT_DIR/env-vars.txt"

# 3. Check file encoding (UTF-8 with BOM)
echo "📝 Checking .cs file encoding..."
{
    echo "=== File Encoding Check ==="
    echo "Files should be UTF-8 with BOM (first 3 bytes: ef bb bf)"
    echo ""
    
    if [ -d "src" ]; then
        SEARCH_DIR="src"
    else
        SEARCH_DIR="."
    fi
    
    echo "Checking files in: $SEARCH_DIR"
    echo ""
    
    # Find .cs files and check for BOM
    find "$SEARCH_DIR" -name "*.cs" -type f 2>/dev/null | head -50 | while read -r file; do
        if [ -f "$file" ]; then
            HEX=$(head -c 3 "$file" | od -An -tx1 2>/dev/null | tr -d ' \n')
            if [ "$HEX" = "efbbbf" ]; then
                echo "✅ $file (UTF-8 with BOM)"
            else
                echo "❌ $file (NO BOM - hex: $HEX)"
            fi
        fi
    done
} > "$OUTPUT_DIR/encoding-check.txt"

# 4. Check for MetadataUpdateHandler
echo "🔄 Checking MetadataUpdateHandler..."
{
    echo "=== MetadataUpdateHandler Search ==="
    echo ""
    grep -rn "MetadataUpdateHandler" --include="*.cs" . 2>/dev/null || echo "No MetadataUpdateHandler found"
    echo ""
    echo "=== Assembly Attributes ==="
    grep -rn "assembly:.*MetadataUpdateHandler" --include="*.cs" . 2>/dev/null || echo "No assembly-level MetadataUpdateHandler attribute found"
} > "$OUTPUT_DIR/metadata-handler.txt"

# 5. Check MauiReactor setup
echo "🔧 Checking MauiReactor hot reload setup..."
{
    echo "=== MauiReactor Hot Reload Setup ==="
    echo ""
    echo "--- Package References ---"
    grep -rn "MauiReactor.HotReload\|Reactor.Maui" --include="*.csproj" . 2>/dev/null || echo "No MauiReactor references found"
    echo ""
    echo "--- MauiProgram.cs Hot Reload Setup ---"
    find . -name "MauiProgram.cs" -exec grep -Hn "EnableMauiReactorHotReload\|HotReload" {} \; 2>/dev/null || echo "No hot reload setup found in MauiProgram.cs"
} > "$OUTPUT_DIR/mauireactor-setup.txt"

# 6. Check project configuration
echo "📦 Checking project configuration..."
{
    echo "=== Project Files ==="
    find . -name "*.csproj" -type f 2>/dev/null | head -10
    echo ""
    echo "=== Build Configurations ==="
    grep -rn "<Configuration>" --include="*.csproj" . 2>/dev/null | head -20 || echo "No explicit configurations found"
} > "$OUTPUT_DIR/project-config.txt"

# 7. Check VS Code settings if present
echo "⚙️  Checking VS Code settings..."
{
    echo "=== VS Code Hot Reload Settings ==="
    if [ -f ".vscode/settings.json" ]; then
        echo "Found .vscode/settings.json:"
        grep -E "(hotReload|csharp.experimental|csharp.debug)" .vscode/settings.json 2>/dev/null || echo "No hot reload settings found"
    else
        echo "No .vscode/settings.json found"
    fi
} > "$OUTPUT_DIR/vscode-settings.txt"

# Summary
echo ""
echo "✅ Diagnostics collected in: $OUTPUT_DIR/"
echo ""
echo "Files created:"
ls -la "$OUTPUT_DIR/"
echo ""
echo "📌 Next steps:"
echo "   1. Review encoding-check.txt for files without BOM"
echo "   2. Check env-vars.txt to ensure logging is enabled"
echo "   3. Verify metadata-handler.txt shows your hot reload handler"
echo "   4. If issues persist, run: dotnet build -bl:$OUTPUT_DIR/build.binlog -c Debug"
echo ""
echo "To enable full diagnostic logging, run:"
echo "   export Microsoft_CodeAnalysis_EditAndContinue_LogDir=/tmp/HotReloadLog"
echo "   export HOTRELOAD_XAML_LOG_MESSAGES=1"
echo "Then launch your IDE from this terminal."
