using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public float speed = 5;
    [SerializeField] Rigidbody RigidbodyRef;
    [SerializeField] Transform CameraRef;
    [SerializeField] Transform GroundChecker;
    [SerializeField] LayerMask GroundLayerMask;
    bool _grounded = true;
    float _jumpCDTimer = 0f;
    float _speed;

    void Update()
    {
        if (!isLocalPlayer)
        {
            CameraRef.gameObject.SetActive(false);
            Destroy(RigidbodyRef);
            return;
        }

        if (!_grounded)            
        {
            _jumpCDTimer += Time.deltaTime;
            if (Physics.Raycast(GroundChecker.transform.position, new Vector3(0, -1, 0), 0.4f, GroundLayerMask) && _jumpCDTimer > 0.2f)
            {
                _grounded = true;
            }
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float horizontal = Input.GetAxis("Mouse X") * .03f;
        float vertical = Input.GetAxis("Mouse Y") * -3;

        Quaternion rot = transform.rotation * new Quaternion(0, horizontal, 0, 1);
        transform.localRotation = rot;
        Vector3 dir = v * transform.forward + h * transform.right;
        _speed = speed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _speed *= 2;
        }

        Vector3 velocity = (dir.normalized * _speed);
        if (Input.GetKey(KeyCode.Space) && _grounded)
        {
            RigidbodyRef.AddForce(new Vector3(0, 350f, 0), ForceMode.Impulse);
            _grounded = false;
            _jumpCDTimer = 0f;
        }

        velocity.y = RigidbodyRef.velocity.y;
        RigidbodyRef.velocity = velocity;

        CameraRef.Rotate(new Vector3(1,0,0), vertical);
        float vertRot = CameraRef.localRotation.x;
        vertRot = Mathf.Clamp(vertRot, -0.35f, 0.35f);
        Quaternion cameraRot = CameraRef.localRotation;
        cameraRot.x = vertRot;
        CameraRef.localRotation = cameraRot;
    }
}
