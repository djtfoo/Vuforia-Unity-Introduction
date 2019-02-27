/*==============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.

* Class: CustomTrackableEventHandler
* A generic Vuforia tracking event handler implementing the ITrackableEventHandler interface.
* Also allows testing of codes without requiring actual tracking,
*  via pressing a button to simulate tracking found or lost.
* Modified By: Foo Jing Ting
* Date: 27 February 2019
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
    [Tooltip("To simulate having tracked the marker, and move it in front of ARCamera")]
    public bool enableTrackingWithoutMarker = false;
    [Tooltip("Key to press to track/untrack this ImageTarget")]
    public KeyCode toggleTrackingKey;
    [Tooltip("Texture for this ImageTarget's image marker")]
    public Texture imageTargetTexture;

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

    protected virtual void Update()
    {
#if UNITY_EDITOR
        if (enableTrackingWithoutMarker)    // trigger tracking via user input is enabled
        {
            if (Input.GetKeyDown(toggleTrackingKey))    // trigger tracking via button press
            {
                TriggerTrackingToggle();
            }
        }
#endif
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS

    #region SIMULATION_HELPER_METHODS

    /// <summary>
    ///     Toggle between tracking and not tracking.
    ///     Can be called by other function callbacks and not just in Update().
    /// </summary>
    public void TriggerTrackingToggle()
    {
        if (isFound)    // was already found
        {
            OnTrackingLost();   // toggle tracking off
        }
        else    // previously not found yet
        {
            OnTrackingFound();  // toggle tracking on

            // recreate image marker
            if (GetComponent<Renderer>() == null)   // no marker yet
            {
                CreateImageTarget();
            }

            // get position of ARCamera
            Transform camera = GameObject.Find("ARCamera").transform;
            // set position in front of camera, with scale of this ImageTarget to determine distance from camera
            //transform.position = camera.position + camera.forward * Mathf.Max(transform.localScale.x, transform.localScale.z) - new Vector3(0f, 0.25f * transform.localScale.y, 0f);

            // for better precision, get bounds of all children objects of this ImageTarget
            Bounds bounds = getBoundsOfChildren(transform);

            Vector3 boundsSize = bounds.size;
            // place camera to view object
            float distance = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            distance /= (2.0f * Mathf.Tan(0.5f * camera.GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad));
            //transform.position = camera.position + distance * camera.forward; // place object in front of camera
            camera.position = transform.position - distance * camera.forward;
        }
    }

    /// <summary>
    ///     Funcion to get encapsulated Bounds of all children GameObjects recursively.
    /// </summary>
    /// <returns> Bounds encapsulating all Collider and Mesh Bounds in the GameObject hierarchy </returns>
    protected Bounds getBoundsOfChildren(Transform t)
    {
        // create total bounds of this GameObject's Mesh and Collider Bounds
        Bounds totalBounds = new Bounds();
        if (t.GetComponent<Renderer>()) // there is MeshFilter Component
        {
            totalBounds.Encapsulate(t.GetComponent<Renderer>().bounds);
        }
        if (t.GetComponent<Collider>())   // there is a Collider Component
        {
            totalBounds.Encapsulate(t.GetComponent<Collider>().bounds);
        }

        if (t.childCount > 0)
        {
            foreach (Transform child in t)
            {
                totalBounds.Encapsulate(getBoundsOfChildren(child));
            }
        }

        return totalBounds;
    }

    protected void CreateImageTarget()
    {
        // attach Components
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshFilter>();

        // create Mesh
        Vector2 size = GetComponent<ImageTargetBehaviour>().GetSize();
        Vector3[] newVertices = { new Vector3(-0.5f,0f,-0.5f), new Vector3(0.5f,0f,-0.5f), new Vector3(0.5f,0f,0.5f), new Vector3(-0.5f,0f,0.5f) };
        for (int i = 0; i < newVertices.Length; i++)
        {
            // actual vertices will be scaled by transform's localScale, so cancel it off by dividing
            newVertices[i].x *= (size.x / transform.localScale.x);
            newVertices[i].z *= (size.y / transform.localScale.x);
        }
        Vector2[] newUV = { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };
        int[] newTriangles = { 2, 1, 0,
            3, 2, 0,
            0, 1, 2,    // back side
            0, 2, 3 };  // back side
        Mesh mesh = new Mesh();
        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;
        GetComponent<MeshFilter>().mesh = mesh;

        // create Material
        GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Texture"));
        GetComponent<Renderer>().material.mainTexture = imageTargetTexture;
    }

    #endregion  // SIMULATION_HELPER_METHODS

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

    #region TRACKING_HELPER_METHODS

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

        // Set canvas:
        foreach (var component in canvasComponents)
            component.enabled = enable;
    }

    #endregion // TRACKING_HELPER_METHODS
}
