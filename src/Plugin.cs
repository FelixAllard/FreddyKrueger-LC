using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BepInEx;
using LethalLib.Modules;
using BepInEx.Logging;
using System.IO;
using HarmonyLib;

namespace ExampleEnemy {
    [BepInDependency("com.sigurd.csync")] 
    [BepInPlugin(ModGUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)] 
    
    public class Plugin : BaseUnityPlugin {
        // It is a good idea for our GUID to be more unique than only the plugin name. Notice that it is used in the BepInPlugin attribute.
        // The GUID is also used for the config file name by default.
        public const string ModGUID = "hamunii." + PluginInfo.PLUGIN_NAME;
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        internal static new ManualLogSource Logger;
        public static AssetBundle FreddyModAssets;
        
        private FreddyConfig _configuration;

        private void Awake() {
            Logger = base.Logger;
            _configuration = new FreddyConfig(Config);

            // If you don't want your mod to use a configuration file, you can remove this line, Configuration.cs, and other references.

            // This should be ran before Network Prefabs are registered.
            InitializeNetworkBehaviours();

            // We load the asset bundle that should be next to our DLL file, with the specified name.
            // You may want to rename your asset bundle from the AssetBundle Browser in order to avoid an issue with
            // asset bundle identifiers being the same between multiple bundles, allowing the loading of only one bundle from one mod.
            // In that case also remember to change the asset bundle copying code in the csproj.user file.
            var bundleName = "freddymodassets";
            FreddyModAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), bundleName));
            if (FreddyModAssets == null) {
                Logger.LogError($"Failed to load custom assets.");
                return;
            }

            // We load our assets from our asset bundle. Remember to rename them both here and in our Unity project.
            var FreddyKrueger = FreddyModAssets.LoadAsset<EnemyType>("FreddyKrueger");
            var FreddyEnemyTN = FreddyModAssets.LoadAsset<TerminalNode>("FreddyTN");
            var FreddyEnemyTK = FreddyModAssets.LoadAsset<TerminalKeyword>("FreddyTK");
            
            // Network Prefabs need to be registered. See https://docs-multiplayer.unity3d.com/netcode/current/basics/object-spawning/
            // LethalLib registers prefabs on GameNetworkManager.Start.
            NetworkPrefabs.RegisterNetworkPrefab(FreddyKrueger.enemyPrefab);
			Enemies.RegisterEnemy(FreddyKrueger, 99999, Levels.LevelTypes.All, Enemies.SpawnType.Outside, FreddyEnemyTN, FreddyEnemyTK);
            harmony.PatchAll(typeof(EndOfRoundFixes));
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Debug.Log("                                                                                                                             \n                                                                    \u2591                                                        \n                                                                   \u2592\u2591                                                        \n                                                                   \u2592\u2591              \u2591\u2591                                        \n                                                                   \u2592\u2591              \u2591\u2591                                        \n                                                                  \u2592\u2592\u2591              \u2591\u2591                                        \n                                                                  \u2592\u2591\u2592             \u2591\u2591\u2591                                        \n                                                                  \u2592\u2591\u2593             \u2591\u2591\u2591                                        \n                                                                 \u2592\u2592\u2592              \u2591\u2591\u2591                                        \n                                                                 \u2592\u2591\u2592             \u2591\u2591\u2591               \u2591\u2591                        \n                                                                \u2592\u2591\u2592\u2591             \u2591\u2592\u2591              \u2592\u2592\u2591                        \n                                                                \u2591\u2592\u2593             \u2592\u2593\u2592\u2591             \u2592\u2592\u2591                         \n                                                               \u2591\u2591\u2592\u2591            \u2592\u2593\u2592\u2592             \u2592\u2591\u2591\u2591                         \n                                                               \u2591\u2592\u2593            \u2592\u2592\u2593\u2592            \u2591\u2592\u2591\u2591\u2592                          \n                                                              \u2591\u2591\u2592            \u2592\u2592\u2588\u2592            \u2591\u2591\u2591\u2591\u2592                           \n                                                             \u2591\u2591\u2592\u2593           \u2592\u2592\u2593\u2593\u2592           \u2591\u2591\u2591\u2591\u2592               \u2593            \n                                                            \u2591\u2591\u2592\u2593           \u2592\u2592\u2593\u2593\u2592          \u2591\u2591\u2591\u2591\u2591\u2592              \u2593\u2593\u2592            \n                                               \u2588            \u2591\u2592\u2593\u2593          \u2592\u2593\u2593\u2593\u2592          \u2591\u2591\u2591\u2591\u2592\u2592            \u2588\u2588\u2593\u2592              \n                                         \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593       \u2593\u2592\u2593\u2588          \u2592\u2593\u2588\u2593\u2592         \u2591\u2591\u2591\u2591\u2591\u2592            \u2588\u2588\u2592\u2592\u2591               \n                                      \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593    \u2588\u2588\u2593\u2593\u2588          \u2592\u2593\u2588\u2593\u2592         \u2591\u2591\u2591\u2591\u2592\u2593           \u2588\u2588\u2592\u2592\u2592                 \n                                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593   \u2588\u2588\u2588\u2588\u2588\u2592         \u2592\u2593\u2593\u2592\u2592        \u2591\u2591\u2591\u2591\u2593\u2593          \u2588\u2588\u2588\u2592\u2591\u2592\u2592                  \n                              \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588    \u2588\u2588\u2588\u2588\u2588\u2593       \u2592\u2593\u2593\u2593\u2593\u2592\u2593      \u2591\u2591\u2591\u2591\u2591\u2592\u2593          \u2588\u2588\u2593\u2592\u2592\u2592\u2592                    \n                          \u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588     \u2588\u2588\u2593\u2588\u2588\u2588\u2588      \u2593\u2588\u2588\u2588\u2593\u2593\u2592\u2592     \u2591\u2591\u2591\u2591\u2591\u2592\u2593         \u2588\u2588\u2593\u2592\u2592\u2593\u2593                       \n                        \u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588       \u2588\u2588\u2588\u2593\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2593\u2592\u2591\u2591\u2591\u2591\u2592      \u2588\u2588\u2588\u2593\u2593\u2592\u2592\u2593\u2593                          \n                     \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588       \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588 \u2588\u2588\u2588\u2593\u2591\u2591     \u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2592\u2593                              \n              \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588   \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588                                 \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593                                     \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                         \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                               \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                                      \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                                           \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2591\u2591                                                              \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2592\u2591\u2591\u2592\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2592\u2593\u2593                                                                  \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2591\u2591\u2591\u2591\u2592\u2592\u2592\u2593\u2588\u2588\u2593\u2593\u2593\u2593\u2588                                                                       \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588                                                                           \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2593                                                                                          \n                  \u2588\u2588\u2588                                                                                                        \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             ");
        }

        private static void InitializeNetworkBehaviours() {
            // See https://github.com/EvaisaDev/UnityNetcodePatcher?tab=readme-ov-file#preparing-mods-for-patching
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
        } 
        [HarmonyPatch(typeof(RoundManager))]
        internal class EndOfRoundFixes
        {
            [HarmonyPatch(nameof(RoundManager.BeginEnemySpawning))]
            [HarmonyPostfix]
            private static void PostfixSpawn()
            {
                if (RoundManager.Instance.IsServer && RoundManager.Instance.allEnemyVents.Length>0)
                {
                    Debug.Log("Freddy is here! Wait till you sleep tho...");
                    var allEnemiesList = new List<SpawnableEnemyWithRarity>();
                    allEnemiesList.AddRange(RoundManager.Instance.currentLevel.Enemies);
                    allEnemiesList.AddRange(RoundManager.Instance.currentLevel.OutsideEnemies);
                    var enemyToSpawn = allEnemiesList.Find(x => x.enemyType.enemyName.Equals("FreddyKrueger"));
                    
                    Debug.Log(enemyToSpawn.enemyType.enemyName + "This is the name!");
                    //EnemyAI.Instantiate()
                    RoundManager.Instance.SpawnEnemyGameObject(RoundManager.Instance.allEnemyVents[0].transform.position, 0f,RoundManager.Instance.currentLevel.OutsideEnemies.IndexOf(enemyToSpawn),enemyToSpawn.enemyType);
                    /*UnityEngine.Object.Instantiate(enemyToSpawn.enemyType.enemyPrefab.gameObject,
                        RoundManager.Instance.allEnemyVents[0].transform.position,
                        Quaternion.identity);*/
                
                }
            }
        }
    }
}