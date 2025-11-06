using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class ExteriorCam : MonoBehaviour {

    public bool useCalc = false, enableCorrection = false, directCorr = false;
    public Transform target;
    public bool rotateFromCenter = false, camMovement = true;
    public Text fpsCounter;

    [Header("Rotation Speed")]
    public float xSpeed = 0.1f;
    public float ySpeed = 1.0f;

    [Header("Zoom Settings")]
    [SerializeField] bool infiniteZoom = false;
    public float distanceMin = 1f;
    public float distanceMax = 10f;
    public float smoothTime = 2f;
    //   public float pinchSensi = 2f;

    [Header("Auto Rotation")]
    [SerializeField]
    bool autoRotate = false;
    bool controlCam = true;
    public float autoRotateSpeed = 3;

    [Header("Camera Rotation Limits")]
    public float yMinLimit = -90f;
    public float yMaxLimit = 90f;

    [Header("Camera Distance")]
    public float distance = 2.0f;

    public float rotationYAxis = 0.0f;
    public float rotationXAxis = 0.0f;
    float velocityX = 0.0f;
    float velocityY = 0.0f;
    [Header("Pan Settings")]
    [SerializeField] bool panning = false, panLeft, panRight, autoPan;
    bool useAutoPan = false;
    public float panSpeed = 1f;
    public float maxPanRadius = 2f, curRad, autoPanTime, panStat, panVal;
    [SerializeField] Vector3 panAmt;
    [Space(10)]
    public Slider zoomDist;

    [SerializeField]
    float orbitPoint = 0;
    [SerializeField] bool touchSupported = false, readingInput = false, isZooming = false, useSmoothing = false;
    float xS, yS, smoothDistance;
    public Vector3 prevpos, mousedelta, centerPoint, targetPoint,
         firstPanPoint, secondPanPoint, lastSecondPanPoint,
         panLastMouseDelta, panTargetMouseDelta, panResetPos, totalPan;
    Bounds bounds;
    [SerializeField] bool uiInteraction = false;
    public void CallMovement() {
        //     Vector3 v = new Vector3(0.0f, 0.0f, -distance); 
        //   Vector3 position = Quaternion.Euler(1,1,0) * v + target.position;
        Debug.Log("CALL MOVEMENT");
        readingInput = true;
        if (!panning)
            transform.LookAt(target);
    }
    // Use this for initialization
    void OnEnable() {

        //distanceMin = 0.07f;
        //zoomSpeed = 0.03f;
        // if(SceneManager.sceneCount > 1)
        // {
        //     infiniteZoom = false;
        // }
        if (Input.touchSupported)
        {
            touchSupported = true;
        }
        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>()) {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
        if (target != null) {
            centerPoint = target.transform.position;
            targetPoint = target.transform.position;

        }

        // InvokeRepeating("PrintFPS", 0.5f, 0.5f);
    }

    void PrintFPS() {
        if (fpsCounter)
            fpsCounter.text = "FPS: " + (1 / Time.deltaTime).ToString("00");
    }

    void AutoRotateCam() {
        if (!readingInput) {
            orbitPoint += autoRotateSpeed * Time.deltaTime;
            orbitPoint = (orbitPoint > 359.9) ? 0 : orbitPoint;
            transform.RotateAround(rotateFromCenter ? centerPoint : target.position, Vector3.up, orbitPoint);
        }
        else {
            orbitPoint = 0;
        }
    }
    public static bool MouseOverUI() {
        bool result = false;
        if (EventSystem.current != null) {
            GameObject g = EventSystem.current.currentSelectedGameObject;
            if (g)
                result = (EventSystem.current.currentSelectedGameObject.layer == 5);
        }
        return result;
    }
    public static bool MouseOverObject() {
        bool mouseOverUI = false;
        //var eventData = new PointerEventData(EventSystem.current);
        //eventData.position = Input.mousePosition;
        //var results = new List<RaycastResult>();
        //if (EventSystem.current != null)
        //    EventSystem.current.RaycastAll(eventData, results);
        //mouseOverUI = results.Count > 0;
        if (Application.platform == RuntimePlatform.Android) {
            foreach (Touch touch in Input.touches) {
                int id = touch.fingerId;
                if (EventSystem.current.IsPointerOverGameObject(id)) {
                    // ui touched
                    mouseOverUI = true;
                }
            }
        }
        else
        if (EventSystem.current.IsPointerOverGameObject()) {
            // ui touched
            mouseOverUI = true;
        }

        return mouseOverUI;
    }

    void Update() {
        if (distance < 0.1f)
        {
            totalPan = Vector3.zero;
            target.position = transform.position + (transform.forward * 0.1f);
            distance = 0.1f;
        }
        if (target) {
            uiInteraction = MouseOverObject();
            //  uiInteraction = false;
            if (!uiInteraction) {
                Vector3 angles = transform.eulerAngles;
                rotationYAxis = angles.y;
                rotationXAxis = angles.x;
                if (rotationXAxis > 180f) {
                    rotationXAxis -= 360f;
                }
                if (Input.GetMouseButtonDown(0)) {

                    prevpos = Input.mousePosition;
                }
                //else 
                if (Input.GetMouseButton(0)) {
                    mousedelta = prevpos - Input.mousePosition;
                    prevpos = Input.mousePosition;
                }
                GetPanInput();
            }
            //+++++++++++++++++++
            CamMovement();
        }

        /* if (Input.GetMouseButton(1)) {
             transform.Translate(-Input.GetAxis("Mouse X") * Time.deltaTime,  -Input.GetAxis("Mouse Y") * Time.deltaTime, 0);
         }*/
    }
    float panThreshold = 0.25f;

    void GetPanInput() {
        if (panResetPos != target.position)
            ResetPan();
        if (Input.GetMouseButtonDown(1)) {
            firstPanPoint = Input.mousePosition;
            //  panLastMouseDelta = panTargetMouseDelta;
            panResetPos = target.position;
            lastSecondPanPoint = firstPanPoint;
        }

        if (Input.GetMouseButton(1)) {
            secondPanPoint = Input.mousePosition;
            panning = true;
            Pan(secondPanPoint, lastSecondPanPoint);
            lastSecondPanPoint = secondPanPoint;
        }
        if (Input.GetMouseButtonUp(1))
            panning = false;
    }
    public void Pan(Vector3 pt2, Vector3 pt1) {
        Debug.Log("PAN!");
        Vector3 delta = (pt2 - pt1) * -1;
        delta.x = delta.x / Screen.width;
        delta.y = delta.y / Screen.height;
        panTargetMouseDelta = (transform.right * delta.x + transform.up * delta.y) * panSpeed;
        totalPan += panTargetMouseDelta;
    }
    public void SetZoomSpeed(float val) {
        zoomSpeed = val;
    }
    bool performCorrection = false;
    float smoothSpeed = 1f;
    void SmoothReset() {
        //   Debug.Log("SMOOTH");
        float dist = Vector3.Distance(transform.position, nxtPos);
        smoothSpeed += Time.deltaTime;
        smoothStat += Time.deltaTime * smoothSpeed;
        transform.position = Vector3.Lerp(transform.position, nxtPos, smoothStat);
        transform.localRotation = Quaternion.Lerp(transform.rotation, nxtRot, smoothStat);

        if (smoothStat >= 1f)
        {
            isSmoothing = false;
            
            if (enableCorrection)
            {
                if (!performCorrection)
                {
                    Vector3 newPos = transform.position;
                    Quaternion newRot = Quaternion.LookRotation(target.position - transform.position);
                    pastPos = transform.position;
                    pastRot = transform.localRotation;
                    nxtRot = newRot;
                    performCorrection = true;
                    smoothStat = 0f;
                    SyncControllerWithTransform();
                }
                else
                    performCorrection = false;
            }
        }
    }
    void GetCalculatedVals(Vector3 newPos, Quaternion newRot, out Vector3 pos, out Quaternion rot) {

        rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);
        Quaternion fromRotation = Quaternion.Euler(newRot.eulerAngles.x, newRot.eulerAngles.y, 0);
        Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
        rot = toRotation;

        Vector3 negDistance = Vector3.zero;
        negDistance = new Vector3(0.0f, 0.0f, -distance);
        pos = rot * negDistance + newPos;
    }
    // void CamMovement() {
    //     if (target) {
    //         if (Input.GetMouseButton(0)) {
    //             if (controlCam && (!pinchZooming || !panning)) {
    //                 velocityX += xSpeed * -mousedelta.x * distance * 0.002f;
    //                 velocityY += ySpeed * -mousedelta.y * 0.002f;
    //             }
    //         }
    //         CheckZoomGesture2();
    //         rotationYAxis += velocityX;
    //         rotationXAxis -= velocityY;
    //         rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);
    //         Quaternion fromRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    //         Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
    //         Quaternion rotation = toRotation;

    //         Vector3 negDistance = Vector3.zero;
    //         negDistance = new Vector3(0.0f, 0.0f, -distance);
    //         Vector3 position = rotation * negDistance + target.position + totalPan;

    //         velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
    //         velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);

    //         readingInput = (Input.touchCount > 0 || Input.GetMouseButton(0) || Input.mouseScrollDelta.y != 0 || panning) ? true : false;
    //         if (autoRotate) {
    //             AutoRotateCam();
    //         }
    //         if (isZooming) {
    //             distance = Mathf.Lerp(distance, smoothDistance, smoothTime * Time.deltaTime);
    //         }
    //         if (pinchZooming) {
    //             if (infiniteZoom) {

    //                 if (distance < (distanceMin + 0.2f)) {
    //                     Debug.Log("Pushing Forward: " + distance + "<" + distanceMin);
    //                     transform.position = position + transform.forward * distance;
    //                 }
    //                 if (distance > (distanceMax - 0.1f)) {
    //                     Debug.Log("Pushing Back: " + distance + ">" + distanceMax);
    //                     transform.position = position - transform.forward * distance;
    //                 }
    //             }
    //             else {
    //                 transform.position = position;
    //                 transform.localRotation = rotation;
    //             }
    //         }
    //         if (readingInput && !uiInteraction) {
    //             //     Debug.Log("READING: " + readingInput);
    //             //       transform.localPosition += panTargetMouseDelta;
    //             if (panning) {
    //                 transform.localPosition += panTargetMouseDelta;
    //                 //  transform.localPosition = Vector3.Lerp(position, transform.localPosition + panTargetMouseDelta, panSpeed * Time.deltaTime);
    //                 curRad = Vector3.Distance(position, transform.localPosition);
    //                 if (curRad > maxPanRadius)
    //                     transform.localPosition = Vector3.Lerp(transform.position, panResetPos, Time.deltaTime * 2);
    //             }
    //             else {
    //                 //if (Vector3.Distance(transform.position, position) > 0.01f || Quaternion.Angle(transform.localRotation, rotation) > 5f)
    //                 //    NewPosRot(position, rotation);
    //                 //else
    //                 {
    //                     transform.position = position;
    //                     transform.localRotation = rotation;
    //                 }
    //             }
    //         }
    //         else {
    //             if (isSmoothing || performCorrection) {
    //                 SmoothReset();
    //             }
    //         }
    //         //if(useSmoothing)
    //         //if (performCorrection)
    //         //{
    //         //    smoothStat = 0;
    //         //    smoothSpeed = 3f;
    //         //    nxtPos = position;
    //         //    nxtRot = rotation;
    //         //}

    //     }
    // }

    // 🔄  Completely replace your current CamMovement() with this version
void CamMovement()
{
    if (target == null) { return; }

    /* ──────────────────────────────
     * 1.  Handle gesture updates first
     * ────────────────────────────── */
    CheckZoomGesture2();                //  ➜ updates: distance, pinchZooming, panning, panTargetMouseDelta

    /* ──────────────────────────────
     * 2.  Orbit-drag with one finger / mouse
     *     (disabled while pinching OR panning)
     * ────────────────────────────── */
    bool singleFingerDrag =
        (Input.touchSupported && Input.touchCount == 1 && !pinchZooming && !panning);

    bool mouseDrag =
        (!Input.touchSupported && Input.GetMouseButton(0) && !pinchZooming && !panning);

    if (controlCam && (singleFingerDrag || mouseDrag))
    {
        velocityX += xSpeed * -mousedelta.x * distance * 0.002f;
        velocityY += ySpeed * -mousedelta.y * 0.002f;
    }

    /* ──────────────────────────────
     * 3.  Build final rotation & position
     * ────────────────────────────── */
    rotationYAxis += velocityX;
    rotationXAxis  = ClampAngle(rotationXAxis - velocityY, yMinLimit, yMaxLimit);

    Quaternion rotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0f);
    Vector3   position  =
        rotation * new Vector3(0f, 0f, -distance) + target.position + totalPan;

    // inertia damping
    velocityX = Mathf.Lerp(velocityX, 0f, Time.deltaTime * smoothTime);
    velocityY = Mathf.Lerp(velocityY, 0f, Time.deltaTime * smoothTime);

    /* ──────────────────────────────
     * 4.  Apply movement priority
     *     pinch-zoom   ➜ pan   ➜ normal orbit
     * ────────────────────────────── */
    if (pinchZooming)
    {
        transform.position      = position;
        transform.localRotation = rotation;
    }
    else if (panning)
    {
        transform.localPosition += panTargetMouseDelta;
    }
    else
    {
        transform.position      = position;
        transform.localRotation = rotation;
    }

    /* ──────────────────────────────
     * 5.  Extra behaviours
     * ────────────────────────────── */
    readingInput = Input.touchCount > 0 ||
                   Input.GetMouseButton(0) ||
                   Mathf.Abs(Input.mouseScrollDelta.y) > 0f ||
                   panning;

    if (autoRotate && !readingInput)
        AutoRotateCam();

    if (isZooming)
        distance = Mathf.Lerp(distance, smoothDistance, smoothTime * Time.deltaTime);

    // Optional “push-out” at min/max zoom when infiniteZoom is enabled
    if (infiniteZoom && pinchZooming)
    {
        if (distance < distanceMin + 0.2f)
            transform.position = position + transform.forward * distance;
        else if (distance > distanceMax - 0.1f)
            transform.position = position - transform.forward * distance;
    }

    // Smooth correction (if you already use these flags)
    if (isSmoothing || performCorrection)
        SmoothReset();
}


    void LateUpdate() {
        //   if(camMovement)
        //   CamMovement();//------------------------------------
        //  panning = (panLeft || panRight);
        if (autoPan)
            if (panStat <= autoPanTime) {
                panning = true;
                panStat += Time.deltaTime * panSpeed;
                PanLeft(panVal, autoPanTime);
            }
            else {
                panVal = 0;
                autoPan = false;
                panning = false;
            }

        if (panLeft) {
            //panStat = 0;
            //autoPan = true;
            //panVal = (-0.25f  * maxPanRadius);
            //autoPanTime = 1.5f;
            //PanLeft(panVal, autoPanTime);
            PanRL(-0.15f, 1f);

            panLeft = false;
        }
        if (panRight) {
            //panStat = 0;
            //autoPan = true;
            //autoPanTime = 1.5f;
            //panVal =(0.25f * maxPanRadius);
            PanRL(0.15f, 1f);
            panRight = false;
        }
    }
    Vector3 pastPos, nxtPos;
    Quaternion pastRot, nxtRot;
    [SerializeField] bool isSmoothing;
    [SerializeField] float smoothStat = 0;
    void ResetPan() {
        panTargetMouseDelta = Vector3.zero;
        totalPan = Vector3.zero;
    }
    public void PanLeft() {
        if (useAutoPan)
            Debug.Log("<color=white>Pan Left!</color>");
        //   panLeft = true;
    }

    public void PanRight() {
        if (useAutoPan)
            Debug.Log("<color=white>Pan Right!</color>");
        //    panRight = true;
    }

    public void PanRL(float val, float time) {
        readingInput = true;
        //  autoPan = true;
        panStat = 0;
        autoPanTime = time;
        panVal = val / (time / Time.deltaTime);
        Pan(Vector3.right * val * Screen.width, Vector3.zero);
    }
    void PanLeft(float val, float time) {
        Pan(Vector3.right * val * Screen.width, Vector3.zero);
    }
    public void AutoPan(Vector3 amt, float time) {
        panStat += Time.deltaTime * panSpeed;
        Pan(Vector3.Lerp(Vector3.zero, amt / time, Time.deltaTime * maxPanRadius), Vector3.zero);
    }
    IEnumerator GoLeft(Vector3 dir, float time) {
        Debug.Log("LEFT!");
        float timePassed = 0;
        float tSection = Time.deltaTime;
        while (timePassed < time) {
            Debug.Log("LEFT: " + timePassed);
            timePassed += Time.deltaTime * maxPanRadius;
            Pan(Vector3.Lerp(Vector3.zero, dir, timePassed), Vector3.zero);
            yield return null;
        }
    }
    
    /// <summary>Synchronise controller state with the camera’s current transform.</summary>
void SyncControllerWithTransform()
{
    // distance measured from target -- honour both orbit centre & pan offset
    Vector3 dir = transform.position - target.position - totalPan;
    distance = dir.magnitude;

    // copy current Euler angles back into the controller
    Vector3 eul = transform.rotation.eulerAngles;
    rotationXAxis = eul.x;
    rotationYAxis = eul.y;

    // flush inertial drift
    velocityX = 0f;
    velocityY = 0f;

    // make sure the next gesture starts clean
    pinchZooming = false;
    panning      = false;
}

    public void NewPosRot(Vector3 pos, Quaternion rot)
    {
        //Debug.Log("distance: " + distance);
        pastPos = transform.position;
        pastRot = transform.localRotation;
        Vector3 test;

        ResetPan();
        if (useCalc)
        {
            //   distance = Vector3.Distance(pos, targetPoint);

            GetCalculatedVals(pos, rot, out nxtPos, out nxtRot);
        }
        else
        {
            nxtPos = pos;
            nxtRot = rot;
        }
        //   distance = Vector3.Distance(pos, targetPoint);
        if (useSmoothing)
        {
            camMovement = false;
            performCorrection = false;
            isSmoothing = true;
            smoothSpeed = 1.5f;
            smoothStat = 0;
        }
        else
        {
            transform.position = pos;
            transform.localRotation = rot;
            camMovement = true;
            SyncControllerWithTransform();
        }
        Debug.Log("distance2: " + distance);
    }

    //Change cam angle
    public void NewPosTarget(Vector3 pos, Transform newTarget) {
        target = newTarget;
        //Debug.Log("distance: " + distance);
        pastPos = transform.position;
        pastRot = transform.localRotation;
        distance = Vector3.Distance(pos, newTarget.position);
        ResetPan();
        nxtPos = pos;
        nxtRot = Quaternion.LookRotation(newTarget.position - pos);

        //   distance = Vector3.Distance(pos, targetPoint);
        if (useSmoothing)
        {
            camMovement = false;
            performCorrection = false;
            isSmoothing = true;
            smoothSpeed = 1.5f;
            smoothStat = 0;
        }
        else
        {
            transform.position = pos;
            transform.localRotation = nxtRot;
            camMovement = true;
            SyncControllerWithTransform();
        }
    }
    public static float ClampAngle(float angle, float min, float max) {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    public void SetDistance(float newDistance) {
        readingInput = true;
        uiInteraction = false;
        distance = Mathf.Clamp(newDistance, distanceMin, distanceMax);
    }

    public void ToggleAutoRotate() {
        autoRotate = !autoRotate;
    }

    public void StartZoom() {
        isZooming = true;
        controlCam = false;
    }

    public void EndZoom() {
        controlCam = true;
        isZooming = false;
    }

    public void SetZoom(float newDistance) {
        readingInput = false;
        float d = distanceMax - newDistance;
        smoothDistance = Mathf.Clamp(d, distanceMin, distanceMax);
        if (!isZooming) {
            distance = smoothDistance;
        }
    }

    public float prevPinchAmt = 0f;
    public float zoomSpeed = 0.5f;
    public float zoomThreshold = 0.2f;
    bool pinchZooming = false;
    // void CheckZoomGesture2() {
    //     //  zoomSpeed = 0.1f;
    //     if (touchSupported) {
    //         // Pinch to zoom
    //         if (Input.touchCount == 2)// && (Input.GetTouch(0).phase==TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved))
    //         {

    //             // get current touch positions
    //             Touch t0 = Input.GetTouch(0);
    //             Touch t1 = Input.GetTouch(1);
    //             // get touch position from the previous frame
    //             Vector2 lastT0 = t0.position - t0.deltaPosition;
    //             Vector2 lastT1 = t1.position - t1.deltaPosition;
    //             //float ltctDist = Vector3.Distance((t0.position + t1.position) / 2, (t0.deltaPosition + t1.deltaPosition) / 2);
    //             //if (ltctDist > panThreshold)
    //             //{
    //             //    Debug.Log("Touch Dist: " + ltctDist);
    //             panning = true;
    //             Pan((t0.position + t1.position) / 2, (lastT0 + lastT1) / 2);
    //             //}
    //             //else
    //             //    panning = false;

    //             float lastTouchDist = Vector2.Distance(lastT0, lastT1);
    //             float currTouchDist = Vector2.Distance(t0.position, t1.position);

    //             // get offset value
    //             float deltaDistance = lastTouchDist - currTouchDist;
    //             float deltaTimeSpeed = deltaDistance * Time.deltaTime * zoomSpeed;
    //             if (Mathf.Abs(deltaDistance) > zoomThreshold) {
    //                 pinchZooming = true;
    //                 Debug.Log("Zoom");
    //                 distance += deltaTimeSpeed;
    //             }
    //             else
    //                 pinchZooming = false;

    //             if (infiniteZoom)
    //                 distance = Mathf.Clamp(distance, distance - 0.1f, distanceMax);
    //             Debug.Log("D: " + distance + "| deltaDist: " + deltaDistance + " | DeltaTS: " + deltaTimeSpeed);
    //         }
    //         else {
    //             pinchZooming = false;
    //             if (panning) {
    //                 panning = false;
    //             }
    //         }
    //     }
    //     else {
    //         float scroll = Input.GetAxis("Mouse ScrollWheel");
    //         if (infiniteZoom)
    //             distance = Mathf.Clamp(distance - scroll, distance - 0.1f, distanceMax);
    //         else
    //             distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel"), distanceMin, distanceMax);
    //     }
    // }
    //     void CheckZoomGesture2() {
    //     if (touchSupported) {
    //         // Pinch to zoom
    //         if (Input.touchCount == 2) {
    //             // Get current touch positions
    //             Touch t0 = Input.GetTouch(0);
    //             Touch t1 = Input.GetTouch(1);

    //             // Get touch position from the previous frame
    //             Vector2 lastT0 = t0.position - t0.deltaPosition;
    //             Vector2 lastT1 = t1.position - t1.deltaPosition;

    //             panning = false;
    //             Pan((t0.position + t1.position) / 2, (lastT0 + lastT1) / 2);

    //             // Compute distances
    //             float lastTouchDist = Vector2.Distance(lastT0, lastT1);
    //             float currTouchDist = Vector2.Distance(t0.position, t1.position);

    //             // Get offset value
    //             float deltaDistance = lastTouchDist - currTouchDist;
    //             float deltaTimeSpeed = deltaDistance * Time.deltaTime * zoomSpeed;

    //             if (Mathf.Abs(deltaDistance) > zoomThreshold) {
    //                 pinchZooming = true;
    //                 Debug.Log("Zoom");
    //                 distance += deltaTimeSpeed;
    //             }
    //             else {
    //                 pinchZooming = false;
    //             }

    //             // Clamp zoom for touch input
    //             distance = Mathf.Clamp(distance, distanceMin, distanceMax);

    //             Debug.Log("D: " + distance + "| deltaDist: " + deltaDistance + " | DeltaTS: " + deltaTimeSpeed);
    //         }
    //         else {
    //             pinchZooming = false;
    //             panning = false;
    //         }
    //     }
    //     else {
    //         // Mouse Scroll Zoom
    //         float scroll = Input.GetAxis("Mouse ScrollWheel");
    //         distance -= scroll;  // Adjust distance based on scroll input

    //         // Clamp zoom for mouse input
    //         distance = Mathf.Clamp(distance, distanceMin, distanceMax);
    //     }
    // }

    // 🔄 Replace your entire CheckZoomGesture2() with this
void CheckZoomGesture2()
{
    // ---------- DESKTOP : mouse-wheel ----------
    if (!touchSupported)
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scroll * zoomSpeed, distanceMin, distanceMax);
        return;
    }

    // ---------- MOBILE : two-finger gesture ----------
    if (Input.touchCount == 2)
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
        {
        // panning = false;
        // pinchZooming = false;
        return;
        }


        Vector2 prevT0 = t0.position - t0.deltaPosition;
        Vector2 prevT1 = t1.position - t1.deltaPosition;

        float prevDist  = Vector2.Distance(prevT0, prevT1);
        float currDist  = Vector2.Distance(t0.position, t1.position);
        float pinchDelta = prevDist - currDist;          // (+) ⇒ zoom-in , (−) ⇒ zoom-out

        // Zoom or Pan?
        if (Mathf.Abs(pinchDelta) > zoomThreshold)
        {
            // -------- PINCH-ZOOM --------
            pinchZooming = true;
            panning      = false;

            distance += pinchDelta * zoomSpeed * Time.deltaTime;
            distance  = Mathf.Clamp(distance, distanceMin, distanceMax);
        }
        else
        {
            // -------- TWO-FINGER PAN --------
            pinchZooming = false;
            panning      = true;

            Vector2 currMid = (t0.position + t1.position) * 0.5f;
            Vector2 prevMid = (prevT0     + prevT1)     * 0.5f;
            Pan(currMid, prevMid);
        }
    }
    else
    {
        // Reset flags when fingers are lifted
        pinchZooming = false;
        panning      = false;
    }
}


    void CheckZoomGesture() {
        if (touchSupported) {
            // Pinch to zoom
            if (Input.touchCount == 2) {
                Vector2 touch0 = Input.GetTouch(0).position;
                Vector2 touch1 = Input.GetTouch(1).position;
                float pinchAmt = Vector2.Distance(touch0, touch1);
                Debug.Log("Prev Pinch: " + prevPinchAmt + " | Pinch:" + pinchAmt);
                if (prevPinchAmt != 0f && Mathf.Abs(pinchAmt - prevPinchAmt) > 2f) {
                    distance -= (pinchAmt - prevPinchAmt) * Time.deltaTime * zoomSpeed;
                }
                prevPinchAmt = pinchAmt;
                distance = Mathf.Clamp(distance, distanceMin, distanceMax);
            }
            else
                pinchZooming = false;
        }
        else {
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel"), distanceMin, distanceMax);
        }
    }

    void Zoom(float deltaMagnitudeDiff, float speed) {
        distance -= deltaMagnitudeDiff * speed;
        distance = Mathf.Clamp(distance, distanceMin, distanceMax);
    }
}