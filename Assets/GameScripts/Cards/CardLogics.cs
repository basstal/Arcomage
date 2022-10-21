using System;
using Unity.VisualScripting;
using UnityEngine;

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
                        // target.BricksAddEffect:Play()
                    }
                    else
                    {
                        // target.REF.BricksDropEffect:Play()
                    }

                    break;
                }
                case CostType.Gem:
                {
                    target.gem = Math.Max(target.gem + changeAmount, 0);
                    // if inCostAmount > 0 then
                    // target.REF.GemsAddEffect:Play()
                    // else
                    // target.REF.GemsDropEffect:Play()
                    // end
                    break;
                }
                case CostType.Recruit:
                {
                    target.recruit = Math.Max(target.recruit + changeAmount, 0);
                    if (changeAmount > 0)
                    {
                        // target.REF.RecruitsAddEffect:Play()
                    }
                    else
                    {
                        // target.REF.RecruitsDropEffect:Play()
                    }

                    break;
                }
            }
        }

        public static void BuildingChange(GamePlayer target, BuildingType buildingType, int changeAmount)
        {
            switch (buildingType)
            {
                case BuildingType.Wall:
                {
                    target.wall = target.wall + changeAmount;
                    // if change > 0 then
                    // player.REF.WallAddEffect:Play()
                    // else
                    // player.REF.WallDropEffect:Play()
                    // end
                    break;
                }
                case BuildingType.Tower:
                {
                    target.tower = target.tower + changeAmount;
                    // if change > 0 then
                    // player.REF.TowerAddEffect:Play()
                    // else
                    // player.REF.TowerDropEffect:Play()
                    // end
                    break;
                }
            }
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
    }

    public class ResChange : Unit
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger; //Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inPlayer; // Adding the ValueInput variable for myValueA

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inCostType; // Adding the ValueInput variable for myValueB

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inCostAmount; // Adding the ValueInput variable for myValueB

        [DoNotSerialize] // No need to serialize ports
        public ValueOutput result; // Adding the ValueOutput variable for result

        private string resultValue; // Adding the string variable for the processed result value

        protected override void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("inputTrigger", (flow) =>
            {
                //Making the resultValue equal to the input value from myValueA concatenating it with myValueB.
                int costAmount = flow.GetValue<int>(inCostAmount);
                CostType costType = flow.GetValue<CostType>(inCostType);
                GamePlayer player = flow.GetValue<GamePlayer>(inPlayer);
                resultValue = $"{costType} - {costAmount}!!!";
                if (costAmount == 0)
                {
                    return outputTrigger;
                }

                SharedLogics.ResChange(player, costType, costAmount);


                resultValue = $"Player{player.playerID}/Refresh";
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<GamePlayer>("player", default);
            //Making the myValueB input value port visible, setting the port label name to myValueB and setting its default value to an empty string.
            inCostType = ValueInput<CostType>("costType", CostType.None);
            inCostAmount = ValueInput<int>("costAmount", 0);

            result = ValueOutput<string>("result", (flow) => resultValue);
        }
    }

    public class BuildingChange : Unit
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger; //Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inPlayer; // Adding the ValueInput variable for myValueA

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inBuilding; // Adding the ValueInput variable for myValueB

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inChangeAmount; // Adding the ValueInput variable for myValueB

        [DoNotSerialize] // No need to serialize ports
        public ValueOutput result; // Adding the ValueOutput variable for result

        private string resultValue; // Adding the string variable for the processed result value

        protected override void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("inputTrigger", (flow) =>
            {
                //Making the resultValue equal to the input value from myValueA concatenating it with myValueB.
                int changeAmount = flow.GetValue<int>(inChangeAmount);
                BuildingType buildingType = flow.GetValue<BuildingType>(inBuilding);
                GamePlayer player = flow.GetValue<GamePlayer>(inPlayer);
                resultValue = $"{buildingType} - {changeAmount}!!!";
                if (changeAmount == 0)
                {
                    return outputTrigger;
                }

                SharedLogics.BuildingChange(player, buildingType, changeAmount);

                resultValue = $"Player{player.playerID}/Refresh";
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<GamePlayer>("player", default);
            //Making the myValueB input value port visible, setting the port label name to myValueB and setting its default value to an empty string.
            inBuilding = ValueInput<BuildingType>("building", BuildingType.None);
            inChangeAmount = ValueInput<int>("changeAmount", 0);

            result = ValueOutput<string>("result", (flow) => resultValue);
        }
    }

    public class GrowthChange : Unit
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger; //Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inPlayer; // Adding the ValueInput variable for myValueA

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inCostType; // Adding the ValueInput variable for myValueB

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inChangeAmount; // Adding the ValueInput variable for myValueB

        [DoNotSerialize] // No need to serialize ports
        public ValueOutput result; // Adding the ValueOutput variable for result

        private string resultValue; // Adding the string variable for the processed result value

        protected override void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("inputTrigger", (flow) =>
            {
                //Making the resultValue equal to the input value from myValueA concatenating it with myValueB.
                int changeAmount = flow.GetValue<int>(inChangeAmount);
                CostType buildingType = flow.GetValue<CostType>(inCostType);
                GamePlayer player = flow.GetValue<GamePlayer>(inPlayer);
                resultValue = $"{buildingType} - {changeAmount}!!!";
                if (changeAmount == 0)
                {
                    return outputTrigger;
                }

                switch (buildingType)
                {
                    case CostType.Brick:
                    {
                        player.brickIncRate = player.brickIncRate + changeAmount;
//         if change > 0 then
//             player.REF.BricksIncRateAnimation:DORestartById("startInc")
//         else
//             player.REF.BricksDecRateAnimation:DORestartById("startDec")
//         end
                        break;
                    }
                    case CostType.Gem:
                    {
                        player.gemIncRate = player.gemIncRate + changeAmount;
//         if change > 0 then
//             player.REF.GemsIncRateAnimation:DORestartById("startInc")
//         else
//             player.REF.GemsDecRateAnimation:DORestartById("startDec")
//         end
                        break;
                    }
                    case CostType.Recruit:
                    {
                        player.recruitIncRate = player.recruitIncRate + changeAmount;
//         if change > 0 then
//             player.REF.RecruitsIncRateAnimation:DORestartById("startInc")
//         else
//             player.REF.RecruitsDecRateAnimation:DORestartById("startDec")
//         end
                        break;
                    }
                }
                //     local player = player.player

                resultValue = $"Player{player.playerID}/Refresh";
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<GamePlayer>("player", default);
            //Making the myValueB input value port visible, setting the port label name to myValueB and setting its default value to an empty string.
            inCostType = ValueInput<CostType>("building", CostType.None);
            inChangeAmount = ValueInput<int>("changeAmount", 0);

            result = ValueOutput<string>("result", (flow) => resultValue);
        }
    }


    public class CauseDamage : Unit
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger; //Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inPlayer; // Adding the ValueInput variable for myValueA

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inDamage; // Adding the ValueInput variable for myValueB

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inDirect; // Adding the ValueInput variable for myValueB

        [DoNotSerialize] // No need to serialize ports
        public ValueOutput result; // Adding the ValueOutput variable for result

        private string resultValue; // Adding the string variable for the processed result value

        protected override void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("inputTrigger", (flow) =>
            {
                //Making the resultValue equal to the input value from myValueA concatenating it with myValueB.
                bool direct = flow.GetValue<bool>(inDirect);
                int damage = flow.GetValue<int>(inDamage);
                GamePlayer player = flow.GetValue<GamePlayer>(inPlayer);
                GamePlayer enemy = GameMain.FindEnemyById(player.playerID);
                var wall = player.wall;
                // ** 直接对塔楼的伤害
                if (direct)
                {
                    SharedLogics.BuildingChange(enemy, BuildingType.Tower, -damage);
                }
                else if (damage > wall)
                {
                    // **如果伤害大于城墙，溢出的伤害由塔楼承受
                    SharedLogics.BuildingChange(enemy, BuildingType.Wall, -wall);
                    SharedLogics.BuildingChange(enemy, BuildingType.Tower, -(damage - wall));
                }

                else
                {
                    // **若干伤害小于等于城墙，则城墙承受全部伤害
                    SharedLogics.BuildingChange(enemy, BuildingType.Wall, -damage);
                }

                resultValue = $"Player{player.playerID}/Refresh";
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<GamePlayer>("player", default);
            //Making the myValueB input value port visible, setting the port label name to myValueB and setting its default value to an empty string.
            inDamage = ValueInput<int>("damage", 0);
            inDirect = ValueInput("direct", false);

            result = ValueOutput<string>("result", (flow) => resultValue);
        }
    }


    public class GetEnemyPlayer : Unit
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger; //Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inPlayer; // Adding the ValueInput variable for myValueA


        [DoNotSerialize] // No need to serialize ports
        public ValueOutput result; // Adding the ValueOutput variable for result

        private GamePlayer resultValue; // Adding the string variable for the processed result value

        protected override void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("inputTrigger", (flow) =>
            {
                //Making the resultValue equal to the input value from myValueA concatenating it with myValueB.
                GamePlayer player = flow.GetValue<GamePlayer>(inPlayer);
                resultValue = GameMain.FindEnemyById(player.playerID);
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<GamePlayer>("player", default);

            result = ValueOutput<GamePlayer>("result", (flow) => resultValue);
        }
    }


    public class HandleCost : Unit
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger; //Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inPlayer; // Adding the ValueInput variable for myValueA

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inCostType; // Adding the ValueInput variable for myValueB

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inCostAmount; // Adding the ValueInput variable for myValueB

        [DoNotSerialize] // No need to serialize ports
        public ValueOutput result; // Adding the ValueOutput variable for result

        private int resultValue; // Adding the string variable for the processed result value

        protected override void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("inputTrigger", (flow) =>
            {
                //Making the resultValue equal to the input value from myValueA concatenating it with myValueB.
                int costAmount = flow.GetValue<int>(inCostAmount);
                CostType costType = flow.GetValue<CostType>(inCostType);
                GamePlayer player = flow.GetValue<GamePlayer>(inPlayer);
                resultValue = SharedLogics.HandleCost(player, costType, costAmount);
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<GamePlayer>("player", default);
            //Making the myValueB input value port visible, setting the port label name to myValueB and setting its default value to an empty string.
            inCostType = ValueInput<CostType>("costType", CostType.None);
            inCostAmount = ValueInput("costAmount", 0);

            result = ValueOutput<int>("result", (flow) => resultValue);
        }
    }

    public class PlayerResourceGrowth : Unit
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger; //Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inPlayer; // Adding the ValueInput variable for myValueA

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inCostType; // Adding the ValueInput variable for myValueB


        [DoNotSerialize] // No need to serialize ports
        public ValueOutput result; // Adding the ValueOutput variable for result

        private string resultValue; // Adding the string variable for the processed result value

        protected override void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("inputTrigger", (flow) =>
            {
                //Making the resultValue equal to the input value from myValueA concatenating it with myValueB.
                CostType costType = flow.GetValue<CostType>(inCostType);
                GamePlayer player = flow.GetValue<GamePlayer>(inPlayer);
                SharedLogics.PlayerResourceGrowth(player, costType);
                resultValue = $"Player{player.playerID}/Refresh";
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<GamePlayer>("player", default);
            //Making the myValueB input value port visible, setting the port label name to myValueB and setting its default value to an empty string.
            inCostType = ValueInput<CostType>("costType", CostType.None);

            result = ValueOutput<string>("result", (flow) => resultValue);
        }
    }

    public class PlayerResourceGrowthAll : Unit
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger; //Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inPlayer; // Adding the ValueInput variable for myValueA


        [DoNotSerialize] // No need to serialize ports
        public ValueOutput result; // Adding the ValueOutput variable for result

        private string resultValue; // Adding the string variable for the processed result value

        protected override void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("inputTrigger", (flow) =>
            {
                //Making the resultValue equal to the input value from myValueA concatenating it with myValueB.
                GamePlayer player = flow.GetValue<GamePlayer>(inPlayer);
                SharedLogics.PlayerResourceGrowthAll(player);
                resultValue = $"Player{player.playerID}/Refresh";
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<GamePlayer>("player", default);

            result = ValueOutput<string>("result", (flow) => resultValue);
        }
    }

    public class ValidCardCost : Unit
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger; //Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inPlayer; // Adding the ValueInput variable for myValueA

        [DoNotSerialize] // No need to serialize ports
        public ValueInput inCard; // Adding the ValueInput variable for myValueA

        [DoNotSerialize] // No need to serialize ports
        public ValueOutput result; // Adding the ValueOutput variable for result

        private bool resultValue; // Adding the string variable for the processed result value

        protected override void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("inputTrigger", (flow) =>
            {
                //Making the resultValue equal to the input value from myValueA concatenating it with myValueB.
                GamePlayer player = flow.GetValue<GamePlayer>(inPlayer);
                ArcomageCard card = flow.GetValue<ArcomageCard>(inCard);
                var left = SharedLogics.HandleCost(player, card.costType, card.cost);
                resultValue = left >= 0;
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<GamePlayer>("player", default);
            inCard = ValueInput<ArcomageCard>("card", default);
            result = ValueOutput<bool>("result", (flow) => resultValue);
        }
    }
}