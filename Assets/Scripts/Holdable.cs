﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Holdable : MonoBehaviour
{

    public string optionalAction = "";
    public AudioClip pickUpSound;
    public AudioClip setDownSound;
    public float pickUpTime = 0.5f;

    private float heldState = 0.0f; // 0 if not held, 1 if held
    private Transform originalParent;
    private Transform playerHoldTransform;
    Collider physicsCollider;
    Rigidbody myRigidbody;
    Vector3 oldPosition;
    private AudioSource objectAudio; 
    private Bounds myColliderBounds;

    public bool IsHeld {
        get => this.transform.parent == playerHoldTransform;
        private set {
            if (value) {
                this.transform.SetParent(playerHoldTransform);
            } else {
                this.transform.SetParent(originalParent);
            }
        }
    }

    void Start(){
        originalParent = this.transform.parent.transform;
        playerHoldTransform = GameObject.FindWithTag("Player").transform.Find("HoldLocation");
        physicsCollider = GetComponent<Collider>();
        myColliderBounds = physicsCollider.bounds;
        myRigidbody = GetComponent<Rigidbody>();
        objectAudio = GetComponent<AudioSource>();
    }


    // Update is called once per frame
    void Update(){
        if (IsHeld && heldState < 1f) {
            heldState += Time.deltaTime / pickUpTime;
            heldState = Mathf.Min(heldState, 1f);

            Vector3 newPosition = playerHoldTransform.position;
            this.transform.position =
                Vector3.Lerp(oldPosition, newPosition, QuadInterpolate(heldState));
            gameObject.SendMessage("UpdateHeldState", heldState);
        } else if (!IsHeld && heldState > 0f) {
            heldState -= Time.deltaTime / pickUpTime;
            heldState = Mathf.Max(heldState, 0f);
            gameObject.SendMessage("UpdateHeldState", heldState);
        }
    }

    public void SetDown(){
        IsHeld = false;
        objectAudio.PlayOneShot(setDownSound, 0.5f);
        physicsCollider.enabled = true;
        myRigidbody.isKinematic = false;
        playerHoldTransform.parent.GetComponent<HoldObject>().OnDropObject(gameObject);
    }

    public void PickUp(){
        IsHeld = true;
        objectAudio.PlayOneShot(pickUpSound, 0.5f);
        physicsCollider.enabled = false;
        myRigidbody.isKinematic = true;
        oldPosition = this.transform.position;
        this.transform.rotation = playerHoldTransform.rotation;
        playerHoldTransform.parent.GetComponent<HoldObject>().OnHoldObject(gameObject);
    }

    public float GetColliderWidth(){
        // this is broken out here becuase bounds can only be queried when collider is active
        return myColliderBounds.size.z;
    }

    private float QuadInterpolate(float x) {
        return -x * (x - 2);
    }
}
