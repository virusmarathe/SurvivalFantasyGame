using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPlayerView : MonoBehaviour, IPlayerView
{
    [SerializeField] private TextMesh _name;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
