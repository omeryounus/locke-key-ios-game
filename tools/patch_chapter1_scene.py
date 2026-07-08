#!/usr/bin/env python3
"""Patch Chapter1.unity with greybox visuals, passage zone, and gameplay components."""

from pathlib import Path

SCENE = Path(__file__).resolve().parents[1] / "Assets/_Project/Scenes/Chapter1/Chapter1.unity"

SPRITE_RENDERER_GROUND = """--- !u!212 &600003
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 600000}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {fileID: 0}
  m_ProbeAnchor: {fileID: 0}
  m_LightProbeVolumeOverride: {fileID: 0}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {fileID: 0}
  m_SortingLayerID: 1234567892
  m_SortingLayer: 1
  m_SortingOrder: 0
  m_Sprite: {fileID: 10905, guid: 0000000000000000f000000000000000, type: 0}
  m_Color: {r: 0.18, g: 0.16, b: 0.22, a: 1}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {x: 1, y: 1}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
"""

SPRITE_RENDERER_DOOR = """--- !u!212 &500004
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 500000}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {fileID: 0}
  m_ProbeAnchor: {fileID: 0}
  m_LightProbeVolumeOverride: {fileID: 0}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {fileID: 0}
  m_SortingLayerID: 1234567892
  m_SortingLayer: 1
  m_SortingOrder: 1
  m_Sprite: {fileID: 10905, guid: 0000000000000000f000000000000000, type: 0}
  m_Color: {r: 0.55, g: 0.2, b: 0.25, a: 1}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {x: 1, y: 1}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
"""

PASSAGE_ZONE = """--- !u!1 &700000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 700001}
  - component: {fileID: 700002}
  - component: {fileID: 700003}
  m_Layer: 11
  m_Name: PassageZone
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!4 &700001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 700000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 5, y: -1.5, z: 0}
  m_LocalScale: {x: 3, y: 3, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!61 &700002
BoxCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 700000}
  m_Enabled: 1
  m_Density: 1
  m_Material: {fileID: 0}
  m_IsTrigger: 1
  m_UsedByEffector: 0
  m_UsedByComposite: 0
  m_Offset: {x: 0, y: 0}
  m_SpriteTilingProperty:
    border: {x: 0, y: 0, z: 0, w: 0}
    pivot: {x: 0.5, y: 0.5}
    oldSize: {x: 1, y: 1}
    newSize: {x: 1, y: 1}
    adaptiveTilingThreshold: 0.5
    drawMode: 0
    adaptiveTiling: 0
  m_AutoTiling: 0
  serializedVersion: 2
  m_Size: {x: 1, y: 1}
  m_EdgeRadius: 0
--- !u!212 &700003
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 700000}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {fileID: 0}
  m_ProbeAnchor: {fileID: 0}
  m_LightProbeVolumeOverride: {fileID: 0}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {fileID: 0}
  m_SortingLayerID: 1234567892
  m_SortingLayer: 1
  m_SortingOrder: -1
  m_Sprite: {fileID: 10905, guid: 0000000000000000f000000000000000, type: 0}
  m_Color: {r: 0.2, g: 0.55, b: 0.35, a: 0.25}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {x: 1, y: 1}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
"""

FOYER_WALL = """--- !u!1 &800000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 800001}
  - component: {fileID: 800002}
  - component: {fileID: 800003}
  m_Layer: 11
  m_Name: FoyerWall
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &800001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 800000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 1, z: 0}
  m_LocalScale: {x: 1, y: 6, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!61 &800002
BoxCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 800000}
  m_Enabled: 1
  m_Density: 1
  m_Material: {fileID: 0}
  m_IsTrigger: 0
  m_UsedByEffector: 0
  m_UsedByComposite: 0
  m_Offset: {x: 0, y: 0}
  m_SpriteTilingProperty:
    border: {x: 0, y: 0, z: 0, w: 0}
    pivot: {x: 0.5, y: 0.5}
    oldSize: {x: 1, y: 1}
    newSize: {x: 1, y: 1}
    adaptiveTilingThreshold: 0.5
    drawMode: 0
    adaptiveTiling: 0
  m_AutoTiling: 0
  serializedVersion: 2
  m_Size: {x: 1, y: 1}
  m_EdgeRadius: 0
--- !u!212 &800003
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 800000}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {fileID: 0}
  m_ProbeAnchor: {fileID: 0}
  m_LightProbeVolumeOverride: {fileID: 0}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {fileID: 0}
  m_SortingLayerID: 1234567891
  m_SortingLayer: 0
  m_SortingOrder: 0
  m_Sprite: {fileID: 10905, guid: 0000000000000000f000000000000000, type: 0}
  m_Color: {r: 0.1, g: 0.09, b: 0.14, a: 1}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {x: 1, y: 1}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
"""

INTERACTION_ON_PLAYER = """--- !u!114 &400016
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 400000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef11, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  interactRadius: 2.5
"""

TOUCH_ON_GAME = """--- !u!114 &300003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef12, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  player: {fileID: 400012}
  keyManager: {fileID: 300012}
  interaction: {fileID: 400016}
"""

CAMERA_FOLLOW = """--- !u!114 &100005
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef13, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  target: {fileID: 400001}
  offset: {x: 0, y: 1, z: -10}
  smoothSpeed: 6
"""


def main() -> None:
    text = SCENE.read_text()

    # Ground sprite + component list
    text = text.replace(
        "  m_Component:\n  - component: {fileID: 600001}\n  - component: {fileID: 600002}\n  m_Layer: 11\n  m_Name: Ground",
        "  m_Component:\n  - component: {fileID: 600001}\n  - component: {fileID: 600002}\n  - component: {fileID: 600003}\n  m_Layer: 11\n  m_Name: Ground",
    )

    # Sealed door sprite + component list
    text = text.replace(
        "  m_Component:\n  - component: {fileID: 500001}\n  - component: {fileID: 500002}\n  - component: {fileID: 500003}\n  m_Layer: 10\n  m_Name: SealedDoor",
        "  m_Component:\n  - component: {fileID: 500001}\n  - component: {fileID: 500002}\n  - component: {fileID: 500003}\n  - component: {fileID: 500004}\n  m_Layer: 10\n  m_Name: SealedDoor",
    )

    # Wire passage + door renderer on puzzle
    text = text.replace(
        "  doorCollider: {fileID: 500002}\n  passageTrigger: {fileID: 0}",
        "  doorCollider: {fileID: 500002}\n  passageTrigger: {fileID: 700000}\n  doorRenderer: {fileID: 500004}",
    )

    # Player interaction component
    text = text.replace(
        "  - component: {fileID: 400015}\n  m_Layer: 8\n  m_Name: Player",
        "  - component: {fileID: 400015}\n  - component: {fileID: 400016}\n  m_Layer: 8\n  m_Name: Player",
    )

    # Camera follow component
    text = text.replace(
        "  - component: {fileID: 100004}\n  m_Layer: 0\n  m_Name: Main Camera",
        "  - component: {fileID: 100004}\n  - component: {fileID: 100005}\n  m_Layer: 0\n  m_Name: Main Camera",
    )

    if "&600003" not in text:
        text = text.rstrip() + "\n" + SPRITE_RENDERER_GROUND

    if "&500004" not in text:
        text = text.rstrip() + "\n" + SPRITE_RENDERER_DOOR

    if "&700000" not in text:
        text = text.rstrip() + "\n" + PASSAGE_ZONE

    if "&800000" not in text:
        text = text.rstrip() + "\n" + FOYER_WALL

    if "&400016" not in text:
        text = text.rstrip() + "\n" + INTERACTION_ON_PLAYER

    text = text.replace(
        "  m_Component:\n  - component: {fileID: 300001}\n  - component: {fileID: 300002}\n  m_Layer: 0\n  m_Name: --- Game ---",
        "  m_Component:\n  - component: {fileID: 300001}\n  - component: {fileID: 300002}\n  - component: {fileID: 300003}\n  m_Layer: 0\n  m_Name: --- Game ---",
    )

    if "guid: a1b2c3d4e5f6789012345678abcdef12" not in text:
        text = text.rstrip() + "\n" + TOUCH_ON_GAME

    if "&100005" not in text or "abcdef13" not in text:
        if "&100005" not in text:
            text = text.rstrip() + "\n" + CAMERA_FOLLOW

    SCENE.write_text(text + "\n")
    print(f"Patched {SCENE}")


if __name__ == "__main__":
    main()