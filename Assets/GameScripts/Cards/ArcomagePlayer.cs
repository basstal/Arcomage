using UnityEngine;

namespace GameScripts
{
    [CreateAssetMenu(fileName = "ArcomagePlayer", menuName = "ScriptableObjects/ArcomagePlayer", order = 1)]
    public class ArcomagePlayer : ScriptableObject
    {
        public int brick = 0;
        public int gem = 0;
        public int recruit = 0;
        public int brickIncRate = 1;
        public int gemIncRate = 1;
        public int recruitIncRate = 1;

        public int tower = 1;
        public int wall = 1;

        public void TakeSnapshot(GamePlayer player)
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
    }
}