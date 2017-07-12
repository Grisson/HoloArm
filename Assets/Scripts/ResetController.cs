using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ResetController : MonoBehaviour, IInputClickHandler
{
    public string ServiceEndPoint;

    public int ArmId = 4396;

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.LogFormat("Reset arm position");
        
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }

    IEnumerator PushResetCommand()
    {
        //byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
        // TODO: 

        var timestamp = DateTime.Now.Ticks;
        var requestURL = string.Format("http://{0}/api/arm/{1}/go/0/0/0/{2}", ServiceEndPoint, ArmId, timestamp);
        var www = UnityWebRequest.Put(requestURL, new byte[] { });
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
