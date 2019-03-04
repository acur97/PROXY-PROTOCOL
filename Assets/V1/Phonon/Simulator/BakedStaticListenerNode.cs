//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEngine;
using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Phonon
{
    //
    // Phonon Static Listener
    // Represents a baked static listener component.
    //

    [AddComponentMenu("Phonon/Baked Static Listener Node")]
    public class BakedStaticListenerNode : MonoBehaviour
    {
        void OnDrawGizmosSelected()
        {
            Color oldColor = Gizmos.color;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(gameObject.transform.position, bakingRadius);

            Gizmos.color = Color.magenta;
            ProbeBox[] drawProbeBoxes = probeBoxes;
            if (useAllProbeBoxes)
                drawProbeBoxes = FindObjectsOfType<ProbeBox>() as ProbeBox[];

            if (drawProbeBoxes != null)
                foreach (ProbeBox probeBox in drawProbeBoxes)
                    if (probeBox != null)
                        Gizmos.DrawWireCube(probeBox.transform.position, probeBox.transform.localScale);

            Gizmos.color = oldColor;
        }

        public void BeginBake()
        {
            Sphere bakeSphere;
            Vector3 sphereCenter = Common.ConvertVector(gameObject.transform.position);
            bakeSphere.centerx = sphereCenter.x;
            bakeSphere.centery = sphereCenter.y;
            bakeSphere.centerz = sphereCenter.z;
            bakeSphere.radius = bakingRadius;

            if (useAllProbeBoxes)
                phononBaker.BeginBake(FindObjectsOfType<ProbeBox>() as ProbeBox[], BakingMode.StaticListener, uniqueIdentifier, bakeSphere);
            else
                phononBaker.BeginBake(probeBoxes, BakingMode.StaticListener, uniqueIdentifier, bakeSphere);
        }

        public void EndBake()
        {
            phononBaker.EndBake();
        }

        public string GetUniqueIdentifier()
        {
            return bakedListenerPrefix + uniqueIdentifier;
        }

        // Public members.
        public string uniqueIdentifier = "";
        [Range(1f, 1024f)]
        public float bakingRadius = 16f;
        public bool useAllProbeBoxes = false;
        public ProbeBox[] probeBoxes = null;
        public PhononBaker phononBaker = new PhononBaker();

        // Private members.
        string bakedListenerPrefix = "__staticlistener__";
    }
}
