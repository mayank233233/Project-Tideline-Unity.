# PORT SABLE: NO FAVOURS LEFT — Opening campaign mission design

**Document type:** narrative and gameplay design specification  
**Status:** pre-production design; not yet implemented or playtested  
**Canon:** PORT SABLE: NO FAVOURS LEFT

This document expands the three lead prologues and the first shared campaign mission. It is not a screenplay-only document: every sequence needs a player verb, a reason to move through the space, a character beat, a choice or discovery, and a consequence.

## 1. Design principles taken from research

### Three lives, one story

Rockstar's official GTA V Q&A explains that the three protagonists have separate lives, their own acquaintances and personal missions, with some missions combining two or all three leads. The design lesson is not simply “add a character switch button”: each lead needs a life worth visiting when the main plot is not asking for them. Each PORT SABLE lead therefore gets an independent prologue, personal contacts, home/work hub, authored ambient scenes, and mission types that suit their identity.

Reference: Rockstar Games, [Asked & Answered: GTAV](https://www.rockstargames.com/newswire/article/o349k552517929/asked-answered-gtav.html).

### Authored story anchors inside a larger world

Ubisoft Toronto's GDC discussion on narrative writing in AAA games describes using authored anchor moments to hold open-world stories together and developing character/world storytelling that can survive a less linear play order. PORT SABLE uses strong story anchors at Tideglass, Harbour Exchange, All the Way Down, Last Call and Harbour Lights. Between them, side arcs, district changes and optional meetings deepen character relationships without making the main plot incomprehensible.

Reference: Ubisoft Toronto, [Words to Worlds: Exploring Narrative Writing in AAA Games at GDC 2023](https://toronto.ubisoft.com/words-to-worlds-exploring-narrative-writing-in-aaa-games-at-gdc-2023/).

### Character arcs need visible contradictions

The leads should not be three job specialists who only deliver different mission mechanics. A player's actions should repeatedly expose the gap between what a character says they want and what they actually do:
- Mara says she wants independence, but refuses to delegate.
- Rafi says he values people, but treats relationships as access.
- Elias says he values order, but uses “stability” to excuse his old choices.

The campaign tests these contradictions in gameplay, then lets relationships respond. Do not resolve them in one speech.

## 2. Lead introduction missions

Each prologue is a self-contained 8–12 minute first contact with a lead. They should feel like different evenings in the same city. They share a common event invitation, not a common tutorial structure.

### P1 — TWO MINUTES TO CLOSE (Mara Kade)

**Location:** Kade Motor Works, Old Quay streets, Saltline approach  
**Primary verbs:** workshop interaction, vehicle handling, authored night drive  
**Emotional purpose:** establish Mara's competence, stubbornness, humour and unresolved history with Nico.

**Opening image:** Kade Motor Works is closing. The workshop is lit by practical work lamps while an elevated freight train rattles overhead. Mara is finishing a long-running restoration. Juno is counting the day's receipts and pretending the House's overdue notices are ordinary mail.

**Beat 1 — The shop has people in it.** The player moves through the garage, speaks to Juno, checks the work order and sees that Mara remembers every customer and every unpaid favour. A small optional interaction lets the player inspect a wall of old driving photographs; one has a Tideglass event wristband tucked behind its frame.

**Beat 2 — A customer wants the car now.** A customer insists the restored vehicle be delivered before the final payment clears. Mara suspects the request is not as simple as it sounds. Juno wants the job finished; Mara wants the customer to stop treating the garage like a disposable service.

**Beat 3 — Drive the route, not a tutorial loop.** The delivery route crosses Old Quay's tight market streets and opens onto the Saltline road. The route teaches braking, steering, camera look and vehicle feel through traffic, weather and road surface changes. Optional route choices reveal city character: the faster open road, or a slower route passing the old Tideglass waterfront.

**Beat 4 — Nico appears.** Nico Okoye is already at the meeting point. His friendly competition with Mara is obvious, but he knows more about the customer's interest in the vehicle than he admits. He offers a driving challenge rather than an exposition dump. The player can accept the challenge, decline it, or let Nico test the car.

**Choice:** keep control of the vehicle and protect the shop's professional reputation, or let Nico take the short demonstration and risk giving him an advantage. Neither choice makes Mara “good” or “bad”; each changes the next scene and Nico's attitude.

**Aftermath:** Juno reveals that an old House account still lists the garage as security for a loan. She does not explain Tideglass yet. Mara receives an invitation to Harbour Exchange through a contact who knows the House has been asking about Kade Motor Works.

**Failure/recovery:** a missed delivery window changes the customer's reaction and reduces the immediate reward, but the mission continues. Avoid a generic fail screen for ordinary driving mistakes.

**Character signal:** Mara is excellent under pressure, but she assumes every problem is hers alone.

**Current source implementation slice (not yet scene-playable):** the Juno/work-order conversation is generated as `prologue.mara.two_minutes_to_close`; the three final checks are represented by separate steering, door-latch and engine-rattle interaction stations; the delivery route uses ordered trigger checkpoints bound to the assigned customer vehicle; and a separate `prologue.mara.nico_challenge` graph handles Nico's post-delivery choice. `MaraPrologueMissionBuilder` generates a three-objective MissionDefinition for those stages. The optional Tideglass photo inspection, authored customer NPC conversation, alternate shortcut/reward branch and missed-delivery recovery are not yet implemented. No scene/prefab references have been assigned in the repository.

### P2 — LAST SONG, FIRST LIE (Rafi Sayegh)

**Location:** Lowlight venue and The Velvet Mile  
**Primary verbs:** social navigation, event timing, relationship choices  
**Emotional purpose:** establish Rafi's charm as both a gift and a defence mechanism; show Mina's competence and frustration.

**Opening image:** Lowlight is half-lit before opening. Staff are carrying equipment, sound-checking the room and finishing details Rafi promised would be ready earlier. Rafi moves through the venue like he owns every conversation. Mina quietly fixes the problems he leaves behind.

**Beat 1 — A place with a pulse.** The player crosses the venue, talks to staff and makes a few small event decisions: where a performer waits, which room gets opened first, and how to calm a guest whose invitation is disputed. The space should feel like a real working venue, not a static mission room.

**Beat 2 — Two promises collide.** Rafi has promised the headline slot to two different performers. The player learns this through character conversations and schedule changes, not a single exposition scene. Mina tells him one of the promises has already cost the venue trust.

**Beat 3 — Save the night.** The player moves between the entry, backstage and main floor while the performance schedule changes. The gameplay is about reading people, timing a decision and keeping the event from collapsing—not about pressing a dialogue button at every character.

**Choice:** publicly admit that Rafi double-booked the slot and let the room react, or preserve the event's image by making a private promise to the displaced performer. The first costs face now; the second risks a larger betrayal later.

**Beat 4 — The invitation.** A House representative arrives with a formal invitation to Harbour Exchange. Rafi recognises the name and becomes noticeably less playful. Mina notices the change but does not force an explanation.

**Aftermath:** Mina asks Rafi to stop making commitments on behalf of the staff. The player's earlier choice changes whether she offers help with Harbour Exchange or insists he handle the invitation alone.

**Failure/recovery:** if the player neglects a venue task, a staff member covers it and remembers the lack of support. The event still reaches its closing beat.

**Character signal:** Rafi can make strangers feel seen, but avoids being honest with the people who work closest to him.

### P3 — THE OLD TERMS (Elias Ward)

**Location:** Glass Ward meeting rooms and the public arcade  
**Primary verbs:** negotiation, listening, comparing accounts  
**Emotional purpose:** introduce Elias as intelligent and persuasive, but not morally neutral.

**Opening image:** Elias waits in a quiet meeting room above the financial district. Outside the glass, the old port and its newer towers sit in the same view. He has a folder of settlement notes and a cup of coffee he has not touched.

**Beat 1 — A dispute with human stakes.** Two operators disagree over a debt settlement. Each has a believable account; neither is presented as a cartoon. The player listens, asks follow-up questions and compares details. The scene teaches that Elias's value is understanding what people are not saying.

**Beat 2 — The missing context.** A phrase from one account reminds Elias of Tideglass. The camera holds on his reaction for a beat, but the game does not explain the entire history. The player can ask one extra question or move the meeting forward.

**Beat 3 — A compromise that costs someone.** Elias can secure a quick settlement, protect the smaller operator's position at the cost of House approval, or refuse to endorse either account. The outcome changes a relationship entry and the House's initial confidence in him.

**Beat 4 — Silas's invitation.** Silas Rook calls Elias directly. Their history is evident in how little they need to say. Silas asks him to witness the Harbour Exchange settlement. Elias accepts, but the player sees that he understands more about the coming transfer than he admits.

**Aftermath:** Elias receives a second message containing the same Harbour Exchange date sent to Mara and Rafi. He knows they are being drawn into the same event, though he does not yet know why.

**Failure/recovery:** a poor negotiation outcome does not stop the campaign. It changes which contact trusts Elias and what information is volunteered later.

**Character signal:** Elias knows how to resolve a dispute; he is less willing to face the long-term harm caused by the resolutions he once approved.

## 3. Mission 01 — THE CUT

**Campaign role:** first shared mission, the point where separate lives become a crew story  
**Target playtime:** 18–24 minutes on a first run, excluding optional exploration  
**Primary districts:** The Velvet Mile, harbour approach, Old Quay  
**Playable leads:** Mara, Rafi and Elias  
**Primary verbs:** event navigation, driving under pressure, social coordination, observation and choice  
**Tone:** glamorous public event that gradually reveals its uncomfortable private machinery  
**Mission promise:** one event seen from three lives; the player discovers that each lead has a different relationship to the same lie.

### Before the mission

The three prologues must be completed in any order. Their outcomes are stored:
- `mara_race_choice`
- `rafi_event_choice`
- `elias_settlement_choice`

Those variables change reactions and support dialogue. They do not prevent the main mission from being understood.

### Sequence A — Harbour Exchange arrives (2–3 minutes)

Start with a short authored arrival montage rather than a long cutscene. The player arrives as Mara first. Her vehicle is part of the public presentation outside the venue. The camera passes event guests, hired staff, harbour lighting and a distant working ferry. A familiar song motif from Tideglass appears as a low instrumental cue; it should not quote any existing commercial track.

The player can walk a short route around the display, speak to Juno's contact, and notice that the House's staff are unusually tense. The mission objective is simple: make sure the display is ready and get inside before the opening presentation.

**Character beat:** Mara sees Rafi at the far end of the event floor. They exchange a brief, awkward greeting. Their shared humour is still present, but they are not immediately best friends again.

### Sequence B — The event has two stories (4–5 minutes)

Switch to Rafi for a playable event-management section. He has to keep the public schedule moving while the private Harbour Exchange gathering starts behind closed doors. The player resolves one event disruption, speaks to Mina and notices that an invited performer has been moved without explanation.

An optional conversation with a venue manager reveals that Crownline Response is sponsoring more of the event than it did in previous years. Rafi recognises Mireya Voss's name.

**Character beat:** Mina asks Rafi why he is nervous. He jokes, then changes the subject. The player has seen him do this in the prologue, so the moment feels like a pattern rather than new exposition.

### Sequence C — The old institution prepares to change (3–4 minutes)

Switch to Elias. The player enters the private room with Silas and two House representatives. The conversation is a negotiation scene built around pauses, incomplete statements and competing assumptions. Silas explains that the House is not disappearing; it is “modernising.” Elias recognises the Crownline language beneath the promise.

A brief optional interaction with a framed Tideglass photograph reveals that Elias was present when the original settlement was signed. The player still does not know the full details.

**Character beat:** Silas treats Elias as a trusted son of the institution. Elias accepts the compliment without challenging the way Silas describes the past.

### Sequence D — The handover collapses (3–4 minutes)

The event's public presentation begins. A sudden venue-wide interruption causes the public programme to stop and the private settlement to collapse into confusion. Do not explain how the interruption was caused; its function is dramatic, not an instructional set piece.

The game remains playable. Rafi helps staff and guests move away from the disrupted floor. Mara gets the display area clear and notices a familiar vehicle has been moved. Elias is ordered to remain in the private room but sees a chance to leave and check on the others.

The House's sealed agreement folios are missing too, but they are not the whole mystery. Their absence matters because the House has used those agreements to claim authority over old debts and favours. A rival crew is glimpsed in the confusion, but the player cannot yet tell whether it caused the collapse, exploited it or was framed as well. The player does not receive a convenient full list of every secret in the city.

### Sequence E — Three partial truths (4–5 minutes)

The player moves through three short playable vignettes:
1. **Mara:** a vehicle-related pursuit across the harbour approach, focused on traffic reading and protecting a person who has become stranded during the event disruption. The scene emphasizes driving skill and restraint, not destruction.
2. **Rafi:** return through the venue's service corridors and public floor to account for staff and find Mina. The player must decide whether to help a frightened staff member or pursue a contact who may know what happened to the folios.
3. **Elias:** a tense conversation with Silas, followed by a walk through the public arcade. Elias can challenge Silas about the transfer or leave the disagreement for later. The choice changes their relationship and the next phone call.

These are three different gameplay sequences, not three versions of the same chase. A Cutwater Circle mark or member appears in the background of at least one sequence; this is the first seed of a rival crew, not a full introduction.

### Sequence F — The public accusation (2 minutes)

Outside, a House spokesperson issues a carefully prepared public statement: the missing consignment and payment are blamed on a supposed joint move by Mara, Rafi and Elias. The missing folios are presented as supporting proof, although the player has not seen evidence tying the leads to the event. It is a display of the institution's power to define the accepted story.

Mireya is present but does not gloat. She watches the response and quietly tells Silas that his old arrangements are no longer useful. Their disagreement is visible to the player without turning either character into a monologue villain.

### Sequence G — The garage, the first honest argument (3–4 minutes)

The three leads regroup at Kade Motor Works. This is the first long conversation between them. Rafi tries to joke, Mara refuses to let him redirect the discussion, and Elias admits that he knew the House was preparing a transfer but claims he did not know the public accusation was coming.

Mara asks why Elias never told her about the Tideglass loan. Elias does not yet reveal that he signed the agreement. Rafi recognises Tideglass in the conversation and finally realises his career and Mara's garage were entangled in the same rescue arrangement.

The mission ends not with a triumphant “we are a crew” speech, but with a practical choice: split up and let the House isolate them, or keep the garage open as a temporary meeting place until they know what happened.

### Final mission choice

The player chooses the crew's first priority:
- **Protect the garage:** the next mission begins with Juno and the garage staff; Mara's trust improves slightly, but Rafi feels sidelined.
- **Protect Lowlight's people:** the next mission begins with Rafi and Mina dealing with the event's fallout; Rafi's trust improves, but Mara sees it as another promise made before the garage is safe.
- **Find the missing account:** Elias gets the first conversation scene; the crew learns an additional clue, but both Mara and Rafi resent that he wants to turn the crisis into another negotiation.

This is a priority choice, not a morality score. All three routes return to the main story while changing dialogue, support availability and hub state.

## 4. Early rival-crew introduction — Nika Vale

The Cutwater Circle must be seeded before the first major score and Nika Vale must have at least three distinct story appearances before the finale:
1. **THE CUT:** a partial glimpse of the Circle during the event disruption. The player cannot yet identify whether it was responsible.
2. **DRY DOCK:** Nika appears in person during a waterfront dispute. She argues that the House uses “stability” to keep smaller crews indebted, but her proposed solution would shift costs onto people who never agreed to join her fight.
3. **NO NEUTRAL GROUND:** Nika and the crew meet again after the player has seen the costs of both systems. The player may cooperate narrowly, refuse, or challenge her internal leadership.
4. **THE WRONG SIDE OF THE WATER:** the relationship pays off in a personal decision; Nika's help or opposition reflects the player's earlier conduct.

Nika is Elias's former protégé and had an unfinished friendship with Rafi in Lowlight's early years. She is neither a late-game boss nor an automatic ally. Her argument must be understandable without making every action she takes correct.

## 5. Mission outcome matrix

| Player variable | Immediate difference | Later return |
|---|---|---|
| Mara's delivery choice | Nico's first reaction and whether he offers help | The Long Loop and Southline Circuit |
| Rafi's event choice | Mina's trust and Lowlight staff response | The Guest List and A Friendly Face |
| Elias's settlement choice | Old operator's attitude and House confidence | Old Accounts and The Last Collection |
| The Cut — protect garage | Juno and Mara hub scene first | Family Rate and Last Call |
| The Cut — protect Lowlight | Mina and Rafi hub scene first | A Friendly Face and The Auction |
| The Cut — find the account | Elias receives the first investigation lead | Old Accounts and No Neutral Ground |

No choice should secretly lock the player out of an entire protagonist or ending without warning. The choices change relationships, resources and mission presentation; the campaign continues.

## 6. Character writing requirements

### Dialogue should sound like people who know each other

- Mara uses short practical statements, but has dry humour with Nico and old friends.
- Rafi uses rhythm, social references and jokes when uncomfortable; his serious dialogue is plain.
- Elias speaks carefully and rarely wastes words. He can be warm, but warmth is deliberate rather than sentimental.
- Juno has authority inside the garage; she does not wait for Mara to make every decision.
- Mina speaks as the person who makes Lowlight work, not as Rafi's assistant.
- Silas is persuasive and patient. His most dangerous quality is making sacrifice sound reasonable.
- Mireya is precise and confident but should never sound like a corporate slogan generator.
- Anika is curious and methodical, not omniscient.

### Banter rules

Banter should come from:
1. remembered events that the player has already seen;
2. conflicting priorities in the current mission;
3. small personal rituals in hubs;
4. callbacks that change meaning after a betrayal.

Do not write interchangeable sarcastic lines for every character. A joke that only Mara could make is stronger than a joke anyone could say.

### Relationship memory

Track the relationship as separate axes rather than a single like/dislike score:
- trust;
- respect;
- resentment;
- obligation.

A character can respect Mara's driving and resent her stubbornness. Mina can trust Rafi to deliver an event while resenting his personal promises. An old operator can respect Elias's intelligence but refuse his apology.

## 6. Mission variety contract

Across the opening four missions, do not repeat the same dominant verb:
- TWO MINUTES TO CLOSE: workshop plus driving.
- LAST SONG, FIRST LIE: venue navigation and social timing.
- THE OLD TERMS: negotiation and conflicting accounts.
- THE CUT: multi-lead event disruption, driving, social coordination and relationship choices.

Across the full campaign, every mission pitch must answer:
- What is the player doing for most of the mission?
- What makes this activity specific to this lead or location?
- What changes halfway through?
- What can the player choose?
- What is different in the world afterward?
- Which character relationship does the mission move?
- What is the quiet aftermath beat?

If two missions have the same answers with different locations, redesign one.

## 7. Quality gates

A narrative playtest is not passed because the writer likes the dialogue. A small external tester group should be able to:
- identify each lead by voice and behaviour without looking at the HUD;
- explain the Tideglass connection in their own words;
- describe why the three leads do not trust one another yet;
- remember at least two supporting characters and what each wants;
- describe the difference between the three prologues;
- name a choice in THE CUT and explain what it changed;
- recall one quiet character beat after the action;
- finish the mission without getting lost or waiting through excessive exposition.

Record confusion, boredom, repeated mission verbs, choices that feel cosmetic and characters who sound alike. Revise from that evidence. Do not label the story “10/10” before blind playtests.

## 8. Implementation handoff

The existing Unity project currently has one legacy Ari Solis test actor and a compact Old Quay technical testbed. It does not yet have the bespoke Mara/Rafi/Elias prefabs, the new prologue scenes, the Harbour Exchange event space, the new mission trigger graph or the mobile character-switch UI wired into a scene.

Implementation order:
1. Build and validate the three distinct hero prefabs against `PORT_SABLE_THREE_LEAD_SYSTEM.md`.
2. Create the new Port Sable opening scene with Old Quay as one district, not the whole city.
3. Build Mara's prologue first to validate hero rig, movement animation, vehicle handoff and workshop interactions.
4. Build Rafi's venue prologue to validate interior navigation, crowd staging, event timing and dialogue.
5. Build Elias's negotiation prologue to validate dialogue choices and relationship memory.
6. Implement THE CUT as the first shared mission once the three leads work independently.
7. Record compiled Play Mode and Android tests before calling any sequence playable.

