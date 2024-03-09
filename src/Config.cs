using System;
using System.Runtime.Serialization;
using BepInEx.Configuration;
using CSync.Lib;
using CSync.Util;

namespace FreddyKrueger;

[DataContract]
public class Config : SyncedConfig<Config>
{
    //Unsynced field :public ConfigEntry<float> DISPLAY_DEBUG_INFO { get; private set; }
    [DataMember] public SyncedEntry<int> ENTER_SLEEP { get; private set; } 
    [DataMember] public SyncedEntry<int> SLEEP_MAX { get; private set; } 
    
    public Config(ConfigFile cfg) : base("FreddyKrueger")
    {
        ConfigManager.Register(this); 
        ENTER_SLEEP = cfg.BindSyncedEntry("Sleep Parameter", "Enter Sleep Value", 260,
            "You get 1 of value every seconds you are alone and when you reach the decided number, freddy can officially target you (Base 260, Minimum 50)"
        );

        SLEEP_MAX = cfg.BindSyncedEntry("Sleep Parameter", "Sleep Meter Before Rampage", 400,
            "This is the amount of sleep before Freddy wants you dead and starts running (Base 400, Minimum 80 higher than 'Enter Sleep Value', suggested at very least 100 higher)"
        );
    }
}
