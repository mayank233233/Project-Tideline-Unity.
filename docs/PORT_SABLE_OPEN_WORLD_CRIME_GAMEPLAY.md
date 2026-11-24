# PORT SABLE — open-world crime gameplay pillars

## Genre contract

PORT SABLE is an original modern open-world crime-action game in the same broad genre as Grand Theft Auto V / Grand Theft Auto VI. The reference is the genre's player fantasy and systemic structure: a living city, free-roam exploration, vehicles, criminal missions, police pressure, rival groups, side activities, character switching and a cinematic multi-protagonist campaign.

This is not a peaceful delivery simulator, a linear driving game or a generic low-poly walking prototype. The delivery sequence is a small early-game tutorial that introduces Mara, the city, vehicle handling and a customer relationship. It must not become the dominant mission pattern.

The game must use original characters, locations, writing, mission scenarios, UI identity, art and audio. "Same genre and mechanics" means equivalent categories of player interaction, not copying Rockstar's protected expression or exact mission content.

## Content tone and boundaries

The intended tone is a mature crime drama, with serious subject matter, tense confrontations, high-stakes jobs, consequential choices and intense action. Keep violence non-graphic in its presentation: do not depict gore or gruesome injury detail. Nudity is not part of PORT SABLE: do not include nude characters or nudity scenes. These are creative content boundaries, not a claim that an official age classification has been awarded.

## Core player loop

1. Explore Port Sable on foot or in a vehicle; discover story missions, side jobs, activities, shops, contacts and emergent city events.
2. Meet a contact or encounter a story opportunity; choose a lead, crew and approach where the mission supports it.
3. Complete a fictional criminal job, score, rivalry, escape or character-driven objective using the game's own interaction and vehicle systems.
4. Let the world react: public attention, police pressure, faction standing, witness attitudes, property access and character relationships can change.
5. Return to a safehouse, garage, venue or other hub; spend rewards on vehicles, properties, equipment cosmetics and story progression.
6. Continue the campaign through three protagonists whose separate lives and decisions intersect.

## Required gameplay pillars

### 1. Free-roam city

Seven planned districts must feel connected and distinct: Old Quay, Southline, the Velvet Mile, Breaker Island, Crown Heights, Saltline and Glass Ward. The city needs roads, alleys, pedestrian routes, public spaces, interiors, landmarks, shops, garages, venues, safe locations and traversal links. District streaming and population simulation must be designed for mobile memory and frame-time budgets.

### 2. Vehicles and driving

Vehicles are a primary traversal and mission system, not a one-off tutorial prop. The intended breadth includes traffic vehicles, player-owned vehicles, mission vehicles, vehicle acquisition, garages, customisation, damage presentation, vehicle classes, handling differences, pursuits and authored driving activities. Vehicle interactions must support both keyboard/controller development and mobile touch controls.

### 3. Criminal jobs and heists

The campaign and side content need a range of fictional job structures: setup and preparation missions, contact-based jobs, crew selection, multi-stage scores, timed objectives, branching complications, getaway sequences, rival interference and aftermath. The design must focus on game-state, character stakes and cinematic outcomes, not real-world criminal procedures.

A job should have a distinct premise, location, cast, player objective, gameplay variation, meaningful complication and persistent consequence. Avoid reskinning the same delivery/checkpoint loop as every mission.

### 4. Police pressure and public attention

The world should react to disruptive player actions through a readable, game-facing pressure model. Escalation can change pursuit intensity, available mission opportunities, NPC reactions and narrative attention. It should be fair, legible and tuned for fun. It is a fictional game system, not a real-world law-enforcement or evasion simulator.

`PublicAttentionState` tracks attention driven by vehicle-takeover, collision and confrontation signals. The Old Quay scene builder wires an event-driven `PolicePressureHUD` and a `PoliceResponseDirector`: Noticed spawns one generated patrol vehicle; Escalated adds a second. `PolicePursuitVehicle` uses bounded arcade steering, acceleration and braking to follow the player actor. When attention stops increasing, a configurable quiet period begins and attention gradually decays; once it returns to Calm, the director keeps units active for a short grace period before clearing them. Patrol lightbars alternate using material property blocks, and generated cars include simple body/cabin/wheel visuals. The Editor builder now chains a four-stage first score after Mara's eight-stage delivery tutorial: accept a job from a Saltline contact, recover a marked case, re-enter the designated vehicle, and reach a safehouse arrival zone. Case recovery raises attention; the safehouse event resolves it. The vehicle seat has a separate mission gate so it only becomes available for the score's getaway objective. This gives the Old Quay slice its first actual crime-job-to-police-response loop, rather than another delivery route.

This remains a prototype, not a complete wanted system: there is no dispatch network, patrol search state, arrest/custody loop, radio/audio layer, coordinated road-block behaviour or production-quality pursuit handling. The generated units, mission transition and response loop still require Unity compilation and Play Mode/device testing.

### 5. Three playable protagonists

Mara Kade, Rafi Sayegh and Elias Ward must be distinct playable people with their own homes/hubs, relationships, skills, mission chains, dialogue and world access. Character switching should be available in appropriate free-roam states and restricted during authored mission beats when required. Story missions can bring the leads together, split them apart and let the player's decisions alter their relationships.

### 6. Criminal factions and city relationships

The House, Cutwater Circle, Breakers, Velvet Circuit, Southline Motors and Crownline Response should have distinct motives, characters, territory interests and reactions. Reputation is not a single universal good/bad meter. Relationships with a faction can open content while creating opposition elsewhere.

### 7. Side activities and ownership

The city needs optional activities that are fun independently of the main plot: driving events, vehicle restoration, venue/nightlife management, character encounters, local challenges, property hubs and faction storylines. Garages, venues and safehouses should have a gameplay function, not exist only as scenery.

### 8. Narrative and consequence

The 18 core missions and three endings should be delivered through executable mission assets and tested scene wiring—not only outline documents. Choices should affect relationships, access, faction alignments, later dialogue, mission variants and ending conditions. The Harbour Exchange incident and Tideglass history remain the central campaign spine.

## Visual and presentation target

The visual target is premium authored mobile presentation: recognisable lead characters, distinctive vehicle silhouettes, dense but readable street composition, strong day/night atmosphere, coherent district materials, convincing animation and polished UI. Preserve hero quality while scaling distant crowds, traffic, shadows and render resolution according to measured device performance.

Do not claim GTA V/VI visual parity or equivalent world scale on the target Android phone. The benchmark is a coherent, high-quality original game that sustains performance on the actual device.

## Vertical-slice acceptance target

The first complete crime-game vertical slice should prove more than the current delivery tutorial:

- A connected, walkable/drivable Old Quay neighbourhood with at least one interior and one hub.
- One bespoke playable lead with production-intent character art and animation.
- Traffic and ambient pedestrians with basic world reactions.
- A free-roam-to-mission start and return loop.
- One original multi-stage criminal job with a meaningful choice and a distinct driving or pursuit sequence.
- A readable, fictional police-pressure response with clear escalation and resolution.
- A side activity that is not another main-mission checkpoint route.
- A persistent consequence visible in a later conversation or world state.
- Save/load recovery, mobile controls, Android build and measured device performance.

The first slice can use one lead and a limited district. It must establish the actual crime-action genre loop before expanding content breadth.

## Current implementation truth

The repository has substantial narrative design documents, mission/relationship infrastructure, vehicle prototypes, route checkpoints, an eight-stage Old Quay delivery tutorial, a small public-attention state and source for three-lead switching. These are not equivalent to a finished open-world crime game.

Not yet demonstrated in a complete playable scene: a full city, free-roam mission discovery, production-ready three-lead characters, complete police/wanted response, robust traffic/pedestrian simulation, reusable criminal-job/heist framework, property/safehouse loop, faction reputation gameplay, the full executable campaign, or Android performance.

Static repository checks do not prove Unity compilation, Play Mode behaviour, APK launch or device performance. Every pillar must be implemented, wired into a real scene and tested before it is described as playable.
