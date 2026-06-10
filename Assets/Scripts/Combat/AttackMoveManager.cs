using System.Collections.Generic;
using AoE.RTS.Selection;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Combat
{
    public class AttackMoveManager : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureExistsInPlayMode()
        {
            if (!Application.isPlaying || Object.FindAnyObjectByType<AttackMoveManager>() != null)
                return;

            GameObject systems = GameObject.Find("Systems");
            Transform parent = systems != null ? systems.transform : null;
            GameObject attackMoveObject = new GameObject("AttackMoveManager");
            if (parent != null)
                attackMoveObject.transform.SetParent(parent);

            attackMoveObject.AddComponent<AttackMoveManager>();
            Debug.Log("[AttackMove] AttackMoveManager was missing from the scene — created at runtime.");
        }

        public static bool IsUnitAttackMoving(Unit unit)
        {
            return FormationMoveManager.IsUnitAttackMoving(unit);
        }

        public static void Register(IReadOnlyList<Unit> units, Vector3 center, float spacing)
        {
            FormationMoveManager.Register(units, center, spacing, isAttackMove: true);
        }

        public static void CancelForUnits(IReadOnlyList<Unit> units)
        {
            FormationMoveManager.CancelForUnits(units);
        }

        public static void CancelForUnit(Unit unit)
        {
            FormationMoveManager.CancelForUnit(unit);
        }
    }
}
