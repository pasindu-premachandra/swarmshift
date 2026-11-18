---
name: unity-cli-command-names
description: Unity CLI 1.0.0-beta.13 with Pipeline 0.9.0-exp.1 renamed the Editor commands (editor.playmode start, gameobject create); find real names by tag, prefix MSYS_NO_PATHCONV=1, and save captures inside the project.
type: reference
---

Checked 21 Oct 2026 on this project (Editor 6000.6.7f1).

- Old names such as `create_gameobject`, `get_scene_hierarchy`, `editor_play` return "Command Not Found"; the error names the new one.
- List real names: `unity command --tag <tag> --format json` (tags from `unity command --tags`) or `unity command --query <word> --detail compact`. The group is in the `command` field, for example `editor.playmode start`.
- Worked: `unity command eval "<C#>"`, `gameobject create -- --name X --primitive cube`, `gameobject find`, `gameobject delete -- --target /X`, `editor.playmode start|stop|set_run_in_background`, `console get -- --tail 20 --level error`, `console status`, `console clear`.
- Git Bash rewrites an argument starting with `/` into `C:/Program Files/Git/...`: `export MSYS_NO_PATHCONV=1` first.
- `capture game_view` needs `--source camera` when no Game view is open, and its `save_path` must be inside the project root (use `Temp/captures/`).
- Command names follow the Pipeline package version, so re-list after any package update.
