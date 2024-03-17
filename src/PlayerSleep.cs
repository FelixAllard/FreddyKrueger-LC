using Unity.Netcode;

namespace ExampleEnemy;

public class PlayerSleep
{
    public PlayerSleep(ulong clientID, int sleepMeter)
    {
        this.clientID = clientID;
        this.sleepMeter = sleepMeter;
        this.targetPoint = 0;
    }
    public PlayerSleep()
    {
        this.targetPoint = 0;
    }
    public ulong ClientID
    {
        get => clientID;
        set => clientID = value;
    }

    public int SleepMeter
    {
        get => sleepMeter;
        set => sleepMeter = value;
    }
    public int TargetPoint
    {
        get => targetPoint;
        set => targetPoint = value;
    }
    private ulong clientID;
    private int sleepMeter;
    private int targetPoint;

}