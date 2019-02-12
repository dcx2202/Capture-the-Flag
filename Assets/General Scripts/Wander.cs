using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace General_Scripts
{
    public class Wander : MonoBehaviour
    {
        public Bounds Bounds;

        private void Awake()
        {
            //StartCoroutine(Wandering());
        }

        private void Update()
        {
            
        }

        private IEnumerator Wandering()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var translate = Bounds.GetRandomPoint();

            while (true)
            {
                transform.position = Vector3.MoveTowards(transform.position, translate, .2f * Time.deltaTime);

                if (stopwatch.Elapsed.TotalSeconds > 10)
                {
                    translate = Bounds.GetRandomPoint();
                    stopwatch.Reset();
                    stopwatch.Start();
                }

                yield return null;
            }
        }

    }
}