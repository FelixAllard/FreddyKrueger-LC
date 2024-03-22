using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using GameNetcodeStuff;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//From Chaos postprocess thingy
using GraphicsAPI;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.HighDefinition.RenderPipelineSettings;
using UnityEngine.Rendering.HighDefinition;
using GraphicsAPI.CustomPostProcessing;
using UnityEngine.Serialization;

namespace ExampleEnemy;

public class FreddyAi :  EnemyAI
{
    //Special Audio clip
    public AudioClip laugh1;
    public AudioClip laugh2;
    public AudioClip laugh3;
    public AudioClip laugh4;
    public AudioClip laugh5;
    public AudioClip laugh6;
    public AudioClip laugh7;
    public AudioClip laugh8;
    public AudioClip laugh9;
    public AudioClip laugh10;
    public AudioClip inTheNightmare;
    public AudioClip enterTheDream;
    public AudioClip terminus;
    //Audio source
    public AudioSource talkingAudioSource;
    public AudioSource oneShotCreature;
    public AudioSource feet1;
    public AudioSource feet2;
    //Particle system
    public ParticleSystem freddyRain;
    public ParticleSystem freddyTeleport;
    //Transform
    public Transform attackArea;
    //Try post process
    public Material BWShadder;

    public Material shirt;
    public Material pants;
    public Material hands;
    public Material claw;
    [FormerlySerializedAs("hat")] public Material face;
    
    
    
    private List<PlayerSleep> _playerSleep;
    
    //Local Player Info
    private ulong _clientId;
    private int _indexSleepArray;
    
    //Diverse Logic Variables;
    private bool _justSwitchedBehaviour;
    private bool _footStepRight;
    
    //Sleep parameter
    private int _enterSleep;
    private int _maxSleep;
    private bool _localPlayerAsleep;

    private int _behaviourIndexServer;
    private float _timer;
    private PlayerSleep _targetPlayerSleep;
    private bool _inCoroutine;
    private bool _firstCries;
    private bool _someoneIsKo;
    private bool _alreadyPlayerIntro = false;
    private bool _alreadyClock = false;
    private bool _freddySeen;
    
    
    //PostProcess
    /*private PostProcessVisualsFreddy freddyPost;*/
    

    

    enum State
    {
        Spawning,
        Walking,
        Running,
        RunningClaw, 
        Sneaking,
        None
    }

    public void Awake()
    {
        _enterSleep = FreddyConfig.Instance.ENTER_SLEEP.Value;
        _maxSleep = FreddyConfig.Instance.SLEEP_MAX.Value;
        //Sleep Validation
        if (_enterSleep == null)
        {
            if (_enterSleep <= 49)
            {
                _enterSleep = 50;
            }

            if (_maxSleep<= _enterSleep+80)
            {
                _maxSleep = _enterSleep + 80;
            }
        }
    }
    
    public override void Start()
    {
        base.Start();
        /*RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        
        // Assign the material to the Render Texture
        Graphics.Blit(null, renderTexture, BWShadder);
        
        // Apply the Render Texture to the camera
        RoundManager.Instance.playersManager.allPlayerScripts[0].gameplayCamera.targetTexture = renderTexture;
        
        if (HUDManager.Instance.gameObject.GetComponent<PostProcessVisualsFreddy>() != null)
        {
            // Get the component
            freddyPost = HUDManager.Instance.GetComponent<PostProcessVisualsFreddy>();
  
            if (freddyPost.fullScreenPass != null)
            {
                freddyPost.fullScreenPass.enabled = true;
                // Use freddyPost.fullScreenPass here
            }
        }*/
        
        
        
        if (FreddyConfig.Instance.SOUND_CHILD_VOLUME > 1.0f)
        {
            creatureSFX.volume = 1.0f;
        }
        else if (FreddyConfig.Instance.SOUND_CHILD_VOLUME <0.0f)
        {
            creatureSFX.volume = 0.0f;
        }
        else
        {
            creatureSFX.volume = FreddyConfig.Instance.SOUND_CHILD_VOLUME;
        }
        _clientId = RoundManager.Instance.playersManager.localPlayerController.actualClientId;
        if (IsHost)
        {
            _behaviourIndexServer = 0;
            _justSwitchedBehaviour = true;
            
        }
        StartCoroutine(SeeIfAccessible());
        _clientId = RoundManager.Instance.playersManager.localPlayerController.actualClientId;
        freddyRain.Stop();
        freddyTeleport.Stop();
        

    }

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
            //Footstep interval handler
        }
    }

    public override void DoAIInterval()
    {

        base.DoAIInterval();
        if (targetPlayer != null)
        {
            SetDestinationToPosition(targetPlayer.transform.position, true);
        
            switch(currentBehaviourStateIndex) {
                case (int)State.Spawning:
                    if (targetPlayer)
                    {
                        agent.speed = 0f;
                        IdleFreddy();
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
                        TeleportRandomlyAroundPlayer(20, 25);
                        IdleFreddy();
                        DoAnimationClientRpc("Walking",true);
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
                        DoAnimationClientRpc("Running",true);
                        agent.speed = 4f + FreddyConfig.Instance.SPEED_MODIFIER;
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
                        DoAnimationClientRpc("RunWithClaw",true);
                        agent.speed = 5+ FreddyConfig.Instance.SPEED_MODIFIER;
                        
                        _justSwitchedBehaviour = false;
                    }
                    agent.speed += FreddyConfig.Instance.SPEED_MODIFIER_LAST_STAGE;
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
                        agent.speed = 1+ FreddyConfig.Instance.SPEED_MODIFIER;
                        DoAnimationClientRpc("Sneaking", true);
                        StartCoroutine(WaitAndChangeBehavior(17));

                        _justSwitchedBehaviour = false;
                        agent.acceleration = 0;
                    }
                    if (IsHost)
                    {
                        DoorHandler(false);
                    }
                    Vector3 screenPoint = targetPlayer.gameplayCamera.WorldToViewportPoint(transform.position);

                    // Check if the object is within the camera's view
                    /*
                    bool isVisible = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
                    if (isVisible)
                    {
                        
                    }
                    */
                    //SNEAKING AHHHHH
                    if (targetPlayer.HasLineOfSightToPosition(transform.position) && _freddySeen ==false)
                    {
                        StartCoroutine(FreddyGotSeen());
                    }
                    break;
                case (int)State.None:
                    
                    break;
                default:
                    Debug.Log("Behavior State Missing");
                    break;
            }
        }
    }

    IEnumerator FreddyGotSeen()
    {
        yield return new WaitForSeconds(2);
        IdleFreddy();
        RandomLaughClientRpc(RandomNumberGenerator.GetInt32(1,11));
        agent.speed = 0f;
        if (!_inCoroutine)
        {
            RandomLaughClientRpc(RandomNumberGenerator.GetInt32(1,11));
            DoAnimationClientRpc("Suprised");
            StartCoroutine(WaitAndChangeBehavior(3f));
            _inCoroutine = true;
        }

        _freddySeen = true;
    }
    //Business Handler Logic Server:
    IEnumerator SeeIfAccessible()
    {
        int count = 0;
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (targetPlayer != null && !CanReach(transform.position, targetPlayer.transform.position))
            {
                count++;
                if (count >= 3)
                {
                    SwitchToBehaviourClientRpc(0);
                    count = -1;
                }
            }
            else
            {
                count = 0;
            }
        }
    }
    
    // Sleep handler server
    public void UpdateSleep()
    {
        


        if (_playerSleep != null)
        {
            CheckIfMissingPlayer();
        }
        else
        {
            _playerSleep = new List<PlayerSleep>();
            foreach (var t in RoundManager.Instance.playersManager.allPlayerScripts.Where(obj=>obj.isPlayerControlled))
            {
                //Adds All the player with There client ID, sleep meter 0 and deactivated the target player
                _playerSleep.Add(new PlayerSleep(
                    t.actualClientId, 
                    0
                ));
                Debug.Log("Added a player");
            }
            
        }
        for (int count = 0; count < _playerSleep.Count; count++)
        {
            PlayerControllerB player = GetPlayerController(_playerSleep[count].ClientID);

            if (CheckIfAlone(player))
            {
                // Add 1 to the value of the player THAT IS ALONE
                
                if (FreddyConfig.Instance.SHIP_MOMMY_TARGET.Value || !player.isInHangarShipRoom)
                {
                    _playerSleep[count].SleepMeter += 1;
                }
                //Solo Gameplay Handler
                if (FreddyConfig.Instance.SOLO_GAMEPLAY.Value)
                {
                    if (_enterSleep + FreddyConfig.Instance.TIME_BEFORE_LEAVING_SLEEP.Value == _playerSleep[count].SleepMeter)
                    {
                        _playerSleep[count].SleepMeter = 0;
                        if (FreddyConfig.Instance.RANDOM_SLEEP.Value)
                        {
                            _enterSleep = (FreddyConfig.Instance.ENTER_SLEEP.Value +
                                           RandomNumberGenerator.GetInt32(0, 50));
                            _maxSleep = _enterSleep + 200;
                        }
                        
                    }
                }
            }
            else
            {
                // Remove 1 from the value of the player in the dictionary
                if (_playerSleep[count].SleepMeter > 0)
                {
                    if (_playerSleep[count].SleepMeter == 1)
                    {
                        //Handle if it is 1
                        _playerSleep[count].SleepMeter = 0;
                    }
                    else
                    {
                        //handle if more than 1
                        _playerSleep[count].SleepMeter -= 2;
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

        // Serialize the list to JSON
        string json = JsonConvert.SerializeObject(_playerSleep);

        // Call the ClientRpc with the JSON string
        SetSleepClientRpc(json, false);

    }
    public void ChooseTarget()
    {
        List<PlayerSleep> possibleTarget = new List<PlayerSleep>();
        //FInd which Player has a score over sleep treshold !!!
        foreach (var t in _playerSleep)
        {
            if (t.SleepMeter >= _enterSleep && GetPlayerController(t.ClientID).isPlayerControlled) // SLEEP METER TRESHOLD --- IMPORTANT
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

        if (possibleTarget.Count != 0 && _playerSleep != null) 
        {
            foreach (var player in possibleTarget)
            {
                PlayerControllerB spaceRamPlayerHandler= GetPlayerController(player.ClientID);
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
            //OLD CODE FOR CHOOSING TARGET
            //Target player Becomes true for the target player and false for the rest
            bool targetPlayerExistent = false;

            foreach (var player in _playerSleep)
            {
                if (player.ClientID == highestSleepPoints.ClientID && highestSleepPoints.ClientID!=9999999999)
                {
                    SetTargetPlayerClientRpc(player.ClientID);
                    return;
                }
                else
                { }
            }
        }
    }
    public void SetBehavior()
    {
        if (targetPlayer != null)
        {
            bool updateToClient = false;
            if (_targetPlayerSleep.SleepMeter >=_enterSleep && _targetPlayerSleep.SleepMeter <_maxSleep)
            {
                
                if (_targetPlayerSleep.SleepMeter >= RandomNumberGenerator.GetInt32((_maxSleep-50),(_maxSleep+20))-FreddyConfig.Instance.STATE_INCREASE)
                {
                    if (currentBehaviourStateIndex != 2)
                    {
                        _behaviourIndexServer = 2;
                        updateToClient = true;
                    }
                    //RUNNING
                }
                else if (_targetPlayerSleep.SleepMeter >= RandomNumberGenerator.GetInt32((_maxSleep-50),(_maxSleep+20))-FreddyConfig.Instance.STATE_INCREASE)
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
        else
        {
            if (_behaviourIndexServer != 5)
            {
                SwitchToBehaviourClientRpc(5);
            }
        }
    }
    
    //Sleep handler Client
    private void LocalPlayerFreddyHandler()
        {
            if (_indexSleepArray != -1 && !RoundManager.Instance.playersManager.localPlayerController.isPlayerDead)
            {
                if (targetPlayer != null && !targetPlayer.isPlayerControlled)
                {
                    if (_playerSleep[_indexSleepArray].SleepMeter == _enterSleep - 50)
                    {
                        //Currently FOrce Loaded
                        //enterTheDream.LoadAudioData();
                    }
                }
                if (_playerSleep[_indexSleepArray].SleepMeter >=_enterSleep-25)
                {
                    if (_alreadyPlayerIntro == false)
                    {
                        Debug.Log("Enter the dream! Sweet dream...");
                        creatureSFX.PlayOneShot(enterTheDream);
                        _alreadyPlayerIntro = true;
                    }
                    //Currently Force Loaded
                    //terminus.LoadAudioData();
                }
                if (_playerSleep[_indexSleepArray].SleepMeter >= _enterSleep)
                {
                    EnemyMeshAndPerson(true);
                    if (!creatureSFX.isPlaying)
                    {
                        creatureSFX.Play();
                        _firstCries = true;
                    }
                
                }
                else
                {
                    EnemyMeshAndPerson(false);
                    if (_firstCries)
                    {
                        creatureSFX.Stop();
                    }
                }
                if (_playerSleep[_indexSleepArray].SleepMeter >= _maxSleep-80)
                {
                    if (!_alreadyClock)
                    {
                        Debug.Log("You better run!");
                        creatureSFX.PlayOneShot(terminus);
                        _alreadyClock = true;
                    }
                }
                else if (_playerSleep[_indexSleepArray].SleepMeter < _maxSleep-80)
                {
                }
            }
            else
            {
                if (_firstCries)
                {
                    creatureSFX.Stop();
                }

                creatureVoice.Stop();
                creatureSFX.Stop();
            }
        }

        
    
    
    // CLIENT RPC SECTION
    [ClientRpc]
    public void SetSleepClientRpc(string json, bool reinitialiseArray)
    {
        _playerSleep = JsonConvert.DeserializeObject<List<PlayerSleep>>(json);
        if (reinitialiseArray)
        {
            SetLocalIndex();
        }
        // We set the value
        //_playerSleep = playerSleeps;
        LocalPlayerFreddyHandler();
    }

    [ClientRpc]
    public void SetTargetPlayerClientRpc(ulong targetPlayerId)
    {
        targetPlayer = GetPlayerController(targetPlayerId);
        _targetPlayerSleep = _playerSleep.Find(obj => obj.ClientID == targetPlayerId);
    }
    [ClientRpc]
    public void ShowTeleportParticleClientRpc()
    {
        if (_playerSleep != null && _indexSleepArray != -1)
        {
            StartCoroutine(TransitionMaterial(false, 1f));
            //TODO remove unused freddyTeleport
            //freddyTeleport.Play();
        }
    }
    
    [ClientRpc]
    public void DoAIStepIntervalClientRpc()
    {
        if (_indexSleepArray != -1)
        {
            if (_playerSleep[_indexSleepArray].SleepMeter >= _enterSleep)
            {
                if (_footStepRight)
                {
                    feet1.Play();
                    _footStepRight = false;
                }
                else
                {
                    feet2.Play();
                    _footStepRight = true;
                }
            }
        }
    }
    [ClientRpc]
    public void DoAnimationClientRpc(string animationName, bool setActive)
    {
        creatureAnimator.SetBool(animationName,setActive);
    }
    [ClientRpc]
    public void DoAnimationClientRpc(string animationName)
    {
        creatureAnimator.SetTrigger(animationName);
    }


    public override void OnCollideWithPlayer(Collider other)
    {
        PlayerControllerB playerControllerB = other.gameObject.GetComponent<PlayerControllerB>();;
        if (playerControllerB != null)
        {
            PlayerSleep sleepTouchedPlayer = _playerSleep.FirstOrDefault(obj => obj.ClientID == playerControllerB.actualClientId);
            if (sleepTouchedPlayer.SleepMeter >= _enterSleep)
            {
                RandomLaughClientRpc(RandomNumberGenerator.GetInt32(1,11));
                playerControllerB.KillPlayer(Vector3.up,true,CauseOfDeath.Unknown,1);
                SwitchToBehaviourClientRpc(0);
            }
        }
    }
    /*
    [ClientRpc]
    public void SwingAttackHitClientRpc()
    {
        
    }*/
    [ClientRpc]
    public void RandomLaughClientRpc(int x)
    {
        if (_indexSleepArray != -1)
        {
            if (_playerSleep[_indexSleepArray].SleepMeter >= _enterSleep)
            {
                switch (x)
                {
                    case(1) :
                        talkingAudioSource.PlayOneShot(laugh1);
                        break;
                    case(2) :
                        talkingAudioSource.PlayOneShot(laugh2);
                        break;
                    case(3):
                        talkingAudioSource.PlayOneShot(laugh3);
                        break;
                    case(4) :
                        talkingAudioSource.PlayOneShot(laugh4);
                        break;
                    case(5) :
                        talkingAudioSource.PlayOneShot(laugh5);
                        break;
                    case(6):
                        talkingAudioSource.PlayOneShot(laugh6);
                        break;
                    case(7) :
                        talkingAudioSource.PlayOneShot(laugh7);
                        break;
                    case(8) :
                        talkingAudioSource.PlayOneShot(laugh8);
                        break;
                    case(9):
                        talkingAudioSource.PlayOneShot(laugh9);
                        break;
                    case(10) :
                        talkingAudioSource.PlayOneShot(laugh10);
                        break;
                    default:
                        Debug.LogError("Number Generated for laugh was a miss!");
                        break;
                }
            }
        }
    }
    [ClientRpc]
    public void TeleportExecuteClientRpc(Vector3 position)
    {
        agent.Warp(position);
        StartCoroutine(TransitionMaterial(true, 7));
        if (IsHost)
        {
            DoAnimationClientRpc("Teleport");
            RandomLaughClientRpc(RandomNumberGenerator.GetInt32(1,11));
        }
    }
    //Utils
    public static PlayerControllerB? GetPlayerController(ulong clientId)
    {
        return StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[clientId]];
    }
    public bool CheckIfAlone(PlayerControllerB player)
    {
        //GET Player Position
        Vector3 currentPlayerPosition = player.transform.position;

        for (int count = 0; count < _playerSleep.Count; count++)
        {
            if (_playerSleep[count].ClientID != player.actualClientId)
            {
                Vector3 otherPlayerPosition = GetPlayerController(_playerSleep[count].ClientID).transform.position;
                // Calculate the distance between the current player and the other player
                float distance = Vector3.Distance(currentPlayerPosition, otherPlayerPosition);

                // Check if the distance is within the specified range
                if (distance <= FreddyConfig.Instance.DISTANCE_FOR_SLEEP && player != GetPlayerController(_playerSleep[count].ClientID))
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
    public void CheckIfMissingPlayer()
    {
        if (_playerSleep.Count >0)
        {
            try
            {
                foreach (var player in _playerSleep)
                {
                    if (GetPlayerController(player.ClientID).isPlayerDead)
                    {
                        Debug.Log("Removing Player From array");
                        _playerSleep.Remove(player);
                        SetSleepClientRpc(JsonConvert.SerializeObject(_playerSleep),true);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Removing player");
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
        /*
            if (Vector3.Distance(transform.position, FindObjectOfType<HangarShipDoor>().transform.position) <= 4f)
            {
                FindObjectOfType<HangarShipDoor>().SetDoorOpen();
            }
            */
    }
    public void TeleportRandomlyAroundPlayer(float minTeleportDistance, float maxTeleportDistance)
    {
        if (targetPlayer != null)
        {
            int maxAttempts = 10;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                // Calculate a random angle around the player
                float angle = Random.Range(0f, 360f);

                // Calculate a random distance from the player
                float distance = Random.Range(minTeleportDistance, maxTeleportDistance);

                // Convert the angle to a direction vector
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                // Calculate the teleport position
                Vector3 teleportPosition = targetPlayer.transform.position + direction * distance;

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
                if (CanReach(teleportPosition, targetPlayer.transform.position))
                {
                    ShowTeleportParticleClientRpc();
                    StartCoroutine(ExecuteTeleportFreddy(teleportPosition));
                    return;
                }
                attempts++;
            }
            // Failed to find a valid teleport position after max attempts
            return;
        }

        // No target player, teleportation not possible
        return;
    }
    IEnumerator ExecuteTeleportFreddy(Vector3 position)
    {
        yield return new WaitForSeconds(1.0f);
        TeleportExecuteClientRpc(position);
    }
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
    private void IdleFreddy()
    {
        DoAnimationClientRpc("Running",false);
        DoAnimationClientRpc("Walking",false);
        DoAnimationClientRpc("RunWithClaw",false);
        DoAnimationClientRpc("Sneaking",false);
        
    }
    public void EnemyMeshAndPerson(bool enable)
    {
        EnableEnemyMesh(enable);
        if (enable == true)
        {

            creatureVoice.volume += 0.1f;

                
            if (!freddyRain.isPlaying)
            {
                freddyRain.Play();
            }
                
                
                
        }
        else
        {
            freddyRain.Stop();
            creatureVoice.volume -= 0.2f;
                
        }
    }
    
    public override void EnableEnemyMesh(bool enable, bool overrideDoNotSet = false)
    {
        if (skinnedMeshRenderers == null || meshRenderers == null)
        {
            return;
        }

        int num = enable ? 19 : 23;
        foreach (var renderer in skinnedMeshRenderers)
        {
            if (renderer != null && (!renderer.CompareTag("DoNotSet") || overrideDoNotSet))
            {
                renderer.gameObject.layer = num;
                renderer.enabled = enable;

            }
        }
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null && (!renderer.CompareTag("DoNotSet") || overrideDoNotSet))
            {
                renderer.gameObject.layer = num;
                renderer.enabled = enable;
            }
        }
    }
    private IEnumerator TransitionMaterial(bool manifest, float time)
    {
        float elapsedTime = 0;

        float startValue = shirt.GetFloat("Vector1_FEFF47F1");

        float targetValue = manifest ? 0f : 1f;
        if (manifest)
        {
            yield return new WaitForSeconds(1);
        }
        while (elapsedTime < time)
        {
            ModifyMaterial(Mathf.Lerp(startValue, targetValue, elapsedTime / time)); 
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ModifyMaterial(targetValue);
    }
    public void ModifyMaterial(float x)
    {
        if (x < 1.01f && x >= 0.0f)
        {
            shirt.SetFloat("Vector1_FEFF47F1",x);;
            pants.SetFloat("Vector1_FEFF47F1",x);;
            hands.SetFloat("Vector1_FEFF47F1",x);;
            claw.SetFloat("Vector1_FEFF47F1",x);;
            face.SetFloat("Vector1_FEFF47F1",x);; 
        }
        
    }
    public void SetLocalIndex()
    {
        // We just set it without doing error detection
        bool targetPlayerAssigned = false;
        int count = 0;
        foreach (var player in _playerSleep)
        {
            if (player.ClientID == _clientId)
            {
                _indexSleepArray = count;
                targetPlayerAssigned = true;
            }

            count++;
        }
        if (!targetPlayerAssigned)
        {
            _indexSleepArray = -1;
        }
    }
    
    //
}