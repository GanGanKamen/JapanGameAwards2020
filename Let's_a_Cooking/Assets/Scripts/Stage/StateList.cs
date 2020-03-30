namespace Cooking.Stage
{
    /// <summary>
    /// ステージシーンの中でのゲーム全体の進行状態 主にSceneManager用
    /// </summary>
    public enum StageGameState
    {
        Preparation,
        Play,
        Finish
    }
    /// <summary>
    /// 画面状態を定義 主にUIManager用
    /// </summary>
    public enum ScreenState
    {
        ChooseFood,
        DecideOrder,
        Start,
        AngleMode,
        SideMode,
        LookDownMode,
        PowerMeterMode,
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
        PowerMeterMode,/// ショットパワー決定中
        ShottingMode,/// ショット中
        ShotEndMode///ショット終了
    }
}
