using System.Runtime.Serialization;
using BepInEx.Configuration;
using CSync.Lib;
using CSync.Util;

namespace ExampleEnemy;

[DataContract]
public class FreddyConfig : SyncedConfig<FreddyConfig>
{
    //Unsynced field :public ConfigEntry<float> DISPLAY_DEBUG_INFO { get; private set; }
    [DataMember] public SyncedEntry<int> ENTER_SLEEP { get; private set; } 
    [DataMember] public SyncedEntry<int> SLEEP_MAX { get; private set; } 
    [DataMember] public SyncedEntry<bool> SHIP_MOMMY_TARGET { get; private set; } 
    [DataMember] public SyncedEntry<bool> SOLO_GAMEPLAY { get; private set; } 
    [DataMember] public SyncedEntry<bool> RANDOM_SLEEP { get; private set; } 
    [DataMember] public SyncedEntry<int> TIME_BEFORE_LEAVING_SLEEP { get; private set; } 
    
    
    
    [DataMember] public SyncedEntry<int> BASE_SPAWN_CHANCES { get; private set; } 
    [DataMember] public SyncedEntry<bool> USE_MOON_CHANCES { get; private set; } 
    
    [DataMember] public SyncedEntry<int> EXPERIMENTATION_SPAWNRATE { get; private set; } 
    [DataMember] public SyncedEntry<int> ASSURANCE_SPAWNRATE { get; private set; } 
    [DataMember] public SyncedEntry<int> VOW_SPAWNRATE { get; private set; } 
    [DataMember] public SyncedEntry<int> OFFENSE_SPAWNRATE { get; private set; } 
    [DataMember] public SyncedEntry<int> MARCH_SPAWNRATE { get; private set; } 
    [DataMember] public SyncedEntry<int> REND_SPAWNRATE { get; private set; } 
    [DataMember] public SyncedEntry<int> DINE_SPAWNRATE { get; private set; } 
    [DataMember] public SyncedEntry<int> TITAN_SPAWNRATE { get; private set; } 
    
    
    // TODO : Create a setting for the distance between player for a player to be alone or not
    
    public FreddyConfig(ConfigFile cfg) : base("FreddyKrueger")
    {
        ConfigManager.Register(this); 
        ENTER_SLEEP = cfg.BindSyncedEntry("Sleep Parameter", "Enter Sleep Value", 260,
            "You get 1 of value every seconds you are alone and when you reach the decided number, freddy can officially target you (Minimum 50)"
        );

        SLEEP_MAX = cfg.BindSyncedEntry("Sleep Parameter", "Sleep Meter Before Rampage", 400,
            "This is the amount of sleep before Freddy wants you dead and starts running (Minimum 80 higher than 'Enter Sleep Value', suggested at very least 100 higher)"
        );
        SHIP_MOMMY_TARGET = cfg.BindSyncedEntry("Target Choosing", "Does sleep meter goes up for people in the ship", true,
            "If true, it will go up for people in the ship, if false, sleep meter will stay the same if inside the ship"
        );
        
        SOLO_GAMEPLAY = cfg.BindSyncedEntry("Solo Run Settings", "Activate Solo Run", false,
            "Activating solo run makes the settings of 'Solo Run Settings' activated. Leaving it to false WILL block all the settings of this section"
        );
        RANDOM_SLEEP = cfg.BindSyncedEntry("Solo Run Settings", "Make Freddy sleep unpredictable", true,
            "ACTIVATE SOLO RUN MUST BE ON : This settings make Enter Sleep Value and Sleep Meter before Rampage useless and instead chooses and random moment for you to enter the dream world"
        );
        TIME_BEFORE_LEAVING_SLEEP = cfg.BindSyncedEntry("Solo Run Settings", "Time Before Leaving Sleep", 60,
            "ACTIVATE SOLO RUN MUST BE ON : This setting decides how much time will it take for you to automatically leave the dream world after entering it"
        );
        
        BASE_SPAWN_CHANCES = cfg.BindSyncedEntry("Spawn Behaviour", "Default Spawn Chance in Percentage", 100,
            "Option must be between 0 and 100 : This config changes the default spawn chances that Freddy manifest when you lend on a moon. This value is override on basic moons. This will be the value used on modded moons"
        );
        
        USE_MOON_CHANCES = cfg.BindSyncedEntry("Spawn Behaviour", "Activate the per moon spawn rate", false,
            "If set to true, the spawn rate will be different based on the moon difficulty. This will not work for non basic moons and will instead use the Default Spawn Chances in percentage"
        );
        
        EXPERIMENTATION_SPAWNRATE = cfg.BindSyncedEntry("Moon Spawn rate", "Spawn rate for Experimentation", 14,
            "This option is only activated if it Activate the per moon spawn rate is true. This value is in percentage %"
        );
        ASSURANCE_SPAWNRATE = cfg.BindSyncedEntry("Moon Spawn rate", "Spawn rate for Assurance", 15,
            "This option is only activated if it Activate the per moon spawn rate is true. This value is in percentage %"
        );
        VOW_SPAWNRATE = cfg.BindSyncedEntry("Moon Spawn rate", "Spawn rate for Assurance", 20,
            "This option is only activated if it Activate the per moon spawn rate is true. This value is in percentage %"
        );
        OFFENSE_SPAWNRATE = cfg.BindSyncedEntry("Moon Spawn rate", "Spawn rate for Offense", 10,
            "This option is only activated if it Activate the per moon spawn rate is true. This value is in percentage %"
        );
        MARCH_SPAWNRATE = cfg.BindSyncedEntry("Moon Spawn rate", "Spawn rate for March", 20,
            "This option is only activated if it Activate the per moon spawn rate is true. This value is in percentage %"
        );
        REND_SPAWNRATE = cfg.BindSyncedEntry("Moon Spawn rate", "Spawn rate for Rend", 30,
            "This option is only activated if it Activate the per moon spawn rate is true. This value is in percentage %"
        );
        
        DINE_SPAWNRATE = cfg.BindSyncedEntry("Moon Spawn rate", "Spawn rate for Dine", 40,
            "This option is only activated if it Activate the per moon spawn rate is true. This value is in percentage %"
        );
        TITAN_SPAWNRATE = cfg.BindSyncedEntry("Moon Spawn rate", "Spawn rate for Titan", 25,
            "This option is only activated if it Activate the per moon spawn rate is true. This value is in percentage %"
        );
        
        
    }
}