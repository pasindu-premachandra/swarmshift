---
name: killed-meta-build-leaves-settings
description: BuildWeb.BuildMeta switches the WebGL template, compression and decompression fallback and restores them in a finally block; a killed batch build skips that, and Unity has already written the Meta values to ProjectSettings.asset.
type: project
---

Seen 18 Nov 2026: Claude Code stopped the `unity build --execute-method BuildWeb.BuildMeta` shell because the machine was low on memory (3.5 GB free).

- The batch Editor had already saved `ProjectSettings/ProjectSettings.asset` with `webGLTemplate: PROJECT:FBInstantTemplate`, `webGLCompressionFormat: 1` and `webGLDecompressionFallback: 1`.
- The YouTube values are `PROJECT:YTGameWrapperTemplate`, `2` (disabled) and `0`. Put them back by hand with the Editor closed before any Playables build.
- After any stopped Meta build, check those three lines first.

See also [[unity-cli-eval-gotchas]].
