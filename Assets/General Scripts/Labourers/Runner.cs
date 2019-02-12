using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using General_Scripts.Enums;
using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace General_Scripts.Labourers
{
    public class Runner : Labourer
    {
        public Teams MyTeam;
        public List<string> Goals;

        private SteeringBasics _steering;
        private Stopwatch _stopwatch;

        protected override void Awake()
        {
            base.Awake();
            Goals = new List<string>{"dropFlag"};
            _steering = GetComponent<SteeringBasics>();
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        /// <summary>
        /// Our only goal will ever be to mine ore. The MineOreAction will be able to fulfill this goal.
        /// </summary>
        /// <returns></returns>
        public override HashSet<KeyValuePair<string,object>> CreateGoalState ()
        {
            var goal = new HashSet<KeyValuePair<string, object>>();
            foreach (var goalString in Goals)
            {
                goal.Add(new KeyValuePair<string, object>(goalString, true));
            }

            return goal;
        }

        private void Update()
        {
            if (_stopwatch.Elapsed.TotalSeconds > .5f)
            {
                SlowDown();
                RecoverSpeed();
                _stopwatch.Reset();
                _stopwatch.Start();
            }
        }

        public void SlowDown()
        {
            if (Backpack.HasFlag == false) return;

            _steering.MaxVelocity -= .5f;
            if (_steering.MaxVelocity < 0)
            {
                _steering.MaxVelocity = 0;
            }
        }

        public void RecoverSpeed()
        {
            if (Backpack.HasFlag) return;

            _steering.MaxVelocity += .25f;
            if (_steering.MaxVelocity > 5)
            {
                _steering.MaxVelocity = 5;
            }
        }
    }
}

