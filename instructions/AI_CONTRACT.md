# AI Contract

## Version gate
ALWAYS constrain suggestions to versions in /instructions/stack.manifest.json.  
If a menu/API/setting isn’t present in those versions, reply: NOT IN VERSION.  
Then propose the nearest supported equivalent and include the official doc URL from the manifest.

## Sources order
1) /instructions/*.md and stack.manifest.json  
2) Official docs listed in the manifest  
Do not use blogs or forums unless explicitly asked.

## Output format
- If code: provide a unified diff or full file.  
- If editor change: provide exact Unity click path + expected value + a short verification checklist.  
- If diagnosis: read /reports/* first and cite the files you used.

## Project assumptions
Unity 6000.2.1f1 • URP 17.2.0 (2D) • Timeline 1.8.9 • Addressables 2.7.2 • Cinemachine 3.1.4 • Input System 1.14.2  
Corgi Engine 9.3 • DOTween Pro 1.0.380 • ES3 3.5.24 • Yarn 3.0.3 • Odin 3.3.1.13 • AFPS 1.5.6 • TexturePacker Importer 7.6.0  
Feel 5.7 • Text Animator 2.3.1 • Spine Editor 4.3.01 + spine-unity 4.3.x  
Unity AI Assistant 1.0.0-pre.12 • AI Generators 1.0.0-pre.19

## Mandatory behaviors
- Ask for /reports snapshot if missing (ProjectSettings, URP asset, packages, Addressables analyze).  
- Prefer configuration over new code when a built-in or asset feature exists.  
- Mobile-first: avoid heavy post-processing; target <50 draw calls and <200 MB runtime memory.

