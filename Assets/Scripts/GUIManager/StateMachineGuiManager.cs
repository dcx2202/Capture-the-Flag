using General_Scripts.AI.GOAP;
using UnityEngine;
using UnityEngine.UI;

namespace GUIManager
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class StateMachineGuiManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _worldCanvas;
        [SerializeField]
        public Text StateLabel;

        private GoapAgent _goapAgent;
        private GameObject _laboroures;

        void Awake()
        {
            _laboroures = GameObject.Find("Labourers");
        }
        // Use this for initialization
        void Start ()
        {
            _goapAgent = GetComponent<GoapAgent>();
            _worldCanvas.SetActive(false);
            UpdateStateLabel();
        }

        void Update()
        {
            UpdateStateLabel();
        }
        
        void UpdateStateLabel ()
        {
            if (_goapAgent.CurrentState == "Planning")
                StateLabel.text = "Planning";
            else if(_goapAgent.CurrentState == "Moving")
                StateLabel.text = "Moving";
            else if (_goapAgent.CurrentState == "Acting")
                if(_goapAgent.GetCurrentActions().Count != 0)
                    StateLabel.text = _goapAgent.GetCurrentActions().Peek().ActionName.ToString();
        }

        void HideStateLabel()
        {
            _worldCanvas.SetActive(false);
        }


        void OnMouseDown()
        {
            _worldCanvas.SetActive(_worldCanvas.activeSelf == false);

            if (_worldCanvas.activeSelf == false) return;

            foreach (var stateMachineGui in _laboroures.GetComponentsInChildren<StateMachineGuiManager>())
            {
                if (stateMachineGui == this) continue;
                stateMachineGui.HideStateLabel();
            }
        }


    }
}
