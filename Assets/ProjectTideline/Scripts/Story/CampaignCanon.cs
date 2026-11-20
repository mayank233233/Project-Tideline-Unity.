using System;
using System.Collections.Generic;

namespace ProjectTideline
{
    /// <summary>
    /// Single source of truth for the PORT SABLE: NO FAVOURS LEFT campaign.
    /// This registry is narrative metadata; it does not claim that missions,
    /// character switching, districts or endings are implemented in gameplay.
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

            public Mission(string id, string title, string leadId, bool isPrologue)
            {
                Id = id;
                Title = title;
                LeadId = leadId;
                IsPrologue = isPrologue;
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
            new Character("nico_okoye", "Nico Okoye", "Southline driver and rival", "Can become ally, rival or recurring complication")
        };

        private static readonly string[] FactionEntries =
        {
            "the_house",
            "the_breakers",
            "the_velvet_circuit",
            "southline_motors",
            "crownline_response"
        };

        private static readonly string[] DistrictEntries =
        {
            "old_quay",
            "southline",
            "the_velvet_mile",
            "breaker_island",
            "crown_heights",
            "saltline",
            "glass_ward"
        };

        private static readonly Mission[] MissionEntries =
        {
            new Mission("two_minutes_to_close", "TWO MINUTES TO CLOSE", "mara_kade", true),
            new Mission("last_song_first_lie", "LAST SONG, FIRST LIE", "rafi_sayegh", true),
            new Mission("the_old_terms", "THE OLD TERMS", "elias_ward", true),

            new Mission("the_cut", "THE CUT", "all", false),
            new Mission("no_safe_house", "NO SAFE HOUSE", "all", false),
            new Mission("running_tab", "RUNNING TAB", "all", false),
            new Mission("the_long_loop", "THE LONG LOOP", "mara_kade", false),
            new Mission("the_guest_list", "THE GUEST LIST", "rafi_sayegh", false),
            new Mission("dry_dock", "DRY DOCK", "all", false),
            new Mission("the_offer", "THE OFFER", "all", false),
            new Mission("a_better_number", "A BETTER NUMBER", "elias_ward", false),
            new Mission("all_the_way_down", "ALL THE WAY DOWN", "all", false),
            new Mission("family_rate", "FAMILY RATE", "mara_kade", false),
            new Mission("old_accounts", "OLD ACCOUNTS", "elias_ward", false),
            new Mission("a_friendly_face", "A FRIENDLY FACE", "rafi_sayegh", false),
            new Mission("no_neutral_ground", "NO NEUTRAL GROUND", "all", false),
            new Mission("the_wrong_side_of_the_water", "THE WRONG SIDE OF THE WATER", "all", false),
            new Mission("last_call", "LAST CALL", "all", false),
            new Mission("the_auction", "THE AUCTION", "all", false),
            new Mission("the_last_collection", "THE LAST COLLECTION", "all", false),
            new Mission("no_clean_exit", "NO CLEAN EXIT", "all", false)
        };

        private static readonly string[] EndingEntries =
        {
            "the_empty_chair",
            "the_new_house",
            "clean_exit"
        };

        public static IReadOnlyList<Character> Leads => Array.AsReadOnly(CharacterEntries);
        public static IReadOnlyList<Character> SupportingCast => Array.AsReadOnly(SupportingCharacterEntries);
        public static IReadOnlyList<string> Factions => Array.AsReadOnly(FactionEntries);
        public static IReadOnlyList<string> Districts => Array.AsReadOnly(DistrictEntries);
        public static IReadOnlyList<Mission> Missions => Array.AsReadOnly(MissionEntries);
        public static IReadOnlyList<string> Endings => Array.AsReadOnly(EndingEntries);

        public static void CollectValidationIssues(List<string> issues)
        {
            if (issues == null) throw new ArgumentNullException(nameof(issues));

            if (CharacterEntries.Length != 3)
                issues.Add("Campaign must define exactly three playable leads.");
            if (SupportingCharacterEntries.Length < 6)
                issues.Add("Campaign needs at least six named supporting characters.");
            if (FactionEntries.Length < 5)
                issues.Add("Campaign must define at least five major factions.");
            if (DistrictEntries.Length < 7)
                issues.Add("Campaign must define at least seven districts.");
            if (EndingEntries.Length != 3)
                issues.Add("Campaign must define exactly three ending routes.");

            var characterIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (Character character in CharacterEntries)
            {
                if (character == null || string.IsNullOrWhiteSpace(character.Id) || string.IsNullOrWhiteSpace(character.DisplayName))
                {
                    issues.Add("A playable lead has a missing ID or display name.");
                    continue;
                }
                if (!characterIds.Add(character.Id))
                    issues.Add("Duplicate playable lead ID: " + character.Id);
            }

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
                if (!missionIds.Add(mission.Id))
                    issues.Add("Duplicate mission ID: " + mission.Id);

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

            if (prologueCount != 3)
                issues.Add("Campaign must define three independent lead prologues; found " + prologueCount + ".");
            if (coreCount != 18)
                issues.Add("Campaign must define eighteen core missions; found " + coreCount + ".");
            if (!missionIds.Contains(OpeningMissionId))
                issues.Add("Opening mission ID is not present: " + OpeningMissionId);

            foreach (Character lead in CharacterEntries)
            {
                int count;
                leadMissionCounts.TryGetValue(lead.Id, out count);
                if (count < 3)
                    issues.Add("Lead " + lead.DisplayName + " needs at least three lead-specific missions; found " + count + ".");
            }

            var factionIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string faction in FactionEntries)
                if (string.IsNullOrWhiteSpace(faction) || !factionIds.Add(faction))
                    issues.Add("Faction registry contains a blank or duplicate ID.");

            var districtIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string district in DistrictEntries)
                if (string.IsNullOrWhiteSpace(district) || !districtIds.Add(district))
                    issues.Add("District registry contains a blank or duplicate ID.");

            var endingIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string ending in EndingEntries)
                if (string.IsNullOrWhiteSpace(ending) || !endingIds.Add(ending))
                    issues.Add("Ending registry contains a blank or duplicate ID.");
        }
    }
}