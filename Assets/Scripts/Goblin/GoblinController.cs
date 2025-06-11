using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GoblinController : MonoBehaviour
{
    #region Labels For Animations
    private static readonly int MovingLabel = Animator.StringToHash("Moving");
    private static readonly int LocomotionXLabel = Animator.StringToHash("LocomotionX");
    private static readonly int LocomotionYLabel = Animator.StringToHash("LocomotionY");
    private static readonly int EndStealLabel = Animator.StringToHash("EndSteal");
    private static readonly int StealLabel = Animator.StringToHash("Steal");

    private static readonly int HitXLabel = Animator.StringToHash("HitX");
    private static readonly int HitYLabel = Animator.StringToHash("HitY");
    private static readonly int HitLabel = Animator.StringToHash("Hit");

    #endregion

    private const float arrivedDistanceThreshold = 0.5f;

    public enum State
    {
        IDLE,
        GO_TO_TARGET,
        GATHER,
        PREPARE_TO_RUN_AWAY,
        RUN_AWAY,
        BANISHED
    }

    public enum HitDirection
    {
        FRONT,
        LEFT,
        RIGHT,
        BACK,
        NONE
    }

    [SerializeField] private GoblinDataSO goblinData;
    [SerializeField] private AudioClip runAwayClip;
    [SerializeField] private Material goblinSkinMaterial;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ElementsListSO.ElementType activeElementType;
    [SerializeField] private ElementsListSO elementSos;

    [SerializeField] private Transform backPackTransform;

    [SerializeField] float dampTime = 0.2f;

    [Header("Renderers")]
    [SerializeField] private Renderer skinRenderer;

    private GoblinUI _goblinUI;
    private State _state;
    private float _timer;
    private Transform _spawnPointTransform;
    private int _currentHealth;
    private float _gatherTime;

    private CrystalHandler _linkedCrystalHandler;
    private CrystalHandler _targetCrystalHandler;

    private NavMeshAgent _agent;
    private Animator _anim;

    private float _walkingSpeed;
    private float _runningSpeed;

    private static Vector3 noYMask => new Vector3(1, 0, 1);

    private void Awake()
    {
        _goblinUI = GetComponentInChildren<GoblinUI>();
        _agent = GetComponentInChildren<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        HandleGoblinState();
    }
    
    public void Init(Transform spawnPoint, ElementsListSO.ElementType element)
    {
        // Setting goblin properties
        _currentHealth = goblinData.MaxHealth;
        _gatherTime = goblinData.GatherTime;
        _walkingSpeed = goblinData.WalkingSpeed;
        _runningSpeed = goblinData.RunningSpeed;
        _spawnPointTransform = spawnPoint;
        activeElementType = element;

        _linkedCrystalHandler = null;
        _targetCrystalHandler = null;
        UpdateVisual();
        UpdateHeartsUI();

        // Decide first Target
        if (SelectNearestCrystal(out CrystalHandler destSelected, out Vector3 navAgentActualDest))
        {
            _targetCrystalHandler = destSelected;
            PrepareForLocomotion(navAgentActualDest, 1f);
            ChangeState(State.GO_TO_TARGET);
        }
        else
        {
            Banish();
        }
    }

    /// <summary>
    /// Sets Moving trigger, destination and speed for NavMeshAgent.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="animXSpeedFactor"></param>
    private void PrepareForLocomotion(Vector3 destination, float animXSpeedFactor)
    {
        _agent.SetDestination(destination);
        _anim.SetBool(MovingLabel, true);
        _anim.SetFloat(LocomotionYLabel, animXSpeedFactor, dampTime, Time.deltaTime);
        _agent.speed = animXSpeedFactor > 0.5 ? _runningSpeed : _walkingSpeed;
    }

    private bool SelectNearestCrystal(out CrystalHandler destSelected, out Vector3 agentActualDestination)
    {
        GameObject[] crystals = GameObject.FindGameObjectsWithTag("Crystal");
        float minDistance = float.MaxValue;
        destSelected = null;
        agentActualDestination = Vector3.zero;
        Vector3 myPosition = transform.position;

        foreach (GameObject crystal in crystals)
        {
            CrystalHandler crystalHandler = crystal.GetComponent<CrystalHandler>();
            if (!crystalHandler.IsFree) continue;

            Vector3 crystalPosition = crystal.transform.position;
            float distance = Vector3.Distance(myPosition, crystalPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                agentActualDestination = GetApproxApproachPosition(crystalHandler, myPosition);
                destSelected = crystalHandler;
            }
        }
        
        return destSelected != null;
    }

    private static Vector3 GetApproxApproachPosition(CrystalHandler crystalHandler, Vector3 myPosition)
    {
        Vector3 approxApproachingDir = myPosition - crystalHandler.Bounds.center;
        Vector3 crystalExtents = crystalHandler.Bounds.extents;
        float approxCrystalRadius = Mathf.Max(crystalExtents.x, crystalExtents.z);
        Vector3 agentActualDestination = approxApproachingDir.normalized * approxCrystalRadius + crystalHandler.Bounds.center;
        return agentActualDestination;
    }

    public void ChangeState(State newState)
    {
        if (newState == _state)
        {
            //Debug.LogWarning($"Same state provided: {newState}");
            return;
        }
        _state = newState;

        // reset gather progress bar
        UpdateGatherProgressUI(0f);
        switch (_state)
        {
            case State.IDLE:
                break;
            case State.GO_TO_TARGET:
                break;
            case State.GATHER:
                _timer = 0;
                break;
            case State.RUN_AWAY:
                break;
            case State.BANISHED:
                break;
            case State.PREPARE_TO_RUN_AWAY:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleGoblinState()
    {
        switch (_state)
        {
            case State.IDLE:
                break;
            case State.GO_TO_TARGET:
                if (!_targetCrystalHandler.IsFree)
                {
                    FindNextTarget(State.GO_TO_TARGET, State.RUN_AWAY);
                    break;
                }

                if (_agent.pathPending) { return; }

                if (_agent.remainingDistance > arrivedDistanceThreshold) //reaching target destination
                {
                    HandleLocomotionAnimations(1f);
                }
                else //arrived at destination
                {
                    PrepareForStealing();
                    ChangeState(State.GATHER);
                }
                break;
            case State.GATHER:
                if (!_targetCrystalHandler || !_targetCrystalHandler.IsFree)
                {
                    if (_anim.GetCurrentAnimatorStateInfo(0).IsName("GatheringProcess"))
                    {
                        _anim.SetTrigger(EndStealLabel);
                    }

                    FindNextTarget(State.GO_TO_TARGET, State.RUN_AWAY);
                    break;
                }

                if (_timer > _gatherTime)
                {
                    _linkedCrystalHandler = _targetCrystalHandler;
                    _linkedCrystalHandler.SetBackPackParent(backPackTransform);
                    _targetCrystalHandler = null;
                    
                    _anim.SetTrigger(EndStealLabel);
                    ChangeState(State.PREPARE_TO_RUN_AWAY);

                }
                else
                {
                    _timer += Time.deltaTime;
                    UpdateGatherProgressUI(_timer / _gatherTime);
                }
                break;
            case State.PREPARE_TO_RUN_AWAY:
                if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    PrepareForLocomotion(_spawnPointTransform.transform.position, 1f);
                    ChangeState(State.RUN_AWAY);
                }
                break;
            case State.RUN_AWAY:
                if (_agent.pathPending) { return; }

                if (_agent.remainingDistance > arrivedDistanceThreshold)
                {
                    HandleLocomotionAnimations(1f);
                }
                else
                {
                    StopAgent();

                    if (_linkedCrystalHandler != null)
                    {
                        Destroy(_linkedCrystalHandler.gameObject);
                        _linkedCrystalHandler = null;
                        StartCoroutine(nameof(RemovedCrystal));
                    }

                    Banish();
                }
                break;
            case State.BANISHED:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void FindNextTarget(State stateToReachFound, State stateToReachNotFound)
    {
        Vector3 targetPos;
        // if target crystal is not free, try to find another one
        if (SelectNearestCrystal(out CrystalHandler destSelected, out Vector3 navAgentActualDest))
        {
            _targetCrystalHandler = destSelected;
            targetPos = navAgentActualDest;
            ChangeState(stateToReachFound);
        }
        else // no crystal is free, go away
        {
            targetPos = _spawnPointTransform.transform.position;
            ChangeState(stateToReachNotFound);
        }
        PrepareForLocomotion(targetPos, 1f);
    }

    private void StopAgent()
    {
        _agent.ResetPath();
        _anim.SetBool(MovingLabel, false);
    }

    private void PrepareForStealing()
    {
        _timer = 0f;
        StopAgent();
        _anim.SetTrigger(StealLabel);
    }

    private void HandleLocomotionAnimations(float animatorMaxFwdFactor)
    {
        Vector3 nextCorner = _agent.steeringTarget;
        Vector3 agentFwd = transform.forward;
        Vector3 directionToNextCorner = nextCorner - transform.position;
        directionToNextCorner.Scale(noYMask);
        directionToNextCorner.Normalize();

        float degsToTurn = Vector3.SignedAngle(agentFwd, directionToNextCorner, Vector3.up);
        float animatorX = Mathf.Sin(degsToTurn * Mathf.Deg2Rad);

        _anim.SetFloat(LocomotionXLabel, animatorX, dampTime, Time.deltaTime);

        if (Mathf.Abs(degsToTurn) > 45f)
        {
            _anim.SetFloat(LocomotionYLabel, 0.1f, dampTime, Time.deltaTime);
        }
        else
        {
            _anim.SetFloat(LocomotionYLabel, animatorMaxFwdFactor, dampTime, Time.deltaTime);
        }
    }

    private IEnumerator RemovedCrystal()
    {
        yield return null;
        GameManager.Instance.RemovedCrystal();
    }

    private void UpdateHeartsUI()
    {
        _goblinUI.UpdateHeartsUI(_currentHealth);
    }

    private void UpdateGatherProgressUI(float value)
    {
        _goblinUI.UpdateGatherProgressBar(value);
    }

    public void ApplyDamage(int damage)
    {
        // Already going to be removed from game
        if (_currentHealth <= 0) return;

        Invoke(nameof(PlayRunEffect), 0f);
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Banish();
        }
        UpdateHeartsUI();
    }

    private void Banish()
    {
        //Debug.Log($"{transform.GetInstanceID()} is being banished.");

        GameManager.Instance.GoblinBanished();
        
        if (_linkedCrystalHandler != null)
        {
            _linkedCrystalHandler.ResetParent();
            _linkedCrystalHandler = null;
        }

        ChangeState(State.BANISHED);
        StopAgent();
        // remove goblin from scene after some time
        Invoke(nameof(RemoveGoblin), 6f);
    }

    private void PlayRunEffect()
    {
        FantasyAudioManager.Instance.PlayEffect(audioSource, runAwayClip);
    }

    public void RemoveGoblin()
    {
        Destroy(gameObject);
    }

    private HitDirection CalculateHitDirection(Vector3 hitPoint)
    {
        hitPoint.y = 0;
        Vector3 postionNoY = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 agentFwd = transform.forward;
        Vector3 directionToNextCorner = hitPoint - postionNoY;
        directionToNextCorner.Scale(noYMask);
        directionToNextCorner.Normalize();
        float degsToTurn = Vector3.SignedAngle(agentFwd, directionToNextCorner, Vector3.up);
        if (degsToTurn is < -135 or > 135) return HitDirection.BACK;
        if (degsToTurn < -45) return HitDirection.LEFT;
        if (degsToTurn > 45) return HitDirection.RIGHT;
        return HitDirection.FRONT;
    }

    public void HandleBallHit(Vector3 point, ElementsListSO.ElementType sourceElement)
    {
        if (_state == State.BANISHED) return;
        if (sourceElement != activeElementType) return;
        ApplyDamage(1);
        var direction = CalculateHitDirection(point);
        switch (direction)
        {
            case HitDirection.LEFT:
            case HitDirection.FRONT:
                if (_currentHealth <= 0)
                {
                    _anim.SetFloat(HitXLabel, 0);
                    _anim.SetFloat(HitYLabel, 1);
                    _anim.SetTrigger(HitLabel);
                }

                break;

            case HitDirection.RIGHT:
            case HitDirection.BACK:
                if (_currentHealth <= 0)
                {
                    _anim.SetFloat(HitXLabel, 0);
                    _anim.SetFloat(HitYLabel, -1);
                    _anim.SetTrigger(HitLabel);
                }
                break;
            case HitDirection.NONE:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateVisual()
    {
        Color goblinColor = elementSos.GetColorFromElement(activeElementType);
        Material newMaterial = new Material(goblinSkinMaterial);
        float startAlpha = goblinSkinMaterial.color.a;
        newMaterial.color = new Color(goblinColor.r, goblinColor.g, goblinColor.b, startAlpha);
        skinRenderer.material = newMaterial;
    }
    
    private void OnValidate()
    {
        UpdateVisual();
    }
}
