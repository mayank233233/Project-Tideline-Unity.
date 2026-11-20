# PORT SABLE — visual direction and mobile rendering contract

## Quality ambition

The visual reference bar is the cinematic environmental detail, character presence, vehicle presentation and atmospheric composition associated with GTA V, GTA VI and Red Dead Redemption 2. Those are craft references, not a promise that the game can reproduce their platform scale or rendering cost on a Vivo T2x 5G.

Rockstar describes its current GTA VI extended look as captured from PlayStation 5. The target phone instead uses MediaTek Dimensity 6020 with Arm Mali-G57 MC2; Vivo lists 4 GB, 6 GB and 8 GB physical-RAM configurations. This is a different hardware class. The realistic goal is premium authored mobile visuals, not literal GTA VI parity.

## The art direction: Port Sable

Port Sable should have its own visual identity rather than imitate Vice City or a generic neon coastal city:

- **Old Quay:** weathered limestone, painted steel shutters, concrete repairs, cable runs, ferry haze, narrow warm shop interiors and damp road surfaces.
- **Southline:** dense low-rise housing, repair shops, community courts, layered signage and lived-in street furniture.
- **The Velvet Mile:** polished stone, brass, tinted glass, warm interior pools, event lighting and carefully controlled reflections.
- **Breaker Island:** salt-corroded metal, shipyard cranes, faded safety paint, hard industrial silhouettes and sodium-lit service roads.
- **Crown Heights:** older stone homes, tropical planting, expensive renovations and framed harbour views.
- **Saltline:** open sky, dry scrub, salt flats, wind-shaped vegetation and long road sightlines.
- **Glass Ward:** cool glass, reflective stone, corporate lobbies and visible contrast between public polish and service infrastructure.

The world must read through shape, lighting, materials, signage and sound before the player reads labels. Each district needs a recognisable skyline and signature locations.

## Character and vehicle priority

Do not present four shared CC0 pedestrian models as the final principal cast. They are useful for crowd prototypes and animation-pipeline validation. The three leads need bespoke production models with clearly different silhouettes, faces, hair, clothing layers, material response, facial expression range and authored movement. Juno, Mina, Silas, Mireya and other recurring characters also need unique visual identities before story release.

Hero vehicles need accurate proportions, believable glass/paint/rubber materials, working lamps, animated wheels, suspension movement and a readable interior for mission-critical vehicles. Distant traffic can use simpler mesh/material tiers; vehicles the player drives cannot.

## Lighting, materials and weather

1. Bake static indirect lighting for city architecture; retain a single principal real-time light for the active sun/moon direction.
2. Use limited, authored real-time shadows near the player. Tune distance and resolution against actual device GPU timings.
3. Create wet-street atmosphere through material roughness variation, selected puddle/decal layers, local light pools, fog depth and sound—not full-scene screen-space reflections.
4. Reserve reflection probes for hero surfaces and interiors where they materially improve the shot. Disable probe blending/box projection on lower-end profiles unless profiling proves acceptable.
5. Use color grading and exposure discipline to establish time-of-day identity. Avoid stacking bloom, SSAO, depth effects and transparent fog merely to make screenshots look expensive.
6. Use weather as an authored lighting/material state: rain changes roughness, droplets, ambience, traffic behaviour and pedestrian reactions. Heavy effects remain profile-gated.

## Mobile rendering budget

The project target is 30 FPS sustained, giving a 33.3 ms frame interval. This is a goal to measure, not an existing result.

- Begin the first device pass around 720p–900p internal resolution, then A/B test render scale on the exact phone. At a 2408×1080 panel, this trades pixels for a meaningful GPU saving; it is not a quality promise.
- Use three LOD levels for characters, vehicles and environment props. Preserve silhouette and material identity at each level.
- Stream one active district cell plus adjacent preload cells. Distant blocks should use simplified geometry/HLOD, not full interior or NPC simulation.
- Keep full character animation and reactions close to the player; reduce update frequency with distance. Far crowds can be pooled, simplified representations.
- Profile NPCs, moving vehicles, shadows and transparent effects independently. Starting caps are hypotheses, not hard limits.
- Prefer SRP Batcher-compatible materials and instancing for repeated meshes where it reduces CPU/GPU work.
- Keep UI at panel resolution while the 3D world uses a tested render scale.
- Compare Vulkan and OpenGL ES 3 only with controlled builds on the same device and thermal state.

Unity's URP 6000.3 guidance identifies CPU work, texture memory, render-texture copies, render passes, draw calls and rendered pixel count as key performance costs. It identifies reduced render scale, fewer shadow passes, fewer additional-light shadows, SRP Batcher and avoiding unnecessary depth/opaque textures as tuning levers. These are candidates to test, not settings to blindly disable.

## Three visual profiles

| Profile | Intended behaviour | Must preserve |
|---|---|---|
| **High** | Highest profiled render scale, close shadow distance, more nearby crowd and vehicle activity | Hero faces, hero vehicles, key animation, material quality and lighting composition |
| **Balanced** | Moderate render scale, fewer shadowed objects, fewer full-AI NPCs and reduced distant traffic | Same hero assets and animation quality; reduce simulation breadth before hero detail |
| **Performance** | Lower render scale, minimal real-time shadowing, simpler far crowd and traffic updates | Mission readability, control response, collision/gameplay logic, hero silhouette and important scene lighting |

Do not solve performance problems by replacing hero characters or mission vehicles with primitives, cutting essential animation or removing story-critical NPCs. Reduce simulation radius and low-visibility cost first. If even the Performance profile cannot sustain the target, simplify the district's authored density and geometry while preserving its identity.

## Acceptance captures and tests

Every district milestone needs repeatable captures from the exact Android device:

- one daylight establishing view;
- one night/rain establishing view;
- one close character conversation with facial and clothing detail;
- one vehicle sequence with body, glass, lights and suspension visible;
- one representative crowded street view;
- one indoor/outdoor transition.

Each capture set must include build hash, device RAM SKU, graphics API, render scale, average/p95/p99 frame time, peak memory and a 20–30 minute thermal-soak result. A screenshot alone does not prove performance. A 30 FPS result at launch does not prove sustained performance.

The exact Vivo T2x RAM variant is not assumed. QA must record whether the phone is 4 GB, 6 GB or 8 GB before setting memory budgets.

## Research sources

- Rockstar Games, [GTA VI: An Extended Look — Now Playing](https://www.rockstargames.com/newswire/article/4k138k8okkk483/grand-theft-auto-vi-an-extended-look-now-playing), 27 August 2026. Rockstar says the extended look was captured entirely from in-game footage on PlayStation 5.
- Vivo India, [T2x 5G specifications](https://www.vivo.com/in/products/param/t2x-5g). Lists the Dimensity 6020, 4/6/8 GB RAM configurations and 2408×1080 display.
- MediaTek, [Dimensity 6020 specifications](https://www.mediatek.com/products/smartphones/mediatek-dimensity-6020). Lists 2× Cortex-A76 up to 2.2 GHz, 6× Cortex-A55 up to 2.0 GHz and Arm Mali-G57 MC2.
- Unity 6.3, [Introduction to performance in URP](https://docs.unity.com/en-us/engine/6000.3/manual/analysis/graphics-performance-profiling/in-urp/understand-performance/understand-performance).
- Unity 6.3, [URP Asset reference](https://docs.unity.com/en-us/engine/6000.3/manual/render-pipelines/universal-render-pipeline/urp-reference/universalrp-asset).
- Unity, [Configure for better performance in URP](https://docs.unity.cn/Manual/urp/configure-for-better-performance.html).
