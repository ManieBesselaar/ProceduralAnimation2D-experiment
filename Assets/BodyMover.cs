using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyMover : MonoBehaviour
{
   // Rigidbody2D _rb;
    [SerializeField] LayerMask groundLayer;
    bool isNoGravity = false;
    [SerializeField] float speed = 3;
    [SerializeField] float groundRaycastDistance = 2.5f;
    [SerializeField] float lookAheadRayCastDistance = 2.5f;
    [SerializeField] float groundClearance = 0.5f;
    [SerializeField] float rotationRate = 2f;
    RaycastHit2D HitDown, hitSide;
    int moveDirection = 1;
    float horizontalInput;
   /*
    TODO: Stop player from walking through walls (probably use lookAhead ray or Collider casting)
   Implement a jump function.
   
    
    
    */
    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        SetRotation(Mathf.Sign( horizontalInput) ); //Get the direction of input sign.

        if (horizontalInput != 0)
        {

            
          

            transform.position += transform.right * Time.deltaTime * Input.GetAxis("Horizontal") * speed;
            //    transform.Translate(transform.right * Time.deltaTime * Input.GetAxis("Horizontal"));
            //  Debug.DrawRay(transform.position, transform.right * 10, Color.red, 1f);
           // Debug.Log("CheckHeight " + Vector3.Distance(HitDown.point, transform.position) + " with hitpoint " + HitDown.point);
            if (!Mathf.Approximately(groundClearance, Vector3.Distance(HitDown.point, transform.position)))
            {

                Vector3 adjustedPosition = HitDown.point + (HitDown.normal * groundClearance);
              //  Debug.Log("Adjusting height  " + Vector3.Distance(HitDown.point, transform.position));
            //    Debug.Log("Moving Towards " + adjustedPosition + " with normal " + HitDown.normal + " and ground clearance of " + groundClearance);
                transform.position = Vector3.MoveTowards(transform.position, adjustedPosition, Time.deltaTime);
            }
            //  Debug.DrawRay(HitDown.point, HitDown.normal * 3, Color.green, 1f);
            // _rb.AddForce(transform.right * Input.GetAxis("Horizontal") * 30);

            //  Debug.Log("transform.right = " + transform.right + " cross of the hit normal = " + Vector3.Cross(hit.normal, Vector3.forward * 10));

        }


        if (HitDown.point == Vector2.zero)
        {
            transform.position += Vector3.down * 9.8f * Time.deltaTime; //Fall if no hit 
        }
    }

    private void SetRotation(float direction)
    {
        // transform.Translate(transform.right );
        HitDown = Physics2D.Raycast(transform.position, -transform.up * groundClearance, groundRaycastDistance, groundLayer);
        Debug.DrawRay(transform.position, -transform.up * groundRaycastDistance, Color.green, 1f);
        hitSide = Physics2D.Raycast(transform.position, transform.right * direction, lookAheadRayCastDistance, groundLayer);
        Debug.DrawRay(transform.position, transform.right * lookAheadRayCastDistance * direction, Color.green, 1f);
      //  hitRear = Physics2D.Raycast(transform.position, -transform.right, lookAheadRayCastDistance, groundLayer);
      //  Debug.DrawRay(transform.position, -transform.right * lookAheadRayCastDistance, Color.green, 1f);

        Debug.DrawRay(transform.position, -transform.up, Color.white);
      //  Debug.Log("Hit  point down " + HitDown.point + " normal " + HitDown.normal);
      //  Debug.Log("Hit  point front " + hitSide.point + " normal " + hitSide.normal);
      

        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(Vector3.forward, HitDown.normal  + hitSide.normal),
            rotationRate * Time.deltaTime);  //Rotate towards an orientation where the transform.up will match the average normal of the hits
    }
}
