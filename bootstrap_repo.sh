#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"

# Init git if missing
if [ ! -d .git ]; then
  git init
  git branch -M main || true
fi

# Unity .gitignore
cat > .gitignore <<GI
Library/
Temp/
Obj/
Build/
Builds/
UserSettings/
Logs/
MemoryCaptures/
*.user
*.csproj
*.sln
*.pidb
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
.DS_Store
.vscode/
*.apk
*.aab
*.unitypackage
GI

# Optional LFS
if command -v git-lfs >/dev/null 2>&1; then
  git lfs install --local
  cat > .gitattributes <<LFS
*.png filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text
*.mp4 filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text
LFS
fi

git add -A
if ! git diff --cached --quiet; then
  git commit -m "Bootstrap: init repo, .gitignore, LFS attrs"
fi
echo "Done."
