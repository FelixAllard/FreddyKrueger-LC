using GraphicsAPI;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace ExampleEnemy;

[HarmonyPatch(typeof (HUDManager))]
public class PostProccessPatchFreddy
{
    [HarmonyPatch(typeof(HUDManager), "Awake")]
    [HarmonyPostfix]
    private static void Awake(ref HUDManager __instance)
    {
        if ((bool)(Object)__instance.gameObject.GetComponent<PostProcessVisualsFreddy>())
        {
            return; // Already has the component
        }

        // Add the component and access fullScreenPass here
        PostProcessVisualsFreddy freddyComponent = __instance.gameObject.AddComponent<PostProcessVisualsFreddy>();
        // Use freddyComponent.fullScreenPass if needed within the patch
    }
}