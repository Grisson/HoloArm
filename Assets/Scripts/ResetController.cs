﻿using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ResetController : MonoBehaviour, IInputClickHandler
{
    public string ServiceEndPoint = "10.125.169.141:8182";

    public int RightArmId = 4396;
    public int LeftArmId = 43967;

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.LogFormat("Reset arm position");
        StartCoroutine(PushResetCommand(LeftArmId));
        StartCoroutine(PushResetCommand(RightArmId));
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);

    }

    IEnumerator PushResetCommand(int armId)
    {
        //byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
        // TODO: 

        var timestamp = DateTime.Now.Ticks;
        var requestURL = string.Format("http://{0}/api/arm/{1}/go/0/0/0/{2}", ServiceEndPoint, armId, timestamp);
        var www = UnityWebRequest.Put(requestURL, new byte[] { 0 });
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
