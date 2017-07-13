using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ArmController : MonoBehaviour, IManipulationHandler
{

    public string ServiceEndPoint = "10.125.169.141:8182";

    public int ArmId = 4396;

    public Text StatusOutPut;

    private bool isManipulating;
    private Vector3 previousPosition;
    private Vector3 currentPosition;
    private Vector3 delta;
    private int frameTrackingNumber = 1;

    public void OnManipulationStarted(ManipulationEventData eventData)
    {
        previousPosition = eventData.CumulativeDelta;
        currentPosition = eventData.CumulativeDelta;
        isManipulating = true;

        StatusOutPut.text = "Moving";

        Debug.LogFormat("OnManipulationStarted\r\nSource: {0}  SourceId: {1}\r\nCumulativeDelta: {2} {3} {4}",
                eventData.InputSource,
                eventData.SourceId,
                eventData.CumulativeDelta.x,
                eventData.CumulativeDelta.y,
                eventData.CumulativeDelta.z);
    }

    public void OnManipulationUpdated(ManipulationEventData eventData)
    {
        //Debug.LogFormat("OnManipulationUpdated Source: {0}  SourceId: {1}",
        //            eventData.InputSource,
        //            eventData.SourceId);
        currentPosition = eventData.CumulativeDelta;

    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
        StatusOutPut.text = "Idle";
        isManipulating = false;
        previousPosition = new Vector3();
        currentPosition = new Vector3();
        //Debug.LogFormat("OnManipulationCompleted\r\nSource: {0}  SourceId:  {1}\r\nCumulativeDelta: {2} {3} {4}",
        //        eventData.InputSource,
        //        eventData.SourceId,
        //        eventData.CumulativeDelta.x,
        //        eventData.CumulativeDelta.y,
        //        eventData.CumulativeDelta.z);
    }

    public void OnManipulationCanceled(ManipulationEventData eventData)
    {
        StatusOutPut.text = "Idle";
        isManipulating = false;
        previousPosition = new Vector3();
        currentPosition = new Vector3();
        //Debug.LogFormat("OnManipulationCanceled\r\nSource: {0}  SourceId: {1}\r\nCumulativeDelta: {2} {3} {4}",
        //        eventData.InputSource,
        //        eventData.SourceId,
        //        eventData.CumulativeDelta.x,
        //        eventData.CumulativeDelta.y,
        //        eventData.CumulativeDelta.z);
    }

    // Use this for initialization
    void Start()
    {
        InputManager.Instance.PushModalInputHandler(this.gameObject);

        StatusOutPut.text = "Idle";
    }

    // Update is called once per frame
    void Update()
    {

        var currentFrame = frameTrackingNumber % 8;
        if (isManipulating)
        {
            if (currentFrame == 0)
            {
                // send out request
                var deltaX = previousPosition.x - currentPosition.x;
                var deltaY = previousPosition.y - currentPosition.y;
                var deltaZ = previousPosition.z - currentPosition.z;

                //Debug.LogFormat("Delta: {0} {1} {2}",
                //        deltaX,
                //        deltaY,
                //        deltaZ);

                Debug.LogFormat("Current: {0} {1} {2}",
                       currentPosition.x,
                       currentPosition.y,
                       currentPosition.z);

                var fingerX = RegulateData((currentPosition.x*-1), -0.2, 0.2);
                var fingerY = RegulateData(currentPosition.y, -0.15, 0.15);
                var fingerZ = RegulateData(currentPosition.z, -0.2, 0.2);

                var armX = Remap(fingerZ, -0.2, 0.2, 60, 180);
                var armY = Remap(fingerX, -0.2, 0.2, -60, 60);
                var armZ = Remap(fingerY, -0.15, 0.15, 0, 120);

                Debug.LogFormat("Current Arm: {0} {1} {2}",
                      armX,
                      armY,
                      armZ);

                StartCoroutine(PushData(armX, armY, armZ));

                // set Data
                frameTrackingNumber = currentFrame;
                previousPosition = currentPosition;
            }
        }
        frameTrackingNumber++;
    }

    IEnumerator PushData(double x, double y, double z)
    {
        //byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
        // TODO: 
        var timestamp = DateTime.Now.Ticks;
        var requestURL = string.Format("http://{0}/api/arm/{1}/goco/{2}/{3}/{4}/{5}", ServiceEndPoint, ArmId, x, y, z, timestamp);
        UnityWebRequest www = UnityWebRequest.Put(requestURL, new byte[] { 0 });
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Upload complete!");
        }
    }


    public double Remap(double value, double from1, double to1, double from2, double to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }


    public double RegulateData(double value, double min, double max)
    {
        return Math.Min(max, Math.Max(min, value));
    }
}
