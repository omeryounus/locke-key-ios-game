#!/usr/bin/env python3
"""Add uGUI wiring, Puzzle 4, key glows, Echo encounter, and accent lights."""

from pathlib import Path

SCENE = Path(__file__).resolve().parents[1] / "Assets/_Project/Scenes/Chapter1/Chapter1.unity"

BLOCK = """
--- !u!114 &300033
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300030}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef20, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  gameplay: {fileID: 300003}
  uiManager: {fileID: 300032}
--- !u!114 &300004
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef26, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  spawnPoint: {fileID: 970001}
  echoSprite: {fileID: 0}
  spawnOnSealedDoor: 1
  spawnOnEchoEvent: 1
--- !u!1 &300040
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 300041}
  - component: {fileID: 300042}
  m_Layer: 0
  m_Name: HeadKey
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &300041
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300040}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 300001}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &300042
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300040}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef21, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  keyName: Head Key
  description: Unlock memories hidden in Keyhouse.
  cooldown: 12
--- !u!114 &400018
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 400000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef24, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  keyManager: {fileID: 300012}
  ghostIntensity: 0.85
  headIntensity: 0.75
  idleIntensity: 0.2
--- !u!1 &950000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 950001}
  - component: {fileID: 950002}
  - component: {fileID: 950003}
  - component: {fileID: 950004}
  m_Layer: 10
  m_Name: HeadKeyPickup
  m_TagString: Interactable
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &950001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 950000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 9.5, y: -0.5, z: 0}
  m_LocalScale: {x: 0.6, y: 0.6, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!212 &950002
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 950000}
  m_Enabled: 0
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
  m_SortingLayerID: 1234567893
  m_SortingLayer: 3
  m_SortingOrder: 4
  m_Sprite: {fileID: 10905, guid: 0000000000000000f000000000000000, type: 0}
  m_Color: {r: 0.85, g: 0.45, b: 0.95, a: 1}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {x: 1, y: 1}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
--- !u!114 &950003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 950000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef23, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  keyManager: {fileID: 300012}
  keyRenderer: {fileID: 950002}
--- !u!114 &950004
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 950000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 073797afb82c5a1438f328866b10b3f0, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_ComponentVersion: 1
  m_LightType: 3
  m_BlendStyleIndex: 0
  m_FalloffIntensity: 0.5
  m_Color: {r: 0.85, g: 0.45, b: 0.95, a: 1}
  m_Intensity: 0.45
  m_LightVolumeIntensity: 1
  m_LightVolumeIntensityEnabled: 0
  m_ApplyToSortingLayers: 00000000
  m_LightCookieSprite: {fileID: 0}
  m_DeprecatedPointLightCookieSprite: {fileID: 0}
  m_LightOrder: 0
  m_AlphaBlendOnOverlap: 0
  m_OverlapOperation: 0
  m_NormalMapDistance: 3
  m_NormalMapQuality: 2
  m_UseNormalMap: 0
  m_ShadowIntensityEnabled: 0
  m_ShadowIntensity: 0.75
  m_ShadowVolumeIntensityEnabled: 0
  m_ShadowVolumeIntensity: 0.75
  m_LocalBounds:
    m_Center: {x: 0, y: 0, z: 0}
    m_Extent: {x: 0, y: 0, z: 0}
  m_PointLightInnerAngle: 360
  m_PointLightOuterAngle: 360
  m_PointLightInnerRadius: 0.1
  m_PointLightOuterRadius: 2
  m_ShapeLightParametricSides: 5
  m_ShapeLightParametricAngleOffset: 0
  m_ShapeLightParametricRadius: 1
  m_ShapeLightFalloffSize: 0.5
  m_ShapeLightFalloffOffset: {x: 0, y: 0}
  m_ShapePath:
  - {x: -0.5, y: -0.5, z: 0}
  - {x: 0.5, y: -0.5, z: 0}
  - {x: 0.5, y: 0.5, z: 0}
  - {x: -0.5, y: 0.5, z: 0}
--- !u!1 &960000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 960001}
  - component: {fileID: 960002}
  - component: {fileID: 960003}
  m_Layer: 10
  m_Name: MemoryPortrait
  m_TagString: Interactable
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &960001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 960000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 11.5, y: 0, z: 0}
  m_LocalScale: {x: 0.8, y: 1.2, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!212 &960002
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 960000}
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
  m_SortingOrder: 4
  m_Sprite: {fileID: 10907, guid: 0000000000000000f000000000000000, type: 0}
  m_Color: {r: 0.55, g: 0.42, b: 0.62, a: 1}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {x: 1, y: 1}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
--- !u!114 &960003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 960000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef22, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  puzzleID: chapter1_memory_fragment
  isSolved: 0
  requiresSpecificKey: 1
  requiredKeyType: 1
  portraitRenderer: {fileID: 960002}
  uiManager: {fileID: 300032}
--- !u!1 &970000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 970001}
  m_Layer: 0
  m_Name: EchoSpawnPoint
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &970001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 970000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 7, y: 0.5, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!1 &980000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 980001}
  - component: {fileID: 980002}
  m_Layer: 0
  m_Name: FoyerAccentLight
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &980001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 980000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 2, y: 1.5, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &980002
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 980000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 073797afb82c5a1438f328866b10b3f0, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_ComponentVersion: 1
  m_LightType: 3
  m_BlendStyleIndex: 0
  m_FalloffIntensity: 0.5
  m_Color: {r: 0.55, g: 0.75, b: 1, a: 1}
  m_Intensity: 0.55
  m_LightVolumeIntensity: 1
  m_LightVolumeIntensityEnabled: 0
  m_ApplyToSortingLayers: 00000000
  m_LightCookieSprite: {fileID: 0}
  m_DeprecatedPointLightCookieSprite: {fileID: 0}
  m_LightOrder: 0
  m_AlphaBlendOnOverlap: 0
  m_OverlapOperation: 0
  m_NormalMapDistance: 3
  m_NormalMapQuality: 2
  m_UseNormalMap: 0
  m_ShadowIntensityEnabled: 0
  m_ShadowIntensity: 0.75
  m_ShadowVolumeIntensityEnabled: 0
  m_ShadowVolumeIntensity: 0.75
  m_LocalBounds:
    m_Center: {x: 0, y: 0, z: 0}
    m_Extent: {x: 0, y: 0, z: 0}
  m_PointLightInnerAngle: 360
  m_PointLightOuterAngle: 360
  m_PointLightInnerRadius: 0.2
  m_PointLightOuterRadius: 5
  m_ShapeLightParametricSides: 5
  m_ShapeLightParametricAngleOffset: 0
  m_ShapeLightParametricRadius: 1
  m_ShapeLightFalloffSize: 0.5
  m_ShapeLightFalloffOffset: {x: 0, y: 0}
  m_ShapePath:
  - {x: -0.5, y: -0.5, z: 0}
  - {x: 0.5, y: -0.5, z: 0}
  - {x: 0.5, y: 0.5, z: 0}
  - {x: -0.5, y: 0.5, z: 0}
--- !u!114 &930004
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 930000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 073797afb82c5a1438f328866b10b3f0, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_ComponentVersion: 1
  m_LightType: 3
  m_BlendStyleIndex: 0
  m_FalloffIntensity: 0.5
  m_Color: {r: 0.45, g: 0.85, b: 1, a: 1}
  m_Intensity: 0.4
  m_LightVolumeIntensity: 1
  m_LightVolumeIntensityEnabled: 0
  m_ApplyToSortingLayers: 00000000
  m_LightCookieSprite: {fileID: 0}
  m_DeprecatedPointLightCookieSprite: {fileID: 0}
  m_LightOrder: 0
  m_AlphaBlendOnOverlap: 0
  m_OverlapOperation: 0
  m_NormalMapDistance: 3
  m_NormalMapQuality: 2
  m_UseNormalMap: 0
  m_ShadowIntensityEnabled: 0
  m_ShadowIntensity: 0.75
  m_ShadowVolumeIntensityEnabled: 0
  m_ShadowVolumeIntensity: 0.75
  m_LocalBounds:
    m_Center: {x: 0, y: 0, z: 0}
    m_Extent: {x: 0, y: 0, z: 0}
  m_PointLightInnerAngle: 360
  m_PointLightOuterAngle: 360
  m_PointLightInnerRadius: 0.1
  m_PointLightOuterRadius: 2.2
  m_ShapeLightParametricSides: 5
  m_ShapeLightParametricAngleOffset: 0
  m_ShapeLightParametricRadius: 1
  m_ShapeLightFalloffSize: 0.5
  m_ShapeLightFalloffOffset: {x: 0, y: 0}
  m_ShapePath:
  - {x: -0.5, y: -0.5, z: 0}
  - {x: 0.5, y: -0.5, z: 0}
  - {x: 0.5, y: 0.5, z: 0}
  - {x: -0.5, y: 0.5, z: 0}
"""


def main() -> None:
    text = SCENE.read_text()

    text = text.replace(
        "  m_Component:\n  - component: {fileID: 300031}\n  - component: {fileID: 300032}\n  m_Layer: 5\n  m_Name: UIManager",
        "  m_Component:\n  - component: {fileID: 300031}\n  - component: {fileID: 300032}\n  - component: {fileID: 300033}\n  m_Layer: 5\n  m_Name: UIManager",
    )

    text = text.replace(
        "  m_Component:\n  - component: {fileID: 300001}\n  - component: {fileID: 300002}\n  - component: {fileID: 300003}\n  m_Layer: 0\n  m_Name: --- Game ---",
        "  m_Component:\n  - component: {fileID: 300001}\n  - component: {fileID: 300002}\n  - component: {fileID: 300003}\n  - component: {fileID: 300004}\n  m_Layer: 0\n  m_Name: --- Game ---",
    )

    text = text.replace(
        "  m_Children:\n  - {fileID: 300011}\n  - {fileID: 300021}\n  - {fileID: 300031}\n  m_Father: {fileID: 0}",
        "  m_Children:\n  - {fileID: 300011}\n  - {fileID: 300021}\n  - {fileID: 300041}\n  - {fileID: 300031}\n  m_Father: {fileID: 0}",
    )

    text = text.replace(
        "  - component: {fileID: 400016}\n  - component: {fileID: 400017}\n  m_Layer: 8\n  m_Name: Player",
        "  - component: {fileID: 400016}\n  - component: {fileID: 400017}\n  - component: {fileID: 400018}\n  m_Layer: 8\n  m_Name: Player",
    )

    text = text.replace(
        "  m_Component:\n  - component: {fileID: 930001}\n  - component: {fileID: 930002}\n  - component: {fileID: 930003}\n  m_Layer: 10\n  m_Name: GhostKeyPickup",
        "  m_Component:\n  - component: {fileID: 930001}\n  - component: {fileID: 930002}\n  - component: {fileID: 930003}\n  - component: {fileID: 930004}\n  m_Layer: 10\n  m_Name: GhostKeyPickup",
    )

    if "&300033" not in text:
        text = text.rstrip() + BLOCK

    SCENE.write_text(text + "\n")
    print(f"Patched final slice into {SCENE}")


if __name__ == "__main__":
    main()