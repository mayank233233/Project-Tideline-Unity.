using System;
using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Pure evaluator for ordered mission-branch predicates.
    /// A missing relationship/story value is distinct from an explicitly stored zero.
    /// </summary>
    public static class MissionBranchEvaluator
    {
        public static bool ConditionsMatch(
            MissionBranch branch,
            Func<string, int?> relationshipValue,
            Func<string, int?> storyFlagValue)
        {
            if (branch == null) return false;
            if (branch.conditions == null || branch.conditions.Length == 0) return true;

            foreach (MissionBranchCondition condition in branch.conditions)
            {
                if (condition == null) return false;

                bool usesRelationship = !string.IsNullOrWhiteSpace(condition.relationshipKey);
                bool usesStoryFlag = !string.IsNullOrWhiteSpace(condition.storyFlagId);

                // Exactly one source is legal. Both-empty and both-populated are invalid.
                if (usesRelationship == usesStoryFlag) return false;

                int? actual = usesRelationship
                    ? relationshipValue?.Invoke(condition.relationshipKey)
                    : storyFlagValue?.Invoke(condition.storyFlagId);

                // An absent ledger/key must never be interpreted as an explicit zero.
                if (!actual.HasValue) return false;

                int minimum = Math.Min(condition.minimumValue, condition.maximumValue);
                int maximum = Math.Max(condition.minimumValue, condition.maximumValue);
                if (actual.Value < minimum || actual.Value > maximum) return false;
            }

            // Multiple conditions are ANDed.
            return true;
        }
    }
}