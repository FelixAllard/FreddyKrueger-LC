using DigitalRuby.ThunderAndLightning;
using DunGen;
using UnityEngine.Bindings;
using UnityEngine.Serialization;

namespace FreddyKrueger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using GameNetcodeStuff;
using LethalNetworkAPI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
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

public class CollisionHandler : EnemyAI
{
    
    //COMPONENTS IMPORT
    //---------------------------------
    //AUDIO
    [FormerlySerializedAs("Laugh1")] public AudioClip laugh1;
    [FormerlySerializedAs("Laugh2")] public AudioClip laugh2;
    [FormerlySerializedAs("Laugh3")] public AudioClip laugh3;
    [FormerlySerializedAs("Laugh4")] public AudioClip laugh4;
    [FormerlySerializedAs("Laugh5")] public AudioClip laugh5;
    [FormerlySerializedAs("Laugh6")] public AudioClip laugh6;
    [FormerlySerializedAs("Laugh7")] public AudioClip laugh7;
    [FormerlySerializedAs("Laugh8")] public AudioClip laugh8;
    [FormerlySerializedAs("Laugh9")] public AudioClip laugh9;
    [FormerlySerializedAs("Laugh10")] public AudioClip laugh10;
    [FormerlySerializedAs("InTheNightmare")] public AudioClip inTheNightmare;
    [FormerlySerializedAs("EnterTheDream")] public AudioClip enterTheDream;
    [FormerlySerializedAs("Terminus")] public AudioClip terminus;
    
    //TRANSFORMER
    public Transform turnCompass;
    public Transform attackArea;
    public AudioSource oneShotCreature;
    public BoxCollider freddyHitBox;
    
    //2D ARRAY 
    private List<PlayerSleep> _playerSleep;

    [NonSerialized]
    public List<PlayerSleep> playerSleepServ;
    
    
    
    //POST MAIL Array 2D
    private LethalServerMessage<List<PlayerSleep>> _serverMessageSleepArray;
    
    private LethalClientMessage<List<PlayerSleep>> _clientReceiveSleepArray;
    
    //Fast info
    private ulong _clientID;
    private ulong _lastSleepMeter;

    private int indexSleepArrayTarget = -1;
    private int _indexSleepArraySleep = -1;
    
    
    private PlayerControllerB _targetPlayer;
    private PlayerSleep _targetPlayerSleep;
    
    private int _behaviourIndexServer;
    private bool _justSwitchedBehaviour;
    //RunningClaw Verifications;
    
    
    
    
    //TRESHOLDS FOR QUICK SWITCHING
    private int _enterSleep;
    private int _maxSleep;
    private bool _freddyVisible;


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
        _enterSleep = 50;
        _maxSleep = 150;
        _inCoroutine = false;
        StartCoroutine(SeeIfAccessible());
        if (creatureVoice == null)
        {
            creatureVoice = FindAudioSourceInChildren(transform, "Jeb");
        }
        
        if (IsServer)
        {
            ReinitialiseList();
            _behaviourIndexServer = 0;
            _justSwitchedBehaviour = true;
        }
        
        //Message service Array 2D
        _serverMessageSleepArray = new LethalServerMessage<List<PlayerSleep>>(identifier: "customIdentifier");
        _clientReceiveSleepArray = new LethalClientMessage<List<PlayerSleep>>(identifier: "customIdentifier", onReceived: ActualiseClientSleep);
        //Mesage service int 8

        
        //ClientReceiveSleepAray.OnReceived += ActualiseClientSleepServer; Useless Server Receive Logic
        
        
        //_clientReceiveSleepArray.OnReceived += ActualiseClientSleep; ADD SECOND METHOD TO CALLING
        //Behavior SEND Int
        _clientID = RoundManager.Instance.playersManager.localPlayerController.GetClientId();
    }

    public void ReinitialiseList()
    {
        playerSleepServ = new List<PlayerSleep>();
        foreach (var t in RoundManager.Instance.playersManager.allPlayerScripts)
        {
            //Adds All the player with There client ID, sleep meter 0 and deactivated the target player
            if (t.isPlayerControlled)
            {
                playerSleepServ.Add(new PlayerSleep(
                    t.GetClientId(), 
                    0, 
                    false
                ));
            }
        }
    }

    [NonSerialized] private double _timer =0;
    private bool _setFirstBehaviour = false;
    private bool _inCoroutine;


    public override void Update()
    {
        base.Update();
        
        if (IsHost)
        {
            _timer += Time.deltaTime;
            if (_timer >= 1)
            {
                
                UpdateSleep();
                _timer = 0;
            }
        }
    }

    public override void DoAIInterval()
    {
        base.DoAIInterval();
        CollisionHandlerFreddy();
        if (_targetPlayer != null)
        {
            SetDestinationToPosition(_targetPlayer.transform.position, true);
        }
       
        
        switch(currentBehaviourStateIndex) {
            case (int)State.Spawning:
                if (_targetPlayer)
                {
                    agent.acceleration = 0;
                    IdleFreddy();
                    
                    _justSwitchedBehaviour = false;
                    creatureAnimator.SetTrigger("Teleport");
                    
                    if (IsHost)
                    {
                        SetBehavior();
                    }
                    
                    _justSwitchedBehaviour = true;
                    StopCoroutine(WaitAndChangeBehavior(5f));

                }
                break;
            case (int)State.Walking:
                if (_justSwitchedBehaviour)
                {
                    agent.acceleration = 8;
                    TeleportRandomlyAroundPlayer(10, 15);
                    IdleFreddy();
                    creatureAnimator.SetBool("Walking",true);
                    agent.speed = 3f;
                    _justSwitchedBehaviour = false;
                    if (!_inCoroutine)
                    {
                        StartCoroutine(WaitAndChangeBehavior(15f)); 
                        _inCoroutine = true;
                    }
                    
                }
                if (IsHost)
                {
                    DoorHandler(false);
                }
                break;
            case (int)State.Running:
                if (_justSwitchedBehaviour)
                {
                    agent.acceleration = 8;
                    
                    TeleportRandomlyAroundPlayer(20, 30);
                    IdleFreddy();
                    creatureAnimator.SetBool("Running", true);
                    agent.speed = 7f;
                    _justSwitchedBehaviour = false;
                    if (!_inCoroutine)
                    {
                        StartCoroutine(WaitAndChangeBehavior(12f));
                        _inCoroutine = true;
                    }
                }
                if (IsHost)
                {
                    DoorHandler(false);
                }
                break;
            case (int)State.RunningClaw:
                if (_justSwitchedBehaviour)
                {

                    float _minTeleport = (40-(_targetPlayerSleep.SleepMeter-_maxSleep));
                    float _maxTeleport = ((40 - (_targetPlayerSleep.SleepMeter - _maxSleep)) / 2) * 3;
                    TeleportRandomlyAroundPlayer((_minTeleport>=0)?_minTeleport:0f,(_minTeleport>=0)?_maxTeleport:3f );
                    IdleFreddy();
                    creatureAnimator.SetBool("RunWithClaw", true);
                    agent.speed = 6;
                    
                    _justSwitchedBehaviour = false;
                }
                if (IsHost)
                {
                    DoorHandler(false);
                }
                agent.acceleration = 20;
                break;
            case (int)State.Sneaking:
                if (_justSwitchedBehaviour)
                {
                    agent.acceleration = 8;
                    TeleportRandomlyAroundPlayer(15, 20);
                    IdleFreddy();
                    agent.speed = 3f;
                    creatureAnimator.SetBool("Sneaking", true);
                    _justSwitchedBehaviour = false;
                    agent.acceleration = 0;
                }
                if (IsHost)
                {
                    DoorHandler(false);
                }
                Vector3 screenPoint = _targetPlayer.gameplayCamera.WorldToViewportPoint(transform.position);

                // Check if the object is within the camera's view
                bool isVisible = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
                if (isVisible)
                {
                    IdleFreddy();
                    RandomLaugh();
                    agent.speed = 0f;
                    if (!_inCoroutine)
                    {
                        RandomLaugh();
                        creatureAnimator.SetTrigger("Suprised");
                        StartCoroutine(WaitAndChangeBehavior(3f));
                        _inCoroutine = true;
                    }
                }
                break;
            default:
                Debug.Log("Behavior State Missing");
                break;
        }
    }
    //IENUMERATOR
    //-----------------------------------------------------------------------------------------------------------------------
 

    IEnumerator WaitAndChangeBehavior(float x)
    {
        yield return new WaitForSeconds(x);
        if (IsHost)
        {
            
            SwitchToBehaviourClientRpc(0);
        }
        else
        {
            
            Debug.Log("Awaiting Host Behavior");
        }
        
        _justSwitchedBehaviour = true;
        _inCoroutine = false;
    }

    IEnumerator SeeIfAccessible()
    {
        int count = 0;
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (_targetPlayer != null && !CanReach(transform.position, _targetPlayer.transform.position))
            {
                count++;
                if (count >= 3)
                {
                    SwitchToBehaviourClientRpc(0);
                    count = -3;
                }
            }
            else
            {
                count = 0;
            }
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
        if(_behaviourIndexServer != 3)
        {
            if (IsHost)
            {
                ChooseTarget();
            }
        }
        //Sending computation to all clients
        _serverMessageSleepArray.SendAllClients(playerSleepServ);
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
                //Handle if same player or to far
            }
        }
        // If no other player is found within 10 units, check if the distance is greater than 20
        // If the distance is greater than 20, the current player is considered alone
        return true;
    }
    
    public void ChooseTarget()
    {
        List<PlayerSleep> possibleTarget = new List<PlayerSleep>();
        //FInd which Player has a score over sleep treshold !!!
        foreach (var t in playerSleepServ)
        {
            if (t.SleepMeter >= _enterSleep && t.ClientID.GetPlayerController().isPlayerControlled ) //SLEEP METER TRESHOLD --- IMPORTANT
            {
                possibleTarget.Add(t);
            }
        }
        //Sort from highest ot lowest
        possibleTarget.Sort((a, b) => b.SleepMeter.CompareTo(a.SleepMeter));
        PlayerSleep highestSleepPoints = new PlayerSleep();
        highestSleepPoints.TargetPoint = -100;
        highestSleepPoints.ClientID = 9999999999;

        for (int count = 0; count < possibleTarget.Count; count++)
        {
            possibleTarget[count].TargetPoint += (5 - count);
        }
        foreach (var player in possibleTarget)
        {
            PlayerControllerB spaceRamPlayerHandler= player.ClientID.GetPlayerController();
            if (spaceRamPlayerHandler.isInsideFactory)
            {
                player.TargetPoint += 3;
            }
            if (spaceRamPlayerHandler.criticallyInjured)
            {
                player.TargetPoint += 5;
            }

            if (CheckIfAlone(spaceRamPlayerHandler))
            {
                player.TargetPoint += 7;
            }

            if (spaceRamPlayerHandler.isInHangarShipRoom)
            {
                player.TargetPoint -= 3;
            }

            if (spaceRamPlayerHandler.carryWeight >= 50)
            {
                player.TargetPoint += 2;
            }
            if (spaceRamPlayerHandler.carryWeight >= 100)
            {
                player.TargetPoint += 2;
            }

            if (spaceRamPlayerHandler.carryWeight == 0)
            {
                player.TargetPoint -= 2;
            }

            if (player.TargetPoint > highestSleepPoints.TargetPoint)
            {
                highestSleepPoints = player;
            }
            //TODO Implement proper EQUAL POINTS behaviour
            /*
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
            */
        }
        //Target player Becomes true for the target player and false for the rest
        foreach (var player in playerSleepServ)
        { 
            if (player.ClientID == highestSleepPoints.ClientID && highestSleepPoints.ClientID!=9999999999)
            {
                player.IsTargetPlayer = true;
                
                if (!_setFirstBehaviour)
                {
                    Debug.Log("Setting first behaviour!");
                    _setFirstBehaviour = true;
                    SetBehavior();
                }
            }
            else
            {
                player.IsTargetPlayer = false;
            }
        }
    }
    public void SetBehavior()
    {
        if (_targetPlayer != null)
        {
            bool updateToClient = false;
            if (_targetPlayerSleep.SleepMeter >=_enterSleep && _targetPlayerSleep.SleepMeter <_maxSleep)
            {
                
                if (_targetPlayerSleep.SleepMeter >= RandomNumberGenerator.GetInt32((_maxSleep-50),(_maxSleep+50)))
                {
                    if (currentBehaviourStateIndex != 2)
                    {
                        _behaviourIndexServer = 2;
                        updateToClient = true;
                    }
                    //RUNNING
                }
                else if (_targetPlayerSleep.SleepMeter >= RandomNumberGenerator.GetInt32((_maxSleep-50),(_maxSleep+50)))
                {
                    //Sneaking
                    if (currentBehaviourStateIndex != 4)
                    {
                        _behaviourIndexServer = 4;
                        updateToClient = true;
                    }
                }
                else
                {
                    if (currentBehaviourStateIndex != 4)
                    {
                        //WALKING
                        _behaviourIndexServer = 1;
                        updateToClient = true;
                    } 
                }
               
            }
            else if(_targetPlayerSleep.SleepMeter >=_maxSleep)
            {
                //KILL time
                _behaviourIndexServer = 3;
                updateToClient = true;
            }
            //If The state is different, it sends to clients
            if (updateToClient)
            {
                Debug.Log("Set behavior state :  " + _behaviourIndexServer + "   Index");
                SwitchToBehaviourClientRpc(_behaviourIndexServer);
            }
        }
    }

    private void DoorHandler(bool breakDoor)
    {
        //TODO create seperate variable for less Fetching
        foreach (DoorLock door in FindObjectsOfType(typeof(DoorLock)) as DoorLock[])
        {
            var thisDoor = door.transform.parent.transform.parent.transform.parent.gameObject;
            if (Vector3.Distance(transform.position, thisDoor.transform.position) <= 4f)
            {
                if (!breakDoor)
                {
                    door.OpenDoorAsEnemyClientRpc();
                }
                if (breakDoor)
                {
                }
            }
                /*
                 WILL BE USED TO BREAK DOORS
            if (!door.GetComponent<Rigidbody>())
            {
            }*/
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
        foreach (var player in _playerSleep)
        {

            //Handle if LocalPlayer = Target player
            if (player.ClientID == _clientID)
            {
                //Add the array number of the local player too indexSleepArraySleep
                if (_indexSleepArraySleep == -1)
                {
                    _indexSleepArraySleep = count;
                }
            }
            //Set target Player
            //CONDITION = Player is target plauer
            if(player.IsTargetPlayer)
            {
                _targetPlayer = player.ClientID.GetPlayerController();
                _targetPlayerSleep = player;
                isThereTarget = true;
            }
            count++;
        }
        if (!isThereTarget)
        {
            Debug.Log("NO TARGET PLAYER");
            _targetPlayer = null;
        }
    }
    public bool CanReach(Vector3 pointA, Vector3 pointB)
    {
        NavMeshPath path = new NavMeshPath();
        // Calculate the path between the two points
        //TODO arrange this shit
        
        if (NavMesh.CalculatePath(pointA, pointB, NavMesh.AllAreas, path))
        {
            // Check if the path is valid
            return path.status == NavMeshPathStatus.PathComplete;
        }

        // Either there's no path or the path is invalid
        return false;
    }

    public bool TeleportRandomlyAroundPlayer(float minTeleportDistance, float maxTeleportDistance)
    {
        Debug.Log("Just Tried teleporting!");
        if (_targetPlayer != null)
        {
            int maxAttempts = 10;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                Debug.Log("Attempt : " + attempts);
                // Calculate a random angle around the player
                float angle = Random.Range(0f, 360f);

                // Calculate a random distance from the player
                float distance = Random.Range(minTeleportDistance, maxTeleportDistance);

                // Convert the angle to a direction vector
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                // Calculate the teleport position
                Vector3 teleportPosition = _targetPlayer.transform.position + direction * distance;

                // Raycast downwards to find the ground
                RaycastHit hit;
                if (Physics.Raycast(teleportPosition + Vector3.up * 100f, Vector3.down, out hit, Mathf.Infinity))
                {
                    // Use the hit point as the teleport position
                    teleportPosition = hit.point;
                }
                else
                {
                    // Failed to find ground, skip this teleport attempt
                    attempts++;
                    continue;
                }

                // Check if there's a path from teleport position to player position
                if (CanReach(teleportPosition, _targetPlayer.transform.position))
                {
                    Debug.Log("WARPING--------------------------------------------------");
                    agent.Warp(teleportPosition);
                    return true;
                }

                attempts++;
            }

            // Failed to find a valid teleport position after max attempts
            return false;
        }

        // No target player, teleportation not possible
        return false;
    }

    //Voice Logic
    public void RandomLaugh()
    {
        switch (RandomNumberGenerator.GetInt32(1,11))
        {
            case(1) :
                creatureVoice.PlayOneShot(laugh1);
                break;
            case(2) :
                creatureVoice.PlayOneShot(laugh2);
                break;
            case(3):
                creatureVoice.PlayOneShot(laugh3);
                break;
            case(4) :
                creatureVoice.PlayOneShot(laugh4);
                break;
            case(5) :
                creatureVoice.PlayOneShot(laugh5);
                break;
            case(6):
                creatureVoice.PlayOneShot(laugh6);
                break;
            case(7) :
                creatureVoice.PlayOneShot(laugh7);
                break;
            case(8) :
                creatureVoice.PlayOneShot(laugh8);
                break;
            case(9):
                creatureVoice.PlayOneShot(laugh9);
                break;
            case(10) :
                creatureVoice.PlayOneShot(laugh10);
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
            foreach (DoorLock door in FindObjectsOfType(typeof(DoorLock)) as DoorLock[])
            {
                var thisDoor = door.transform.parent.transform.parent.transform.parent.gameObject;
                if (!door.GetComponent<Rigidbody>())
                {
                    if (Vector3.Distance(transform.position, thisDoor.transform.position) <= 4f)
                    {
                        Debug.Log("Crying beau coup");
                        Debug.Log(thisDoor.name);
                        BashDoorClientRpc(thisDoor, (targetPlayer.transform.position - transform.position).normalized * 20);
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
        private void LocalPlayerFreddyHandler()
        {
            if(_targetPlayer!=null && !_targetPlayer.isPlayerControlled)
            if (_playerSleep[_indexSleepArraySleep].SleepMeter == _enterSleep - 50)
            {
                //Currently FOrce Loaded
                //enterTheDream.LoadAudioData();
            }
            if (_playerSleep[_indexSleepArraySleep].SleepMeter ==_enterSleep-25)
            {
                creatureSFX.PlayOneShot(enterTheDream);
                //Currently Force Loaded
                //terminus.LoadAudioData();
            }
            if (_playerSleep[_indexSleepArraySleep].SleepMeter >= _enterSleep)
            {
                EnemyMeshAndPerson(true);
                if (!creatureSFX.isPlaying)
                {
                    creatureSFX.Play();
                }
                
            }
            else
            {
                EnemyMeshAndPerson(false);
                if (!creatureSFX.isPlaying)
                {
                    //TODO implement fade volume
                    creatureSFX.Stop();
                }
            }
            if (_playerSleep[_indexSleepArraySleep].SleepMeter == _maxSleep-80)
            {
                creatureSFX.PlayOneShot(terminus);
            }
            else if (_playerSleep[_indexSleepArraySleep].SleepMeter < _maxSleep-80)
            {
                oneShotCreature.Stop();
            }
        }

        public void EnemyMeshAndPerson(bool enable)
        {
            EnableEnemyMesh(enable);
            if (enable == true)
            {
                creatureVoice.mute = false;
            }
            else
            {
                creatureVoice.mute = true;
                
            }
        }
        public override void EnableEnemyMesh(bool enable, bool overrideDoNotSet = false)
        {
            if (skinnedMeshRenderers == null || meshRenderers == null)
            {
                Debug.LogError("skinnedMeshRenderers or meshRenderers array is null.");
                return;
            }

            int num = enable ? 19 : 23;
            foreach (var renderer in skinnedMeshRenderers)
            {
                if (renderer != null && (!renderer.CompareTag("DoNotSet") || overrideDoNotSet))
                {
                    renderer.gameObject.layer = num;
                }
            }
            foreach (var renderer in meshRenderers)
            {
                if (renderer != null && (!renderer.CompareTag("DoNotSet") || overrideDoNotSet))
                {
                    renderer.gameObject.layer = num;
                }
            }
        }
        
        //Kill Handler :
        public override void OnCollideWithPlayer(Collider other)
        {
            Debug.Log("Oh no");
            PlayerControllerB playerControllerB = other.gameObject.GetComponent<PlayerControllerB>();
            if (playerControllerB != null)
            {
                foreach (var player in _playerSleep)
                {
                    if (playerControllerB.GetClientId() == player.ClientID && playerControllerB.isPlayerControlled)
                    {
                        if (player.SleepMeter>=_enterSleep)
                        {
                            playerControllerB.KillPlayer(Vector3.zero,true, CauseOfDeath.Unknown, 1);
                            Debug.Log("KILL");
                        }
                    }
                }
            }
            //TODO Implement kill behaviour
        }
    //FOOTSTEP SOUND WAY TO DO IT
    
        /*
        public void SelectAndPlayFootstep()
            {     
                switch (currentTerrain)
                {
                    case CURRENT_TERRAIN.GRAVEL:
                        PlayFootstep(1);
                        break;
        
                    case CURRENT_TERRAIN.GRASS:
                        PlayFootstep(0);
                        break;
        
                    case CURRENT_TERRAIN.WOOD_FLOOR:
                        PlayFootstep(2);
                        break;
        
                    case CURRENT_TERRAIN.WATER:
                        PlayFootstep(3);
                        break;
        
                    default:
                        PlayFootstep(0);
                        break;
                }
            }
        */

        public override void OnDestroy()
        {
            base.OnDestroy();
            _playerSleep = null;
            StopAllCoroutines();

        }


        //______________________________________________________//
        
        
        
        
    
    //MESSAGE HANDLER LOGIC
    //TODO Stop it from running 2 times for ntg
    private void ActualiseClientSleep(List<PlayerSleep> x)
    {
        //Reload Index If death of player
        if (_playerSleep!=null)
        {
            if (_playerSleep.Count!=x.Count)
            {
                _indexSleepArraySleep = -1;
            }
        }
        _playerSleep = x;
        SetTargetPlayer();
        LocalPlayerFreddyHandler();
    }
    
    /*
    private float transitionDuration = 5f; // Transition duration in seconds
    private float startTime;
    private bool transitioning = false;

    private Color[] originalColors; // Store original colors for each material
    
    //
    // private void Start()
    // {
    //     startTime = Time.time;
    //     CacheOriginalColors();
    // }
    //
    // private void Update()
    // {
    //     if (transitioning)
    //     {
    //         float t = Mathf.Clamp01((Time.time - startTime) / transitionDuration);
    //        ApplyDesaturation(t);
    //     }
    // }

    public void StartTransition()
    {
        transitioning = true;
    }

    private void CacheOriginalColors()
    {
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    private void ApplyDesaturation(float t)
    {
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = renderers[i].material;
            Color originalColor = originalColors[i];

            float grayscaleValue = 0.299f * originalColor.r + 0.587f * originalColor.g + 0.114f * originalColor.b;
            Color grayscaleColor = new Color(grayscaleValue, grayscaleValue, grayscaleValue, originalColor.a);

            material.color = Color.Lerp(originalColor, grayscaleColor, t);
        }
    }
/*
 * using UnityEngine;

public class ColorToBWTransition : MonoBehaviour
{
    public float transitionDuration = 5f; // Transition duration in seconds
    private float startTime;
    private bool transitioning = false;
    private bool isBlackAndWhite = false; // Current mode

    private Color[] originalColors; // Store original colors for each material

    private void Start()
    {
        startTime = Time.time;
        CacheOriginalColors();
    }

    private void Update()
    {
        if (transitioning)
        {
            float t = Mathf.Clamp01((Time.time - startTime) / transitionDuration);
            ApplyDesaturation(t);
        }
    }

    public void StartTransition()
    {
        transitioning = true;
        isBlackAndWhite = !isBlackAndWhite; // Toggle mode
    }

    public void SwitchToOriginalColors()
    {
        transitioning = false;
        isBlackAndWhite = false; // Switch back to color mode
        RestoreOriginalColors();
    }

    private void CacheOriginalColors()
    {
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    private void ApplyDesaturation(float t)
    {
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = renderers[i].material;
            Color originalColor = originalColors[i];

            float grayscaleValue = 0.299f * originalColor.r + 0.587f * originalColor.g + 0.114f * originalColor.b;
            Color grayscaleColor = new Color(grayscaleValue, grayscaleValue, grayscaleValue, originalColor.a);

            Color targetColor = isBlackAndWhite ? grayscaleColor : originalColor;
            material.color = Color.Lerp(material.color, targetColor, t);
        }
    }

    private void RestoreOriginalColors()
    {
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = renderers[i].material;
            material.color = originalColors[i];
        }
    }
}

 */
    //FIXING METHODS
    public void CollisionHandlerFreddy()
    {
        if(freddyHitBox !=null)
        {
            Debug.Log("Idk what the issue is");
        }
    }
    private AudioSource FindAudioSourceInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            AudioSource audioSource = child.GetComponent<AudioSource>();
            if (audioSource != null && child.name == name)
            {
                return audioSource;
            }

            // Recursive call to search in the children of this child
            AudioSource found = FindAudioSourceInChildren(child, name);
            if (found != null)
            {
                return found;
            }
        }

        // If not found among the children, return null
        return null;
    }

}