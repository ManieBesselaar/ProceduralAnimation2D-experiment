using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FootPlacementSolver : MonoBehaviour
{

  [SerializeField]  Transform[] FootTargets;
   Transform[] defaultFootPositions;
    List<Vector2> hits = new List<Vector2>(); //RaycastHits from scan
    Vector2[] footTargetNextPositions;
    Vector2[] footTargetHalfwayPoint;
    [SerializeField] float gaitLength = 2;
   [SerializeField]float footSpeed = 2;

    [SerializeField] float footLiftHeight = .75f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float footCastDepth = 3f;
    Vector2[] footLiftTargets;
    bool footGotHit = false;
    [SerializeField][Tooltip("How close a foot target should be to its intended position to be considered on target")] float targetDistanceTolerance = .5f;
    enum FOOTSTATE
    {
        PLANTED,
        LIFTING,
        MOVING_TO_PLANT
    }
    FOOTSTATE[] _currentFootStates;
    int _numberOfPlantedLegs = 0;


    /*
     * TODO: 
     * 
     * Lift feet to halfway point before moving to final.
     * Check collision with walls to stop feet going through.
     * 
     * 
     If more than 2 legs maybe let closest leg reach for a wall if getting close 
     If around a corner maybe have foot do a kind of searching motion to find the floor.
     */

    // Start is called before the first frame update

   
    void Start()
    {
        defaultFootPositions = new Transform[FootTargets.Length];
        footTargetNextPositions = new Vector2[defaultFootPositions.Length];
        footLiftTargets = new Vector2[FootTargets.Length];
        _currentFootStates = new FOOTSTATE[FootTargets.Length];

    
        for (int i = 0; i < FootTargets.Length;i++) 
        {
            //create an empty game object as the child of this at the foot target pos and ad its transform to the defaultFootPositions
            defaultFootPositions[i] = Instantiate(new GameObject(), FootTargets[i].transform.position,
                Quaternion.identity, gameObject.transform).transform;
            footTargetNextPositions[i] = defaultFootPositions[i].position;
            FootTargets[i].position = footTargetNextPositions[i];
            _currentFootStates[i] = FOOTSTATE.MOVING_TO_PLANT;
            defaultFootPositions[i].gameObject.name = "DefaultFootPosition" + i;
        }
    }

    // Update is called once per frame
    void Update()
    {

        // if (Input.GetKeyUp(KeyCode.U)) UpdateFeet();
        UpdateFeet();
    }

    private void OnDrawGizmos()
    {
       
    }
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (footTargetNextPositions == null || FootTargets == null || !Application.isPlaying ) return;
        //Red for the deafault posiotions which will be used to determine when to move the foot
        //Blue for the foottargets  next target positions
        for (int i = 0; i < FootTargets.Length; i++)
        {
           
            
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(FootTargets[i].position, 0.05f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(footTargetNextPositions[i], .1f);
            Gizmos.color = Color.green;
            Gizmos.DrawCube(defaultFootPositions[i].position, Vector3.one *.1f);
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(footLiftTargets[i], .05f);

        }
#endif
    } 
   bool CheckGround(Vector2 nextPos, out Vector2 result)
    {
        RaycastHit2D hit = Physics2D.Raycast(nextPos, -transform.up,footCastDepth,groundLayer );
        Debug.DrawRay(nextPos, hit.point - nextPos, Color.red,Time.deltaTime);
        Debug.Log(" Nextpos before " + nextPos); 
        if( hit.point != Vector2.zero)
        {
            nextPos = hit.point;
            Debug.Log(" Nextpos after true " + nextPos);
            result = hit.point;
            return true;
        }
        result = nextPos;
        Debug.Log(" Nextpos after false " + nextPos);
        return false;
    }

    void UpdateFeet()
    {
        for (int i = 0; i < FootTargets.Length; i++)
        {

            if (Vector2.Distance(defaultFootPositions[i].position, footTargetNextPositions[i]) > gaitLength
                && _currentFootStates[i] == FOOTSTATE.PLANTED && _numberOfPlantedLegs > 1 )
            {
             
                footTargetNextPositions[i] = defaultFootPositions[i].position;
                footGotHit = CheckGround(footTargetNextPositions[i], out footTargetNextPositions[i]);
                if (!footGotHit)
                {

                    footTargetNextPositions[i] = defaultFootPositions[i].position; //If no ground hit move foot to default pos
                    Debug.LogError("Weeeeeee");
                  
                }
                Vector2 tmpTarget = ((Vector3)footTargetNextPositions[i] + FootTargets[i].position) / 2;
                Vector2 liftVector = transform.up * footLiftHeight;
                footLiftTargets[i] = tmpTarget + liftVector;
                // footLiftTargets[i] = tmpTarget;
                Debug.Log("Lift target calculated at " + footLiftTargets[i] + " using foot pos " + FootTargets[i].position +
                    " and nextPos   " + footTargetNextPositions[i] + " tmpTarget was set to  " + tmpTarget + " the liftvector was " + liftVector);
                _currentFootStates[i] = FOOTSTATE.LIFTING;
                _numberOfPlantedLegs--;
             
                // A new target has been set calculate the halfway and lift the foot to itss
                // Debug.Log("Nextpos back in update " + footTargetNextPositions[i] + " footHit " + footGotHit);
            }






            if (_currentFootStates[i] == FOOTSTATE.LIFTING)
            {
                FootTargets[i].position = Vector2.MoveTowards(FootTargets[i].position, footLiftTargets[i], footSpeed * Time.deltaTime);
                if (Vector2.Distance(FootTargets[i].position, footLiftTargets[i]) < targetDistanceTolerance) // use distance with a tolerance becaus the positions may not match up exactly
                {

                    _currentFootStates[i] = FOOTSTATE.MOVING_TO_PLANT;
                }
            }
            else if (_currentFootStates[i] == FOOTSTATE.MOVING_TO_PLANT)
            {
                FootTargets[i].position = Vector2.MoveTowards(FootTargets[i].position, footTargetNextPositions[i], footSpeed * Time.deltaTime);
                if (Vector2.Distance(FootTargets[i].position, footTargetNextPositions[i]) < targetDistanceTolerance)
                {
                    _currentFootStates[i] = FOOTSTATE.PLANTED;
                    _numberOfPlantedLegs++;
                }
            }

            
        }
    }
}
