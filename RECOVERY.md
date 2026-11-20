# Project Tideline — recovered baseline

This repository is being populated from the verified local Git bundle `Project_Tideline_v0.4.9.bundle`.

## Source checkpoint

- Original branch: `main`
- Source commit: `ba72e8d7ebec35f2158e318dab8afad75bf518b0`
- Annotated tag: `v0.4.9` (peeled target is the source commit above)
- Source tree: 77 tracked files, including 45 C# files
- Unity Editor target: `6000.3.28f1`

The original bundle was cloned and checked locally. `python tools/validate_static.py` passed. That check is static only; it does not compile C# or run Unity.

## Publication note

The connected GitHub API cannot import a Git bundle or preserve its original commit objects and author metadata directly. The project source is being published as a source-tree snapshot. Its source checkpoint SHA is recorded above so the snapshot is traceable to the recovered bundle. Do not interpret the GitHub snapshot commit SHA as the original project commit SHA.

## Current blockers

- The 48 pinned CC0 GLB files are downloaded locally by the Unity Editor installer and are not included in the source bundle.
- Unity package import, compilation, scene generation, Play Mode, Android build and physical-device testing still require Unity Editor and a connected development device.

## Working rule

Every new project source, documentation or configuration file created for Tideline must be committed to this repository as part of the deliverable. Large third-party GLB assets should only be added through an intentional large-file strategy; they must not be silently replaced with primitive stand-ins.
