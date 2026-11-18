# SWARMSHIFT

A portrait, one-thumb crowd game for YouTube Playables first, then Android and iOS from the same project.
Hold to launch white drones from a mothership, slide to steer; gates multiply or cut the swarm; warp rings charge drones cyan, yellow or magenta; a drone beats an enemy of its own colour and trades one for one with any other; crack the hive's number to win.
Pasindu's own venture (startup pillar).

## Where things are

- Plan, decisions, research and evidence: `E:\Projects\.claude\tasks\youtube-playables\README.md` (newest section first).
- Visual spec: `E:\Projects\.claude\tasks\youtube-playables\review\design\run1b\` (`DESIGN.md`, `tokens.css`, the HTML pages, fonts). Later design runs land beside it as `run2`, `run3`.
- How to build for Playables (settings checklist, size budget, rules mapped to Unity): `E:\Projects\.claude\tasks\youtube-playables\evidence\scout-unity-playables\report.md`.
- Skills for this project: `.claude/skills/` (Unity's official `unity-cli`, `optimize-web`, `ui`, `ui-ugui`, `unity-package-management`, plus four Unity skills from gamedev-skills/awesome-gamedev-agent-skills; licences in `.claude/skills/_licenses/`).

## Stack

- Unity 6000.6.7f1, URP (`com.unity.template.urp-blank`), Input System, uGUI with TextMeshPro, Pipeline package for the Unity CLI.
- Drive the Editor with the Unity CLI (`unity status`, `unity command ...`, `unity build`); see `.claude/skills/unity-cli/SKILL.md`. The CLI and Editor are installed: never run an install script or `unity install`, never `unity self-update`.
- No Unity Cloud, no Unity services, no analytics.

## Unity 6 rules

- Read `ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json` first. Target that version and only installed packages.
- Never invent Unity APIs. Say "unverified" if unsure.
- Keep all needed `using` directives, and never remove one you have not checked.
- Write complete compilable files. The file name must match the MonoBehaviour class name.
- Unity null checks: `== null`, never `?.` or `??` on `UnityEngine.Object`.
- Unity 6: `linearVelocity` and `linearDamping`, not `velocity` and `drag`.
- Before saying "done", confirm the code compiles and the console has no errors.

(Rule lines from scoyoc5/unity-ai-agent-rules, MIT.)

## YouTube Playables limits that shape the code

- Initial download under 30 MiB (aim for 15), every file under 30 MiB. Build with Compression Format Disabled.
- No network calls of any kind. No analytics, no Unity services, no remote content.
- Save progress only through the Playables SDK `saveData`, never `PlayerPrefs` (IndexedDB on Web).
- Pause and mute only through the SDK callbacks; no master mute button in the game.
- One thread: no C# threads, no parallel jobs. Units are plain data drawn with GPU instancing; no Rigidbody or collider per unit.
- No in-app purchases, no outbound links, no login.

## House rules

- No commits unless Pasindu asks; he reviews diffs. No `Co-Authored-By` or any trailer. Local git identity is set to his personal email.
- No em dash or en dash anywhere, code comments included. Comments only where the logic is genuinely non-obvious, one short line.
- Code: `Assets/Scripts/Game/` gameplay, `Assets/Scripts/Platform/` Playables and later mobile services. One class per file.
