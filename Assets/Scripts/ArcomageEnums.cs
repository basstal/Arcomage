namespace GameScripts
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public enum CostType
    {
        None,
        Brick,
        Recruit,
        Gem,
    }

    /// <summary>
    /// 建筑类型
    /// </summary>
    public enum BuildingType
    {
        None,
        Wall,
        Tower,
    }

    /// <summary>
    /// 本地化
    /// </summary>
    public enum Localization
    {
        EN,
        CN
    }

    /// <summary>
    /// 难度
    /// </summary>
    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }
}