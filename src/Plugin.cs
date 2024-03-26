using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using BepInEx;
using LethalLib.Modules;
using BepInEx.Logging;
using System.IO;
using System.Security.Cryptography;
using GraphicsAPI.CustomPostProcessing;
using HarmonyLib;
using UnityEngine.Rendering.HighDefinition;
using Debug = UnityEngine.Debug;

namespace ExampleEnemy {
    [BepInDependency("LethalNetworkAPI")]
    [BepInDependency("com.sigurd.csync")] 
    [BepInPlugin(ModGUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)] 
    
    
    public class Plugin : BaseUnityPlugin {
        // It is a good idea for our GUID to be more unique than only the plugin name. Notice that it is used in the BepInPlugin attribute.
        // The GUID is also used for the config file name by default.
        public const string ModGUID = "Xilef992." + PluginInfo.PLUGIN_NAME;
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
            Debug.Log("                                            \u2591\u2591                                                     \n                                            \u2591\u2591\u2591\u2591\u2591                                                  \n                                              \u2591\u2591\u2591\u2591                                                 \n                                               \u2591\u2591\u2591\u2591                                                \n                                                \u2591\u2591\u2591\u2591                                               \n                                                 \u2591\u2591\u2591\u2591                                              \n                                                  \u2591\u2591\u2591\u2591                                             \n                                                   \u2591\u2591\u2591\u2591                                            \n                                                   \u2591\u2591\u2591\u2591\u2591                                           \n                                                    \u2591\u2591\u2591\u2591                                           \n                                                     \u2591\u2591\u2591\u2591                                          \n                                                      \u2591\u2591\u2591\u2591                                         \n                                                      \u2591\u2591\u2591\u2591                                         \n                                                       \u2591\u2591                                          \n                                                       \u2591\u2591\u2591                                         \n                                                        \u2591\u2591\u2591\u2591                                       \n                                                        \u2591\u2591\u2591\u2591                                       \n                                                        \u2591\u2591\u2591\u2591\u2591                                      \n                \u2591\u2591\u2592\u2592\u2591\u2591\u2591\u2591                                 \u2591\u2592\u2591\u2591\u2591                                     \n             \u2591\u2591\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591                          \u2591\u2591\u2591\u2591\u2591                                     \n                     \u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591                     \u2592\u2591\u2591\u2591\u2591\u2591                                    \n                            \u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591                \u2592\u2592\u2592\u2592\u2592\u2591                                    \n                                 \u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591           \u2591\u2592\u2592\u2592\u2592\u2592\u2592\u2592                                    \n                                    \u2591\u2591\u2591\u2591\u2592\u2592\u2592\u2592\u2592\u2592\u2591        \u2592\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2591                                   \n                                       \u2591\u2591\u2591\u2592\u2592\u2592\u2592\u2592\u2592\u2591\u2591\u2591    \u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592                                   \n                                          \u2591\u2592\u2592\u2593\u2593\u2592\u2592\u2592\u2592\u2591\u2591\u2591\u2592\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2591                                  \n                                            \u2592\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2593\u2593\u2592\u2591                                  \n                                             \u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2591                                 \n                                              \u2591\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2593\u2593\u2592\u2591                                 \n                                              \u2591\u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2593\u2593\u2592\u2591                                \n                                                \u2592\u2592\u2592\u2593\u2593\u2592\u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2593\u2593\u2592\u2592\u2592                               \n                                                \u2592\u2593\u2588\u2588\u2588\u2588\u2592\u2591\u2592\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2593\u2593\u2593\u2593\u2592                              \n                                                 \u2591\u2592\u2593\u2593\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2593\u2593\u2593\u2592\u2591                             \n                                              \u2591\u2591\u2592\u2592\u2593\u2593\u2593\u2593\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2592\u2591\u2592\u2593\u2593\u2592\u2591                            \n               \u2591\u2591\u2591\u2591\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2592\u2588\u2588\u2588\u2593\u2591\u2591\u2592\u2593\u2593\u2592                            \n        \u2591\u2591\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2591\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2593\u2593\u2593\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2588\u2588\u2588\u2593\u2591\u2591\u2592\u2593\u2593\u2592\u2591                           \n   \u2591\u2591\u2591\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2592\u2591\u2591\u2591\u2591\u2591\u2591\u2591             \u2591\u2592\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2588\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2593\u2592\u2591\u2591\u2593\u2593\u2593\u2592\u2591                          \n \u2591\u2591\u2592\u2591\u2592\u2591\u2591\u2591\u2591\u2591                          \u2591\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2592\u2592\u2588\u2588\u2588\u2588\u2588\u2593\u2592\u2591\u2593\u2593\u2593\u2588\u2588\u2593\u2591\u2591\u2592\u2593\u2593\u2593\u2592                          \n                                       \u2591\u2592\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2593\u2592\u2593\u2588\u2588\u2593\u2593\u2588\u2593\u2592\u2592\u2592\u2593\u2593\u2593\u2593\u2591                         \n                                            \u2591\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2592\u2592\u2592\u2592\u2588\u2588\u2593\u2592\u2591                        \n                                                     \u2591\u2592\u2593\u2593\u2593\u2588\u2593\u2588\u2588\u2593\u2588\u2588\u2593\u2592\u2592\u2592\u2592\u2593\u2593\u2593\u2593\u2592\u2591                       \n                                                  \u2591\u2591 \u2591\u2592\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2593\u2593\u2591                       \n                                 \u2591\u2592\u2593\u2592\u2591        \u2591\u2593\u2588\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2593\u2592\u2592\u2593\u2593\u2593\u2593\u2593\u2591                       \n                                 \u2592\u2593\u2593\u2593\u2593\u2593\u2591\u2591\u2591\u2592\u2593\u2588\u2588\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2588\u2593\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593                       \n                                 \u2591\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2588\u2593\u2588\u2593\u2588\u2588\u2588\u2593\u2592\u2592\u2593\u2593\u2593\u2593                       \n                               \u2591\u2591\u2592\u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2588\u2588\u2593\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2593\u2588\u2593\u2588\u2588\u2588\u2593\u2592\u2592\u2593\u2593\u2593\u2592                       \n                           \u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2592\u2593\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2593\u2593\u2593\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2591                       \n                       \u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591\u2591   \u2593\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2592\u2592\u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2593\u2593\u2593\u2593\u2593\u2592\u2593\u2593\u2593\u2592                       \n                    \u2591\u2591\u2591\u2591\u2591\u2591\u2591          \u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2591                      \n                \u2591\u2591\u2591\u2591\u2591\u2591\u2591              \u2591\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2591\u2591\u2593\u2593\u2593\u2592\u2591                     \n             \u2591\u2591\u2591\u2591\u2591\u2591\u2591                  \u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2593\u2593\u2593\u2593\u2591                     \n           \u2591\u2591\u2591\u2591\u2591                      \u2591\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2591                    \n        \u2591\u2591\u2591\u2591\u2591                           \u2591\u2593\u2593\u2593\u2593\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2591                    \n      \u2591\u2591\u2591\u2591                               \u2591\u2592\u2593\u2593\u2593\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2591                   \n     \u2591\u2591\u2591                                  \u2591\u2593\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2591                   \n                                           \u2591\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2592                   \n                                             \u2591\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2588\u2593\u2588\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2593\u2592\u2593\u2593\u2593\u2592\u2591                   \n                                              \u2591\u2592\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2593\u2593\u2593\u2593\u2588\u2593\u2588\u2588\u2588\u2593\u2588\u2588\u2593\u2593\u2588\u2593\u2592\u2591                   \n                                                 \u2592\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2591                  \n                                                  \u2591\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2588\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2591                 \n                                                   \u2591\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2593\u2588\u2593                 \n                                                    \u2592\u2593\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593                \n                                                    \u2591\u2593\u2593\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592               \n                                                    \u2591\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2591              \n                                                     \u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2592              \n                                                     \u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593              \n                                                      \u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2591\u2593\u2591             \n                                                      \u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2593\u2593\u2588\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2592\u2593\u2592             \n                                                     \u2591\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2591             \n                                                       \u2592\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593             \n                                                      \u2591\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2592            \n                                                      \u2591\u2593\u2593\u2593\u2588\u2588\u2593\u2588\u2593\u2588\u2593\u2588\u2593\u2588\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2591           \n                                                      \u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2593\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2591          \n                                                    \u2591\u2592\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2591          \n                                                   \u2591\u2592\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2591          \n                                                   \u2592\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592         \n                                                  \u2591\u2593\u2588\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2592\u2591      \n                                                  \u2591\u2592\u2593\u2593\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2591     ");
            //Debug.Log("                                                                                                                             \n                                                                    \u2591                                                        \n                                                                   \u2592\u2591                                                        \n                                                                   \u2592\u2591              \u2591\u2591                                        \n                                                                   \u2592\u2591              \u2591\u2591                                        \n                                                                  \u2592\u2592\u2591              \u2591\u2591                                        \n                                                                  \u2592\u2591\u2592             \u2591\u2591\u2591                                        \n                                                                  \u2592\u2591\u2593             \u2591\u2591\u2591                                        \n                                                                 \u2592\u2592\u2592              \u2591\u2591\u2591                                        \n                                                                 \u2592\u2591\u2592             \u2591\u2591\u2591               \u2591\u2591                        \n                                                                \u2592\u2591\u2592\u2591             \u2591\u2592\u2591              \u2592\u2592\u2591                        \n                                                                \u2591\u2592\u2593             \u2592\u2593\u2592\u2591             \u2592\u2592\u2591                         \n                                                               \u2591\u2591\u2592\u2591            \u2592\u2593\u2592\u2592             \u2592\u2591\u2591\u2591                         \n                                                               \u2591\u2592\u2593            \u2592\u2592\u2593\u2592            \u2591\u2592\u2591\u2591\u2592                          \n                                                              \u2591\u2591\u2592            \u2592\u2592\u2588\u2592            \u2591\u2591\u2591\u2591\u2592                           \n                                                             \u2591\u2591\u2592\u2593           \u2592\u2592\u2593\u2593\u2592           \u2591\u2591\u2591\u2591\u2592               \u2593            \n                                                            \u2591\u2591\u2592\u2593           \u2592\u2592\u2593\u2593\u2592          \u2591\u2591\u2591\u2591\u2591\u2592              \u2593\u2593\u2592            \n                                               \u2588            \u2591\u2592\u2593\u2593          \u2592\u2593\u2593\u2593\u2592          \u2591\u2591\u2591\u2591\u2592\u2592            \u2588\u2588\u2593\u2592              \n                                         \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593       \u2593\u2592\u2593\u2588          \u2592\u2593\u2588\u2593\u2592         \u2591\u2591\u2591\u2591\u2591\u2592            \u2588\u2588\u2592\u2592\u2591               \n                                      \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593    \u2588\u2588\u2593\u2593\u2588          \u2592\u2593\u2588\u2593\u2592         \u2591\u2591\u2591\u2591\u2592\u2593           \u2588\u2588\u2592\u2592\u2592                 \n                                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2593\u2593   \u2588\u2588\u2588\u2588\u2588\u2592         \u2592\u2593\u2593\u2592\u2592        \u2591\u2591\u2591\u2591\u2593\u2593          \u2588\u2588\u2588\u2592\u2591\u2592\u2592                  \n                              \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588    \u2588\u2588\u2588\u2588\u2588\u2593       \u2592\u2593\u2593\u2593\u2593\u2592\u2593      \u2591\u2591\u2591\u2591\u2591\u2592\u2593          \u2588\u2588\u2593\u2592\u2592\u2592\u2592                    \n                          \u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588     \u2588\u2588\u2593\u2588\u2588\u2588\u2588      \u2593\u2588\u2588\u2588\u2593\u2593\u2592\u2592     \u2591\u2591\u2591\u2591\u2591\u2592\u2593         \u2588\u2588\u2593\u2592\u2592\u2593\u2593                       \n                        \u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588       \u2588\u2588\u2588\u2593\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2593\u2592\u2591\u2591\u2591\u2591\u2592      \u2588\u2588\u2588\u2593\u2593\u2592\u2592\u2593\u2593                          \n                     \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588       \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588 \u2588\u2588\u2588\u2593\u2591\u2591     \u2593\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2592\u2593                              \n              \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588   \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588  \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2593\u2588\u2588                                 \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593                                     \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                         \n               \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                               \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                                      \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588                                                           \n                \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2591\u2591                                                              \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2592\u2591\u2591\u2592\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2592\u2593\u2593                                                                  \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2592\u2591\u2591\u2591\u2591\u2592\u2592\u2592\u2593\u2588\u2588\u2593\u2593\u2593\u2593\u2588                                                                       \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2593\u2593\u2593\u2593\u2592\u2592\u2593\u2593\u2593\u2588\u2588\u2588\u2588\u2588\u2588                                                                           \n                 \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2593\u2588\u2593                                                                                          \n                  \u2588\u2588\u2588                                                                                                        \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             \n                                                                                                                             ");
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
            //public FullScreenCustomPass blackAndWhitePass;
            
            [HarmonyPatch(nameof(RoundManager.BeginEnemySpawning))]
            [HarmonyPostfix]
            private static void PostfixSpawn()
            {
                if (RoundManager.Instance.IsServer && RoundManager.Instance.allEnemyVents.Length>0)
                {
                    if (FreddyConfig.Instance.USE_MOON_CHANCES)
                    {
                        bool spawnFreddy = false;
                        switch (RoundManager.Instance.currentLevel.PlanetName)
                        {
                            case "41 Experimentation" :
                                if (RandomNumberGenerator.GetInt32(0, 101) <= FreddyConfig.Instance.EXPERIMENTATION_SPAWNRATE)
                                {
                                    spawnFreddy = true;
                                }
                                break;
                            case "220 Assurance":
                                if (RandomNumberGenerator.GetInt32(0, 101) <= FreddyConfig.Instance.ASSURANCE_SPAWNRATE)
                                {
                                    spawnFreddy = true;
                                    Debug.Log("Money ain't real O_O ...");
                                }
                                break;
                            case "56 Vow" :
                                if (RandomNumberGenerator.GetInt32(0, 101) <= FreddyConfig.Instance.VOW_SPAWNRATE)
                                {
                                    spawnFreddy = true;
                                }
                                break;
                            case "12 Offense":
                                if (RandomNumberGenerator.GetInt32(0, 101) <= FreddyConfig.Instance.OFFENSE_SPAWNRATE)
                                {
                                    spawnFreddy = true;
                                }
                                break;
                            case "61 March":
                                if (RandomNumberGenerator.GetInt32(0, 101) <= FreddyConfig.Instance.MARCH_SPAWNRATE)
                                {
                                    spawnFreddy = true;
                                }
                                break;
                            case "85 Rend":
                                if (RandomNumberGenerator.GetInt32(0, 101) <= FreddyConfig.Instance.REND_SPAWNRATE)
                                {
                                    spawnFreddy = true;
                                }
                                break;
                            case "7 Dine":
                                if (RandomNumberGenerator.GetInt32(0, 101) <= FreddyConfig.Instance.DINE_SPAWNRATE)
                                {
                                    spawnFreddy = true;
                                }
                                break;
                            case "8 Titan":
                                if (RandomNumberGenerator.GetInt32(0, 101) <= FreddyConfig.Instance.TITAN_SPAWNRATE)
                                {
                                    spawnFreddy = true;
                                }
                                break;
                            default:
                                if (RandomNumberGenerator.GetInt32(0, 101) <= FreddyConfig.Instance.BASE_SPAWN_CHANCES)
                                {
                                    spawnFreddy = true;
                                }
                                break;
                        }
                        if (spawnFreddy)
                        {
                            Debug.Log("Freddy is here! Wait till you sleep tho...");
                            var allEnemiesList = new List<SpawnableEnemyWithRarity>();
                            allEnemiesList.AddRange(RoundManager.Instance.currentLevel.Enemies);
                            allEnemiesList.AddRange(RoundManager.Instance.currentLevel.OutsideEnemies);
                            var enemyToSpawn = allEnemiesList.Find(x => x.enemyType.enemyName.Equals("FreddyKrueger"));
                        
                            Debug.Log(enemyToSpawn.enemyType.enemyName + "This is the name!");
                            //EnemyAI.Instantiate()
                            RoundManager.Instance.SpawnEnemyGameObject(RoundManager.Instance.allEnemyVents[0].transform.position, 0f,RoundManager.Instance.currentLevel.OutsideEnemies.IndexOf(enemyToSpawn),enemyToSpawn.enemyType);

                        }
                        else
                        {
                            Debug.Log("Freddy's Spawn was a miss");
                        }
                    }
                    else
                    {
                        if (RandomNumberGenerator.GetInt32(0, 101) <= FreddyConfig.Instance.BASE_SPAWN_CHANCES)
                        {
                            Debug.Log("Freddy is here! Wait till you sleep tho...");
                            var allEnemiesList = new List<SpawnableEnemyWithRarity>();
                            allEnemiesList.AddRange(RoundManager.Instance.currentLevel.Enemies);
                            allEnemiesList.AddRange(RoundManager.Instance.currentLevel.OutsideEnemies);
                            var enemyToSpawn = allEnemiesList.Find(x => x.enemyType.enemyName.Equals("FreddyKrueger"));
                    
                            Debug.Log(enemyToSpawn.enemyType.enemyName + "This is the name!");
                            //EnemyAI.Instantiate()
                            RoundManager.Instance.SpawnEnemyGameObject(RoundManager.Instance.allEnemyVents[0].transform.position, 0f,RoundManager.Instance.currentLevel.OutsideEnemies.IndexOf(enemyToSpawn),enemyToSpawn.enemyType);
                        }
                        else
                        {
                            Debug.Log("Freddy's Spawn was a miss");
                        }
                    }
                }
            }
            
        }
    }
}