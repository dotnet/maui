#!/usr/bin/env bash
set -e

ARCH=$1
LINK_ARCH=$ARCH

case "$ARCH" in
    arm)
        TIZEN_ARCH="armv7hl"
        ;;
    armel)
        TIZEN_ARCH="armv7l"
        LINK_ARCH="arm"
        ;;
    arm64)
        TIZEN_ARCH="aarch64"
        ;;
    x86)
        TIZEN_ARCH="i686"
        ;;
    x64)
        TIZEN_ARCH="x86_64"
        LINK_ARCH="x86"
        ;;
    *)
        echo "Unsupported architecture for tizen: $ARCH"
        exit 1
esac

__CrossDir=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
__TIZEN_CROSSDIR="$__CrossDir/${ARCH}/tizen"

if [[ -z "$ROOTFS_DIR" ]]; then
    echo "ROOTFS_DIR is not defined."
    exit 1;
fi

TIZEN_TMP_DIR=$ROOTFS_DIR/tizen_tmp
mkdir -p $TIZEN_TMP_DIR

# Download files
echo ">>Start downloading files"
VERBOSE=1 $__CrossDir/tizen-fetch.sh $TIZEN_TMP_DIR $TIZEN_ARCH
echo "<<Finish downloading files"

echo ">>Start constructing Tizen rootfs"
TIZEN_RPM_FILES=`ls $TIZEN_TMP_DIR/*.rpm`
cd $ROOTFS_DIR
for f in $TIZEN_RPM_FILES; do
    rpm2cpio $f  | cpio -idm --quiet
done
echo "<<Finish constructing Tizen rootfs"

# Cleanup tmp
rm -rf $TIZEN_TMP_DIR

# Configure Tizen rootfs
echo ">>Start configuring Tizen rootfs"
ln -sfn asm-${LINK_ARCH} ./usr/include/asm
patch -p1 < $__TIZEN_CROSSDIR/tizen.patch
echo "<<Finish configuring Tizen rootfs"
