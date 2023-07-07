using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public int roomIndex;

    public RoomType type;

    private SpriteRenderer _spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (type)
        {
            case RoomType.BattleRoom:
                _spriteRenderer.color = Color.magenta;
                break;
            case RoomType.NormalRoom:
                break;
            case RoomType.EncounterRoom:
                _spriteRenderer.color = Color.white;
                break;
        }
    }

    private void OnMouseDown()
    {
        RoomManager.NextRoom(roomIndex);
    }
}
