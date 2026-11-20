# PORT SABLE — Three-lead roster and control-handoff specification

## Status

The runtime control-transfer code, input/camera/interactor bindings, active-lead save migration, mobile switching widget and prefab audit are committed as source snapshots. They are **not yet wired to a playable PORT SABLE scene**, compiled in Unity or tested on the Vivo device.

The existing Old Quay / Ari Solis builder remains a retired technical testbed. Do not use its crowd character as Mara, Rafi or Elias.

## Required hero prefabs

Create three original character prefabs at exactly these paths:

- `Assets/ProjectTideline/Characters/Leads/MaraKade.prefab`
- `Assets/ProjectTideline/Characters/Leads/RafiSayegh.prefab`
- `Assets/ProjectTideline/Characters/Leads/EliasWard.prefab`

These must be three distinct hero characters. The four CC0 pedestrian families in the current prototype are crowd-pipeline test assets, not substitutes for the leads.

Each prefab root must have:
- `CharacterController`
- `ThirdPersonMotor`
- `ProximityInteractor`
- `CharacterAnimationDriver`

Each must contain a child `Animator` with:
- a valid imported Avatar;
- an assigned RuntimeAnimatorController;
- a float parameter named `MoveSpeed`;
- non-empty Idle, Walk, Run and Reaction/Gesture/Talk clips;
- a working transition setup for locomotion and interaction.

Each must contain an authored `LODGroup` with at least three levels and skinned character geometry. The main skinned mesh asset must be unique to each lead. The art team must author a recognisable, distinct silhouette, face, hairstyle, wardrobe layers and material response for each protagonist. Facial expression/gesture quality is part of the hero production target; it is not proven by the minimum asset audit.

Run **Project Tideline → 8 — Audit Bespoke Lead Prefabs**. The audit writes `ProjectTideline-Lead-Prefab-Audit.txt`. It blocks missing prefabs, incomplete root components, invalid avatar/controller/MoveSpeed, missing locomotion/interaction clips, fewer than three LOD levels, missing skinned mesh and reused primary mesh assets. It is an asset contract audit, not a visual-quality or playback test.

## Scene wiring

A new PORT SABLE scene needs one shared:
- `PlayerInputRouter`;
- `ThirdPersonCameraRig` with its Camera on the same object;
- `OnFootControlVisibility`;
- `SessionSaveCoordinator`;
- `PlayableLeadSwitcher`.

On `PlayableLeadSwitcher`, populate exactly three `leads` entries:

| canonId | actorRoot |
|---|---|
| `mara_kade` | instance of MaraKade.prefab |
| `rafi_sayegh` | instance of RafiSayegh.prefab |
| `elias_ward` | instance of EliasWard.prefab |

Set `startingLeadId` to `mara_kade`. Give all three actors distinct, non-overlapping spawn positions and ensure each prefab's root has a CharacterController sized to its own model. Assign the scene's shared input router, camera rig and on-foot controls. Do not put a second camera or AudioListener on each lead.

Add `PlayableLeadSwitcherUI` to a mobile Canvas object. Assign two visible Buttons and one Text label to previous, next and active lead fields. The controls call `RequestPreviousLead` / `RequestNextLead`; for Editor testing, Tab cycles leads if the editor keyboard option is enabled.

The active lead receives the `Player` tag. Inactive lead roots remain visible in the world but have CharacterController, motor, interactor and animation driver disabled. Camera and movement input are rebound to the selected root.

## Handoff safety rules

Switching is rejected when:
- a dialogue modal has blocked gameplay;
- the active lead is in a vehicle;
- the active lead is transitioning into or out of a vehicle;
- the cinematic confrontation sequence is active;
- the target lead is locked (ordinary user switching only).

At handoff, transient movement/look/actions are cleared, motor momentum is reset, the active actor's tag/control components are changed, the camera is retargeted and on-foot controls are rebound. Save schema v2 records the active lead ID and can still read schema v1 saves. Old v1 saves have no active-lead field; they continue from the configured starting lead.

## Test plan before calling it working

1. Open the project in Unity 6000.3.28f1 and resolve all package imports.
2. Run the static Python validator from the project root.
3. Run the campaign canon audit, outline asset builder and bespoke lead prefab audit.
4. Build a new PORT SABLE scene from the three original prefabs; do not edit the retired testbed scene in place.
5. Compile with zero C# errors and capture the Console.
6. In Play Mode, test Mara → Rafi → Elias → Mara; confirm each actor has separate transform/rotation and its own animation driver.
7. Test handoff while dialogue is open, while the current lead occupies a vehicle, during a vehicle seat transition and during the cinematic confrontation. Each request must be refused without changing active actor/camera.
8. Test movement, jump, sprint, camera look, interaction focus and prompt ownership after every successful handoff.
9. Save with each lead active, restart Play Mode and verify the same lead and position restore. Test an older v1 save.
10. Build Android, install on the exact Vivo T2x RAM variant and test touch controls, frame time, memory and thermal stability.

No step is considered passed until its actual evidence is attached to the test report.

## Known limitations

- No bespoke Mara, Rafi or Elias hero prefabs are currently bundled in v0.4.9.
- The current Old Quay scene builder still generates Ari as the legacy player and is not converted to the new story.
- The mobile switch UI component exists, but no scene Canvas Buttons are wired yet.
- The runtime handoff code is not compiled or Play Mode verified.
- Save restore depends on the roster initializing before SessionSaveCoordinator Start, which is why the roster has an earlier execution order. Validate this in the new scene and do not rely only on static checks.
- There is no automatic cinematic camera blend yet; retargeting uses the existing camera follow smoothing.

