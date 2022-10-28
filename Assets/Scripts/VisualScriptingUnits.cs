using System;
using Unity.VisualScripting;

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
                CostType costType = flow.GetValue<CostType>(inCostType);
                GamePlayer player = flow.GetValue<GamePlayer>(inPlayer);
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
                GamePlayer enemy = player.GameCombat.FindEnemyById(player.playerID);
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
                resultValue = player.GameCombat.FindEnemyById(player.playerID);
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