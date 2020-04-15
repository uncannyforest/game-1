﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PickUpObject : MonoBehaviour
{
    private Transform playerHoldTransform;
    private HashSet<GameObject> nearObjects = new HashSet<GameObject>();
    Animator m_Animator;
    private Vector3 leftHandlePosition, rightHandlePosition;

    // Start is called before the first frame update
    void Awake()
    {
        playerHoldTransform = gameObject.transform.Find("HoldLocation");
        m_Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SimpleInput.GetButtonDown("Interact1")) {
            Interact();
        }
    }

    void OnTriggerEnter(Collider other) {
        Debug.Log("Hiiiii");
        if(other.tag == "CanPickUp") {
            GameObject objectToPickUp;
            if(other.gameObject.GetComponent<PickMeUp>() != null) {
                objectToPickUp = other.gameObject;
            } else if (other.transform.parent.GetComponent<PickMeUp>() != null) {
                objectToPickUp = other.transform.parent.gameObject;
            } else {
                Debug.LogError("Object tagged CanPickUp has no PickMeUp script on it or parent");
                return;
            }
            nearObjects.Add(objectToPickUp);
            Debug.Log("Near = true");
        }
    }

	void OnTriggerExit(Collider other) {
        if(other.tag == "CanPickUp") {
            GameObject objectToPickUp;
            if(other.gameObject.GetComponent<PickMeUp>() != null) {
                objectToPickUp = other.gameObject;
            } else if (other.transform.parent.GetComponent<PickMeUp>() != null) {
                objectToPickUp = other.transform.parent.gameObject;
            } else {
                Debug.LogError("Object tagged CanPickUp has no PickMeUp script on it or parent");
                return;
            }
			bool foundObject = nearObjects.Remove(objectToPickUp);
            if (!foundObject) {
                Debug.LogWarning("Tried to remove object from nearObjects that was not there");
            }
            Debug.Log("Too far away from this object!");
		}
    }
    
    void Interact() {
        if (playerHoldTransform.childCount > 0) {
            DropAnyPickedUpObjects();
            return;
        }

        if (nearObjects.Count == 0) {
            return;
        }

        GameObject closestObject = nearObjects.OrderBy(
                o => Vector3.Distance(o.transform.position, gameObject.transform.position)
            ).First();

        closestObject.GetComponent<PickMeUp>().StartPickUp();
    }

    void OnAnimatorIK()
    {
        if(playerHoldTransform.childCount > 0) { // if you're holding something 

            Transform heldObject = playerHoldTransform.GetChild(0);
            float objectWidth = heldObject.GetComponent<PickMeUp>().GetColliderWidth();
            rightHandlePosition = playerHoldTransform.position + (.5f * objectWidth * this.transform.right);
            leftHandlePosition = playerHoldTransform.position - (.5f * objectWidth * this.transform.right);

            // Set the hands' target positions and rotations
            m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
            m_Animator.SetIKRotationWeight(AvatarIKGoal.RightHand,1);
            m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,1);
            m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,1);  
            m_Animator.SetIKPosition(AvatarIKGoal.RightHand,rightHandlePosition);
            m_Animator.SetIKRotation(AvatarIKGoal.RightHand,playerHoldTransform.rotation);
            m_Animator.SetIKPosition(AvatarIKGoal.LeftHand,leftHandlePosition);
            m_Animator.SetIKRotation(AvatarIKGoal.LeftHand,playerHoldTransform.rotation);  
        }else{
            // Let the hands relax :)
            m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand,0);
            m_Animator.SetIKRotationWeight(AvatarIKGoal.RightHand,0); 	
            m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,0);
            m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,0); 
        }
    }
    
    void DropAnyPickedUpObjects() {
        foreach (Transform child in playerHoldTransform) {
            PickMeUp childPickMeUp = child.GetComponent<PickMeUp>();
            if (childPickMeUp == null) {
                Debug.LogWarning("Child of playerHold had no PickMeUp script!");
            } else {
                childPickMeUp.SetDown();
            }
        }
    }

}
