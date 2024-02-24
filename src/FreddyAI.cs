namespace FreddyKrueger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using DunGen;
using GameNetcodeStuff;
using JetBrains.Annotations;
using LethalLib.Modules;
using LethalNetworkAPI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
//COMPONENTS IMPORTS


//PLAYER CLASS
[Serializable]
public class PlayerSleep
{
    public PlayerSleep(ulong clientID, int sleepMeter, bool isTargetPlayer)
    {
        this.clientID = clientID;
        this.sleepMeter = sleepMeter;
        this.isTargetPlayer = isTargetPlayer;
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

    public bool IsTargetPlayer
    {
        get => isTargetPlayer;
        set => isTargetPlayer = value;
    }
    public int TargetPoint
    {
        get => targetPoint;
        set => targetPoint = value;
    }
    public ulong clientID;
    public int sleepMeter;
    public bool isTargetPlayer;
    public int targetPoint;
}

public class FreddyAI : EnemyAI
{
    
    //COMPONENTS IMPORT
    //---------------------------------
    //AUDIO
    public AudioClip Laugh1;
    public AudioClip Laugh2;
    public AudioClip Laugh3;
    public AudioClip Laugh4;
    public AudioClip Laugh5;
    public AudioClip Laugh6;
    public AudioClip Laugh7;
    public AudioClip Laugh8;
    public AudioClip Laugh9;
    public AudioClip Laugh10;
    public AudioClip InTheNightmare;
    public AudioClip EnterTheDream;
    public AudioClip Terminus;
    
    //TRANSFORMER
    public Transform turnCompass;
    public Transform attackArea;
    public AudioSource oneShotCreature;
    
    //2D ARRAY 
    private List<PlayerSleep> playerSleep;

    private List<PlayerSleep> playerSleepServ;
    
    
    
    //POST MAIL Array 2D
    private LethalServerMessage<List<PlayerSleep>> ServerMessageSleepArray;
    
    private LethalClientMessage<List<PlayerSleep>> ClientReceiveSleepAray;
    
    //Post Mail behavior Int
    private LethalServerMessage<int> ServerMessageBehavior;
    
    private LethalClientMessage<int> ClientReceiveBehavior;
    //Fast info
    private ulong clientID;
    private ulong lastSleepMeter;

    private int indexSleepArrayTarget = -1;
    private int indexSleepArraySleep = -1;
    
    
    private PlayerControllerB TargetPlayer;
    private int BehaviourIndexServer;
    private bool justSwitchedBehaviour;
    //RunningClaw Verifications;
    private bool wasInsideFactory;
    private bool triggerTeleportDoor;

    enum State {
        Spawning,
        Walking,
        Running,
        RunningClaw, 
        Sneaking
    }
    public override void Start()
    {
        base.Start();
        EnemyMeshAndPerson(false);
        if (IsServer)
        {
            playerSleepServ = new List<PlayerSleep>();
            for (int count = 0; count < RoundManager.Instance.playersManager.allPlayerScripts.Length; count++)
            {
                //Adds All the player with There client ID, sleep meter 0 and Desactivate the target player
                if (RoundManager.Instance.playersManager.allPlayerScripts[count].isPlayerControlled)
                {
                    playerSleepServ.Add(new PlayerSleep(
                        RoundManager.Instance.playersManager.allPlayerScripts[count].GetClientId(), 
                        0, 
                        false
                        ));
                }
            }

            PlayerControllerB me = RoundManager.Instance.playersManager.allPlayerScripts[0];
            
            
            BehaviourIndexServer = 0;
            justSwitchedBehaviour = true;
        }
        
        //Message service Array 2D
        ServerMessageSleepArray = new LethalServerMessage<List<PlayerSleep>>(identifier: "customIdentifier");
        ClientReceiveSleepAray = new LethalClientMessage<List<PlayerSleep>>(identifier: "customIdentifier", onReceived: ActualiseClientSleep);
        //Mesage service int 8
        ServerMessageBehavior = new LethalServerMessage<int>(identifier: "customIdentifier");
        ClientReceiveBehavior = new LethalClientMessage<int>(identifier: "customIdentifier", onReceived: SetClientBehavior);
        
        //ClientReceiveSleepAray.OnReceived += ActualiseClientSleepServer; Useless Server Receive Logic
        ClientReceiveSleepAray.OnReceived += ActualiseClientSleep;
        //Behavior SEND Int
        ClientReceiveBehavior.OnReceived += SetClientBehavior;

        clientID = RoundManager.Instance.playersManager.localPlayerController.GetClientId();
    }
    
    [NonSerialized] private double timer =0;
    public override void Update()
    {
        base.Update();
        
        if (IsHost)
        {
            timer += Time.deltaTime;
            if (timer >= 1)
            {
                UpdateSleep();
                timer = 0;
            }
        }
    }

    public override void DoAIInterval()
    {
        base.DoAIInterval();
        
        if (TargetPlayer != null)
        {
            SetDestinationToPosition(TargetPlayer.transform.position);
            if (TargetPlayer.isInsideFactory != wasInsideFactory)
            {
                triggerTeleportDoor = true;
                wasInsideFactory = TargetPlayer.isInsideFactory;
            }
        }

        
        switch(currentBehaviourStateIndex) {
            // case (int)State.Spawning:
            //     if (justSwitchedBehaviour)
            //     {
            //         IdleFreddy();
            //         agent.speed = 0f;
            //         justSwitchedBehaviour = false;
            //     }
            //     EnableEnemyMesh(true);
            //     
            //     break;
            case (int)State.Walking:
                if (justSwitchedBehaviour)
                {
                    TeleportRandomlyAroundPlayer(10, 20);
                    IdleFreddy();
                    creatureAnimator.SetBool("Walking",true);
                    agent.speed = 3f;
                    justSwitchedBehaviour = false;
                    
                }
                if (!SetDestinationToPosition(TargetPlayer.transform.position, true) && TargetPlayer != null)
                {
                    TeleportRandomlyAroundPlayer(20, 30);
                }
                StartCoroutine(teleportCooldown());
                break;
            case (int)State.Running:
                if (justSwitchedBehaviour)
                {
                    
                    TeleportRandomlyAroundPlayer(20, 30);
                    IdleFreddy();
                    creatureAnimator.SetBool("Running", true);
                    agent.speed = 7f;
                    justSwitchedBehaviour = false;
                }
                StartCoroutine(teleportCooldown());
                break;
            case (int)State.RunningClaw:
                if (justSwitchedBehaviour)
                {
                    TeleportRandomlyAroundPlayer(40, 70);
                    IdleFreddy();
                    creatureAnimator.SetBool("RunWithClaw", true);
                    agent.speed = 6f;
                    
                    justSwitchedBehaviour = false;
                }
                if (triggerTeleportDoor)
                {
                    TeleportRandomlyAroundPlayer(30, 60);
                    triggerTeleportDoor = false;
                }
                if (!TargetPlayer.isPlayerControlled && IsHost)
                {
                    ChooseTarget();
                }
                
                break;
            case (int)State.Sneaking:
                if (justSwitchedBehaviour)
                {
                    TeleportRandomlyAroundPlayer(15, 20);
                    IdleFreddy();
                    agent.speed = 2f;
                    creatureAnimator.SetBool("Sneaking", true);
                    justSwitchedBehaviour = false;
                    
                    
                }
                Bounds bounds = GetComponent<Renderer>().bounds;

                // Calculate the screen position of the object's center
                //IDK IF IT IS THE RIGHT CAMERA
                Vector3 screenPoint = TargetPlayer.gameplayCamera.WorldToViewportPoint(bounds.center);

                // Check if the object is within the camera's view
                bool isVisible = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
                if (isVisible)
                {
                    IdleFreddy();
                    creatureAnimator.SetTrigger("Suprised");
                    RandomLaugh();
                    agent.speed = 0f;
                    StartCoroutine(WaitAndChangeBehavior(3f));
                }
                
                break;
            default:
                Debug.Log("Behavior State Missing");
                break;
        }
    }
    //IENUMERATOR
    //-----------------------------------------------------------------------------------------------------------------------
    IEnumerator teleportCooldown()
    {
        
        
        yield return new WaitForSeconds(12);
        creatureAnimator.SetTrigger("Teleport");
        RandomLaugh();
        yield return new WaitForSeconds(3);
        if (IsHost)
        {
            SetBehavior();
        }
    }
    IEnumerator WaitAndChangeBehavior(float x)
    {
        yield return new WaitForSeconds(x);
        if (IsHost)
        {
            SetBehavior();
        }
        else
        {
            Debug.Log("Awaiting Host Behavior");
        }
    }
    //SERVER LOGIC
    //-----------------------------------------------------------------------------------------------------------------------START SERVER
    
    public void UpdateSleep()
    {

        for (int count = 0; count < playerSleepServ.Count; count++)
        {
            PlayerControllerB player = playerSleepServ[count].ClientID.GetPlayerController();
            //HANDLE DEAD PLAYER
            if (!player.isPlayerControlled)
            {
                //Remove DEAD player for target list
                playerSleepServ.RemoveAt(count);
            }
            else
            {
                if (CheckIfAlone(player))
                {
                    // Add 1 to the value of the player THAT IS ALONE
                    playerSleepServ[count].SleepMeter += 1;
                }
                else
                {
                    // Remove 1 from the value of the player in the dictionary
                    if (playerSleepServ[count].SleepMeter > 0)
                    {
                        if (playerSleepServ[count].SleepMeter == 1)
                        {
                            //Handle if it is 1
                            playerSleepServ[count].SleepMeter = 0;
                        }
                        else
                        {
                            //handle if more than 1
                            playerSleepServ[count].SleepMeter -= 2;
                        }
                    }
                }
            }
            
        }
        //IF not running with claw
        if(BehaviourIndexServer != 3)
        {
            if (IsHost)
            {
                ChooseTarget();
            }
        }
        Debug.Log("SERVER SIDE" + playerSleepServ[0].SleepMeter);
        
        //Sending computation to all clients
        ServerMessageSleepArray.SendAllClients(playerSleepServ);
    }
    public bool CheckIfAlone(PlayerControllerB player)
    {
        //GET Player Position
        Vector3 currentPlayerPosition = player.transform.position;

        for (int count = 0; count < playerSleepServ.Count; count++)
        {
            if (playerSleepServ[count].ClientID != player.GetClientId())
            {
                Vector3 otherPlayerPosition = playerSleepServ[count].ClientID.GetPlayerController().transform.position;
                // Calculate the distance between the current player and the other player
                float distance = Vector3.Distance(currentPlayerPosition, otherPlayerPosition);

                // Check if the distance is within the specified range
                if (distance <= 10f && player != playerSleepServ[count].ClientID.GetPlayerController())
                {
                    // If the distance is less than or equal to 10, the current player is considered with another player
                    return false;
                }
            }
            else
            {
                Debug.Log("Same Player Reference");
            }
        }
        // If no other player is found within 10 units, check if the distance is greater than 20
        // If the distance is greater than 20, the current player is considered alone
        return true;
    }
    
    public void ChooseTarget()
    {
        List<PlayerSleep> possibleTarget = new List<PlayerSleep>();
        //FInd which Player has a score over 260!!!
        for (int count = 0; count < playerSleepServ.Count; count++)
        {
            if (playerSleepServ[count].SleepMeter >= 260) //SLEEP METER TRESHOLD --- IMPORTANT
            {
                possibleTarget.Add(playerSleepServ[count]);
            }
        }
        //Sort from highest ot lowest
        possibleTarget.Sort((a, b) => b.SleepMeter.CompareTo(a.SleepMeter));
        PlayerSleep highestSleepPoints = new PlayerSleep();

        for (int count = 0; count < possibleTarget.Count; count++)
        {
            possibleTarget[count].TargetPoint += (5 - count);
        }
        foreach (var player in possibleTarget)
        {
            if (player.ClientID.GetPlayerController().isInsideFactory)
            {
                player.TargetPoint += 3;
            }
            if (player.ClientID.GetPlayerController().criticallyInjured)
            {
                player.TargetPoint += 5;
            }

            if (CheckIfAlone(player.ClientID.GetPlayerController()))
            {
                player.TargetPoint += 7;
            }

            if (player.ClientID.GetPlayerController().isInHangarShipRoom)
            {
                player.TargetPoint -= 3;
            }

            if (player.ClientID.GetPlayerController().carryWeight >= 50)
            {
                player.TargetPoint += 2;
            }
            if (player.ClientID.GetPlayerController().carryWeight >= 100)
            {
                player.TargetPoint += 2;
            }

            if (player.ClientID.GetPlayerController().carryWeight == 0)
            {
                player.TargetPoint -= 2;
            }

            if (player.TargetPoint > highestSleepPoints.TargetPoint)
            {
                highestSleepPoints = player;
            }
            else if(player.TargetPoint == highestSleepPoints.TargetPoint)
            {
                if (player.IsTargetPlayer)
                {
                    highestSleepPoints = player;
                }
                else if (highestSleepPoints.IsTargetPlayer)
                {
                    //DO NOTHING
                }
                else if (!player.IsTargetPlayer && !highestSleepPoints.IsTargetPlayer)
                {
                    switch (RandomNumberGenerator.GetInt32(0, 2))
                    {
                        case 0:
                            highestSleepPoints = player;
                            break;
                        case 1:
                            //NOTHING
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        //Target player Becomes true for the target player and false for the rest
        foreach (var player in playerSleepServ)
        {
            if (player.ClientID == highestSleepPoints.ClientID)
            {
                player.IsTargetPlayer = true;
            }
            else
            {
                player.IsTargetPlayer = false;
            }
        }
    }
    /*
     *STATE IDEA
     * SLEEP LULLABY 240
     *
     * 260-400 WALKING
     * 300-400
     * sneaking 350-400
     * KILL CLAW : 400+
     * 
     *
     * 
     */
    
    public void SetBehavior()
    {
        if (TargetPlayer != null)
        {
            bool updateToClient = false;
            if (playerSleepServ[indexSleepArraySleep].SleepMeter >=260 && playerSleepServ[indexSleepArraySleep].SleepMeter <400)
            {
                //WALKING
                BehaviourIndexServer = 1;
                
                if (playerSleepServ[indexSleepArraySleep].SleepMeter >= RandomNumberGenerator.GetInt32(300,450))
                {
                    if (currentBehaviourStateIndex != 2)
                    {
                        BehaviourIndexServer = 2;
                        updateToClient = true;
                    }
                    //RUNNING
                }
                else if (playerSleepServ[indexSleepArraySleep].SleepMeter >= RandomNumberGenerator.GetInt32(350, 450))
                {
                    //Sneaking
                    if (currentBehaviourStateIndex != 4)
                    {
                        BehaviourIndexServer = 4;
                        updateToClient = true;
                    }
                }
               
            }
            else if(playerSleepServ[indexSleepArraySleep].SleepMeter >=400)
            {
                //KILL time
                BehaviourIndexServer = 3;
            }
            //If The state is different, it sends to clients
            if (updateToClient)
            {
                ServerMessageBehavior.SendAllClients(BehaviourIndexServer,true);
            }
        }
    }
    
    //------------------------------------------------------------------------------------------------------------------------END SERVER
    //Also Assign target Player
    private void IdleFreddy()
    {
        creatureAnimator.SetBool("Running", false);
        creatureAnimator.SetBool("Walking", false);
        creatureAnimator.SetBool("RunWithClaw", false);
        creatureAnimator.SetBool("Sneaking",false);
        
    }
    
    private void SetTargetPlayer()
    {
        var count = 0;
        bool isThereTarget = false;
        foreach (var player in playerSleep)
        {

            //Handle if LocalPlayer = Target player
            if (player.ClientID == clientID)
            {
                //Add the array number of the local player too indexSleepArraySleep
                if (indexSleepArraySleep == -1)
                {
                    indexSleepArraySleep = count;
                }
            }
            //Set target Player
            //CONDITION = Player is target plauer
            if(player.IsTargetPlayer)
            {
                TargetPlayer = player.ClientID.GetPlayerController();
                isThereTarget = true;
            }
            count++;
        }
        if (isThereTarget)
        {
            TargetPlayer = null;
        }
    }

 public bool TeleportRandomlyAroundPlayer(float minTeleportDistance,float maxTeleportDistance)
    {
        if (targetPlayer != null)
        {
            Vector3 teleportPosition = Vector3.zero;
            bool foundValidPosition = false;
            int maxAttempts = 10;
            int attempts = 0;

            while (!foundValidPosition && attempts < maxAttempts)
            {
                // Calculate a random angle around the player
                float angle = Random.Range(0f, 360f);

                // Calculate a random distance from the player
                float distance = Random.Range(minTeleportDistance, maxTeleportDistance);

                // Convert the angle to a direction vector
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                // Calculate the teleport position
                teleportPosition = TargetPlayer.transform.position + direction * distance;

                // Ensure the teleport position is on the NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(teleportPosition, out hit, maxTeleportDistance, NavMesh.AllAreas))
                {
                    // Teleport the Krueger to the calculated position
                    agent.Warp(hit.position); // Teleport the Krueger to the valid position on the NavMesh
                    if(SetDestinationToPosition(TargetPlayer.transform.position, true))
                    {
                        foundValidPosition = true;
                    }
                    turnCompass.LookAt(TargetPlayer.gameplayCamera.transform.position);

                    // If a valid path is found, set Krueger's destination to the player's position
                    //agent.SetDestination(targetPlayer.transform.position);
                    SwitchToBehaviourState(0);
                    return true;

                }
                else
                {
                    attempts++;
                }
            }

            if (!foundValidPosition)
            {
                return false;
            }
        }

        return false;
    }
 
    //Voice Logic
    public void RandomLaugh()
    {
        switch (RandomNumberGenerator.GetInt32(1,11))
        {
            case(1) :
                creatureVoice.PlayOneShot(Laugh1);
                break;
            case(2) :
                creatureVoice.PlayOneShot(Laugh2);
                break;
            case(3):
                creatureVoice.PlayOneShot(Laugh3);
                break;
            case(4) :
                creatureVoice.PlayOneShot(Laugh4);
                break;
            case(5) :
                creatureVoice.PlayOneShot(Laugh5);
                break;
            case(6):
                creatureVoice.PlayOneShot(Laugh6);
                break;
            case(7) :
                creatureVoice.PlayOneShot(Laugh7);
                break;
            case(8) :
                creatureVoice.PlayOneShot(Laugh8);
                break;
            case(9):
                creatureVoice.PlayOneShot(Laugh9);
                break;
            case(10) :
                creatureVoice.PlayOneShot(Laugh10);
                break;
            default:
                Debug.LogError("Number Generated for laugh was a miss!");
                break;
        }
    }

    
    //Break Door On touch
    //---------------------------------------------------
    [ServerRpc]
        public void BreakDoorServerRpc()
        {
            foreach (DoorLock Door in FindObjectsOfType(typeof(DoorLock)) as DoorLock[])
            {
                var ThisDoor = Door.transform.parent.transform.parent.transform.parent.gameObject;
                if (!ThisDoor.GetComponent<Rigidbody>())
                {
                    if (Vector3.Distance(transform.position, ThisDoor.transform.position) <= 4f)
                    {
                        BashDoorClientRpc(ThisDoor, (targetPlayer.transform.position - transform.position).normalized * 20);
                    }
                }
            }
        }
        [ClientRpc]
        public void BashDoorClientRpc(NetworkObjectReference netObjRef, Vector3 Position)
        {
            if (netObjRef.TryGet(out NetworkObject netObj))
            {
                var ThisDoor = netObj.gameObject;
                var rig = ThisDoor.AddComponent<Rigidbody>();
                var newAS = ThisDoor.AddComponent<AudioSource>();
                newAS.spatialBlend = 1;
                newAS.maxDistance = 60;
                newAS.rolloffMode = AudioRolloffMode.Linear;
                newAS.volume = 3;
                StartCoroutine(TurnOffC(rig, .12f));
                rig.AddForce(Position, ForceMode.Impulse);
                creatureAnimator.SetTrigger("KickDoor");
                //Add audio clip for door  Bash.0
                //newAS.PlayOneShot(audioClips[3]);
            }
        }
        public bool CheckDoor()
        {
            foreach (DoorLock Door in FindObjectsOfType(typeof(DoorLock)) as DoorLock[])
            {
                var ThisDoor = Door.transform.parent.transform.parent.gameObject;
                if (Vector3.Distance(transform.position, ThisDoor.transform.position) <= 4f)
                {
                    return true;
                }
            }
            return false;
        }
        IEnumerator TurnOffC(Rigidbody rigidbody,float time)
        {
            rigidbody.detectCollisions = false;
            yield return new WaitForSeconds(time);
            rigidbody.detectCollisions = true;
            Destroy(rigidbody.gameObject, 5);
        }
        //-------------------------------------------------------------
        //FREDDY Stage local handler
        private void localPlayerFreddyHandler()
        {
            if (playerSleep[indexSleepArraySleep].SleepMeter ==235)
            {
                oneShotCreature.PlayOneShot(EnterTheDream);
            }
            if (playerSleep[indexSleepArraySleep].SleepMeter >= 260)
            {
                EnemyMeshAndPerson(true);
            }
            else
            {
                EnemyMeshAndPerson(false);
                oneShotCreature.Stop();
            }

            if (playerSleep[indexSleepArraySleep].SleepMeter == 320)
            {
                oneShotCreature.PlayOneShot(Terminus);
            }
            else if (playerSleep[indexSleepArraySleep].SleepMeter < 320)
            {
                oneShotCreature.Stop();
            }
        }

        public void EnemyMeshAndPerson(bool Enable)
        {
            this.EnableEnemyMesh(Enable, false);
            if (Enable)
            {
                creatureVoice.volume = 100;
                creatureSFX.volume = 100;
            }
            else
            {
                creatureVoice.volume = 0;
                creatureSFX.volume = 0;
            }
        }
        //______________________________________________________//
        
        
        
        
    
    //MESSAGE HANDLER LOGIC
    private void ActualiseClientSleep(List<PlayerSleep> x)
    {
        Debug.Log("ClientSide "+x[0].SleepMeter);
        //Reload Index If death of player
        if (playerSleep!=null)
        {
            if (playerSleep.Count!=x.Count)
            {
                indexSleepArraySleep = -1;
            }
        }
        playerSleep = x;
        SetTargetPlayer();
        localPlayerFreddyHandler();
    }

    private void SetClientBehavior(int x)
    {
        currentBehaviourStateIndex = x;
        justSwitchedBehaviour = true;
    }
}