using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace General_Scripts
{
    public class Timer : MonoBehaviour
    {
        public Text TimerText;


        private void Awake()
        {
            StartCoroutine(StartTimer());
        }

        private IEnumerator StartTimer()
        {
            var time = 0;
            while (true)
            {
                TimerText.text = (time / 60).ToString("D2") + ":" + (time % 60).ToString("D2");
                time++;
                yield return new WaitForSeconds(1);
            }
        }
    }
}