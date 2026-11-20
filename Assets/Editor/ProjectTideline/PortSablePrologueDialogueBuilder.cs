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
                    "The lights are still on. That means you are still working. The shop closes at nine, Mara.",
                    Choice("The customer's car comes first.", "finish_customer_job",
                        "prologue.mara.finish_customer_job", "juno_kade", 2),
                    Choice("What is that notice?", "ask_about_notice",
                        "prologue.mara.ask_about_notice", "juno_kade", -1)),
                Node("finish_customer_job", "MARA",
                    "One last check. I promised the car would leave clean, not that I would be home early.",
                    Choice("Take the keys and go.", "nico_at_door",
                        "prologue.mara.prioritise_work", "juno_kade", 1)),
                Node("ask_about_notice", "JUNO",
                    "It is an old account dressed up as a new emergency. I will explain it when I can do it properly. Not in front of customers.",
                    Choice("You should have told me.", "nico_at_door",
                        "prologue.mara.challenge_juno", "juno_kade", -2),
                    Choice("Then we talk after the drive.", "nico_at_door",
                        "prologue.mara.defer_argument", "juno_kade", 1)),
                Node("nico_at_door", "NICO",
                    "You still take the long way when the short one is faster. Some habits are expensive.",
                    Choice("Then try to keep up.", "mara_close",
                        "prologue.mara.accept_challenge", "nico_okoye", 2),
                    Choice("I am delivering a car, not proving a point.", "mara_close",
                        "prologue.mara.decline_challenge", "nico_okoye", 1)),
                Node("mara_close", "MARA",
                    "Keys, route, customer. In that order. Whatever this old notice is, it can wait until the garage is safe for the night.",
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
                    "The room is ready. The staff are ready. The performers are ready. The schedule is not. Tell me which promise you intend to keep.",
                    Choice("Keep the first promise.", "honour_first_slot",
                        "prologue.rafi.honour_first_slot", "mina_sato", 3),
                    Choice("Keep the headline event intact.", "protect_headline",
                        "prologue.rafi.protect_headline", "mina_sato", -2)),
                Node("honour_first_slot", "RAFI",
                    "Then we tell the room the truth. It will sting for ten minutes. A lie can keep charging interest for years.",
                    Choice("Tell the performers yourself.", "mina_reply",
                        "prologue.rafi.own_promise", "mina_sato", 2)),
                Node("protect_headline", "RAFI",
                    "The night matters to everyone here. I can fix the order. I cannot pretend nobody will notice.",
                    Choice("Make the call and take responsibility.", "mina_reply",
                        "prologue.rafi.take_responsibility", "mina_sato", 1),
                    Choice("Let Mina make the announcement.", "mina_reply",
                        "prologue.rafi.shift_burden", "mina_sato", -3)),
                Node("mina_reply", "MINA",
                    "You make a room feel like it belongs to everyone. Remember that the people carrying the speakers belong here too.",
                    Choice("I know. I will act like it.", "house_invitation",
                        "prologue.rafi.staff_commitment", "mina_sato", 2)),
                Node("house_invitation", "RAFI",
                    "Harbour Exchange. That name has not appeared on my calendar in years. Put it on the list, Mina. I want to know who is asking.",
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
                    "You keep calling it a settlement. It feels like a decision made before I arrived.",
                    Choice("Tell me what you were promised.", "hear_operator",
                        "prologue.elias.listen_first", "silas_rook", 0),
                    Choice("The terms are not as simple as they look.", "explain_terms",
                        "prologue.elias.explain_terms", "silas_rook", 1)),
                Node("hear_operator", "ELIAS",
                    "Start at the beginning. Leave out the parts you think I want to hear. I can make my own mind up.",
                    Choice("Continue the meeting.", "silas_call",
                        "prologue.elias.listened", "anika_bose", 1)),
                Node("explain_terms", "ELIAS",
                    "A signature can end an argument. It cannot make the outcome fair. I know the difference. I have not always acted like it.",
                    Choice("Continue the meeting.", "silas_call",
                        "prologue.elias.acknowledged_cost", "silas_rook", -1)),
                Node("silas_call", "SILAS",
                    "You have been away long enough, Elias. Harbour Exchange needs someone who remembers what the promises meant before everyone started changing their names.",
                    Choice("I will witness the settlement.", "accept_invitation",
                        "prologue.elias.accept_invitation", "silas_rook", 2),
                    Choice("I will attend, but I will not sign for you.", "refuse_signature",
                        "prologue.elias.refuse_signature", "silas_rook", -2)),
                Node("accept_invitation", "ELIAS",
                    "I will attend. I did not say I would agree.",
                    Choice("Leave the meeting room.", string.Empty,
                        "prologue.elias.complete", "silas_rook", 1)),
                Node("refuse_signature", "ELIAS",
                    "I will hear what you have to say. My name is not a blank space for you to fill in.",
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
            string relationshipId, int relationshipDelta)
        {
            return new DialogueGraph.DialogueChoice
            {
                label = label,
                nextNodeId = nextId,
                signalId = signalId,
                signalAmount = 1,
                relationshipCharacterId = relationshipId,
                relationshipDelta = relationshipDelta
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
