using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using static LethalLib.Modules.Levels;
using static LethalLib.Modules.Enemies;
using BepInEx.Logging;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using GameNetcodeStuff;
using Unity.Netcode;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;


namespace FreddyKrueger {
    //Yup! Forgor to put this here
    [BepInDependency("LethalNetworkAPI")]
    [BepInDependency("com.sigurd.csync")] 
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)] 
    public class Plugin : BaseUnityPlugin
    {
        
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static EnemyType ExampleEnemy;
        internal static new ManualLogSource Logger;
        private FreddyConfig _configuration;

        private void Awake() {
            Logger = base.Logger;
            _configuration = new FreddyConfig(Config);
            Assets.PopulateAssets();
            

            ExampleEnemy = Assets.FreddyKruegerAssetBundle.LoadAsset<EnemyType>("FreddyKrueger");
            var tlTerminalNode = Assets.FreddyKruegerAssetBundle.LoadAsset<TerminalNode>("FreddyTN");
            var tlTerminalKeyword = Assets.FreddyKruegerAssetBundle.LoadAsset<TerminalKeyword>("FreddyTK");
            
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
            Debug.Log("PATCHING THE START");
            harmony.PatchAll(typeof(RoundManagerBeginEnemySpawningPatch));
            harmony.PatchAll(typeof(EndOfRoundFixes));
            //harmony.PatchAll(typeof(NetworkObjectSimpleFixFixes));
            //ClawMark
            Debug.Log("                                                                                                                             \n                                                                    \u2591                                                        \n                                                                   \u2592\u2591                                                        \n                                                                   \u2592\u2591              \u2591\u2591                                        \n                                                                   \u2592\u2591              \u2591\u2591                                        \n                                                                  \u2592\u2592\u2591              \u2591\u2591                                        \n                                                                  \u2592\u2591\u2592             \u2591\u2591\u2591                                        \n                                                                  \u2592\u2591\u2593             \u2591\u2591\u2591                                        \n                                                                 \u2592\u2592\u2592              \u2591\u2591\u2591                                        \n                                                                 \u2592\u2591\u2592             \u2591\u2591\u2591               \u2591\u2591                        \n                                                                \u2592\u2591\u2592\u2591             \u2591\u2592\u2591              \u2592\u2592\u2591                        \n                                                                \u2591\u2592\u2593             \u2592\u2593\u2592\u2591             \u2592\u2592\u2591                         \n                                                               \u2591\u2591\u2592\u2591            \u2592\u2593\u2592\u2592             \u2592\u2591\u2591\u2591                         \n                                                               \u2591\u2592\u2593            \u2592\u2592\u2593\u2592            \u2591\u2592\u2591\u2591\u2592                          \n                                                              \u2591\u2591\u2592            \u2592\u2592\u2588\u2592            \u2591\u2591\u2591\u2591\u2592                           \n                                                             \u2591\u2591\u2592\u2593           \u2592\u2592\u2593\u2593\u2592           \u2591\u2591\u2591\u2591\u2592               \u2593            \n                                                            \u2591\u2591\u2592\u2593           \u2592\u2592\u2593\u2593\u2592          \u2591\u2591\u2591\u2591\u2591\u2592              \u2593\u2593\u2592            \n                                               \u2588            \u2591\u2592\u2593\u2593          \u2592\u2593\u2593\u2593\u2592          \u2591\u2591\u2591\u2591\u2592\u2592            \u2588\u2588\u2593\u2592              \n                                         \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593       \u2593\u2592\u2593\u2588          \u2592\u2593\u2588\u2593\u2592         \u2591\u2591\u2591\u2591\u2591\u2592            \u2588\u2588\u2592\u2592\u2591               \n                                      \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593    \u2588\u2588\u2593\u2593\u2588          \u2592\u2593\u2588\u2593\u2592         \u2591\u2591\u2591\u2591\u2592\u2593           \u2588\u2588\u2592\u2592\u2592                 \n                                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593   \u2588\u2588\u2588\u2588\u2588\u2592         \u2592\u2593\u2593\u2592\u2592        \u2591\u2591\u2591\u2591\u2593\u2593          \u2588\u2588\u2588\u2592\u2591\u2592\u2592                  \n                              \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588    \u2588\u2588\u2588\u2588\u2588\u2593       \u2592\u2593\u2593\u2593\u2593\u2592\u2593      \u2591\u2591\u2591\u2591\u2591\u2592\u2593          \u2588\u2588\u2593\u2592\u2592\u2592\u2592                    \n                          \u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588     \u2588\u2588\u2593\u2588\u2588\u2588\u2588      \u2593\u2588\u2588\u2588\u2593\u2593\u2592\u2592     \u2591\u2591\u2591\u2591\u2591\u2592\u2593         \u2588\u2588\u2593\u2592\u2592\u2593\u2593                       \n                        \u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588       \u2588\u2588\u2588\u2593\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2593\u2592\u2591\u2591\u2591\u2591\u2592      \u2588\u2588\u2588\u2593\u2593\u2592\u2592\u2593\u2593                          \n                     \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588       \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588 \u2588\u2588\u2588\u2593\u2591\u2591     \u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2592\u2593                              \n              \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588   \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588                                 \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593                                     \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                         \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                               \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                                      \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                                           \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2591\u2591                                                              \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2592\u2591\u2591\u2592\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2592\u2593\u2593                                                                  \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2591\u2591\u2591\u2591\u2592\u2592\u2592\u2593\u2588\u2588\u2593\u2593\u2593\u2593\u2588                                                                       \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588                                                                           \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2593                                                                                          \n                  \u2588\u2588\u2588                                                                                                        \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             ");
        }
    }

    public static class Assets {
        public static AssetBundle FreddyKruegerAssetBundle = null;
        public static void PopulateAssets() {
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            FreddyKruegerAssetBundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "freddymodassets"));
            if (FreddyKruegerAssetBundle == null) {
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
                    RoundManager.Instance.SpawnEnemyGameObject(new Vector3(82f,2f,55f), 0f, +1,
                        Assets.FreddyKruegerAssetBundle.LoadAsset<EnemyType>("FreddyKrueger"));

                }
                else
                {
                    UnityEngine.Object.FindObjectOfType<FreddyAI>().ReinitialiseList();
                }
            }

        }
    }

    [HarmonyPatch(typeof(RoundManager))]
    internal class EndOfRoundFixes
    {
        [HarmonyPatch(nameof(RoundManager.BeginEnemySpawning))]
        [HarmonyPostfix]
        private static void PostfixSpawn()
        {
            if (RoundManager.Instance.IsServer)
            {
                if (!UnityEngine.Object.FindObjectOfType<FreddyAI>())
                {
                    RoundManager.Instance.SpawnEnemyGameObject(new Vector3(0f,200f,0f), 0f, +1,
                        Assets.FreddyKruegerAssetBundle.LoadAsset<EnemyType>("FreddyKrueger"));
                }
                else
                {
                    UnityEngine.Object.FindObjectOfType<FreddyAI>().ReinitialiseList();
                }
            }
        }
        
        /*[HarmonyPatch(nameof(RoundManager.UnloadSceneObjectsEarly))]
        [HarmonyPostfix]
        private static void PostfixNoDespawn()
        {
            if (RoundManager.Instance.IsServer)
            {
                UnityEngine.Object.FindObjectOfType<FreddyAI>().ReinitialiseList();
            }
        }*/
        
        [HarmonyPatch(nameof(RoundManager.UnloadSceneObjectsEarly))]
        [HarmonyPrefix]
        private static bool Prefix()
        {
            if (!RoundManager.Instance.IsServer)
                return false;
            Debug.Log((object) "Despawning props and enemies #3 MODIFIED CLASS :D");
            RoundManager.Instance.isSpawningEnemies = false;
            EnemyAI[] findObjectsOfType = UnityEngine.Object.FindObjectsOfType<EnemyAI>();
            EnemyAI freddy = UnityEngine.Object.FindObjectOfType<FreddyAI>();
            
            Debug.Log((object) string.Format("Enemies on map: {0}", (object) findObjectsOfType.Length));
            for (int index = 0; index < findObjectsOfType.Length; ++index)
            {
                if (findObjectsOfType[index].thisNetworkObject.IsSpawned && freddy.enemyType.ToString() != findObjectsOfType[index].enemyType.ToString())
                {
                    Debug.Log((object) ("Despawning enemy: " + findObjectsOfType[index].gameObject.name));
                    findObjectsOfType[index].thisNetworkObject.Despawn();
                }
                else
                    Debug.Log((object) string.Format("{0} was not spawned on network, so it could not be removed. OR MAYBE IT WAS FREDDY", (object) findObjectsOfType[index].thisNetworkObject));
            }
            RoundManager.Instance.SpawnedEnemies.Clear();
            RoundManager.Instance.currentEnemyPower = 0;
            RoundManager.Instance.currentDaytimeEnemyPower = 0;
            RoundManager.Instance.currentOutsideEnemyPower = 0;
            if (RoundManager.Instance.IsServer)
            {
                UnityEngine.Object.FindObjectOfType<FreddyAI>().ReinitialiseList();
            }
            return false;
        }
        
        
        
        

    }
    /*
    [HarmonyPatch(typeof(NetworkObject))]
    internal class NetworkObjectSimpleFixFixes
    {
        [HarmonyPatch(nameof(NetworkObject.Despawn))]
        [HarmonyPrefix]
        private static bool Prefix()
        {
            EnemyAI[] objectsOfType = UnityEngine.Object.FindObjectsOfType<EnemyAI>();

            // Check if a FreddyAI instance is in the first place of the array
            if (objectsOfType.Length > 0)
            {
                if (objectsOfType[0] == UnityEngine.Object.FindObjectOfType<FreddyAI>())
                { 
                    Debug.Log("Are we stuck?");
                    return false;
                }   
            }
            Debug.Log("Not Freddy - Good to go!");
            return true;
        }
    }
    */
}