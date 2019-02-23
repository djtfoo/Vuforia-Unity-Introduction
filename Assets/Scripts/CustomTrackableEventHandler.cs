/*==============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.

* A generic Vuforia tracking event handler implementing the ITrackableEventHandler interface.
* Also allows testing of codes without requiring actual tracking,
*  via pressing a button to simulate tracking found or lost.
* Modified By: Foo Jing Ting
==============================================================================*/

using UnityEngine;
using Vuforia;
using UnityEngine.Events;
using System;

public class CustomTrackableEventHandler : MonoBehaviour, ITrackableEventHandler
{
    // for Tracking Events
    public EventHandler<EventArgs> whenTrackingBegun;
    public EventHandler<EventArgs> whenTrackingFound;
    public EventHandler<EventArgs> whenTrackingLost;

    #region PRIVATE_MEMBER_VARIABLES

    protected TrackableBehaviour mTrackableBehaviour;

    // for assignment via Inspector
    [SerializeField]
    protected UnityEvent onTrackingBegin;
    [SerializeField]
    protected UnityEvent onTrackingFound;
    [SerializeField]
    protected UnityEvent onTrackingLost;

    protected bool isFound = false; // whether this marker is currently found or not

    #endregion // PRIVATE_MEMBER_VARIABLES

    #region VARIABLES_FOR_TESTING_WITHOUT_CAMERA_OR_MARKER
    [SerializeField]
    [Tooltip("To simulate having tracked the marker, and move it in front of ARCamera")]
    protected bool enableTrackingWithoutMarker = false;
    [SerializeField]
    [Tooltip("Key to press to track/untrack this ImageTarget")]
    protected KeyCode toggleTrackingKey;

    #endregion  // TESTING_WITHOUT_CAMERA_OR_MARKER_VARIABLES

    #region UNTIY_MONOBEHAVIOUR_METHODS

    protected virtual void Awake()
    {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
            mTrackableBehaviour.RegisterTrackableEventHandler(this);

        // NOTE: if tracking begins instantly
        OnTrackingBegin();
    }

    protected void Update()
    {
        if (enableTrackingWithoutMarker)    // trigger tracking via user input is enabled
        {
            if (Input.GetKeyDown(toggleTrackingKey))    // trigger tracking via button press
            {
                if (isFound)    // was already found
                {
                    OnTrackingLost();   // toggle tracking off
                }
                else    // previously not found yet
                {
                    OnTrackingFound();  // toggle tracking on
                    
                    // get position of ARCamera
                    Transform camera = GameObject.Find("ARCamera").transform;
                    // set position in front of camera, with scale of this ImageTarget to determine distance from camera
                    transform.position = camera.position + camera.forward * Mathf.Max(transform.localScale.x, transform.localScale.z) - new Vector3(0f, 0.25f * transform.localScale.y, 0f);

                    // for better precision, get bounds of all children objects of this ImageTarget

                }
            }
        }
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS

    #region PUBLIC_METHODS

    /// <summary>
    ///     Function to be called whenever tracking is to begin
    /// </summary>
    public virtual void OnTrackingBegin()
    {
        /// Custom behaviour
        isFound = false;

        // Disable rendering components
        SetRendering(false);

        // Invoke Function Callbacks
        onTrackingBegin.Invoke();

        if (whenTrackingBegun != null) {
            whenTrackingBegun.Invoke(this, EventArgs.Empty);
        }
        
        Debug.Log("Vuforia Tracking: BEGIN");
    }

    /// <summary>
    ///     Function to be called whenever this image target is tracked or already found
    /// </summary>
    public virtual void OnTrackingFound()
    {
        /// Custom behaviour
        isFound = true;

        // Enable rendering components
        SetRendering(true);

        // Invoke Function Callbacks
        onTrackingFound.Invoke();

        if (whenTrackingFound != null) {
            whenTrackingFound.Invoke(this, EventArgs.Empty);
        }

        Debug.Log("Vuforia Tracking: FOUND");
    }


    /// <summary>
    ///     Whether Vuforia is currently being tracked
    /// </summary>
    /// <returns> Is found by Vuforia tracking or not </returns>
    public bool IsFound()
    {
        return isFound;
    }

    /// <summary>
    ///     Implementation of the ITrackableEventHandler function called when the
    ///     tracking state changes.
    /// </summary>
    public void OnTrackableStateChanged(
        TrackableBehaviour.Status previousStatus,
        TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");
            OnTrackingFound();
        }
        else if (previousStatus == TrackableBehaviour.Status.TRACKED &&
                 newStatus == TrackableBehaviour.Status.NOT_FOUND)
        {
            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
            OnTrackingLost();
        }
        else
        {
            // For combo of previousStatus=UNKNOWN + newStatus=UNKNOWN|NOT_FOUND
            // Vuforia is starting, but tracking has not been lost or found yet
            // Call OnTrackingLost() to hide the augmentations
            OnTrackingLost();
        }
    }

    #endregion // PUBLIC_METHODS

    #region PRIVATE_METHODS
    protected virtual void OnTrackingLost()
    {
        // Disable rendering components
        SetRendering(false);

        // Invoke Function Callbacks
        onTrackingLost.Invoke();

        /// Custom behaviour (if any)
        isFound = false;

        if (whenTrackingLost != null) {
            whenTrackingLost.Invoke(this, EventArgs.Empty);
        }

        Debug.Log("Vuforia Tracking: LOST");
    }

    #endregion // PRIVATE_METHODS

    #region HELPER_METHODS

    /// <summary>
    ///     Enable or disable rendering components
    /// </summary>
    /// <param name="enable"></param>
    protected void SetRendering(bool enable)
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);
        //var colliderComponents = GetComponentsInChildren<Collider>(true);

        // Set rendering:
        foreach (var component in rendererComponents)
            component.enabled = enable;

        // Set colliders:
        /*foreach (var component in colliderComponents)
            component.enabled = enable;*/

        // Set canvas':
        foreach (var component in canvasComponents)
            component.enabled = enable;
    }

    #endregion // HELPER_METHODS
}
