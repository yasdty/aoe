"""Generate Assets/Scenes/Phase10.unity from Phase9.unity with Phase10-specific objects."""

from __future__ import annotations

import re
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PHASE9 = ROOT / "Assets/Scenes/Phase9.unity"
PHASE10 = ROOT / "Assets/Scenes/Phase10.unity"
PHASE10_META = ROOT / "Assets/Scenes/Phase10.unity.meta"

GAME_TIME_HUD_GUID = "0ea47d6922bc180468c8753d04059490"
CPU_MILITARY_GUID = "eabaa17be3974014ba458ccc8894f3a0"
TREE_RESOURCE_GUID = "ee33088af6b97794a90907cd8cdd0604"
TREE_MATERIAL_REF = "{fileID: 577105146}"

EXTRA_TREE_POSITIONS = [
    (0, 12),
    (6, 16),
    (-8, 14),
    (14, 0),
    (-18, 2),
    (4, -8),
    (-6, -6),
    (0, -14),
    (10, -22),
    (-12, -20),
    (6, -28),
    (-8, -30),
    (0, -32),
    (14, -34),
    (-14, -36),
    (8, -40),
    (-6, -42),
    (0, -24),
    (-20, -28),
    (20, -26),
]

GAME_TIME_HUD_COMPONENT_ID = 910010001
CPU_MILITARY_GO_ID = 910020001
CPU_MILITARY_TRANSFORM_ID = 910020002
CPU_MILITARY_SCRIPT_ID = 910020003
TREE_ID_BASE = 910100000


def make_tree_block(index: int, x: float, z: float) -> str:
    base = TREE_ID_BASE + index * 10
    go_id = base + 1
    resource_id = base + 2
    renderer_id = base + 3
    collider_id = base + 4
    filter_id = base + 5
    transform_id = base + 6
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {transform_id}}}
  - component: {{fileID: {filter_id}}}
  - component: {{fileID: {collider_id}}}
  - component: {{fileID: {renderer_id}}}
  - component: {{fileID: {resource_id}}}
  m_Layer: 11
  m_Name: Tree
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!114 &{resource_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {TREE_RESOURCE_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: AoE.RTS::AoE.RTS.Economy.TreeResource
  data: {{fileID: 0}}
--- !u!23 &{renderer_id}
MeshRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_CastShadows: 1
  m_ReceiveShadows: 1
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 2
  m_RayTraceProcedural: 0
  m_RayTracingAccelStructBuildFlagsOverride: 0
  m_RayTracingAccelStructBuildFlags: 1
  m_SmallMeshCulling: 1
  m_ForceMeshLod: -1
  m_MeshLodSelectionBias: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {TREE_MATERIAL_REF}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 1
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 3
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_GlobalIlluminationMeshLod: 0
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 0
  m_MaskInteraction: 0
  m_AdditionalVertexStreams: {{fileID: 0}}
--- !u!136 &{collider_id}
CapsuleCollider:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_IsTrigger: 0
  m_ProvidesContacts: 0
  m_Enabled: 1
  serializedVersion: 2
  m_Radius: 0.5
  m_Height: 2
  m_Direction: 1
  m_Center: {{x: 0, y: 0, z: 0}}
--- !u!33 &{filter_id}
MeshFilter:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Mesh: {{fileID: 10206, guid: 0000000000000000e000000000000000, type: 0}}
--- !u!4 &{transform_id}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {x}, y: 2.05, z: {z}}}
  m_LocalScale: {{x: 1.2, y: 2, z: 1.2}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
"""


def main() -> None:
    if not PHASE9.exists():
        raise SystemExit(f"Missing source scene: {PHASE9}")

    content = PHASE9.read_text(encoding="utf-8")

    selection_manager_pattern = (
        r"(--- !u!1 &768265777\nGameObject:.*?m_Component:\n"
        r"(?:  - component: \{fileID: \d+\}\n)+)"
    )
    match = re.search(selection_manager_pattern, content, re.DOTALL)
    if not match:
        raise SystemExit("SelectionManager block not found")

    old_components = match.group(1)
    new_components = old_components.replace(
        "  - component: {fileID: 768265778}\n",
        "  - component: {fileID: 768265778}\n"
        f"  - component: {{fileID: {GAME_TIME_HUD_COMPONENT_ID}}}\n",
        1,
    )
    content = content.replace(old_components, new_components, 1)

    game_time_hud_block = f"""--- !u!114 &{GAME_TIME_HUD_COMPONENT_ID}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 768265777}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GAME_TIME_HUD_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: AoE.RTS::AoE.RTS.Selection.GameTimeHudView
"""
    content = content.replace(
        "--- !u!114 &768265779\nMonoBehaviour:",
        game_time_hud_block + "--- !u!114 &768265779\nMonoBehaviour:",
        1,
    )

    cpu_military_block = f"""--- !u!1 &{CPU_MILITARY_GO_ID}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {CPU_MILITARY_TRANSFORM_ID}}}
  - component: {{fileID: {CPU_MILITARY_SCRIPT_ID}}}
  m_Layer: 0
  m_Name: CpuMilitaryAiManager
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{CPU_MILITARY_TRANSFORM_ID}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {CPU_MILITARY_GO_ID}}}
  serializedVersion: 2
  m_LocalRotation: {{x: -0, y: -0, z: -0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 613311842}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &{CPU_MILITARY_SCRIPT_ID}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {CPU_MILITARY_GO_ID}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {CPU_MILITARY_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: AoE.RTS::AoE.RTS.AI.CpuMilitaryAiManager
  barracksData: {{fileID: 0}}
  barracksBuildDelaySeconds: 120
  attackWaveIntervalSeconds: 60
"""
    content = content.replace(
        "--- !u!1 &1364136857\nGameObject:",
        cpu_military_block + "--- !u!1 &1364136857\nGameObject:",
        1,
    )

    content = content.replace(
        "  - {fileID: 1364136858}\n  - {fileID: 768265778}\n",
        "  - {fileID: 1364136858}\n"
        f"  - {{fileID: {CPU_MILITARY_TRANSFORM_ID}}}\n"
        "  - {fileID: 768265778}\n",
        1,
    )

    tree_blocks = []
    tree_root_ids = []
    for index, (x, z) in enumerate(EXTRA_TREE_POSITIONS):
        tree_blocks.append(make_tree_block(index, x, z))
        tree_root_ids.append(TREE_ID_BASE + index * 10 + 6)

    insert_point = content.rfind("--- !u!1660057539 &9223372036854775807")
    if insert_point == -1:
        raise SystemExit("SceneRoots block not found")

    content = content[:insert_point] + "\n".join(tree_blocks) + "\n" + content[insert_point:]

    roots_pattern = r"(SceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n(?:  - \{fileID: \d+\}\n)+)"
    roots_match = re.search(roots_pattern, content)
    if not roots_match:
        raise SystemExit("SceneRoots list not found")

    roots_block = roots_match.group(1)
    extra_roots = "".join(f"  - {{fileID: {transform_id}}}\n" for transform_id in tree_root_ids)
    content = content.replace(
        roots_block,
        roots_block + extra_roots,
        1,
    )

    PHASE10.write_text(content, encoding="utf-8", newline="\n")

    meta_guid = uuid.uuid4().hex
    PHASE10_META.write_text(
        f"""fileFormatVersion: 2
guid: {meta_guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
""",
        encoding="utf-8",
        newline="\n",
    )

    print(f"Wrote {PHASE10}")
    print(f"Wrote {PHASE10_META}")
    print(f"Added {len(EXTRA_TREE_POSITIONS)} trees, GameTimeHudView, CpuMilitaryAiManager")


if __name__ == "__main__":
    main()
