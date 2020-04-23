using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// アクティブプレイヤーのプレイヤーポイント取得のため、ターンコントローラーを経由してポイント取得
/// </summary>
public class PlayerPoint : MonoBehaviour
{
    /// <summary>
    /// 初期ポイント + 獲得ポイント
    /// </summary>
    public int Point
    {
        get { return _getPoint + _firstPoint; }
    }
    /// <summary>
    /// 獲得ポイント
    /// </summary>
    private int _getPoint = 0;
    /// <summary>
    /// 初期ポイント
    /// </summary>
    private const int _firstPoint = 100;
    /// <summary>
    /// 初回フラグ
    /// </summary>
    bool _isFirstWash = true , _isFirstTowel = true;
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
        _getPoint += 50;
        _isFirstWash = false;
    }
    /// <summary>
    /// 初めてタオルに触れる
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private void FirstTowelTouch()
    {
        _getPoint += 50;
        _isFirstTowel = false;
    }
    /// <summary>
    /// 調味料に触れる
    /// </summary>
    private void TouchSeasoning()
    {
        _getPoint *= 2;
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
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Towel" && _isFirstTowel)
        {
            FirstTowelTouch();
        }
    }
}
