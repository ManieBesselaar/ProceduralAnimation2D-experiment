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
    Vector2[] footTargetNextHalfwayPoint;
    [SerializeField] float gaitLength = 2;
   [SerializeField]float footSpeed = 2;

    [SerializeField] float footLiftHeight = .75f;
    [SerializeField] LayerMask groundLayer;
    bool footGotHit = false;
    enum FOOTSTATE
    {
        PLANTED,
        LIFTING,
        MOVING_TO_PLANT
    }
    /*
     * TODO: Lift feet to halfway point before moving to final.
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

        for(int i = 0; i < FootTargets.Length;i++) 
        {
            //create an empty game object as the child of this at the foot target pos and ad its transform to the defaultFootPositions
            defaultFootPositions[i] = Instantiate(new GameObject(), FootTargets[i].transform.position,
                Quaternion.identity, gameObject.transform).transform;
            footTargetNextPositions[i] = defaultFootPositions[i].position;
            FootTargets[i].position = footTargetNextPositions[i];
        }
    }

    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < FootTargets.Length; i++)
        {
           // Debug.Log("foot distance " + Vector2.Distance(footTargetNextPositions[i],transform.position) +" nextPos " +
             //   footTargetNextPositions[i] + " default pos " + defaultFootPositions[i].position);
           if (Vector2.Distance(footTargetNextPositions[i], defaultFootPositions[i].position) > gaitLength)
            {
                footTargetNextPositions[i] = defaultFootPositions[i].position;
              footGotHit=  CheckGround(footTargetNextPositions[i], out footTargetNextPositions[i]);
                if (!footGotHit) {
                    
                    footTargetNextPositions[i] = defaultFootPositions[i].position; //If no ground hit move foot to default pos
                }
                Debug.Log("Nextpos back in update " + footTargetNextPositions[i] + " footHit " + footGotHit);
            }
            FootTargets[i].position =Vector2.MoveTowards(FootTargets[i].position, footTargetNextPositions[i],footSpeed * Time.deltaTime);
        }

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

        }
#endif
    }
   bool CheckGround(Vector2 nextPos, out Vector2 result)
    {
        RaycastHit2D hit = Physics2D.Raycast(nextPos, -transform.up,2,groundLayer );
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
}
