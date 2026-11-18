#!/usr/bin/env bash
set -euo pipefail
umask 022

ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
VERSION="${1:?Usage: bash archive.sh <version> <output directory> <executable> [native directory]}"
OUTPUT="${2:?Usage: bash archive.sh <version> <output directory> <executable> [native directory]}"
EXECUTABLE="${3:?Usage: bash archive.sh <version> <output directory> <executable> [native directory]}"
NATIVE="${4:-$ROOT/external/LibreWPF/freebsd-x64}"
APPLICATION_ID="io.github.KloBraticc.BetterStrap"
RUNTIME_DEPENDENCIES="libX11 libXext libXrandr libXinerama libXcursor libXi libXcomposite libXdamage libxkbcommon fontconfig freetype2 vulkan-loader mesa-libs sqlite3 libarchive onnxruntime ffmpeg krb5"
PUBLISH="$(cd "$(dirname "$EXECUTABLE")" && pwd)"

[ -s "$EXECUTABLE" ] || { echo "The FreeBSD executable is missing"; exit 1; }
mkdir -p "$OUTPUT"
OUTPUT="$(cd "$OUTPUT" && pwd)"
FINAL_TARGET="$OUTPUT/BetterStrap_${VERSION}_freebsd-x64.tar.gz"
rm -f "$FINAL_TARGET"

STAGE="$(mktemp -d "${TMPDIR:-/tmp}/betterstrap-freebsd.XXXXXX")"
trap 'rm -rf "$STAGE"' EXIT
BUNDLE="$STAGE/BetterStrap"
mkdir -p "$BUNDLE/share/applications" "$BUNDLE/share/icons/hicolor/256x256/apps" "$BUNDLE/share/metainfo" "$BUNDLE/share/licenses/betterstrap"
cp "$EXECUTABLE" "$BUNDLE/BetterStrap"
chmod 755 "$BUNDLE/BetterStrap"
printf 'Install the FreeBSD runtime dependencies:\n\n  pkg install %s\n\nRun ./BetterStrap from this directory.\n' "$RUNTIME_DEPENDENCIES" > "$BUNDLE/README.txt"
cp "$ROOT/build/Packaging/Linux/betterstrap.desktop" "$BUNDLE/share/applications/$APPLICATION_ID.desktop"
cp "$ROOT/src/BetterStrap.App/BetterStrap.png" "$BUNDLE/share/icons/hicolor/256x256/apps/$APPLICATION_ID.png"
cp "$ROOT/build/Packaging/Linux/$APPLICATION_ID.metainfo.xml" "$BUNDLE/share/metainfo/$APPLICATION_ID.metainfo.xml"
cp "$ROOT/LICENSE.BETTERSTRAP" "$BUNDLE/share/licenses/betterstrap/LICENSE"
cp "$ROOT/THIRD-PARTY-NOTICES.txt" "$ROOT/legal/sources/Bibata-Cursor-v2.0.7.tar.gz" "$BUNDLE/share/licenses/betterstrap/"
[ ! -f "$NATIVE/GLFW-LICENSE.txt" ] || cp "$NATIVE/GLFW-LICENSE.txt" "$BUNDLE/share/licenses/betterstrap/GLFW-LICENSE.txt"
[ ! -d "$PUBLISH/licenses" ] || cp "$PUBLISH/licenses/"* "$BUNDLE/share/licenses/betterstrap/"
find "$BUNDLE" -type f ! -path "$BUNDLE/BetterStrap" -exec chmod 644 {} +
chmod -R go-w "$BUNDLE"

UNCOMPRESSED="$STAGE/BetterStrap_${VERSION}_freebsd-x64.tar"
if tar --version 2>/dev/null | grep -q 'GNU tar'; then
  tar --sort=name --mtime="@${SOURCE_DATE_EPOCH:-0}" --owner=0 --group=0 --numeric-owner --mode=u=rwX,go=rX --exclude="BetterStrap/BetterStrap" -C "$STAGE" -cf "$UNCOMPRESSED" BetterStrap
  tar --mtime="@${SOURCE_DATE_EPOCH:-0}" --owner=0 --group=0 --numeric-owner --mode=0755 -C "$STAGE" -rf "$UNCOMPRESSED" BetterStrap/BetterStrap
else
  tar --uid 0 --gid 0 --numeric-owner -C "$STAGE" -cf "$UNCOMPRESSED" BetterStrap
fi
if split -n 8 -a 1 "$UNCOMPRESSED" "$STAGE/part." 2>/dev/null; then
  pids=()
  for part in "$STAGE"/part.?; do
    gzip -n -1 "$part" &
    pids+=("$!")
  done
  for pid in "${pids[@]}"; do wait "$pid"; done
  cat "$STAGE"/part.?.gz > "$FINAL_TARGET"
else
  gzip -n -1 -c "$UNCOMPRESSED" > "$FINAL_TARGET"
fi
echo "Built $FINAL_TARGET"
echo "Runtime packages: pkg install $RUNTIME_DEPENDENCIES"
