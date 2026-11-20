using System;
using System.Collections.Generic;

namespace ProjectTideline
{
    /// <summary>
    /// Single source of truth for PORT SABLE: NO FAVOURS LEFT.
    /// Narrative metadata only: this does not claim that missions, switching,
    /// districts or endings are implemented in gameplay.
    /// </summary>
    public static class CampaignCanon
    {
        public const string CampaignTitle = "PORT SABLE: NO FAVOURS LEFT";
        public const string CityName = "Port Sable";
        public const string OpeningMissionId = "the_cut";

        public sealed class Character
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly string Role;
            public readonly string GameplayIdentity;

            public Character(string id, string displayName, string role, string gameplayIdentity)
            {
                Id = id;
                DisplayName = displayName;
                Role = role;
                GameplayIdentity = gameplayIdentity;
            }
        }

        public sealed class Mission
        {
            public readonly string Id;
            public readonly string Title;
            public readonly string LeadId;
            public readonly bool IsPrologue;
            public readonly string DistrictId;
            public readonly string PrimaryVerb;
            public readonly string PlayerGoal;
            public readonly string CharacterComplication;
            public readonly string PlayerChoice;
            public readonly string PersistentConsequence;

            public Mission(string id, string title, string leadId, bool isPrologue,
                string districtId, string primaryVerb, string playerGoal,
                string characterComplication, string playerChoice, string persistentConsequence)
            {
                Id = id;
                Title = title;
                LeadId = leadId;
                IsPrologue = isPrologue;
                DistrictId = districtId;
                PrimaryVerb = primaryVerb;
                PlayerGoal = playerGoal;
                CharacterComplication = characterComplication;
                PlayerChoice = playerChoice;
                PersistentConsequence = persistentConsequence;
            }
        }

        private static readonly Character[] CharacterEntries =
        {
            new Character("mara_kade", "Mara Kade", "Driver and mechanic; co-owner of Kade Motor Works", "Driving, vehicle handling, workshop interaction and road set pieces"),
            new Character("rafi_sayegh", "Rafi Sayegh", "Nightlife promoter and Lowlight venue operator", "Social navigation, event access, negotiation and relationship-led missions"),
            new Character("elias_ward", "Elias Ward", "Former House mediator and strategist", "Planning choices, faction relationships and interpreting conflicting accounts")
        };

        private static readonly Character[] SupportingCharacterEntries =
        {
            new Character("silas_rook", "Silas Rook", "Chairman of the House", "Institutional antagonist; believes his order prevents wider chaos"),
            new Character("mireya_voss", "Mireya Voss", "Crownline Response executive", "Corporate antagonist with a different vision from Silas"),
            new Character("juno_kade", "Juno Kade", "Kade Motor Works co-owner", "Mara's aunt; made the Tideglass rescue agreement"),
            new Character("mina_sato", "Mina Sato", "Lowlight production manager", "Rafi's most important working relationship"),
            new Character("bash_rahal", "Bash Rahal", "Harbour mechanic and skipper", "Breaker Island contact with personal boundaries"),
            new Character("anika_bose", "Anika Bose", "Investigator", "Persistent pressure and a grounded counterpoint to the crew"),
            new Character("nico_okoye", "Nico Okoye", "Southline driver and rival", "Can become ally, rival or recurring complication"),
            new Character("nika_vale", "Nika Vale", "Leader of the Cutwater Circle; former Elias protégé", "Recurring rival/ally whose breakaway crew challenges the House")
        };

        private static readonly string[] FactionEntries =
        {
            "the_house", "the_cutwater_circle", "the_breakers", "the_velvet_circuit", "southline_motors", "crownline_response"
        };

        private static readonly string[] DistrictEntries =
        {
            "old_quay", "southline", "the_velvet_mile", "breaker_island", "crown_heights", "saltline", "glass_ward"
        };

        private static readonly Mission[] MissionEntries =
        {
            // Three independent introductions.
            new Mission("two_minutes_to_close", "TWO MINUTES TO CLOSE", "mara_kade", true,
                "old_quay", "Workshop interaction and precision driving",
                "Resolve a disputed customer vehicle claim and protect the garage's reputation during a short night drive.",
                "Nico knows the claim is false but has his own reason not to expose the customer.",
                "Protect the garage's reputation or accept an easy payout that harms an old relationship.",
                "Changes the Southline driving circle's opening attitude and the first garage contact."),
            new Mission("last_song_first_lie", "LAST SONG, FIRST LIE", "rafi_sayegh", true,
                "the_velvet_mile", "Social navigation and live-event management",
                "Keep Lowlight's comeback night alive while resolving a booking dispute and protecting exhausted staff.",
                "Mina knows Rafi promised the same headline slot to two different performers.",
                "Honor the original promise or protect the event and make a new promise in public.",
                "Changes Mina's trust and which Velvet Circuit contact will answer Rafi's calls."),
            new Mission("the_old_terms", "THE OLD TERMS", "elias_ward", true,
                "glass_ward", "Negotiation and conflicting-account analysis",
                "Mediate a debt dispute and identify why the House is preparing an institutional transition.",
                "A familiar name exposes Elias's past role in the Tideglass agreement.",
                "Protect a smaller operator's position or preserve the House's confidence in Elias.",
                "Sets an old House contact's trust and the crew's knowledge of Harbour Exchange."),

            // Act I — The Entry Fee.
            new Mission("the_cut", "THE CUT", "all", false,
                "the_velvet_mile", "Three-perspective playable set piece",
                "Survive the staged collapse of Harbour Exchange as a high-value physical consignment and its payment disappear, then discover why the House publicly blames the three leads.",
                "Each lead sees only one part of the staged failure; a Cutwater Circle member appears in the confusion, while Elias withholds how close he was to Silas's decision.",
                "Save a contact's reputation or protect the first concrete opportunity to build an independent crew.",
                "Determines which contact reaches the garage first and establishes initial crew cohesion."),
            new Mission("no_safe_house", "NO SAFE HOUSE", "all", false,
                "old_quay", "Hub setup and relationship interaction",
                "Turn Kade Motor Works into a temporary crew base without pretending the leads already trust one another.",
                "Mara wants privacy, Rafi wants access for his people and Elias wants a secure meeting space.",
                "Spend the first scarce resources on the garage, Lowlight's staff or Elias's old contacts.",
                "Changes the first hub upgrade and which support contact is immediately available."),
            new Mission("running_tab", "RUNNING TAB", "all", false,
                "southline", "Compact open-world job and negotiation",
                "Complete a small independent job after Harbour Exchange and learn that the Cutwater Circle is offering work outside the House.",
                "The local operator cannot pay cash without breaking a promise to someone else; a Cutwater contact offers an alternative with a hidden relationship cost.",
                "Take the favour, accept reduced payment or walk away and lose the contact.",
                "Introduces the debt ledger and opens or closes a Southline Motors contact."),
            new Mission("the_long_loop", "THE LONG LOOP", "mara_kade", false,
                "saltline", "Authored route race and vehicle mastery",
                "Compete against Nico in a night-driving event that tests route reading, vehicle control and nerve.",
                "Nico's car suffers a failure at the worst moment and the crowd expects Mara to exploit it.",
                "Take the win, protect the rival or expose a personal weakness to humiliate him.",
                "Nico becomes an ally, rival or unreliable contact; event commentary changes."),

            // Act II — Build a Name.
            new Mission("the_guest_list", "THE GUEST LIST", "rafi_sayegh", false,
                "the_velvet_mile", "Live social navigation and event timing",
                "Move through a high-status event, manage competing promises and learn what Crownline is negotiating with the House.",
                "A guest Rafi vouched for is being quietly excluded by the event's decision-makers.",
                "Protect the guest, protect the event's backers or reveal the arrangement to the room.",
                "Unlocks one social contact while making another colder; the Lowlight guest list changes."),
            new Mission("dry_dock", "DRY DOCK", "all", false,
                "breaker_island", "Harbour traversal and multi-party negotiation",
                "Resolve a waterfront property dispute while Nika Vale and the Cutwater Circle try to recruit the same independent operators.",
                "Bash has a personal stake and refuses any plan that treats his crew as expendable; Nika argues that the dispute proves the House's rules must be broken.",
                "Back independent operators, accept a compromise or support the faction with the strongest claim.",
                "Changes Breaker Island access, Bash's trust and Breaker presence in the hub."),
            new Mission("the_offer", "THE OFFER", "all", false,
                "crown_heights", "Three-perspective negotiation and choice",
                "Hear Silas's separate promises as Nika's breakaway challenge grows, then decide whether the crew bargains with the old order, the rival crews or neither.",
                "Silas offers Mara security for the garage, Rafi access to events and Elias a route back to influence.",
                "Accept, feign acceptance or reject the offer openly.",
                "Sets Silas's attitude and which faction makes the next approach."),
            new Mission("a_better_number", "A BETTER NUMBER", "elias_ward", false,
                "old_quay", "Crew selection and score preparation",
                "Choose a crew for a major score and understand each recruit's motive, strengths and boundaries.",
                "A less experienced recruit needs the opportunity; a veteran expects special treatment.",
                "Choose reliability, personal loyalty or a specialist whose risk may pay off.",
                "The recruit gains or loses confidence, changing later support scenes and hub activity."),
            new Mission("all_the_way_down", "ALL THE WAY DOWN", "all", false,
                "the_velvet_mile", "Three-lead auction score, event navigation, authored vehicle set piece and relationship choice",
                "Decide what happens to TIDEGLASS ONE, the custom car that headlined the waterfront festival eight years earlier and is now the centrepiece of a Crown Heights auction.",
                "A support contact acts on a promise made during preparation; the complication exposes how the House and Crownline are rewriting the car's history and forces the three leads to confront their different Tideglass memories.",
                "Return the car to Kade Motor Works, give it to the Cutwater Circle as a public symbol, or expose the auction's arrangement and accept a smaller immediate reward.",
                "Changes Juno's garage hub, Nika/Cutwater influence or the House/Crownline public standing, and sets relationship conditions for Family Rate, The Offer and The Auction."),
            new Mission("family_rate", "FAMILY RATE", "mara_kade", false,
                "old_quay", "Environmental storytelling and character-led exploration",
                "Understand how the Tideglass rescue arrangement tied the Kade garage to the House.",
                "Juno admits she accepted the loan to save staff and hid the collateral terms from Mara.",
                "Let Juno explain, challenge her or ask Elias to account for his part in the settlement.",
                "Changes Juno's hub role and Mara's available final-act support."),

            // Act III — The Price of Power.
            new Mission("old_accounts", "OLD ACCOUNTS", "elias_ward", false,
                "southline", "Dialogue with persistent memory and return-to-place exploration",
                "Face the people harmed by Elias's old settlement and decide what repair can realistically mean.",
                "The old crew remembers the outcome differently and does not accept Elias's explanation as an apology.",
                "Listen, challenge their account or leave without asking for forgiveness.",
                "Determines whether the old crew considers helping in the finale."),
            new Mission("a_friendly_face", "A FRIENDLY FACE", "rafi_sayegh", false,
                "the_velvet_mile", "Social crisis management and public event navigation",
                "Respond after Rafi's private Crownline deal becomes public and Lowlight's staff feel betrayed.",
                "Mina has arranged a solution without Rafi and asks him not to make another empty promise.",
                "Own the decision, blame Elias's advice or make a specific repair commitment.",
                "Changes Lowlight's future, Mina's trust and Rafi's access to the Velvet Circuit."),
            new Mission("no_neutral_ground", "NO NEUTRAL GROUND", "all", false,
                "southline", "Parallel conversations and faction-state decision",
                "Mediate a dispute that Silas allowed to grow while the Cutwater Circle presses for a different outcome; choose which conflict the crew can live with.",
                "Every faction presents a defensible account; Nika's argument is persuasive, but her preferred outcome would impose costs on people outside her crew.",
                "Choose one side, negotiate a narrow compromise or refuse to mediate.",
                "Updates faction standing and visible authored activity in affected districts."),
            new Mission("the_wrong_side_of_the_water", "THE WRONG SIDE OF THE WATER", "all", false,
                "breaker_island", "Cinematic pursuit and relationship decision",
                "Protect a familiar contact whose loyalties are being pulled between the crew, the Cutwater Circle and the old waterfront network.",
                "The contact refuses to be treated as property by either the crew or their old allies.",
                "Protect their independence, ask them to commit publicly or accept that they leave.",
                "The contact joins, leaves or becomes a recurring complication in later support scenes."),
            new Mission("last_call", "LAST CALL", "all", false,
                "old_quay", "Playable hub drama and mission-preparation choice",
                "Resolve the crew's internal fracture before the final act begins.",
                "Mara wants the garage safe, Rafi wants Lowlight protected and Elias wants to settle the House on his terms.",
                "Choose what the crew promises to protect first, knowing one priority must wait.",
                "Determines which allies remain available and which hub spaces open in the final act."),

            // Act IV — The Empty Chair.
            new Mission("the_auction", "THE AUCTION", "all", false,
                "glass_ward", "Three-lead parallel set piece and event navigation",
                "Disrupt the public-facing House/Crownline deal during Harbour Lights and expose their incompatible agendas.",
                "An optional objective can protect one ally only by sacrificing another contact's trust.",
                "Protect the crew's strongest ally or secure a broader advantage for the final confrontation.",
                "Sets the faction controlling the finale's opening conditions and which allies appear."),
            new Mission("the_last_collection", "THE LAST COLLECTION", "all", false,
                "breaker_island", "Branching multi-lead score with crew support",
                "Make the final move against the House using relationships and district access earned earlier.",
                "The plan changes according to who helps, which lead is trusted and what the crew promised.",
                "Choose what to preserve, what to trade and which ally's request takes priority.",
                "Locks the finale state and determines the supporting cast present in the epilogue."),
            new Mission("no_clean_exit", "NO CLEAN EXIT", "all", false,
                "old_quay", "Final multi-lead confrontation and authored epilogue",
                "Decide what happens to the Empty Chair and show consequences in the garage, Lowlight and city districts.",
                "The three leads want different things and no outcome preserves every relationship or institution.",
                "Break the House's monopoly, take control of it or accept Crownline's clean exit.",
                "Resolves as THE EMPTY CHAIR, THE NEW HOUSE or CLEAN EXIT with distinct city and character epilogues.")
        };

        // Institutions are not characters or criminal factions; keep law pressure in its own registry.
        private static readonly string[] InstitutionEntries =
        {
            "port_sable_major_crimes"
        };

        private static readonly string[] EndingEntries =
        {
            "the_empty_chair", "the_new_house", "clean_exit"
        };

        public static IReadOnlyList<Character> Leads => Array.AsReadOnly(CharacterEntries);
        public static IReadOnlyList<Character> SupportingCast => Array.AsReadOnly(SupportingCharacterEntries);
        public static IReadOnlyList<string> Factions => Array.AsReadOnly(FactionEntries);
        public static IReadOnlyList<string> Districts => Array.AsReadOnly(DistrictEntries);
        public static IReadOnlyList<Mission> Missions => Array.AsReadOnly(MissionEntries);
        public static IReadOnlyList<string> Endings => Array.AsReadOnly(EndingEntries);
        public static IReadOnlyList<string> Institutions => Array.AsReadOnly(InstitutionEntries);

        public static void CollectValidationIssues(List<string> issues)
        {
            if (issues == null) throw new ArgumentNullException(nameof(issues));

            if (CharacterEntries.Length != 3) issues.Add("Campaign must define exactly three playable leads.");
            if (SupportingCharacterEntries.Length < 6) issues.Add("Campaign needs at least six named supporting characters.");
            if (FactionEntries.Length < 6) issues.Add("Campaign must define at least six major factions.");
            if (DistrictEntries.Length < 7) issues.Add("Campaign must define at least seven districts.");
            if (EndingEntries.Length != 3) issues.Add("Campaign must define exactly three ending routes.");
            if (InstitutionEntries.Length < 1) issues.Add("Campaign must define its investigative/law-pressure institution separately from character entries.");

            var characterIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (Character character in CharacterEntries)
            {
                if (character == null || string.IsNullOrWhiteSpace(character.Id) || string.IsNullOrWhiteSpace(character.DisplayName))
                {
                    issues.Add("A playable lead has a missing ID or display name.");
                    continue;
                }
                if (!characterIds.Add(character.Id)) issues.Add("Duplicate playable lead ID: " + character.Id);
            }

            var supportingIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (Character character in SupportingCharacterEntries)
            {
                if (character == null || string.IsNullOrWhiteSpace(character.Id) || string.IsNullOrWhiteSpace(character.DisplayName))
                {
                    issues.Add("A supporting character has a missing ID or display name.");
                    continue;
                }
                if (characterIds.Contains(character.Id) || !supportingIds.Add(character.Id))
                    issues.Add("Duplicate character ID across leads/supporting cast: " + character.Id);
            }

            var institutionIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string institution in InstitutionEntries)
                if (string.IsNullOrWhiteSpace(institution) || !institutionIds.Add(institution))
                    issues.Add("Institution registry contains a blank or duplicate ID.");

            var districtIds = new HashSet<string>(DistrictEntries, StringComparer.Ordinal);
            var missionIds = new HashSet<string>(StringComparer.Ordinal);
            int prologueCount = 0;
            int coreCount = 0;
            var leadMissionCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (Mission mission in MissionEntries)
            {
                if (mission == null || string.IsNullOrWhiteSpace(mission.Id) || string.IsNullOrWhiteSpace(mission.Title))
                {
                    issues.Add("A campaign mission has a missing ID or title.");
                    continue;
                }
                if (!missionIds.Add(mission.Id)) issues.Add("Duplicate mission ID: " + mission.Id);
                if (string.IsNullOrWhiteSpace(mission.DistrictId) || !districtIds.Contains(mission.DistrictId))
                    issues.Add("Mission " + mission.Id + " references an unknown district.");
                if (string.IsNullOrWhiteSpace(mission.PrimaryVerb) || string.IsNullOrWhiteSpace(mission.PlayerGoal) ||
                    string.IsNullOrWhiteSpace(mission.CharacterComplication) || string.IsNullOrWhiteSpace(mission.PlayerChoice) ||
                    string.IsNullOrWhiteSpace(mission.PersistentConsequence))
                    issues.Add("Mission " + mission.Id + " is missing a gameplay verb, goal, complication, choice or consequence.");

                if (mission.IsPrologue) prologueCount++;
                else coreCount++;

                if (mission.LeadId == "all") continue;
                if (!characterIds.Contains(mission.LeadId))
                    issues.Add("Mission " + mission.Id + " references unknown lead " + mission.LeadId + ".");
                else
                {
                    int count;
                    leadMissionCounts.TryGetValue(mission.LeadId, out count);
                    leadMissionCounts[mission.LeadId] = count + 1;
                }
            }

            if (prologueCount != 3) issues.Add("Campaign must define three independent lead prologues; found " + prologueCount + ".");
            if (coreCount != 18) issues.Add("Campaign must define eighteen core missions; found " + coreCount + ".");
            if (!missionIds.Contains(OpeningMissionId)) issues.Add("Opening mission ID is not present: " + OpeningMissionId);

            foreach (Character lead in CharacterEntries)
            {
                int count;
                leadMissionCounts.TryGetValue(lead.Id, out count);
                if (count < 3) issues.Add("Lead " + lead.DisplayName + " needs at least three lead-specific missions; found " + count + ".");
            }

            var factionIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string faction in FactionEntries)
                if (string.IsNullOrWhiteSpace(faction) || !factionIds.Add(faction))
                    issues.Add("Faction registry contains a blank or duplicate ID.");

            var endingIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string ending in EndingEntries)
                if (string.IsNullOrWhiteSpace(ending) || !endingIds.Add(ending))
                    issues.Add("Ending registry contains a blank or duplicate ID.");
        }
    }
}