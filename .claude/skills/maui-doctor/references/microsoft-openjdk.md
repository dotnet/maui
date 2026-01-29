# Microsoft OpenJDK Requirements

.NET MAUI requires **Microsoft Build of OpenJDK** for Android development. Other JDK distributions (Oracle, Azul, Amazon Corretto, etc.) are **not supported**.

## Why Microsoft OpenJDK Only?

- **Tested and validated** with .NET MAUI toolchain
- **Consistent behavior** across all platforms
- **Long-term support** with security updates
- **Official recommendation** from Microsoft documentation

## Installation Paths by Platform

### macOS

**Standard installation path:**
```
/Library/Java/JavaVirtualMachines/microsoft-{VERSION}.jdk/Contents/Home
```

**Examples:**
- `/Library/Java/JavaVirtualMachines/microsoft-17.jdk/Contents/Home`
- `/Library/Java/JavaVirtualMachines/microsoft-21.jdk/Contents/Home`

**Detection commands:**
```bash
# List all Microsoft JDKs
ls -d /Library/Java/JavaVirtualMachines/microsoft-*.jdk 2>/dev/null

# Using java_home to find Microsoft JDKs
/usr/libexec/java_home -V 2>&1 | grep -i microsoft

# Get specific version path
/usr/libexec/java_home -v 17 2>/dev/null
```

**Identification:** Look for `"Microsoft"` in the vendor string or `microsoft-` prefix in directory name.

### Windows

**Standard installation paths:**
```
C:\Program Files\Microsoft\jdk-{VERSION}\
C:\Program Files (x86)\Microsoft\jdk-{VERSION}\  (32-bit, rare)
%LOCALAPPDATA%\Programs\Microsoft\jdk-{VERSION}\  (per-user install)
```

**Examples:**
- `C:\Program Files\Microsoft\jdk-17.0.14+7\`
- `C:\Program Files\Microsoft\jdk-21.0.5+11\`

**Registry keys:**
```
HKLM\SOFTWARE\Microsoft\JDK\{VERSION}
HKLM\SOFTWARE\JavaSoft\JDK\{VERSION}  (if FeatureOracleJavaSoft enabled)
```

**Detection commands (PowerShell):**
```powershell
# List Microsoft JDK installations
Get-ChildItem "C:\Program Files\Microsoft" -Filter "jdk-*" -ErrorAction SilentlyContinue

# Check registry
Get-ChildItem "HKLM:\SOFTWARE\Microsoft\JDK" -ErrorAction SilentlyContinue

# Check java version output
java -version 2>&1 | Select-String "Microsoft"
```

**Detection commands (cmd):**
```cmd
dir "C:\Program Files\Microsoft\jdk-*" /b 2>nul
reg query "HKLM\SOFTWARE\Microsoft\JDK" 2>nul
java -version 2>&1 | findstr /i "Microsoft"
```

### Linux

**Package-based installations (recommended):**
```
/usr/lib/jvm/msopenjdk-{VERSION}/
/usr/lib/jvm/msopenjdk-{VERSION}-amd64/
/usr/lib/jvm/msopenjdk-{VERSION}-arm64/
```

**Examples:**
- `/usr/lib/jvm/msopenjdk-17/`
- `/usr/lib/jvm/msopenjdk-21-amd64/`

**Detection commands:**
```bash
# List Microsoft JDKs
ls -d /usr/lib/jvm/msopenjdk-* 2>/dev/null

# Check alternatives
update-java-alternatives -l 2>/dev/null | grep msopenjdk

# Check java version output
java -version 2>&1 | grep -i "Microsoft"
```

---

## Version Detection

### Identifying Microsoft OpenJDK

When running `java -version`, Microsoft OpenJDK output looks like:

```
openjdk version "17.0.14" 2025-01-21 LTS
OpenJDK Runtime Environment Microsoft-XXXXXXX (build 17.0.14+7-LTS)
OpenJDK 64-Bit Server VM Microsoft-XXXXXXX (build 17.0.14+7-LTS, mixed mode, sharing)
```

**Key identifiers:**
- Vendor line contains `Microsoft`
- Runtime name includes `Microsoft-`

### Non-Microsoft JDKs (NOT Supported)

These will **NOT** have "Microsoft" in the output:

```
# Oracle JDK
Java(TM) SE Runtime Environment (build 17.0.x+y)

# Azul Zulu
OpenJDK Runtime Environment Zulu17.xx+yy-CA (build 17.0.x+y)

# Amazon Corretto
OpenJDK Runtime Environment Corretto-17.0.x.y.z (build 17.0.x+y)

# AdoptOpenJDK/Temurin
OpenJDK Runtime Environment Temurin-17.0.x+y (build 17.0.x+y)
```

---

## Detection Script Examples

### macOS Detection Script

```bash
#!/bin/bash

# Find Microsoft OpenJDK on macOS
find_ms_jdk_macos() {
  local required_major=$1
  
  # Method 1: Check standard path
  local jdk_path="/Library/Java/JavaVirtualMachines/microsoft-${required_major}.jdk/Contents/Home"
  if [ -d "$jdk_path" ]; then
    echo "$jdk_path"
    return 0
  fi
  
  # Method 2: Use java_home and filter for Microsoft
  local all_jdks=$(/usr/libexec/java_home -V 2>&1)
  local ms_jdk=$(echo "$all_jdks" | grep -i "microsoft" | grep "^[[:space:]]*${required_major}" | head -1 | awk '{print $NF}')
  if [ -n "$ms_jdk" ]; then
    echo "$ms_jdk"
    return 0
  fi
  
  return 1
}

# Usage
JDK_PATH=$(find_ms_jdk_macos 17)
if [ $? -eq 0 ]; then
  echo "✅ Found Microsoft JDK: $JDK_PATH"
else
  echo "❌ Microsoft OpenJDK 17 not found"
fi
```

### Windows Detection Script (PowerShell)

```powershell
function Find-MicrosoftJdk {
  param([int]$RequiredMajor)
  
  # Method 1: Check Program Files
  $paths = @(
    "C:\Program Files\Microsoft",
    "C:\Program Files (x86)\Microsoft",
    "$env:LOCALAPPDATA\Programs\Microsoft"
  )
  
  foreach ($basePath in $paths) {
    $jdkDirs = Get-ChildItem $basePath -Filter "jdk-$RequiredMajor*" -Directory -ErrorAction SilentlyContinue
    if ($jdkDirs) {
      return $jdkDirs[0].FullName
    }
  }
  
  # Method 2: Check registry
  $regPath = "HKLM:\SOFTWARE\Microsoft\JDK\$RequiredMajor*"
  $regKeys = Get-ChildItem $regPath -ErrorAction SilentlyContinue
  if ($regKeys) {
    $javaHome = (Get-ItemProperty $regKeys[0].PSPath -ErrorAction SilentlyContinue).Path
    if ($javaHome -and (Test-Path $javaHome)) {
      return $javaHome
    }
  }
  
  return $null
}

# Usage
$jdkPath = Find-MicrosoftJdk -RequiredMajor 17
if ($jdkPath) {
  Write-Host "✅ Found Microsoft JDK: $jdkPath" -ForegroundColor Green
} else {
  Write-Host "❌ Microsoft OpenJDK 17 not found" -ForegroundColor Red
}
```

### Linux Detection Script

```bash
#!/bin/bash

# Find Microsoft OpenJDK on Linux
find_ms_jdk_linux() {
  local required_major=$1
  
  # Method 1: Check standard package path
  local jdk_paths=(
    "/usr/lib/jvm/msopenjdk-${required_major}"
    "/usr/lib/jvm/msopenjdk-${required_major}-amd64"
    "/usr/lib/jvm/msopenjdk-${required_major}-arm64"
  )
  
  for path in "${jdk_paths[@]}"; do
    if [ -d "$path" ]; then
      echo "$path"
      return 0
    fi
  done
  
  # Method 2: Check alternatives
  local alt_path=$(update-java-alternatives -l 2>/dev/null | grep "msopenjdk-${required_major}" | awk '{print $3}')
  if [ -n "$alt_path" ] && [ -d "$alt_path" ]; then
    echo "$alt_path"
    return 0
  fi
  
  return 1
}

# Usage
JDK_PATH=$(find_ms_jdk_linux 17)
if [ $? -eq 0 ]; then
  echo "✅ Found Microsoft JDK: $JDK_PATH"
else
  echo "❌ Microsoft OpenJDK 17 not found"
fi
```

---

## Installation Commands

### macOS

```bash
# Homebrew (recommended)
brew install --cask microsoft-openjdk@17
brew install --cask microsoft-openjdk@21

# Or download PKG from:
# https://learn.microsoft.com/en-us/java/openjdk/download
```

### Windows

```powershell
# Using winget (recommended)
winget install Microsoft.OpenJDK.17
winget install Microsoft.OpenJDK.21

# Or download EXE/MSI from:
# https://learn.microsoft.com/en-us/java/openjdk/download
```

### Linux (Ubuntu/Debian)

```bash
# Add Microsoft repository
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install
sudo apt update
sudo apt install msopenjdk-17
# or
sudo apt install msopenjdk-21
```

### Linux (Fedora/RHEL/CentOS)

```bash
# Add Microsoft repository
sudo rpm -Uvh https://packages.microsoft.com/config/rhel/8/packages-microsoft-prod.rpm

# Install
sudo dnf install msopenjdk-17
# or
sudo dnf install msopenjdk-21
```

---

## Using AndroidSdk.Tool for Detection

If `AndroidSdk.Tool` is installed, it provides better detection:

```bash
# List all JDKs (shows vendor info)
android jdk list

# Find Microsoft JDK by version
android jdk find --version 17

# Output includes vendor identification
```

The tool automatically identifies Microsoft OpenJDK installations and can filter by vendor.

---

## Troubleshooting

### JAVA_HOME Guidance

**JAVA_HOME is NOT required** - .NET MAUI tools auto-detect JDK installations.

**When JAVA_HOME matters:**
- ✅ Not set → Fine, auto-detection works
- ✅ Set to Microsoft JDK path → Fine  
- ❌ Set to non-Microsoft JDK → **Problem!** Fix it (see below)

### Wrong JDK Selected (JAVA_HOME points to non-Microsoft JDK)

If `JAVA_HOME` is set to a non-Microsoft JDK, either unset it or point to Microsoft:

**Option 1: Unset JAVA_HOME (recommended - let tools auto-detect):**
```bash
# macOS/Linux
unset JAVA_HOME

# Windows PowerShell
Remove-Item Env:JAVA_HOME
```

**Option 2: Point JAVA_HOME to Microsoft JDK:**

**macOS:**
```bash
export JAVA_HOME=/Library/Java/JavaVirtualMachines/microsoft-17.jdk/Contents/Home
```

**Windows:**
```powershell
# Session only
$env:JAVA_HOME = "C:\Program Files\Microsoft\jdk-17.0.14+7"

# Permanent (requires admin)
[Environment]::SetEnvironmentVariable("JAVA_HOME", "C:\Program Files\Microsoft\jdk-17.0.14+7", "Machine")
```

**Linux:**
```bash
# Use update-java-alternatives (preferred)
sudo update-java-alternatives --set msopenjdk-17-amd64

# Or set JAVA_HOME manually
export JAVA_HOME=/usr/lib/jvm/msopenjdk-17
```

### Multiple JDKs Installed

If multiple JDK vendors are installed:

1. Check current JDK: `java -version`
2. Look for "Microsoft" in output
3. If wrong vendor and `JAVA_HOME` is set, unset it or point to Microsoft path
4. If wrong vendor and `JAVA_HOME` is NOT set, the non-Microsoft JDK may be first in PATH - install Microsoft JDK and it should take precedence

### JDK Not Found After Installation

1. Restart terminal/shell to pick up PATH changes
2. Verify installation path exists
3. If `JAVA_HOME` is set, verify it points to correct path (or unset it)
4. On Windows, verify PATH includes the JDK bin directory

---

## Official Resources

- [Microsoft OpenJDK Download](https://learn.microsoft.com/en-us/java/openjdk/download)
- [Microsoft OpenJDK Installation Guide](https://learn.microsoft.com/en-us/java/openjdk/install)
- [Microsoft OpenJDK GitHub](https://github.com/microsoft/openjdk)
