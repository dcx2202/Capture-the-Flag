using UnityEngine;

namespace Utilities
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        internal GameObject Camera;

        [SerializeField]
        private float _speed;
        [SerializeField]
        private float _zoomSpeed;
        [SerializeField]
        private float _dragSpeed;

        [SerializeField]
        private float _minZoomY;
        [SerializeField]
        private float _maxZoomY;
        
        void Update()
        {
            MoveCamera();
        }


        private void MoveCamera()
        {
            var scroll = Input.GetAxisRaw("Mouse ScrollWheel"); // zoom

            if (scroll != 0)
            {
                var newZoom = Camera.transform.localPosition + Camera.transform.forward * _speed * scroll;

                if ((newZoom.y < _minZoomY || newZoom.y > _maxZoomY) == false)
                    Camera.transform.localPosition = newZoom;              
            }

            if (Input.GetKey(KeyCode.Mouse3))
                transform.position = Vector3.Lerp(transform.position,
                    new Vector3(transform.position.x, transform.position.y - _zoomSpeed / 10, transform.position.z), Time.deltaTime * _zoomSpeed);
            if (Input.GetKey(KeyCode.Mouse4))
                transform.position = Vector3.Lerp(transform.position,
                    new Vector3(transform.position.x, transform.position.y + _zoomSpeed / 10, transform.position.z), Time.deltaTime * _zoomSpeed);
            
            if (Input.GetKey(KeyCode.Mouse2))
            {
                //print("Draging");
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                transform.position = new Vector3(-Input.GetAxis("Mouse X") * _dragSpeed, 0, -Input.GetAxis("Mouse Y") * _dragSpeed) + transform.position;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
