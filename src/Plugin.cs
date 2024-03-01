﻿using System.Reflection;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using LethalLib.Modules;
using static LethalLib.Modules.Levels;
using static LethalLib.Modules.Enemies;
using BepInEx.Logging;
using System.IO;

namespace FreddyKrueger {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)] 
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static EnemyType ExampleEnemy;
        internal static new ManualLogSource Logger;

        private void Awake() {
            Logger = base.Logger;
            Assets.PopulateAssets();

            ExampleEnemy = Assets.MainAssetBundle.LoadAsset<EnemyType>("FreddyKrueger");
            var tlTerminalNode = Assets.MainAssetBundle.LoadAsset<TerminalNode>("FreddyTN");
            var tlTerminalKeyword = Assets.MainAssetBundle.LoadAsset<TerminalKeyword>("FreddyTK");
            
            // Network Prefabs need to be registered first. See https://docs-multiplayer.unity3d.com/netcode/current/basics/object-spawning/
            NetworkPrefabs.RegisterNetworkPrefab(ExampleEnemy.enemyPrefab);
			RegisterEnemy(ExampleEnemy, 999999999, LevelTypes.All, SpawnType.Outside, tlTerminalNode, tlTerminalKeyword);
            
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            // Required by https://github.com/EvaisaDev/UnityNetcodePatcher maybe?
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
            harmony.PatchAll(typeof(RoundManagerBeginEnemySpawningPatch));
            //ClawMark
            Debug.Log("                                                                                                                             \n                                                                    \u2591                                                        \n                                                                   \u2592\u2591                                                        \n                                                                   \u2592\u2591              \u2591\u2591                                        \n                                                                   \u2592\u2591              \u2591\u2591                                        \n                                                                  \u2592\u2592\u2591              \u2591\u2591                                        \n                                                                  \u2592\u2591\u2592             \u2591\u2591\u2591                                        \n                                                                  \u2592\u2591\u2593             \u2591\u2591\u2591                                        \n                                                                 \u2592\u2592\u2592              \u2591\u2591\u2591                                        \n                                                                 \u2592\u2591\u2592             \u2591\u2591\u2591               \u2591\u2591                        \n                                                                \u2592\u2591\u2592\u2591             \u2591\u2592\u2591              \u2592\u2592\u2591                        \n                                                                \u2591\u2592\u2593             \u2592\u2593\u2592\u2591             \u2592\u2592\u2591                         \n                                                               \u2591\u2591\u2592\u2591            \u2592\u2593\u2592\u2592             \u2592\u2591\u2591\u2591                         \n                                                               \u2591\u2592\u2593            \u2592\u2592\u2593\u2592            \u2591\u2592\u2591\u2591\u2592                          \n                                                              \u2591\u2591\u2592            \u2592\u2592\u2588\u2592            \u2591\u2591\u2591\u2591\u2592                           \n                                                             \u2591\u2591\u2592\u2593           \u2592\u2592\u2593\u2593\u2592           \u2591\u2591\u2591\u2591\u2592               \u2593            \n                                                            \u2591\u2591\u2592\u2593           \u2592\u2592\u2593\u2593\u2592          \u2591\u2591\u2591\u2591\u2591\u2592              \u2593\u2593\u2592            \n                                               \u2588            \u2591\u2592\u2593\u2593          \u2592\u2593\u2593\u2593\u2592          \u2591\u2591\u2591\u2591\u2592\u2592            \u2588\u2588\u2593\u2592              \n                                         \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593       \u2593\u2592\u2593\u2588          \u2592\u2593\u2588\u2593\u2592         \u2591\u2591\u2591\u2591\u2591\u2592            \u2588\u2588\u2592\u2592\u2591               \n                                      \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593    \u2588\u2588\u2593\u2593\u2588          \u2592\u2593\u2588\u2593\u2592         \u2591\u2591\u2591\u2591\u2592\u2593           \u2588\u2588\u2592\u2592\u2592                 \n                                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593   \u2588\u2588\u2588\u2588\u2588\u2592         \u2592\u2593\u2593\u2592\u2592        \u2591\u2591\u2591\u2591\u2593\u2593          \u2588\u2588\u2588\u2592\u2591\u2592\u2592                  \n                              \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588    \u2588\u2588\u2588\u2588\u2588\u2593       \u2592\u2593\u2593\u2593\u2593\u2592\u2593      \u2591\u2591\u2591\u2591\u2591\u2592\u2593          \u2588\u2588\u2593\u2592\u2592\u2592\u2592                    \n                          \u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588     \u2588\u2588\u2593\u2588\u2588\u2588\u2588      \u2593\u2588\u2588\u2588\u2593\u2593\u2592\u2592     \u2591\u2591\u2591\u2591\u2591\u2592\u2593         \u2588\u2588\u2593\u2592\u2592\u2593\u2593                       \n                        \u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588       \u2588\u2588\u2588\u2593\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2593\u2592\u2591\u2591\u2591\u2591\u2592      \u2588\u2588\u2588\u2593\u2593\u2592\u2592\u2593\u2593                          \n                     \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588       \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588 \u2588\u2588\u2588\u2593\u2591\u2591     \u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2592\u2593                              \n              \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588   \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588                                 \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593                                     \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                         \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                               \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                                      \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                                           \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2591\u2591                                                              \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2592\u2591\u2591\u2592\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2592\u2593\u2593                                                                  \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2591\u2591\u2591\u2591\u2592\u2592\u2592\u2593\u2588\u2588\u2593\u2593\u2593\u2593\u2588                                                                       \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588                                                                           \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2593                                                                                          \n                  \u2588\u2588\u2588                                                                                                        \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             ");
        }
    }

    public static class Assets {
        public static AssetBundle MainAssetBundle = null;
        public static void PopulateAssets() {
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            MainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "modassets"));
            if (MainAssetBundle == null) {
                Plugin.Logger.LogError("Failed to load custom assets.");
                return;
            }
        }
    }
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerBeginEnemySpawningPatch
    {
        [HarmonyPatch(nameof(RoundManager.BeginEnemySpawning))]
        [HarmonyPostfix]
        private static void PostfixSpawn()
        {
            if (RoundManager.Instance.IsServer)
            {
                if (!UnityEngine.Object.FindObjectOfType<FreddyAI>())
                {
                    RoundManager.Instance.SpawnEnemyGameObject(Vector3.up, 0f, +1,
                        Assets.MainAssetBundle.LoadAsset<EnemyType>("FreddyKrueger"));

                }
                else
                {
                    UnityEngine.Object.FindObjectOfType<FreddyAI>().ReinitialiseList();
                }
            }

        }

        [HarmonyPatch(nameof(RoundManager.UnloadSceneObjectsEarly))]
        [HarmonyPostfix]
        private static void PostfixNoDespawn()
        {
            if (RoundManager.Instance.IsServer)
            {
                UnityEngine.Object.FindObjectOfType<FreddyAI>().ReinitialiseList();
            }
        }
    }
}