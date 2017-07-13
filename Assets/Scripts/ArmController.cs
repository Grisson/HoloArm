using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ArmController : MonoBehaviour, IManipulationHandler
{

    public string ServiceEndPoint = "10.125.169.141:8182";

    public int RightArmId = 4396;
    public int LeftArmId = 43967;

    public Text StatusOutPut;

    private bool isManipulating;
    private Vector3 LeftArmCurrentPosition;
    private Vector3 RightArmCurrentPosition;

    private int frameTrackingNumber = 1;

    public void OnManipulationStarted(ManipulationEventData eventData)
    {
        isManipulating = true;

        if (eventData.SourceId == 0)
        {
            LeftArmCurrentPosition = eventData.CumulativeDelta;
        }
        else
        {
            RightArmCurrentPosition = eventData.CumulativeDelta;
        }

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
        if (eventData.SourceId == 0)
        {
            LeftArmCurrentPosition = eventData.CumulativeDelta;
        }
        else
        {
            RightArmCurrentPosition = eventData.CumulativeDelta;
        }
    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
        StatusOutPut.text = "Idle";
        isManipulating = false;
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
                PreprocessArmPosition(LeftArmId, LeftArmCurrentPosition);
                PreprocessArmPosition(RightArmId, RightArmCurrentPosition);

                // set Data
                frameTrackingNumber = currentFrame;
            }
        }
        frameTrackingNumber++;
    }

    IEnumerator PushData(int armId, double x, double y, double z)
    {
        //byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
        // TODO: 
        var timestamp = DateTime.Now.Ticks;
        var requestURL = string.Format("http://{0}/api/arm/{1}/goco/{2}/{3}/{4}/{5}", ServiceEndPoint, armId, x, y, z, timestamp);
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

    void PreprocessArmPosition(int armId, Vector3 armPosition)
    {
        if ((armPosition.x == 0) && (armPosition.y == 0) && (armPosition.z == 0))
        {
            return;
        }

        Debug.LogFormat("Current Arm {3}: {0} {1} {2}",
              armPosition.x,
              armPosition.y,
              armPosition.z,
              armId);

        var armCooridnate = ConvertCoordinate(armPosition);

        Debug.LogFormat("Current Arm {3}: {0} {1} {2}",
              armCooridnate.x,
              armCooridnate.y,
              armCooridnate.z,
              armId);

        StartCoroutine(PushData(armId, armCooridnate.x, armCooridnate.y, armCooridnate.z));
    }

    public Vector3 ConvertCoordinate(Vector3 current)
    {
        var fingerX = RegulateData((current.x * -1), -0.2, 0.2);
        var fingerY = RegulateData(current.y, -0.15, 0.15);
        var fingerZ = RegulateData(current.z, -0.2, 0.2);

        var armX = Remap(fingerZ, -0.2, 0.2, 60, 180);
        var armY = Remap(fingerX, -0.2, 0.2, -60, 60);
        var armZ = Remap(fingerY, -0.15, 0.15, 0, 120);

        return new Vector3((float)armX, (float)armY, (float)armZ);
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
