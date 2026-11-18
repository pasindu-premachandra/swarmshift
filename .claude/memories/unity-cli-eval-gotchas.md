---
name: unity-cli-eval-gotchas
description: unity command eval turns \n inside a C# string into a real newline (use (char)10), recompile_status sticks on "compiling" after an eval-requested compile, FindObjectsByType(FindObjectsSortMode) warns as obsolete in 6.6, and unity open reopens SampleScene.
type: reference
---

Found by lane 5 on 25 Oct 2026 (Editor 6000.6.7f1, Unity CLI 1.0.0-beta.13, Pipeline 0.9.0-exp.1).

- `unity command eval "<C#>"`: a `\n` inside a C# string literal arrives as a real newline and breaks the literal. Use `(char)10` instead.
- `script recompile_status` can stay "compiling" after a compile requested from eval; check the console status or the Editor log instead.
- `FindObjectsByType(FindObjectsSortMode)` is obsolete in 6.6 and adds a compile warning. Lanes 3 and 4's eval strings still use it; prefer `FindAnyObjectByType` or the overload without the sort mode.
- `unity open` reopens on `SampleScene`, not the last level; open `Level01` by hand when a brief says to leave it loaded.
- `unity close` deletes `Temp/`, so copy captures out before closing.
- A test script that holds a virtual Input System device must unhook when the device is removed, or the Input System logs errors every frame.

See also [[unity-cli-command-names]], [[unity-licensing-dialog-blocks-editor]].
