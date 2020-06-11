using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCoroutine : MonoBehaviour
{
    IEnumerator someCorotine;
    // Start is called before the first frame update
    void Start()
    {
        someCorotine = SomeCoroutine();
        StartCoroutine(someCorotine);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopCoroutine(someCorotine);
            someCorotine = null;
            Debug.Log("stop");
        }   
    }

    private IEnumerator SomeCoroutine()
    {
        Debug.Log("start");
        yield return new WaitForSeconds(5f);
        Debug.Log("over");
        yield break;
    }
}
