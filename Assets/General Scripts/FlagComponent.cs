using System.Collections;
using General_Scripts.Enums;
using General_Scripts.Labourers;
using UnityEngine;
using UnityEngine.UI;

namespace General_Scripts
{
    public class FlagComponent : WorkComponent
    {
        public Text TeamAScore;
        public Text TeamBScore;

        public Runner Carrier;
        private Rigidbody _rb;
        private float _pulse = 300f;
        private bool _beingCarried;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }
        public void PickUp(Runner runner)
        {
            Carrier = runner;
            //print("Assigned runner: " + Carrier.name);
            //transform.SetParent(Carrier.transform);
            _rb.useGravity = false;
            StartCoroutine(BeingCarried());
        }

        public void Drop()
        {
            if (Carrier.MyTeam == Teams.A)
                TeamAScore.text = (int.Parse(TeamAScore.text) + 1).ToString();
            else
                TeamBScore.text = (int.Parse(TeamBScore.text) + 1).ToString();
            StopAllCoroutines();
            StartCoroutine(Reset(true));
        }

        public void ThrowFlag(Vector3 direction)
        {
            _rb.AddForce(direction.normalized * _pulse);
            StopAllCoroutines();
            StartCoroutine(Reset());
        }

        private IEnumerator Reset(bool reset = false)
        {
            transform.parent = null;
            
            if(Carrier == null)
                yield break;

            _rb.useGravity = true;
            _beingCarried = false;
            //print("removed Runner: " + Carrier.name);
            Carrier = null;

            yield return new WaitForSeconds(1f);

            if (reset)
                transform.position = Vector3.up;

            RemoveAllWorkers();

            yield return new WaitForSeconds(10f);

            //if after 10 seconds no1 got it, return to middle

            transform.position = Vector3.up;
        }

        private IEnumerator BeingCarried()
        {
            yield return null; // wait for next frame
            _beingCarried = true;
            while (_beingCarried)
            {
                _rb.position = Carrier.transform.position + (Carrier.transform.forward * 1f) + Carrier.transform.up * .7f;

                yield return null; // wait for next frame
            }
        }
    }
}