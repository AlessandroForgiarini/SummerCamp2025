using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallHandler : MonoBehaviour
{
    [SerializeField] private BallVisual ballVisual;
    [SerializeField] private BallAudioEffects ballAudioEffects;
    [SerializeField, Range(0,5)] private float explosionRadius = 5f;
    
    private Rigidbody _rigidbody;
    private Vector3 _oldPosition;
    private float _currentBallVelocityMagnitude;
    
    public bool PickedUp { get; private set; }
    public bool HitHandled { get; private set; }
    public ElementsListSO.ElementType ActiveElementType { get; private set; }

    private bool _isDebugThrow;
    private Transform _targetDebugTransform;
    private float _speedDebug;
    
    public void GoToTarget(Transform target, float speed)
    {
        _isDebugThrow = true;
        _rigidbody.isKinematic = true;
        _targetDebugTransform = target;
        _speedDebug = speed;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        if (transform.position.y < -1)
        {
            DestroyBall();
            return;
        }
        
        // Calculating Velocity
        Vector3 currentPosition = transform.position;
        _currentBallVelocityMagnitude = Vector3.Distance(_oldPosition, currentPosition) / Time.fixedDeltaTime;
        _oldPosition = currentPosition;
    }

    protected void Update()
    {
        if (_isDebugThrow)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetDebugTransform.position,
                _speedDebug * Time.deltaTime);
        }
    }

    public void Init(Vector3 startingPosition, ElementsListSO.ElementType element)
    {
        _isDebugThrow = false;
        // Reset ball properties
        HitHandled = false;
        PickedUp = false;
        _rigidbody.isKinematic = false;

        // Placing in the correct Position
        ActiveElementType = element;
        transform.position = startingPosition;
        _oldPosition = startingPosition;

        ballVisual.UpdateVisual(ActiveElementType);
    }

    public void PickUp()
    {
        ballVisual.EnableVisuals();
        _rigidbody.isKinematic = false;
        PickedUp = true;
    }

    public void Throw()
    {
        if (!PickedUp) return;

        ballAudioEffects.Throw(_currentBallVelocityMagnitude);
        PickedUp = false;
    }

    public void BallHitSomething()
    {
        if (HitHandled) return;
        _rigidbody.isKinematic = true;
        float explosionDuration = 2.5f;
        ballVisual.ExplodeVisual(explosionRadius, explosionDuration);
        ballAudioEffects.PlayExplodeEffect();
        
        Vector3 currentPosition = transform.position;
        List<GoblinController> goblins = GetGoblinInRange(currentPosition, explosionRadius);
        foreach (var g in goblins)
        {
            g.HandleBallHit(currentPosition, ActiveElementType);
        }
        
        HitHandled = true;
        Invoke(nameof(DestroyBall), explosionDuration);
    }

    public void DestroyBall()
    {
        Destroy(gameObject);
    }

    public void DisableVisuals()
    {
        ballVisual.DisableVisuals();
    }
    
    private static List<GoblinController> GetGoblinInRange(Vector3 position, float radius)
    {
        Collider[] inRange = Physics.OverlapSphere(position, radius);
        List<GoblinController> goblinControllers = new();
        foreach (var coll in inRange)
        {
            GoblinController controller = coll.gameObject.GetComponentInParent<GoblinController>();
            
            if(controller is null) continue;
            if(!controller.gameObject.CompareTag("Goblin")) continue;

            goblinControllers.Add(controller);
        }

        return goblinControllers;
    }

    private void OnCollisionEnter(Collision other)
    {
        BallHitSomething();
    }

    #region XR Callbacks
    public void OnActivate(ActivateEventArgs activateEnterEventArgs)
    {
        PickUp();
    }

    public void OnDeactivate(DeactivateEventArgs deactivateEventArgs)
    {
        Throw();
    }
    
    public void OnSelectEnter(SelectEnterEventArgs selectEnterEventArgs)
    {
        PickUp();
    }

    public void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        Throw();
    }
    #endregion
}