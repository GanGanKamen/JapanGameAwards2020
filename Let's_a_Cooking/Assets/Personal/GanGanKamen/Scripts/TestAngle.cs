using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAngle : MonoBehaviour
{
    [SerializeField] float angle;
    [SerializeField] float cos;
    [SerializeField] float sin;
    [SerializeField] float angle90;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cos = Mathf.Cos(angle * Mathf.Deg2Rad);
        sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        angle90 = angle % 90;
    }
}
