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

public class FreddyAI : EnemyAI
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
    
    //2D ARRAY 
    private List<PlayerSleep> _playerSleep;

    private List<PlayerSleep> _playerSleepServ;
    
    
    
    //POST MAIL Array 2D
    private LethalServerMessage<List<PlayerSleep>> _serverMessageSleepArray;
    
    private LethalClientMessage<List<PlayerSleep>> _clientReceiveSleepArray;
    
    //Post Mail behavior Int
    private LethalServerMessage<int> _serverMessageBehavior;
    
    private LethalClientMessage<int> _clientReceiveBehavior;
    //Fast info
    private ulong _clientID;
    private ulong _lastSleepMeter;

    private int indexSleepArrayTarget = -1;
    private int _indexSleepArraySleep = -1;
    
    
    private PlayerControllerB _targetPlayer;
    private int _behaviourIndexServer;
    private bool _justSwitchedBehaviour;
    //RunningClaw Verifications;
    private bool _wasInsideFactory;
    private bool _triggerTeleportDoor;

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
            _playerSleepServ = new List<PlayerSleep>();
            foreach (var t in RoundManager.Instance.playersManager.allPlayerScripts)
            {
                //Adds All the player with There client ID, sleep meter 0 and deactivated the target player
                if (t.isPlayerControlled)
                {
                    _playerSleepServ.Add(new PlayerSleep(
                        t.GetClientId(), 
                        0, 
                        false
                    ));
                }
            }
            
            _behaviourIndexServer = 0;
            _justSwitchedBehaviour = true;
        }
        
        //Message service Array 2D
        _serverMessageSleepArray = new LethalServerMessage<List<PlayerSleep>>(identifier: "customIdentifier");
        _clientReceiveSleepArray = new LethalClientMessage<List<PlayerSleep>>(identifier: "customIdentifier", onReceived: ActualiseClientSleep);
        //Mesage service int 8
        _serverMessageBehavior = new LethalServerMessage<int>(identifier: "customIdentifier");
        _clientReceiveBehavior = new LethalClientMessage<int>(identifier: "customIdentifier", onReceived: SetClientBehavior);
        
        //ClientReceiveSleepAray.OnReceived += ActualiseClientSleepServer; Useless Server Receive Logic
        _clientReceiveSleepArray.OnReceived += ActualiseClientSleep;
        //Behavior SEND Int
        _clientReceiveBehavior.OnReceived += SetClientBehavior;

        _clientID = RoundManager.Instance.playersManager.localPlayerController.GetClientId();
    }
    
    [NonSerialized] private double _timer =0;
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
        
        if (_targetPlayer != null)
        {
            SetDestinationToPosition(_targetPlayer.transform.position);
            if (_targetPlayer.isInsideFactory != _wasInsideFactory)
            {
                _triggerTeleportDoor = true;
                _wasInsideFactory = _targetPlayer.isInsideFactory;
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
                if (_justSwitchedBehaviour)
                {
                    TeleportRandomlyAroundPlayer(10, 20);
                    IdleFreddy();
                    creatureAnimator.SetBool("Walking",true);
                    agent.speed = 3f;
                    _justSwitchedBehaviour = false;
                    
                }
                if (!SetDestinationToPosition(_targetPlayer.transform.position, true) && _targetPlayer != null)
                {
                    TeleportRandomlyAroundPlayer(20, 30);
                }
                StartCoroutine(teleportCooldown());
                break;
            case (int)State.Running:
                if (_justSwitchedBehaviour)
                {
                    
                    TeleportRandomlyAroundPlayer(20, 30);
                    IdleFreddy();
                    creatureAnimator.SetBool("Running", true);
                    agent.speed = 7f;
                    _justSwitchedBehaviour = false;
                }
                StartCoroutine(teleportCooldown());
                break;
            case (int)State.RunningClaw:
                if (_justSwitchedBehaviour)
                {
                    TeleportRandomlyAroundPlayer(40, 70);
                    IdleFreddy();
                    creatureAnimator.SetBool("RunWithClaw", true);
                    agent.speed = 6f;
                    
                    _justSwitchedBehaviour = false;
                }
                if (_triggerTeleportDoor)
                {
                    TeleportRandomlyAroundPlayer(30, 60);
                    _triggerTeleportDoor = false;
                }
                if (!_targetPlayer.isPlayerControlled && IsHost)
                {
                    ChooseTarget();
                }
                
                break;
            case (int)State.Sneaking:
                if (_justSwitchedBehaviour)
                {
                    TeleportRandomlyAroundPlayer(15, 20);
                    IdleFreddy();
                    agent.speed = 2f;
                    creatureAnimator.SetBool("Sneaking", true);
                    _justSwitchedBehaviour = false;
                    
                    
                }
                Bounds bounds = GetComponent<Renderer>().bounds;

                // Calculate the screen position of the object's center
                //IDK IF IT IS THE RIGHT CAMERA
                Vector3 screenPoint = _targetPlayer.gameplayCamera.WorldToViewportPoint(bounds.center);

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

        for (int count = 0; count < _playerSleepServ.Count; count++)
        {
            PlayerControllerB player = _playerSleepServ[count].ClientID.GetPlayerController();
            //HANDLE DEAD PLAYER
            if (!player.isPlayerControlled)
            {
                //Remove DEAD player for target list
                _playerSleepServ.RemoveAt(count);
            }
            else
            {
                if (CheckIfAlone(player))
                {
                    // Add 1 to the value of the player THAT IS ALONE
                    _playerSleepServ[count].SleepMeter += 1;
                }
                else
                {
                    // Remove 1 from the value of the player in the dictionary
                    if (_playerSleepServ[count].SleepMeter > 0)
                    {
                        if (_playerSleepServ[count].SleepMeter == 1)
                        {
                            //Handle if it is 1
                            _playerSleepServ[count].SleepMeter = 0;
                        }
                        else
                        {
                            //handle if more than 1
                            _playerSleepServ[count].SleepMeter -= 2;
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
        Debug.Log("SERVER SIDE" + _playerSleepServ[0].SleepMeter);
        
        //Sending computation to all clients
        _serverMessageSleepArray.SendAllClients(_playerSleepServ);
    }
    public bool CheckIfAlone(PlayerControllerB player)
    {
        //GET Player Position
        Vector3 currentPlayerPosition = player.transform.position;

        for (int count = 0; count < _playerSleepServ.Count; count++)
        {
            if (_playerSleepServ[count].ClientID != player.GetClientId())
            {
                Vector3 otherPlayerPosition = _playerSleepServ[count].ClientID.GetPlayerController().transform.position;
                // Calculate the distance between the current player and the other player
                float distance = Vector3.Distance(currentPlayerPosition, otherPlayerPosition);

                // Check if the distance is within the specified range
                if (distance <= 10f && player != _playerSleepServ[count].ClientID.GetPlayerController())
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
        for (int count = 0; count < _playerSleepServ.Count; count++)
        {
            if (_playerSleepServ[count].SleepMeter >= 260) //SLEEP METER TRESHOLD --- IMPORTANT
            {
                possibleTarget.Add(_playerSleepServ[count]);
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
        foreach (var player in _playerSleepServ)
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
        if (_targetPlayer != null)
        {
            bool updateToClient = false;
            if (_playerSleepServ[_indexSleepArraySleep].SleepMeter >=260 && _playerSleepServ[_indexSleepArraySleep].SleepMeter <400)
            {
                //WALKING
                _behaviourIndexServer = 1;
                
                if (_playerSleepServ[_indexSleepArraySleep].SleepMeter >= RandomNumberGenerator.GetInt32(300,450))
                {
                    if (currentBehaviourStateIndex != 2)
                    {
                        _behaviourIndexServer = 2;
                        updateToClient = true;
                    }
                    //RUNNING
                }
                else if (_playerSleepServ[_indexSleepArraySleep].SleepMeter >= RandomNumberGenerator.GetInt32(350, 450))
                {
                    //Sneaking
                    if (currentBehaviourStateIndex != 4)
                    {
                        _behaviourIndexServer = 4;
                        updateToClient = true;
                    }
                }
               
            }
            else if(_playerSleepServ[_indexSleepArraySleep].SleepMeter >=400)
            {
                //KILL time
                _behaviourIndexServer = 3;
            }
            //If The state is different, it sends to clients
            if (updateToClient)
            {
                _serverMessageBehavior.SendAllClients(_behaviourIndexServer,true);
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
                isThereTarget = true;
            }
            count++;
        }
        if (isThereTarget)
        {
            _targetPlayer = null;
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
                teleportPosition = _targetPlayer.transform.position + direction * distance;

                // Ensure the teleport position is on the NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(teleportPosition, out hit, maxTeleportDistance, NavMesh.AllAreas))
                {
                    // Teleport the Krueger to the calculated position
                    agent.Warp(hit.position); // Teleport the Krueger to the valid position on the NavMesh
                    if(SetDestinationToPosition(_targetPlayer.transform.position, true))
                    {
                        foundValidPosition = true;
                    }
                    turnCompass.LookAt(_targetPlayer.gameplayCamera.transform.position);

                    // If a valid path is found, set destination to the player's position
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
            if (_playerSleep[_indexSleepArraySleep].SleepMeter ==235)
            {
                oneShotCreature.PlayOneShot(enterTheDream);
            }
            if (_playerSleep[_indexSleepArraySleep].SleepMeter >= 260)
            {
                EnemyMeshAndPerson(true);
            }
            else
            {
                EnemyMeshAndPerson(false);
                oneShotCreature.Stop();
            }

            if (_playerSleep[_indexSleepArraySleep].SleepMeter == 320)
            {
                oneShotCreature.PlayOneShot(terminus);
            }
            else if (_playerSleep[_indexSleepArraySleep].SleepMeter < 320)
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
        //Kill Handler :
        public override void OnCollideWithPlayer(Collider other)
        {
            base.OnCollideWithPlayer(other);
            PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other);
            //Stop if there is no player controller B
            if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null))
                return;
            
        }
        
        
        
        
        
        //______________________________________________________//
        
        
        
        
    
    //MESSAGE HANDLER LOGIC
    private void ActualiseClientSleep(List<PlayerSleep> x)
    {
        Debug.Log("ClientSide "+x[0].SleepMeter);
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
        localPlayerFreddyHandler();
    }

    private void SetClientBehavior(int x)
    {
        currentBehaviourStateIndex = x;
        _justSwitchedBehaviour = true;
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
}