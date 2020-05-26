namespace Cooking.Stage
{
    /// <summary>
    /// ステージシーンの中でのゲーム全体の進行状態 主にSceneManager用
    /// </summary>
    public enum StageGameState
    {
        Preparation,
        FinishFoodInstantiateAndPlayerInOrder,//プレイヤーの生成終了
        Play,
        Finish
    }
    /// <summary>
    /// 画面状態を定義 主にUIManager用
    /// </summary>
    public enum ScreenState
    {
        InitializeChoose,
        DecideOrder,
        Start,
        FrontMode,
        SideMode,
        LookDownMode,
        ShottingMode,
        Finish,
        Pause
    }
    /// <summary>
    /// ショットの状態を表すクラス 主にShotManager用
    /// </summary>
    public enum ShotState
    {
        WaitMode,/// 待機中
        AngleMode,/// 角度の決定中
        ShottingMode,/// ショット中
        ShotEndMode///ショット終了
    }
    /// <summary>
    /// 食材の種類 初期値はえびにする
    /// </summary>
    public enum FoodType
    {
        Shrimp,
        Egg,
        Chicken,
        Sausage
    }
    /// <summary>
    /// 限界値を格納する配列用
    /// </summary>
    public enum LimitValue
    {
        Min, Max
    }
}
