using System;

namespace GameScripts
{
    public static class SharedLogics
    {
        public static void ResChange(Player target, CostType costType, int changeAmount)
        {
            switch (costType)
            {
                case CostType.Brick:
                {
                    target.brick = Math.Max(target.brick + changeAmount, 0);
                    break;
                }
                case CostType.Gem:
                {
                    target.gem = Math.Max(target.gem + changeAmount, 0);
                    break;
                }
                case CostType.Recruit:
                {
                    target.recruit = Math.Max(target.recruit + changeAmount, 0);
                    break;
                }
            }

            target.OnRefresh();
        }

        public static void BuildingChange(Player target, BuildingType buildingType, int changeAmount)
        {
            switch (buildingType)
            {
                case BuildingType.Wall:
                {
                    target.wall = Math.Max(target.wall + changeAmount, 0);
                    break;
                }
                case BuildingType.Tower:
                {
                    target.tower = Math.Max(target.tower + changeAmount, 0);
                    break;
                }
            }

            target.OnRefresh();
        }

        public static void PlayerResourceGrowth(Player target, CostType costType)
        {
            switch (costType)
            {
                case CostType.Brick:
                {
                    target.brick += target.brickIncRate;
                    break;
                }
                case CostType.Gem:
                {
                    target.recruit += target.recruitIncRate;
                    break;
                }
                case CostType.Recruit:
                {
                    target.gem += target.gemIncRate;
                    break;
                }
            }

            target.OnRefresh();
        }

        public static void PlayerResourceGrowthAll(Player target)
        {
            PlayerResourceGrowth(target, CostType.Brick);
            PlayerResourceGrowth(target, CostType.Gem);
            PlayerResourceGrowth(target, CostType.Recruit);
        }

        public static int HandleCost(Player target, CostType costType, int costAmount)
        {
            switch (costType)
            {
                case CostType.Brick:
                {
                    return target.brick - costAmount;
                }
                case CostType.Gem:
                {
                    return target.gem - costAmount;
                }
                case CostType.Recruit:
                {
                    return target.recruit - costAmount;
                }
            }

            throw new Exception();
        }

        public static void GrowthChange(Player target, CostType costType, int changeAmount)
        {
            switch (costType)
            {
                case CostType.Brick:
                {
                    target.brickIncRate = Math.Max(target.brickIncRate + changeAmount, 0);
                    break;
                }
                case CostType.Gem:
                {
                    target.gemIncRate = Math.Max(target.gemIncRate + changeAmount, 0);
                    break;
                }
                case CostType.Recruit:
                {
                    target.recruitIncRate = Math.Max(target.recruitIncRate + changeAmount, 0);
                    break;
                }
            }

            target.OnRefresh();
        }
    }
}