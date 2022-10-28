using System;
using DG.Tweening;

namespace GameScripts
{
    public static class SharedLogics
    {
        public static void ResChange(GamePlayer target, CostType costType, int changeAmount)
        {
            switch (costType)
            {
                case CostType.Brick:
                {
                    target.brick = Math.Max(target.brick + changeAmount, 0);
                    if (changeAmount > 0)
                    {
                        target.bricksAddEffect.Play();
                    }
                    else
                    {
                        target.bricksDropEffect.Play();
                    }

                    break;
                }
                case CostType.Gem:
                {
                    target.gem = Math.Max(target.gem + changeAmount, 0);
                    if (changeAmount > 0)
                    {
                        target.gemsAddEffect.Play();
                    }
                    else
                    {
                        target.gemsDropEffect.Play();
                    }

                    break;
                }
                case CostType.Recruit:
                {
                    target.recruit = Math.Max(target.recruit + changeAmount, 0);
                    if (changeAmount > 0)
                    {
                        target.recruitsAddEffect.Play();
                    }
                    else
                    {
                        target.recruitsDropEffect.Play();
                    }

                    break;
                }
            }

            target.OnRefresh();
        }

        public static void BuildingChange(GamePlayer target, BuildingType buildingType, int changeAmount)
        {
            switch (buildingType)
            {
                case BuildingType.Wall:
                {
                    target.wall = Math.Max(target.wall + changeAmount, 0);
                    if (changeAmount > 0)
                    {
                        target.wallAddEffect.Play();
                        // if (target.trainingMode)
                        // {
                        //     target.AddReward(.1f + (float)changeAmount / Math.Max(target.wall, 1));
                        // }
                    }
                    else
                    {
                        target.wallDropEffect.Play();
                        // if (target.wall == 0)
                        // {
                        //     target.AddReward(-0.2f);
                        // }
                    }

                    break;
                }
                case BuildingType.Tower:
                {
                    target.tower = Math.Max(target.tower + changeAmount, 0);
                    if (changeAmount > 0)
                    {
                        target.towerAddEffect.Play();
                        // if (target.trainingMode)
                        // {
                        //     target.AddReward(.1f + (float)changeAmount / Math.Max(target.tower, 1));
                        // }
                    }
                    else
                    {
                        target.towerDropEffect.Play();
                        // if (target.tower == 0)
                        // {
                        //     target.AddReward(-1.0f);
                        // }
                    }

                    break;
                }
            }

            target.OnRefresh();
        }

        public static void PlayerResourceGrowth(GamePlayer target, CostType costType)
        {
            switch (costType)
            {
                case CostType.Brick:
                {
                    target.brick = target.brick + target.brickIncRate;
                    break;
                }
                case CostType.Gem:
                {
                    target.recruit = target.recruit + target.recruitIncRate;
                    break;
                }
                case CostType.Recruit:
                {
                    target.gem = target.gem + target.gemIncRate;
                    break;
                }
            }

            target.OnRefresh();
        }

        public static void PlayerResourceGrowthAll(GamePlayer target)
        {
            PlayerResourceGrowth(target, CostType.Brick);
            PlayerResourceGrowth(target, CostType.Gem);
            PlayerResourceGrowth(target, CostType.Recruit);
        }

        public static int HandleCost(GamePlayer target, CostType costType, int costAmount)
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

        public static void GrowthChange(GamePlayer target, CostType costType, int changeAmount)
        {
            switch (costType)
            {
                case CostType.Brick:
                {
                    target.brickIncRate = target.brickIncRate + changeAmount;
                    if (changeAmount > 0)
                    {
                        // target.bricksIncRateAnimation.DORestartById("startInc");
                    }
                    else
                    {
                        // target.bricksDecRateAnimation.DORestartById("startDec");
                    }

                    break;
                }
                case CostType.Gem:
                {
                    target.gemIncRate = target.gemIncRate + changeAmount;
                    if (changeAmount > 0)
                    {
                        // target.bricksIncRateAnimation.DORestartById("startInc");
                    }
                    else
                    {
                        // target.bricksDecRateAnimation.DORestartById("startDec");
                    }

                    break;
                }
                case CostType.Recruit:
                {
                    target.recruitIncRate = target.recruitIncRate + changeAmount;
                    if (changeAmount > 0)
                    {
                        // target.bricksIncRateAnimation.DORestartById("startInc");
                    }
                    else
                    {
                        // target.bricksDecRateAnimation.DORestartById("startDec");
                    }

                    break;
                }
            }

            // if (changeAmount > 0 && target.trainingMode)
            // {
            //     target.AddReward(0.05f * changeAmount);
            // }
        }
    }
}