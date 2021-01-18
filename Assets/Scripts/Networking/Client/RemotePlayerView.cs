using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayerView : MonoBehaviour, IPlayerView
{
    [SerializeField] private TextMesh _name;
    RemotePlayer _player;   

    public static RemotePlayerView Create(RemotePlayerView prefab, RemotePlayer player)
    {
        var obj = Instantiate(prefab);
        obj._player = player;
        obj._name.text = player.Name;
        return obj;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _player.UpdatePosition(Time.deltaTime);

        transform.position = _player.Position;
        transform.rotation = _player.Rotation;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
