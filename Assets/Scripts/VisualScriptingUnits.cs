using System;
using Unity.VisualScripting;
using UnityEngine;

namespace GameScripts
{
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
                Player player = flow.GetValue<Player>(inPlayer);
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
            inPlayer = ValueInput<Player>("player", default);
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
                Player player = flow.GetValue<Player>(inPlayer);
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
            inPlayer = ValueInput<Player>("player", default);
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
                CostType costType = flow.GetValue<CostType>(inCostType);
                Player player = flow.GetValue<Player>(inPlayer);
                resultValue = $"{costType} - {changeAmount}!!!";
                if (changeAmount == 0)
                {
                    return outputTrigger;
                }

                SharedLogics.GrowthChange(player, costType, changeAmount);

                resultValue = $"Player{player.playerID}/Refresh";
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<Player>("player", default);
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
                Player player = flow.GetValue<Player>(inPlayer);
                Player enemy = player.combat.FindEnemyById(player.playerID);
                var wallOld = enemy.wall;
                var towerOld = enemy.tower;
                var towerDamage = damage;
                // ** 直接对塔楼的伤害
                if (direct)
                {
                    SharedLogics.BuildingChange(enemy, BuildingType.Tower, -damage);
                }
                else if (damage > wallOld)
                {
                    // **如果伤害大于城墙，溢出的伤害由塔楼承受
                    SharedLogics.BuildingChange(enemy, BuildingType.Wall, -wallOld);
                    towerDamage = damage - wallOld;
                    SharedLogics.BuildingChange(enemy, BuildingType.Tower, -towerDamage);
                }
                else
                {
                    // **若干伤害小于等于城墙，则城墙承受全部伤害
                    SharedLogics.BuildingChange(enemy, BuildingType.Wall, -damage);
                    towerDamage = 0;
                }

                try
                {
                    if (Combat.Database.learningGoal == MLAgentLearningGoal.DamageTower)
                    {
                        player.AddReward(Mathf.Min(1.0f, (float)towerDamage / towerOld));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"AddReward failed : {e.Message}\ntowerDamage : {towerDamage}, towerOld : {towerOld} ");
                }

                resultValue = $"Player{player.playerID}/Refresh";
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<Player>("player", default);
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

        private Player resultValue; // Adding the string variable for the processed result value

        protected override void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("inputTrigger", (flow) =>
            {
                //Making the resultValue equal to the input value from myValueA concatenating it with myValueB.
                Player player = flow.GetValue<Player>(inPlayer);
                resultValue = player.combat.FindEnemyById(player.playerID);
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<Player>("player", default);

            result = ValueOutput<Player>("result", (flow) => resultValue);
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
                Player player = flow.GetValue<Player>(inPlayer);
                resultValue = SharedLogics.HandleCost(player, costType, costAmount);
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<Player>("player", default);
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
                Player player = flow.GetValue<Player>(inPlayer);
                SharedLogics.PlayerResourceGrowth(player, costType);
                resultValue = $"Player{player.playerID}/Refresh";
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<Player>("player", default);
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
                Player player = flow.GetValue<Player>(inPlayer);
                SharedLogics.PlayerResourceGrowthAll(player);
                resultValue = $"Player{player.playerID}/Refresh";
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<Player>("player", default);

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
                Player player = flow.GetValue<Player>(inPlayer);
                ArcomageCard card = flow.GetValue<ArcomageCard>(inCard);
                var left = SharedLogics.HandleCost(player, card.costType, card.cost);
                resultValue = left >= 0;
                return outputTrigger;
            });
            outputTrigger = ControlOutput("outputTrigger");

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            inPlayer = ValueInput<Player>("player", default);
            inCard = ValueInput<ArcomageCard>("card", default);
            result = ValueOutput<bool>("result", (flow) => resultValue);
        }
    }
}