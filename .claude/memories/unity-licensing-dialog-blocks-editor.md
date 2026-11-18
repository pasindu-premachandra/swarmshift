---
name: unity-licensing-dialog-blocks-editor
description: The Unity Licensing Client can die mid-session and leave the Editor frozen on a modal "Connection Lost" dialog that the CLI cannot see; unity status still says ready, so check the screen when eval times out.
type: reference
---

Seen 25 Oct 2026 by lane 5 (Editor 6000.6.7f1, Unity CLI 1.0.0-beta.13), about 10:55 IST after a compile.

- The dialog reads "Connection Lost: The connection with the Unity Licensing Client has been lost." and blocks the Editor until someone presses Retry.
- While it is up, `unity status` still reports `ready`, the console's ground truth goes stale and `unity command eval` times out.
- Pressing Retry (lane 5 did, 21 minutes later) restarts the licensing client and the Editor finishes its reload. No restart of the Editor was needed.
- So when eval or a compile hangs with the Editor "ready", look for this dialog before assuming the code or the CLI is at fault.

See also [[unity-cli-eval-gotchas]].
