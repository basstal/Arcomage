using UnityEngine;

namespace GameScripts
{
    /// <summary>
    /// 难度数据，用来初始化玩家数据
    /// </summary>
    [CreateAssetMenu(fileName = "ArcomagePlayer", menuName = "ScriptableObjects/ArcomagePlayer", order = 1)]
    public class ArcomagePlayer : ScriptableObject
    {
        [Tooltip("拥有砖块数量")] public int brick;
        [Tooltip("拥有宝石数量")] public int gem;
        [Tooltip("拥有怪兽数量")] public int recruit;
        [Tooltip("砖块增长量")] public int brickIncRate = 1;
        [Tooltip("宝石增长量")] public int gemIncRate = 1;
        [Tooltip("怪兽增长量")] public int recruitIncRate = 1;
        [Tooltip("城堡数量")] public int tower = 1;
        [Tooltip("城墙数量")] public int wall = 1;

        /// <summary>
        /// 将<see cref="player"/>玩家当前的数据暂存起来供以后使用。
        /// </summary>
        /// <param name="player">需暂存数据的玩家</param>
        public void TakeSnapshot(Player player)
        {
            brick = player.brick;
            gem = player.gem;
            recruit = player.recruit;
            brickIncRate = player.brickIncRate;
            gemIncRate = player.gemIncRate;
            recruitIncRate = player.recruitIncRate;
            tower = player.tower;
            wall = player.wall;
        }

        /// <summary>
        /// 判断<see cref="player"/>玩家是否获得游戏胜利
        /// </summary>
        /// <param name="player">待判断的玩家</param>
        /// <returns>是否胜利</returns>
        public bool IsPlayerWin(Player player)
        {
            return player.brick > brick || player.gem > gem || player.recruit > recruit || player.tower > tower;
        }


        /// <summary>
        /// 判断<see cref="player"/>玩家是否游戏失败
        /// </summary>
        /// <param name="player">待判断的玩家</param>
        /// <returns>是否失败</returns>
        public bool IsPlayerLose(Player player)
        {
            return player.tower <= 0;
        }
    }
}