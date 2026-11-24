using System.Collections.Generic;
using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Tracks pedestrians physically occupying a marked crosswalk.
    /// Traffic yields while at least one ambient pedestrian is inside the crossing volume.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TrafficPedestrianCrossing : MonoBehaviour
    {
        private readonly HashSet<AmbientPedestrianWalker> _occupants = new HashSet<AmbientPedestrianWalker>();

        public bool IsOccupied => _occupants.Count > 0;

        private void Awake()
        {
            BoxCollider trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            AmbientPedestrianWalker pedestrian = other.GetComponentInParent<AmbientPedestrianWalker>();
            if (pedestrian != null) _occupants.Add(pedestrian);
        }

        private void OnTriggerExit(Collider other)
        {
            AmbientPedestrianWalker pedestrian = other.GetComponentInParent<AmbientPedestrianWalker>();
            if (pedestrian != null) _occupants.Remove(pedestrian);
        }

        private void LateUpdate()
        {
            _occupants.RemoveWhere(pedestrian => pedestrian == null || !pedestrian.isActiveAndEnabled);
        }

        private void OnDisable()
        {
            _occupants.Clear();
        }
    }
}