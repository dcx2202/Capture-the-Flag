using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRunners.GoalOrientedBehaviour.Scripts.GUI
{
    /// <summary>
    /// Controlls the messages displays
    /// </summary>
    public class MessageManager : MonoBehaviour
    {
        #region singleton
        /// <summary>
        /// Singleton reference
        /// </summary>
        public static MessageManager Instance;
        /// <summary>
        /// Singleton checker
        /// </summary>
        private static bool _alreadyExist;

        private void Awake()
        {
            if (_alreadyExist)
            {
                Destroy(gameObject);
                return;
            }

            _alreadyExist = true;
            Instance = this;
        }
        #endregion

        /// <summary>
        /// The text field for the message
        /// </summary>
        [Tooltip("The text field for the message")]
        public Text MessageText;
        /// <summary>
        /// The last message displayed
        /// </summary>
        private Coroutine _currentMessageCoroutine;

        /// <summary>
        /// Displays the message received in the parameters. The message will fadeout after 3 seconds.
        /// </summary>
        /// <param name="message"></param>
        public void DisplayMessage(string message)
        {
            // if there is a message already being displayed, we first need to stop it
            if(_currentMessageCoroutine != null)
                StopCoroutine(_currentMessageCoroutine);

            // start the coroutine that will fade the message
            _currentMessageCoroutine = StartCoroutine(ShowMessage(message));
        }

        private IEnumerator ShowMessage(string message)
        {
            // setupt the colours
            var colorTransparent = new Color(MessageText.color.r, MessageText.color.g, MessageText.color.b, 0); // no alpha
            var colorOpaque = new Color(MessageText.color.r, MessageText.color.g, MessageText.color.b, 1); // max alpha
            MessageText.color = colorOpaque;
            // add the message text
            MessageText.text = message;

            // wait before starting to fade out
            yield return new WaitForSeconds(1f);

            //fade out time
            var timer = 2f;

            // slowly fade out
            while (timer > 0)
            {
                MessageText.color = Color.Lerp(colorTransparent, colorOpaque, timer/2f);
                timer -= Time.deltaTime;
                yield return null;
            }
        }
    }
}
