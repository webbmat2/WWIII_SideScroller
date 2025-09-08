Read /instructions/stack.manifest.json and /instructions/AI_CONTRACT.md first.
Constrain all guidance to pinned versions. If unsure, say NOT IN VERSION and cite the manifest doc URL.
Before recommending changes, look for /reports/ and use those facts.
Output diffs or exact Unity click paths with a verification checklist.


SOURCES
Prefer the DOCS_WHITELIST in .cursorrules. If citing anything else, explain why and include the official doc URL from /instructions/stack.manifest.json.

PLATFORM TARGETS
Primary target: tvOS on Apple TV with PS5 DualSense. Fallback: iOS on iPhone with Backbone. Constrain guidance to pinned versions. Output Unity click paths and a verification checklist for:
- Switching target to tvOS and required Player Settings
- Ensuring Input System supports DualSense and iOS game controllers
- Binding Move, Jump, Dash, Interact, Pause in the project Input Actions
- Verifying UI navigation via DPad and Left Stick

ROUTER_RULES
If a task is better solved by Unity AI Assistant, output a Unity AI Assistant PROMPT (using the template in .cursorrules) and stop.
If a task is better solved by Bezi in-Editor, output a Bezi PROMPT (using the template in .cursorrules) and stop.
Always enforce the VERSION GATE using /instructions/stack.manifest.json and /instructions/AI_CONTRACT.md.

QUICK_PROMPT_TEMPLATES
UnityAI:
Use /instructions/stack.manifest.json and /instructions/AI_CONTRACT.md as ground truth.
Target: Unity 6000.2.1f1 • URP 17.2.0 • Corgi 9.3 • Input System 1.14.2.
If a menu or API is not in these versions, reply NOT IN VERSION and cite the manifest URL.
Output exact click paths and a verification checklist.
Task: [insert]

Bezi:
Generate a report and in-Editor checklist to validate or wire scenes and components.
Respect the VERSION GATE and cite the manifest.
Task: [insert]
