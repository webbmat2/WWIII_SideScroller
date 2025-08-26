#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")"/.. && pwd)"
cd "$ROOT"
ZIPNAME="WWIII_SideScroller_$(date +%Y-%m-%d_%H%M).zip"
{ [ -d Assets ] && echo Assets
  [ -d Packages ] && echo Packages
  [ -d ProjectSettings ] && echo ProjectSettings
  [ -d UserSettings ] && echo UserSettings
  [ -f .gitignore ] && echo .gitignore
  [ -f CHANGELOG_AI.md ] && echo CHANGELOG_AI.md
  find . -maxdepth 1 -type f \( -name "*.sln" -o -name "*.csproj" \) | sed 's|^\./||'
} | zip -r -@ "$ZIPNAME" \
    -x "Library/*" "Temp/*" "Obj/*" "Build/*" "Builds/*" \
       "Logs/*" "MemoryCaptures/*" ".git/*" ".DS_Store"
ls -lh "$ZIPNAME"
SUM=$(shasum -a 256 "$ZIPNAME" | awk '{print $1}')
echo "$SUM  $ZIPNAME" | tee -a CHECKSUMS.txt
{
  echo "### $(date '+%Y-%m-%d %H:%M') - Created project zip"
  echo "- File: $ZIPNAME"
  echo "- SHA256: $SUM"
  echo
} >> CHANGELOG_AI.md
