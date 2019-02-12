using General_Scripts.AI.GOAP;
using UnityEngine;

namespace GUIManager
{
    public class AnimationManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _weaponHolder;
        //[SerializeField]
        //private GameObject _cargoHolder;

        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private LabourerType _laborouerType;

        private GoapAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<GoapAgent>();
        }

        // Use this for initialization
        private void Start ()
        {
            //if(_cargoHolder != null)
            //    _cargoHolder.SetActive(false);
        }
	

        public GameObject GetWeapon()
        {
            return _weaponHolder;
        }

        public void LooseWeapon()
        {
            _weaponHolder.SetActive(false);
        }

        public void GainWeapon()
        {
            _weaponHolder.SetActive(true);
        }

        public void GoIdle()
        {
            if (_animator.GetBool("GoIdle")) return;

            StopMoving();
            StopWorking();

            _animator.SetBool("GoIdle", true);
        }


        public void Move()
        {
            var nextAction = _agent.GetCurrentAction();
            //if (nextAction != null && nextAction.ActionName == Actions.DropOffOre || nextAction.ActionName == Actions.DropOffLogs)
            //    MoveWithCargo();
            //else
            MoveWithoutCargo();
        }

        private void MoveWithoutCargo()
        {
            if (_animator.GetBool("Move")) return;

            StopWorking();
            StopIdling();

            //if (_laborouerType == LabourerType.Miner || _laborouerType == LabourerType.Logger)
            //    _cargoHolder.SetActive(false);

            _animator.SetBool("Move", true);
        }


        public void MoveWithCargo()
        {
            if (_animator.GetBool("Move")) return;

            StopWorking();
            StopIdling();

            //if (_laborouerType == LabourerType.Miner || _laborouerType == LabourerType.Logger)
            //    _cargoHolder.SetActive(true);

            _animator.SetBool("Move", true);
        }


        public void Work()
        {
            if (_animator.GetBool("Mine") || _animator.GetBool("Chop")) return;
            StopMoving();
            StopIdling();

            if (_laborouerType == LabourerType.Blacksmither || _laborouerType == LabourerType.Miner)
                _animator.SetBool("Mine", true);
            else
                _animator.SetBool("Chop", true);
        }

        public void StopMoving()
        {
            _animator.SetBool("Move", false);
        }
        private void StopWorking()
        {
            _animator.SetBool("Chop", false);
            _animator.SetBool("Mine", false);
        }
        private void StopIdling()
        {
            _animator.SetBool("GoIdle", false);
        }
    }
}
