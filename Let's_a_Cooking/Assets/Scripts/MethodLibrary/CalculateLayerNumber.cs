using UnityEngine;
/// <summary>
/// 2進数レイヤーから10進数のレイヤー番号に変換
/// </summary>
public static class CalculateLayerNumber
{
    /// <summary>
    /// 1つのレイヤーマスクをレイヤー番号に変換
    /// </summary>
    /// <param name="layerMask"></param>
    /// <returns></returns>
    public static int ChangeSingleLayerNumberFromLayerMask(LayerMask layerMask)
    {
        if (layerMask <= 0)
        {
            return 0;
        }
        else
        {
            return (int)Mathf.Log(layerMask.value, 2);
        }
    }
}
