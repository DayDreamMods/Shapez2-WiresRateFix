namespace WiresRateFix

open BepInEx
open HarmonyLib
open BepInEx.Logging
open Utils.Operators

module WiresRateFix =
    let mutable internal Plugin: BaseUnityPlugin option = None
    let mutable internal Logger: ManualLogSource option = None
    let mutable internal Patcher: Harmony option = None
open WiresRateFix

[<BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)>]
type WiresRateFixPlugin() =
    inherit BaseUnityPlugin()

    member this.Awake() =
        Plugin <- Some this
        Logger <- Some this.Logger
        Patcher <- Harmony(this.Info.Metadata.GUID) |> Some

        -optional {
            let! patcher = Patcher
            let! logger = Logger
            do patcher.PatchAll()
            let patchedMethodCount = patcher.GetPatchedMethods() |> Array.ofSeq |> (_.Length)
            let patchedMethodSuffix = if patchedMethodCount = 1 then "" else "s"
            do 
                $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} loaded! Successfully patched {patchedMethodCount} method{patchedMethodSuffix}."
                |> logger.LogInfo
            return ()
        } 
        
[<HarmonyPatch(typeof<SimulationGraph>)>]
module SimulationGraphPatcher =
    [<HarmonyPatch("<Update>g__UpdateCluster|29_1")>] [<HarmonyPrefix>]
    let PrepareStartOptionsOverride (requiredDeltaTicks: int byref, __instance: SimulationGraph) =
        requiredDeltaTicks <- 0
        ()
        