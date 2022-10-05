using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootTarget : MonoBehaviour
{
    [SerializeField] FootTarget opposingFoot;
    [SerializeField] Transform defaultPosition;
    Vector2 nextPosition;
    Vector2 halfWayPoint;

    float gaitLength;
    float footMovementSpeed;
    float footLiftHeight;
    LayerMask groundLayer;
    float groundCastDepth;

    enum FOOTSTATE
    {
        PLANTED,
        LIFTING,
        MOVING_TO_PLANT
    }
    FOOTSTATE[] _currentFootStates;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
