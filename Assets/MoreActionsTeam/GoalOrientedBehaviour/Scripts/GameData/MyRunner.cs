//using System.Collections.Generic;
//using System.Diagnostics;
//using General_Scripts.Enums;
//using General_Scripts.Labourers;
//using SteeringBehaviours.Scripts.Basics;

//namespace ThunderRunners.GoalOrientedBehaviour.Scripts.GameData
//{
//    public class MyRunner : Labourer
//    {
//        public TeamManager MyTeamManager;

//        public Teams MyTeam;
//        public List<string> Goals;

//        private SteeringBasics _steering;
//        private Stopwatch _stopwatch;

//        protected override void Awake()
//        {
//            base.Awake();
//            Goals = new List<string> { "dropFlag" };
//            _steering = GetComponent<SteeringBasics>();
//            _stopwatch = new Stopwatch();
//            _stopwatch.Start();
//        }

//        /// <summary>
//        /// Our only goal will ever be to mine ore. The MineOreAction will be able to fulfill this goal.
//        /// </summary>
//        /// <returns></returns>
//        public override HashSet<KeyValuePair<string, object>> CreateGoalState()
//        {
//            var goal = new HashSet<KeyValuePair<string, object>>();
//            foreach (var goalString in Goals)
//            {
//                goal.Add(new KeyValuePair<string, object>(goalString, true));
//            }

//            return goal;
//        }

//        private void Update()
//        {
//            if (_stopwatch.Elapsed.TotalSeconds > 1)
//            {
//                SlowDown();
//                RecoverSpeed();
//                _stopwatch.Reset();
//                _stopwatch.Start();
//            }
//        }

//        public void SlowDown()
//        {
//            if (Backpack.HasFlag == false) return;

//            _steering.MaxVelocity -= .5f;
//            if (_steering.MaxVelocity < 0)
//            {
//                _steering.MaxVelocity = 0;
//            }
//        }

//        public void RecoverSpeed()
//        {
//            if (Backpack.HasFlag) return;

//            _steering.MaxVelocity += .5f;
//            if (_steering.MaxVelocity > 5)
//            {
//                _steering.MaxVelocity = 5;
//            }
//        }

//        public override HashSet<KeyValuePair<string, object>> GetWorldState()
//        {

//            var worldData = new HashSet<KeyValuePair<string, object>>
//            {
//                new KeyValuePair<string, object>("hasFlag", Backpack.HasFlag),
//                new KeyValuePair<string, object>("teamHasFlag", MyTeamManager.WeHaveFlag)
//                //new KeyValuePair<string, object>("hasFirewood", Backpack.NumFirewood > 0),

//                //new KeyValuePair<string, object>("hasLogs", Backpack.NumLogs > 0),
//                //new KeyValuePair<string, object>("hasTool", Backpack.Tool != null && Backpack.Tool.Destroyed() == false)
//            };

//            return worldData;
//        }
//    }
//}