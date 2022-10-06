using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class can be attached to an IK target to move the ik target to a new location determined by 
/// a locally fixed reference transform (defaultPosition) and a raycast to find the "ground " at the new planned location
/// The defaultPosition is locally fixed to the parent (moves with the parent transform) , while the FootTarget is not childed to the parent transform.
/// When the default position gets too far from the current position of the foot, thena new target location is determined as close as possible to the 
/// current position of the defaultPosition ( default position object transform position)
/// This was written with feet and legs in mind , but should work for almost any appendage. Feel free to modify and experiment.
/// </summary>
public class FootTarget : MonoBehaviour
{
    [SerializeField][Tooltip("The opposing foot needs to be grounded for this one to lift")] FootTarget opposingFoot;
    [SerializeField][Tooltip("The transform for the default position the foot should move to if no groundHit is detected")] Transform defaultPosition;
    Vector2 _nextPosition; //Next position for the foot to be planted
    Vector2 _halfWayPoint;// This is the position the foot should reach at the highest part of the step before descending for the planting phase

    float _gaitLength; //How big the steps should be.
    float _footMovementSpeed; // How fast the foot should move.
    float _footLiftHeight;//How hight the foot should lift with each step.
    LayerMask _groundLayer; // Layermask used to filter out all raycast hits except ground.
    float _groundCastDepth; // How far down the foot should cast a ray to try and find the ground.
    Transform _bodyTransform;
  public enum FOOTSTATE //This is used to track which phase of the step we are in.
    {
        PLANTED, //The foot is planted and awaiting it's next target to move to.
        LIFTING,// The foot was lifterd and is moving to the halfway point of the step
        MOVING_TO_PLANT //Foot has passed the halfway point and is heading to the nextPosition (the point where it will be planted.
    }
    public FOOTSTATE _currentFootStates { get; private set; }
bool isReady = false; // Will be set after init to prevent the foot from trying to update without being initialized.
    private float _distanceTolerance; //how close to the intended destination the FootTarget should be to be considered on target.

    public void Init(
    float gaitLength,
    float footMovementSpeed,
    float footLiftHeigh,
    LayerMask groundLayer,
    float groundCastDepth,
        float distanceTolerance,
        Transform bodyTransform)
    {
        //This is set up so that all the foot stats can be set up from the parent that the legs are attached to to save time when adding many legs
        // so that each leg does not need to have so many parameters to set up in the inspector.
        _gaitLength = gaitLength;
            
        _footMovementSpeed = footMovementSpeed;
        _footLiftHeight = footLiftHeigh;
        _groundLayer = groundLayer;
        _groundCastDepth = groundCastDepth;

        _nextPosition = defaultPosition.position;
        _halfWayPoint = defaultPosition.position;

        isReady = true;
        _distanceTolerance = distanceTolerance;
    _bodyTransform = bodyTransform;
    }


    // Update is called once per frame
    void Update()
    {
        if (!isReady) return; //Don't do stuff if you are not ready
        UpdateFootTargetPosition();
    }

    Vector2 CheckGround(Vector2 castPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(defaultPosition.position, -transform.up, _groundCastDepth, _groundLayer);

        return hit.point;
    }
    void UpdateFootTargetPosition()
    {
        if (Vector2.Distance(defaultPosition.position, _nextPosition) > _gaitLength
               && _currentFootStates == FOOTSTATE.PLANTED && opposingFoot._currentFootStates == FOOTSTATE.PLANTED )
        {

      //      Debug.Log("In first footstate " +_currentFootStates + " transform.position  " + transform.position +" nextpos " 
     //           + _nextPosition + " distance to target  " + Vector2.Distance(defaultPosition.position, _nextPosition));
            _nextPosition =CheckGround( defaultPosition.position);
            if (Vector2.Distance(defaultPosition.position, _nextPosition) > 1f) // Sanity check in case a far off hit comes through or a vector of 0
            {
                _nextPosition = defaultPosition.position;
            }
            Vector2 tmpTarget = ((Vector3)_nextPosition + transform.position) / 2;
            Vector2 liftVector = transform.up * _footLiftHeight;
            _halfWayPoint = tmpTarget + liftVector;
            _currentFootStates = FOOTSTATE.LIFTING;
           
          
        }

        if (_currentFootStates == FOOTSTATE.LIFTING)
        {
            transform.position = Vector2.MoveTowards(transform.position, _halfWayPoint, _footMovementSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, _halfWayPoint) < _distanceTolerance) // use distance with a tolerance becaus the positions may not match up exactly
            {

                _currentFootStates = FOOTSTATE.MOVING_TO_PLANT;
            }

        //    Debug.Log("In second footstate " + _currentFootStates + " transform.position  " + transform.position + " nextpos "
       //         + _nextPosition + " distance to target  " + Vector2.Distance(defaultPosition.position, _nextPosition));
        }
        else if (_currentFootStates== FOOTSTATE.MOVING_TO_PLANT)
        {

        //    Debug.Log("In third footstate " + _currentFootStates + " transform.position  " + transform.position + " nextpos "
       //         + _nextPosition + " distance to target  " + Vector2.Distance(defaultPosition.position, _nextPosition));

            transform.position = Vector2.MoveTowards(transform.position, _nextPosition, _footMovementSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, _nextPosition) < _distanceTolerance)
            {
                _currentFootStates = FOOTSTATE.PLANTED;
                
            }
        }
        transform.rotation = _bodyTransform.rotation;
        Debug.DrawRay(transform.position, -transform.up, Color.magenta, 1f);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_nextPosition, .1f);
        Gizmos.color = Color.green;
        Gizmos.DrawCube(defaultPosition.position, Vector3.one * .1f);
        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(_halfWayPoint, .05f);
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Foot " + gameObject.name + " hit by " + collision.gameObject.name);
        //  _currentFootStates = FOOTSTATE.LIFTING;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Foot " + gameObject.name + " triggered by " + collision.gameObject.name);
    }
}
 