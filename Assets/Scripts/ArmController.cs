using HoloToolkit.Unity.InputModule;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ArmController : MonoBehaviour, IManipulationHandler
{

    public string ServiceEndPoint;

    public string ArmId;

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

                Debug.LogFormat("Delta: {0} {1} {2}",
                        deltaX,
                        deltaY,
                        deltaZ);

                //StartCoroutine(PushData(delta));

                // set Data
                frameTrackingNumber = currentFrame;
                previousPosition = currentPosition;

            }
        }
        frameTrackingNumber++;
    }

    IEnumerator PushData(Vector3 delta)
    {
        //byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
        // TODO: 
        var requestURL = string.Format("http://{0}/api/arm/{1}/delta/{2}/{3}/{4}", ServiceEndPoint, ArmId, delta.x, delta.y, delta.z);
        UnityWebRequest www = UnityWebRequest.Put(requestURL, new byte[] { });
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
}
