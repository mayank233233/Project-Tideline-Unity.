#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectTideline.Editor
{
    /// <summary>
    /// Creates the authored dialogue graphs for PORT SABLE's three independent prologues.
    /// Graphs use the existing DialogueController, MissionSignalBus and NPCRelationshipLedger.
    /// </summary>
    public static class PortSablePrologueDialogueBuilder
    {
        private const string Root = "Assets/ProjectTideline/Content/Dialogue/Prologues";

        [MenuItem("Project Tideline/9 — Generate PORT SABLE Prologue Dialogue", priority = 9)]
        public static void Generate()
        {
            EnsureFolder("Assets/ProjectTideline/Content");
            EnsureFolder("Assets/ProjectTideline/Content/Dialogue");
            EnsureFolder(Root);

            var graphs = new List<DialogueGraph>
            {
                BuildMara(),
                BuildRafi(),
                BuildElias()
            };

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (DialogueGraph graph in graphs)
                {
                    string path = Root + "/Dialogue_" + graph.graphId + ".asset";
                    DialogueGraph existing = AssetDatabase.LoadAssetAtPath<DialogueGraph>(path);
                    if (existing == null)
                    {
                        AssetDatabase.CreateAsset(graph, path);
                    }
                    else
                    {
                        existing.graphId = graph.graphId;
                        existing.startNodeId = graph.startNodeId;
                        existing.nodes = graph.nodes;
                        EditorUtility.SetDirty(existing);
                        UnityEngine.Object.DestroyImmediate(graph);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated or updated three PORT SABLE prologue DialogueGraph assets. " +
                      "They are narrative assets only; they are not yet assigned to scene NPCs or mission triggers.");
            EditorUtility.DisplayDialog("PORT SABLE dialogue generated",
                "Three prologue dialogue graphs were created or updated:\n" +
                "• Mara — TWO MINUTES TO CLOSE\n" +
                "• Rafi — LAST SONG, FIRST LIE\n" +
                "• Elias — THE OLD TERMS\n\n" +
                "They are not yet wired into a playable PORT SABLE scene.", "OK");
        }

        private static DialogueGraph BuildMara()
        {
            DialogueGraph graph = ScriptableObject.CreateInstance<DialogueGraph>();
            graph.graphId = "prologue.mara.two_minutes_to_close";
            graph.startNodeId = "garage_close";
            graph.nodes = new[]
            {
                Node("garage_close", "JUNO",
                    "You said you'd be done at eight. It's quarter past. I'm not asking for an apology; I'm asking if the customer knows.",
                    Choice("The customer's car comes first.", "finish_customer_job",
                        "prologue.mara.finish_customer_job", "juno_kade", 2),
                    Choice("What is that notice?", "ask_about_notice",
                        "prologue.mara.ask_about_notice", "juno_kade", -1)),
                Node("finish_customer_job", "MARA",
                    "The steering's straight, the latch holds, and the rattle's gone. He'll find something else to complain about.",
                    Choice("Take the keys and go.", "nico_at_door",
                        "prologue.mara.prioritise_work", "juno_kade", 1)),
                Node("ask_about_notice", "JUNO",
                    "It's an old account. Same name, new paper. I was going to tell you when I had something better than a number and a deadline.",
                    Choice("You should have told me.", "nico_at_door",
                        "prologue.mara.challenge_juno", "juno_kade", -2),
                    Choice("Then we talk after the drive.", "nico_at_door",
                        "prologue.mara.defer_argument", "juno_kade", 1)),
                Node("nico_at_door", "NICO",
                    "That the car? Thought you said you'd fixed the pull. Or are you still blaming the road?",
                    Choice("There is one way to find out.", "mara_close",
                        "prologue.mara.accept_challenge", "nico_okoye", 2, "mara_race_choice", 1),
                    Choice("The customer paid for a delivery, not a race.", "mara_close",
                        "prologue.mara.decline_challenge", "nico_okoye", 1, "mara_race_choice", 2)),
                Node("mara_close", "MARA",
                    "Keys, invoice, lights. In that order. We can argue about the notice when the shop's empty.",
                    Choice("Close the shop and leave.", string.Empty,
                        "prologue.mara.complete", "juno_kade", 1))
            };
            return graph;
        }

        private static DialogueGraph BuildRafi()
        {
            DialogueGraph graph = ScriptableObject.CreateInstance<DialogueGraph>();
            graph.graphId = "prologue.rafi.last_song_first_lie";
            graph.startNodeId = "lowlight_soundcheck";
            graph.nodes = new[]
            {
                Node("lowlight_soundcheck", "MINA",
                    "You put two names on one slot. Both of them are here. One of them is going to leave angry. Which call are you making first?",
                    Choice("Keep the first promise.", "honour_first_slot",
                        "prologue.rafi.honour_first_slot", "mina_sato", 3, "rafi_event_choice", 1),
                    Choice("Keep the headline event intact.", "protect_headline",
                        "prologue.rafi.protect_headline", "mina_sato", -2, "rafi_event_choice", 2)),
                Node("honour_first_slot", "RAFI",
                    "The first promise stands. I said they had the slot. I'll tell them myself; you keep the room moving.",
                    Choice("Tell the performers yourself.", "mina_reply",
                        "prologue.rafi.own_promise", "mina_sato", 2, "rafi_staff_responsibility", 1)),
                Node("protect_headline", "RAFI",
                    "The headliner stays. That's the decision. The question is who takes the call.",
                    Choice("I'll take it.", "mina_reply",
                        "prologue.rafi.take_responsibility", "mina_sato", 1, "rafi_staff_responsibility", 1),
                    Choice("Mina, tell them.", "mina_reply",
                        "prologue.rafi.shift_burden", "mina_sato", -3, "rafi_staff_responsibility", 2)),
                Node("mina_reply", "MINA",
                    "You make this place feel easy. The rest of us make sure it actually works. Don't make me explain your promises twice.",
                    Choice("I know. I'll make the call.", "house_invitation",
                        "prologue.rafi.staff_commitment", "mina_sato", 2)),
                Node("house_invitation", "RAFI",
                    "Harbour Exchange. Haven't heard that name in years. Put it on the board. And find out who remembered us.",
                    Choice("End the sound check.", string.Empty,
                        "prologue.rafi.complete", "mina_sato", 1))
            };
            return graph;
        }

        private static DialogueGraph BuildElias()
        {
            DialogueGraph graph = ScriptableObject.CreateInstance<DialogueGraph>();
            graph.graphId = "prologue.elias.the_old_terms";
            graph.startNodeId = "settlement_room";
            graph.nodes = new[]
            {
                Node("settlement_room", "OPERATOR",
                    "You keep calling it a settlement. From where I'm sitting, it looks like a decision made before I got here.",
                    Choice("Tell me what you were promised.", "hear_operator",
                        "prologue.elias.listen_first", "silas_rook", 0),
                    Choice("The terms are not as simple as they look.", "explain_terms",
                        "prologue.elias.explain_terms", "silas_rook", 1)),
                Node("hear_operator", "ELIAS",
                    "Start with what you were promised. Leave out the parts you think I want to hear. I'll make up my own mind.",
                    Choice("Continue the meeting.", "silas_call",
                        "prologue.elias.listened", "anika_bose", 1)),
                Node("explain_terms", "ELIAS",
                    "The paper is clear. The conversation that led to it isn't. I'm not asking you to like the terms. I'm asking whether anyone gave you a choice.",
                    Choice("Continue the meeting.", "silas_call",
                        "prologue.elias.acknowledged_cost", "silas_rook", -1)),
                Node("silas_call", "SILAS",
                    "Still trying to get people to say the quiet part aloud?",
                    Choice("I'll be there. I'm not promising to like what I hear.", "accept_invitation",
                        "prologue.elias.accept_invitation", "silas_rook", 2, "elias_settlement_choice", 1),
                    Choice("I'll attend. The next page doesn't get my name just because you brought the pen.", "refuse_signature",
                        "prologue.elias.refuse_signature", "silas_rook", -2, "elias_settlement_choice", 2)),
                Node("accept_invitation", "ELIAS",
                    "I'll be there. That's all you've got from me tonight.",
                    Choice("Leave the meeting room.", string.Empty,
                        "prologue.elias.complete", "silas_rook", 1)),
                Node("refuse_signature", "ELIAS",
                    "I'll listen. Don't mistake that for permission to write my name down.",
                    Choice("Leave the meeting room.", string.Empty,
                        "prologue.elias.complete", "silas_rook", 1))
            };
            return graph;
        }

        private static DialogueGraph.DialogueNode Node(
            string id, string speaker, string line,
            params DialogueGraph.DialogueChoice[] choices)
        {
            return new DialogueGraph.DialogueNode
            {
                nodeId = id,
                speaker = speaker,
                line = line,
                choices = choices ?? Array.Empty<DialogueGraph.DialogueChoice>()
            };
        }

        private static DialogueGraph.DialogueChoice Choice(
            string label, string nextId, string signalId,
            string relationshipId, int relationshipDelta,
            string storyFlagId = null, int storyFlagValue = 0)
        {
            return new DialogueGraph.DialogueChoice
            {
                label = label,
                nextNodeId = nextId,
                signalId = signalId,
                signalAmount = 1,
                relationshipCharacterId = relationshipId,
                relationshipDelta = relationshipDelta,
                storyFlagId = storyFlagId,
                storyFlagValue = storyFlagValue
            };
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(folderName))
                throw new InvalidOperationException("Invalid asset folder path: " + path);
            EnsureFolder(parent);
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif
