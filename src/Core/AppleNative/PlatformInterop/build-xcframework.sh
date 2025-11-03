PROJECT="PlatformInterop"
SCHEME="PlatformInterop"
CONFIGURATION="Release"
FRAMEWORK_NAME="PlatformInterop"
OUTPUT_DIR="Framework"
XCFRAMEWORK_DIR="${OUTPUT_DIR}/${FRAMEWORK_NAME}.xcframework"

rm -rf "$OUTPUT_DIR"
rm -rf "${FRAMEWORK_NAME}.xcframework.zip"
mkdir -p "$OUTPUT_DIR"

# iOS Device
xcodebuild archive \
  -project "${PROJECT}.xcodeproj" \
  -scheme "$SCHEME" \
  -configuration "$CONFIGURATION" \
  -destination "generic/platform=iOS" \
  -archivePath "$OUTPUT_DIR/ios_devices.xcarchive" \
  -sdk iphoneos \
  SKIP_INSTALL=NO \
  BUILD_LIBRARY_FOR_DISTRIBUTION=YES

# iOS Simulator
xcodebuild archive \
  -project "${PROJECT}.xcodeproj" \
  -scheme "$SCHEME" \
  -configuration "$CONFIGURATION" \
  -destination "generic/platform=iOS Simulator" \
  -archivePath "$OUTPUT_DIR/ios_simulator.xcarchive" \
  -sdk iphonesimulator \
  SKIP_INSTALL=NO \
  BUILD_LIBRARY_FOR_DISTRIBUTION=YES

# Catalyst
xcodebuild archive \
  -project "${PROJECT}.xcodeproj" \
  -scheme "$SCHEME" \
  -configuration "$CONFIGURATION" \
  -destination "platform=macOS,variant=Mac Catalyst" \
  -archivePath "$OUTPUT_DIR/catalyst.xcarchive" \
  -sdk macosx \
  SKIP_INSTALL=NO \
  BUILD_LIBRARY_FOR_DISTRIBUTION=YES

# Combine into an XCFramework
xcodebuild -create-xcframework \
  -framework "$OUTPUT_DIR/ios_devices.xcarchive/Products/Library/Frameworks/${FRAMEWORK_NAME}.framework" \
  -framework "$OUTPUT_DIR/ios_simulator.xcarchive/Products/Library/Frameworks/${FRAMEWORK_NAME}.framework" \
  -framework "$OUTPUT_DIR/catalyst.xcarchive/Products/Library/Frameworks/${FRAMEWORK_NAME}.framework" \
  -output "$XCFRAMEWORK_DIR"

# Zip the $XCFRAMEWORK_DIR directory
cd "$OUTPUT_DIR"
zip -r --symlinks "../${FRAMEWORK_NAME}.xcframework.zip" "${FRAMEWORK_NAME}.xcframework"
cd ..
rm -rf "$OUTPUT_DIR"
