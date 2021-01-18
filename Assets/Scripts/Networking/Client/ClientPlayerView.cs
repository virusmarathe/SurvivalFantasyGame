using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPlayerView : MonoBehaviour, IPlayerView
{
    [SerializeField] private TextMesh _name;
    [SerializeField] Transform CameraRoot;
    private ClientPlayer _player;
    private Camera _mainCamera;

    public static ClientPlayerView Create(ClientPlayerView prefab, ClientPlayer player)
    {
        var obj = Instantiate(prefab);
        obj._player = player;
        obj._name.text = player.Name;
        obj._mainCamera = Camera.main;
        return obj;
    }

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera.transform.parent = CameraRoot;
        _mainCamera.transform.localPosition = Vector3.zero;
        _mainCamera.transform.localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        float xVal = Input.GetAxis("Horizontal");
        float yVal = Input.GetAxis("Vertical");
        float jump = Input.GetAxis("Jump");
        float horizontal = Input.GetAxis("Mouse X") * .03f;
        Quaternion rot = transform.rotation * new Quaternion(0,horizontal,0,1);
        _player.SetInput(xVal, yVal, jump, rot);

        float lerpT = NetworkClient.Instance.NetworkTimer.LerpAlpha;
        transform.position = Vector3.Lerp(_player.LastPosition, _player.Position, lerpT);
        //rot = Quaternion.Slerp(_player.LastRotation, _player.Rotation, lerpT);
        transform.rotation = rot;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
