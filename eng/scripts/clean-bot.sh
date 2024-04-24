#!/bin/bash -ex

# Print disk status before cleaning
df -h

# We don't care about errors in this section, we just want to clean as much as possible
set +e

# Delete all the simulator devices. These can take up a lot of space over time (I've seen 100+GB on the bots)
/Applications/Xcode.app/Contents/Developer/usr/bin/simctl delete all

# Delete old Xcodes.
ls -lad /Applications/Xcode*.app

oldXcodes=(
	"/Applications/Xcode44.app"
	"/Applications/Xcode5.app"
	"/Applications/Xcode502.app"
	"/Applications/Xcode511.app"
	"/Applications/Xcode6.0.1.app"
	"/Applications/Xcode6.app"
	"/Applications/Xcode601.app"
	"/Applications/Xcode61.app"
	"/Applications/Xcode611.app"
	"/Applications/Xcode62.app"
	"/Applications/Xcode63.app"
	"/Applications/Xcode64.app"
	"/Applications/Xcode7.app"
	"/Applications/Xcode701.app"
	"/Applications/Xcode71.app"
	"/Applications/Xcode711.app"
	"/Applications/Xcode72.app"
	"/Applications/Xcode731.app"
	"/Applications/Xcode8-GM.app"
	"/Applications/Xcode8.app"
	"/Applications/Xcode81-GM.app"
	"/Applications/Xcode81.app"
	"/Applications/Xcode82.app"
	"/Applications/Xcode821.app"
	"/Applications/Xcode83.app"
	"/Applications/Xcode833.app"
	"/Applications/Xcode9-GM.app"
	"/Applications/Xcode9.app"
	"/Applications/Xcode91.app"
	"/Applications/Xcode92.app"
	"/Applications/Xcode93.app"
	"/Applications/Xcode94.app"
	"/Applications/Xcode941.app"
	"/Applications/Xcode10.app"
	"/Applications/Xcode101-beta2.app"
	"/Applications/Xcode101-beta3.app"
	"/Applications/Xcode101.app"
	"/Applications/Xcode102-beta1.app"
	"/Applications/Xcode102.app"
	"/Applications/Xcode1021.app"
	"/Applications/Xcode103.app"
	"/Applications/Xcode10GM.app"
	"/Applications/Xcode11-beta3.app"
	"/Applications/Xcode11-GM.app"
	"/Applications/Xcode11.app"
	"/Applications/Xcode111.app"
	"/Applications/Xcode112.app"
	"/Applications/Xcode1121.app"
	"/Applications/Xcode113.app"
	"/Applications/Xcode1131.app"
	"/Applications/Xcode114-beta1.app"
	"/Applications/Xcode114-beta2.app"
	"/Applications/Xcode114-beta3.app"
	"/Applications/Xcode114.app"
	"/Applications/Xcode1141.app"
	"/Applications/Xcode115-beta1.app"
	"/Applications/Xcode115-beta2.app"
	"/Applications/Xcode115-GM.app"
	"/Applications/Xcode_8.0.app"
	"/Applications/Xcode_8.1.app"
	"/Applications/Xcode_8.2.1.app"
	"/Applications/Xcode_8.3.3.app"
	"/Applications/Xcode_9.0.app"
	"/Applications/Xcode_9.1.0.app"
	"/Applications/Xcode_9.2.0.app"
	"/Applications/Xcode_9.2.app"
	"/Applications/Xcode_9.4.1.app"
# Xcode 10.2.1 is currently used by Binding Tools for Swift # /Applications/Xcode_10.2.1.app
	"/Applications/Xcode_11.3.0.app"
	"/Applications/Xcode_11.5.0.app"
	"/Applications/Xcode_11.6.0-beta1.app"
	"/Applications/Xcode_12.0.0-beta1.app"
	"/Applications/Xcode_12.0.0-beta2.app"
	"/Applications/Xcode_12.0.0-beta3.app"
	"/Applications/Xcode_12.0.0-beta4.app"
	"/Applications/Xcode_12.0.0-beta5.app"
	"/Applications/Xcode_12.0.0-beta6.app"
	"/Applications/Xcode_12.1.0-GM.app"
	"/Applications/Xcode_12.0.0-GMb.app"
	"/Applications/Xcode_12.2.0-beta1.app"
	"/Applications/Xcode_12.2.0-beta2.app"
	"/Applications/Xcode_12.2.0-beta3.app"
	"/Applications/Xcode_12.2.0-beta.3.app"
	"/Applications/Xcode_12.2.0-rc.app"
	"/Applications/Xcode_12.5.0-rc.app"
	"/Applications/Xcode_13.0.0-beta.app"
	"/Applications/Xcode_13.0.0-beta2.app"
	"/Applications/Xcode_13.0.0-beta3.app"
	"/Applications/Xcode_14.3.1.app"
	"/Applications/Xcode_15.0.0.app"
	"/Applications/Xcode_15.0.1.app"
	"/Applications/Xcode_15.1.0.app"
)

# remove wrongly added .xip files under /Applications, confuses provisionator and 
# are not needed and wrong
sudo rm -Rf /Applications/*.xip

# pick the current selected xcode to make sure we do not remove it.
XCODE_SELECT=$(xcode-select -p)

for oldXcode in "${oldXcodes[@]}"; do
	if [ "$XCODE_SELECT" != "$oldXcode/Contents/Developer" ]; then
		sudo rm -Rf "$oldXcode"
	else
		echo "Not removing $oldXcode because is the currently selected one."
	fi
done

DIR="$(dirname "${BASH_SOURCE[0]}")"
"$DIR"/clean-simulator-runtime.sh

# Print disk status after cleaning
df -h
