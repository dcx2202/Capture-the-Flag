using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRunners.GoalOrientedBehaviour.Scripts.GUI
{
    /// <summary>
    /// Displays the cooldown counter near the spawn buttons
    /// </summary>
    public class SpawningCooldownGui : MonoBehaviour
    {
        /// <summary>
        /// The cooldown text field
        /// </summary>
        [Tooltip("The cooldown text field")]
        public Text CooldownText;
        /// <summary>
        /// The target button. It will be disabled while the cooldown is greater than zero.
        /// </summary>
        [Tooltip("The target button. It will be disabled while the cooldown is greater than zero.")]
        public Button Button;
        /// <summary>
        /// True while we are in cooldown
        /// </summary>
        public bool OnCooldown;
        /// <summary>
        /// Cooldown period
        /// </summary>
        public float CooldownTime;

        /// <summary>
        /// Start the cooldown
        /// </summary>
        public void Activate()
        {
            OnCooldown = true;
            StartCoroutine(Cooldown());
        }

        /// <summary>
        /// Updates the gui while the cooldown is active
        /// </summary>
        /// <returns></returns>
        private IEnumerator Cooldown()
        {
            Button.interactable = false;
            var timer = CooldownTime;
            var originalColor = CooldownText.color;
            CooldownText.color = Color.red;
            while (OnCooldown)
            {
                

                timer -= Time.deltaTime;
                CooldownText.text = timer.ToString("F1");
                if (timer <= 0)
                {
                    OnCooldown = false;
                    Button.interactable = true;
                    CooldownText.text = "0";
                    CooldownText.color = originalColor;
                    yield break;
                }
                yield return null;
            }
        }
    }
}
