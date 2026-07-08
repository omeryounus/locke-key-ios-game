#!/usr/bin/env python3
"""Generate Unity .meta files and Chapter1 scene for the Locke Key project."""

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

# Fixed GUID map for cross-references
GUIDS = {
    "Assets": "a0000000000000010000000000000001",
    "Assets/Settings": "a0000000000000020000000000000002",
    "Assets/Settings/UniversalRP.asset": "b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7",
    "Assets/Settings/Renderer2D.asset": "d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6",
    "Assets/Settings/UniversalRenderPipelineGlobalSettings.asset": "f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8",
    "Assets/_Project": "a0000000000000030000000000000003",
    "Assets/_Project/Art": "a0000000000000040000000000000004",
    "Assets/_Project/Art/Sprites": "b1000000000000100000000000000010",
    "Assets/_Project/Art/Sprites/Keys": "b1000000000000110000000000000011",
    "Assets/_Project/Art/Sprites/Characters": "b1000000000000120000000000000012",
    "Assets/_Project/Art/Sprites/Environments": "b1000000000000130000000000000013",
    "Assets/_Project/Art/Sprites/Props": "b1000000000000140000000000000014",
    "Assets/_Project/Art/Sprites/Enemies": "b1000000000000150000000000000015",
    "Assets/_Project/Art/ai_assets_manifest.json": "b1000000000000160000000000000016",
    "Assets/_Project/Audio": "a0000000000000050000000000000005",
    "Assets/_Project/Prefabs": "a0000000000000060000000000000006",
    "Assets/_Project/Scenes": "a0000000000000070000000000000007",
    "Assets/_Project/Scenes/Chapter1": "a0000000000000080000000000000008",
    "Assets/_Project/Scenes/Chapter1/Chapter1.unity": "c4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9",
    "Assets/_Project/ScriptableObjects": "a0000000000000090000000000000009",
    "Assets/_Project/Resources": "a00000000000000a000000000000000a",
    "Assets/_Project/Resources/EventBus.asset": "e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6",
    "Assets/_Project/UI": "a00000000000000b000000000000000b",
    "Assets/_Project/Scripts": "a00000000000000c000000000000000c",
    "Assets/_Project/Scripts/Core": "a00000000000000d000000000000000d",
    "Assets/_Project/Scripts/Keys": "a00000000000000e000000000000000e",
    "Assets/_Project/Scripts/Puzzles": "a00000000000000f000000000000000f",
    "Assets/_Project/Scripts/Player": "a0000000000000100000000000000100",
    "Assets/_Project/Scripts/Narrative": "a0000000000000110000000000000011",
    "Assets/_Project/Scripts/UI": "a0000000000000120000000000000012",
    "Assets/_Project/Scripts/Core/EventBus.cs": "a1b2c3d4e5f6789012345678abcdef01",
    "Assets/_Project/Scripts/Core/IKeyAbility.cs": "a1b2c3d4e5f6789012345678abcdef02",
    "Assets/_Project/Scripts/Core/PuzzleBase.cs": "a1b2c3d4e5f6789012345678abcdef03",
    "Assets/_Project/Scripts/Core/GameBootstrap.cs": "a1b2c3d4e5f6789012345678abcdef08",
    "Assets/_Project/Scripts/Keys/KeyManager.cs": "a1b2c3d4e5f6789012345678abcdef04",
    "Assets/_Project/Scripts/Keys/GhostKey.cs": "a1b2c3d4e5f6789012345678abcdef07",
    "Assets/_Project/Scripts/Player/PlayerController.cs": "a1b2c3d4e5f6789012345678abcdef05",
    "Assets/_Project/Scripts/Player/InteractionController.cs": "a1b2c3d4e5f6789012345678abcdef11",
    "Assets/_Project/Scripts/Player/TouchGameplayController.cs": "a1b2c3d4e5f6789012345678abcdef12",
    "Assets/_Project/Scripts/Player/CameraFollow2D.cs": "a1b2c3d4e5f6789012345678abcdef13",
    "Assets/_Project/Scripts/UI/UIManager.cs": "a1b2c3d4e5f6789012345678abcdef06",
    "Assets/_Project/Scripts/Puzzles/SealedDoorPuzzle.cs": "a1b2c3d4e5f6789012345678abcdef09",
    "Assets/_Project/Scripts/LockeKey.asmdef": "a1b2c3d4e5f6789012345678abcdef10",
    "Assets/_Project/Scripts/Core/IInteractable.cs": "a1b2c3d4e5f6789012345678abcdef14",
    "Assets/_Project/Scripts/Player/PlayerInventory.cs": "a1b2c3d4e5f6789012345678abcdef15",
    "Assets/_Project/Scripts/Puzzles/StuckDoorPuzzle.cs": "a1b2c3d4e5f6789012345678abcdef16",
    "Assets/_Project/Scripts/Puzzles/HouseKeyPickup.cs": "a1b2c3d4e5f6789012345678abcdef17",
    "Assets/_Project/Scripts/Keys/GhostKeyPickup.cs": "a1b2c3d4e5f6789012345678abcdef18",
    "Assets/_Project/Scripts/Puzzles/CollapsedBookshelfPuzzle.cs": "a1b2c3d4e5f6789012345678abcdef19",
    "Assets/_Project/Scripts/UI/GameplayHUD.cs": "a1b2c3d4e5f6789012345678abcdef20",
    "Assets/_Project/Scripts/Keys/HeadKey.cs": "a1b2c3d4e5f6789012345678abcdef21",
    "Assets/_Project/Scripts/Puzzles/MemoryFragmentPuzzle.cs": "a1b2c3d4e5f6789012345678abcdef22",
    "Assets/_Project/Scripts/Keys/HeadKeyPickup.cs": "a1b2c3d4e5f6789012345678abcdef23",
    "Assets/_Project/Scripts/Keys/KeyGlowController.cs": "a1b2c3d4e5f6789012345678abcdef24",
    "Assets/_Project/Scripts/Narrative/EchoEntity.cs": "a1b2c3d4e5f6789012345678abcdef25",
    "Assets/_Project/Scripts/Narrative/EchoEncounterManager.cs": "a1b2c3d4e5f6789012345678abcdef26",
    "Assets/Editor": "a0000000000000130000000000000013",
    "Assets/Editor/IOSBuildMenu.cs": "a1b2c3d4e5f6789012345678abcdef27",
    "Assets/Editor/LockeKey.Editor.asmdef": "a1b2c3d4e5f6789012345678abcdef28",
    "Assets/_Project/Scripts/Environment/ParallaxLayer.cs": "a1b2c3d4e5f6789012345678abcdef30",
    "Assets/_Project/Scripts/UI/UIIconLibrary.cs": "a1b2c3d4e5f6789012345678abcdef31",
    "Assets/_Project/Resources/Art/UI/UIIconLibrary.asset": "b400000000000010000000000000010",
}

FOLDERS = [
    "Assets",
    "Assets/Settings",
    "Assets/_Project",
    "Assets/_Project/Art",
    "Assets/_Project/Audio",
    "Assets/_Project/Prefabs",
    "Assets/_Project/Scenes",
    "Assets/_Project/Scenes/Chapter1",
    "Assets/_Project/ScriptableObjects",
    "Assets/_Project/Resources",
    "Assets/_Project/UI",
    "Assets/_Project/Scripts",
    "Assets/_Project/Scripts/Core",
    "Assets/_Project/Scripts/Keys",
    "Assets/_Project/Scripts/Puzzles",
    "Assets/_Project/Scripts/Player",
    "Assets/_Project/Scripts/Narrative",
    "Assets/_Project/Scripts/UI",
]

CS_FILES = [p for p in GUIDS if p.endswith(".cs")]
ASSET_FILES = [p for p in GUIDS if p.endswith(".asset")]
OTHER_FILES = [p for p in GUIDS if p.endswith(".asmdef")]


def folder_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def cs_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def asset_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def asmdef_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
AssemblyDefinitionImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def scene_content() -> str:
    return """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {fileID: 0}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 9
  m_Fog: 0
  m_FogColor: {r: 0.039215688, g: 0.05882353, b: 0.1254902, a: 1}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {r: 0.1, g: 0.12, b: 0.2, a: 1}
  m_AmbientEquatorColor: {r: 0.05, g: 0.06, b: 0.1, a: 1}
  m_AmbientGroundColor: {r: 0.02, g: 0.02, b: 0.04, a: 1}
  m_AmbientIntensity: 1
  m_AmbientMode: 3
  m_SubtractiveShadowColor: {r: 0.42, g: 0.478, b: 0.627, a: 1}
  m_SkyboxMaterial: {fileID: 0}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {fileID: 0}
  m_SpotCookie: {fileID: 10001, guid: 0000000000000000e000000000000000, type: 0}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {fileID: 0}
  m_Sun: {fileID: 0}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 12
  m_GIWorkflowMode: 1
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 0
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {fileID: 0}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_FinalGather: 0
    m_FinalGatherFiltering: 1
    m_FinalGatherRayCount: 256
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 1
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 1
    m_PVRCulling: 1
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 5
    m_PVRFilteringGaussRadiusAO: 2
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {fileID: 0}
  m_LightingSettings: {fileID: 0}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 3
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    buildHeightMesh: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {fileID: 0}
--- !u!1 &100000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 100001}
  - component: {fileID: 100002}
  - component: {fileID: 100003}
  - component: {fileID: 100004}
  m_Layer: 0
  m_Name: Main Camera
  m_TagString: MainCamera
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &100001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: -10}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!20 &100002
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 2
  m_BackGroundColor: {r: 0.039215688, g: 0.05882353, b: 0.1254902, a: 1}
  m_projectionMatrixMode: 1
  m_GateFitMode: 2
  m_FOVAxisMode: 0
  m_Iso: 200
  m_ShutterSpeed: 0.005
  m_Aperture: 16
  m_FocusDistance: 10
  m_FocalLength: 50
  m_BladeCount: 5
  m_Curvature: {x: 2, y: 11}
  m_BarrelClipping: 0.25
  m_Anamorphism: 0
  m_SensorSize: {x: 36, y: 24}
  m_LensShift: {x: 0, y: 0}
  m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1
  near clip plane: 0.3
  far clip plane: 1000
  field of view: 60
  orthographic: 1
  orthographic size: 5
  m_Depth: -1
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingPath: -1
  m_TargetTexture: {fileID: 0}
  m_TargetDisplay: 0
  m_TargetEye: 3
  m_HDR: 1
  m_AllowMSAA: 1
  m_AllowDynamicResolution: 0
  m_ForceIntoRT: 0
  m_OcclusionCulling: 1
  m_StereoConvergence: 10
  m_StereoSeparation: 0.022
--- !u!81 &100003
AudioListener:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  m_Enabled: 1
--- !u!114 &100004
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a79441f348de89743a2939f4d699eac1, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_RenderShadows: 1
  m_RequiresDepthTextureOption: 2
  m_RequiresOpaqueTextureOption: 2
  m_CameraType: 0
  m_Cameras: []
  m_RendererIndex: -1
  m_VolumeLayerMask:
    serializedVersion: 2
    m_Bits: 1
  m_VolumeTrigger: {fileID: 0}
  m_VolumeFrameworkUpdateModeOption: 2
  m_RenderPostProcessing: 0
  m_Antialiasing: 0
  m_AntialiasingQuality: 2
  m_StopNaN: 0
  m_Dithering: 0
  m_ClearDepth: 1
  m_AllowXRRendering: 1
  m_AllowHDROutput: 1
  m_UseScreenCoordOverride: 0
  m_ScreenSizeOverride: {x: 0, y: 0, z: 0, w: 0}
  m_ScreenCoordScaleBias: {x: 0, y: 0, z: 0, w: 0}
  m_RequiresDepthTexture: 0
  m_RequiresColorTexture: 0
  m_Version: 2
  m_TaaSettings:
    quality: 3
    frameInfluence: 0.1
    jitterScale: 1
    mipBias: 0
    varianceClampScale: 0.9
    contrastAdaptiveSharpening: 0
--- !u!1 &200000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 200001}
  - component: {fileID: 200002}
  m_Layer: 0
  m_Name: Global Light 2D
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &200001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 200000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &200002
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 200000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 073797afb82c5a1438f328866b10b3f0, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_ComponentVersion: 1
  m_LightType: 4
  m_BlendStyleIndex: 0
  m_FalloffIntensity: 0.5
  m_Color: {r: 0.6, g: 0.55, b: 0.8, a: 1}
  m_Intensity: 0.35
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
  m_PointLightInnerRadius: 0
  m_PointLightOuterRadius: 1
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
--- !u!1 &300000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 300001}
  - component: {fileID: 300002}
  m_Layer: 0
  m_Name: --- Game ---
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &300001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {fileID: 300011}
  - {fileID: 300021}
  - {fileID: 300031}
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &300002
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef08, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  keyManager: {fileID: 300012}
  uiManager: {fileID: 300032}
  eventBus: {fileID: 11400000, guid: e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6, type: 2}
--- !u!1 &300010
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 300011}
  - component: {fileID: 300012}
  m_Layer: 0
  m_Name: KeyManager
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &300011
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300010}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 300001}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &300012
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300010}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef04, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  ownedKeys: []
  currentActiveKey:
    keyName: 
    description: 
    abilityType: 0
    isActive: 0
    usesRemaining: 0
    cooldown: 0
    hasRisk: 0
    riskLevel: 0
  player: {fileID: 400012}
  uiManager: {fileID: 300032}
--- !u!1 &300020
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 300021}
  - component: {fileID: 300022}
  m_Layer: 0
  m_Name: GhostKey
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &300021
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300020}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 300001}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &300022
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300020}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef07, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  keyName: Ghost Key
  description: Phase through solid matter for a short time.
  phaseDuration: 5
  cooldown: 8
--- !u!1 &300030
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 300031}
  - component: {fileID: 300032}
  m_Layer: 5
  m_Name: UIManager
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &300031
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300030}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 300001}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &300032
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300030}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef06, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  keyManager: {fileID: 300012}
--- !u!1 &400000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 400001}
  - component: {fileID: 400012}
  - component: {fileID: 400013}
  - component: {fileID: 400014}
  - component: {fileID: 400015}
  m_Layer: 8
  m_Name: Player
  m_TagString: Player
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &400001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 400000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: -2, y: -1, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &400012
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 400000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef05, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  moveSpeed: 4
  jumpForce: 8
  ghostPhaseDuration: 5
  solidLayers:
    serializedVersion: 2
    m_Bits: 4294967295
--- !u!50 &400013
Rigidbody2D:
  serializedVersion: 4
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 400000}
  m_BodyType: 0
  m_Simulated: 1
  m_UseFullKinematicContacts: 0
  m_UseAutoMass: 0
  m_Mass: 1
  m_LinearDrag: 0
  m_AngularDrag: 0.05
  m_GravityScale: 3
  m_Material: {fileID: 0}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_Interpolate: 0
  m_SleepingMode: 1
  m_CollisionDetection: 0
  m_Constraints: 4
--- !u!61 &400014
BoxCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 400000}
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
  m_Size: {x: 0.6, y: 1.2}
  m_EdgeRadius: 0
--- !u!212 &400015
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 400000}
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
  m_SortingLayerID: 1234567893
  m_SortingLayer: 3
  m_SortingOrder: 0
  m_Sprite: {fileID: 10907, guid: 0000000000000000f000000000000000, type: 0}
  m_Color: {r: 0.7, g: 0.75, b: 0.9, a: 1}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {x: 1, y: 1}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
--- !u!1 &500000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 500001}
  - component: {fileID: 500002}
  - component: {fileID: 500003}
  m_Layer: 10
  m_Name: SealedDoor
  m_TagString: Interactable
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &500001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 500000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 3, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 2, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!61 &500002
BoxCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 500000}
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
--- !u!114 &500003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 500000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef09, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  puzzleID: chapter1_sealed_door
  isSolved: 0
  requiresSpecificKey: 1
  requiredKeyType: 0
  doorCollider: {fileID: 500002}
  passageTrigger: {fileID: 0}
--- !u!1 &600000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 600001}
  - component: {fileID: 600002}
  m_Layer: 11
  m_Name: Ground
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &600001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 600000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: -3, z: 0}
  m_LocalScale: {x: 20, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!61 &600002
BoxCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 600000}
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
"""


def write_meta(rel_path: str, content: str) -> None:
    path = ROOT / rel_path
    path.parent.mkdir(parents=True, exist_ok=True)
    meta_path = Path(str(path) + ".meta")
    meta_path.write_text(content)


def main() -> None:
    for folder in FOLDERS:
        (ROOT / folder).mkdir(parents=True, exist_ok=True)
        write_meta(folder, folder_meta(GUIDS[folder]))

    for rel in CS_FILES:
        write_meta(rel, cs_meta(GUIDS[rel]))

    for rel in ASSET_FILES:
        write_meta(rel, asset_meta(GUIDS[rel]))

    for rel in OTHER_FILES:
        write_meta(rel, asmdef_meta(GUIDS[rel]))

    scene_path = ROOT / "Assets/_Project/Scenes/Chapter1/Chapter1.unity"
    scene_path.parent.mkdir(parents=True, exist_ok=True)
    scene_path.write_text(scene_content())
    write_meta("Assets/_Project/Scenes/Chapter1/Chapter1.unity", asset_meta(GUIDS["Assets/_Project/Scenes/Chapter1/Chapter1.unity"]))

    # Placeholder keep files for empty content folders
    for folder in ["Assets/_Project/Art", "Assets/_Project/Audio", "Assets/_Project/Prefabs", "Assets/_Project/ScriptableObjects", "Assets/_Project/UI", "Assets/_Project/Scripts/Narrative"]:
        keep = ROOT / folder / ".gitkeep"
        if not keep.exists():
            keep.write_text("")

    print("Generated Unity meta files and Chapter1 scene.")


if __name__ == "__main__":
    main()