using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPoint : MonoBehaviour
{
    /// <summary>
    /// スコアの獲得はPlayerPointのみで行う予定
    /// </summary>
    public int Point
    {
        get { return _point; }
    }
    private int _point = 100;
    /// <summary>
    /// 初回水洗いフラグ
    /// </summary>
    bool _isFirstWash = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// 初めて水洗い
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private void FirstWash()
    {
        _point += 50;
        _isFirstWash = false;
    }
    /// <summary>
    /// 調味料に触れる
    /// </summary>
    private void TouchSeasoning()
    {
        _point *= 2;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water" && _isFirstWash)
        {
            FirstWash();
        }
        /// とりあえず調味料はトリガーで
        else if (other.tag == "Seasoning")
        {
            TouchSeasoning();
        }
    }

}
