using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum RoomType
{
    BattleRoom,
    NormalRoom,
    EncounterRoom,
    TitleRoom
}

public class RoomManager : MonoBehaviour
{
    private static RoomManager _instants;
    public TitleRoom titleRoom;
    public List<BattleRoom> battleRoomsPool;
    public List<NormalRoom> normalRoomsPool;
    public List<EncounterRoom> encounterRoomsPool;
    private List<Door> _doors = new();
    private bool _readyNextRoom;
    private int _currentFloor;
    private int _currentStep;
    public GameObject roomRoot;
    public GameObject readRoomPrefab;
    public Vector3 rollRoomOffset;
    public Vector3 fadeRoomOffset;
    public GameObject roomMask;
    public GameObject roomMaskPos;

    private RoomType _currentRoomType;

    private List<RoomAsset> _nextRoom = new ();
    
    private void Awake()
    {
        if (_instants == null)
        {
            _instants = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        ToTitleRoom();
    }
    public void ToTitleRoom()
    {
        _currentFloor = 0;
        var rr = Instantiate(titleRoom.roomPrefab, roomRoot.transform);
        _currentRoomType = RoomType.TitleRoom;
        _instants.RoomSwitchFade(rr);
    }
    public void SpawnNextRoom()
    {
        _nextRoom.Clear();
        var roomCount = Random.Range(1, 4);
        for (int i = 0; i < roomCount; i++)
        {
            switch (Random.Range(0,2))
            {
                case 0:
                    _nextRoom.Add(battleRoomsPool[Random.Range(0,battleRoomsPool.Count)]);
                    break;
                case 1:
                    _nextRoom.Add(encounterRoomsPool[Random.Range(0,battleRoomsPool.Count)]);
                    break;
            }
        }
    }
    public void StartGame()
    {
        leking.UIManager.HideTitleUI();
        var room = battleRoomsPool[Random.Range(0, battleRoomsPool.Count)];
        foreach (var monster in room.monsters)
        {
            BattleManager.AddMonster(monster);
        }
        var vr= Instantiate(room.roomPrefab,_instants.roomRoot.transform);
        onSwitched = BattleManager.StartBattle;
        RoomSwitchFade(vr);
    }
    private Action onSwitched = ()=>{};
    public void InitRoom()
    {
        BattleManager.KillAllMonster();
    }
    public void ToRoom(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.BattleRoom:
                break;
            case RoomType.NormalRoom:
                break;
            case RoomType.EncounterRoom:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(roomType), roomType, null);
        }
    }
    public static void OnBattleCompletion()
    {
        leking.UIManager.ShowNextRoomButton();
    }
    public static void ToReadyRoom()
    {
        leking.UIManager.HideNextRoomButton();
        var rr = Instantiate(_instants.readRoomPrefab,_instants.roomRoot.transform);
        _instants.RoomSwitchRoll(rr);
    }
    private GameObject _currentRoomObject;
    private GameObject _nextRoomObject;
    private void RoomSwitchRoll(GameObject targetRoom)
    {
        if(targetRoom == null) return;
        if (_currentRoomObject == null)
        {
            _currentRoomObject = targetRoom;
            return;
        }
        _nextRoomObject = targetRoom;
        StartCoroutine(nameof(RoomSwitchRollCoroutine));
    }
    private void RoomSwitchFade(GameObject targetRoom)
    {
        if(targetRoom == null) return;
        if (_currentRoomObject == null)
        {
            _currentRoomObject = targetRoom;
            return;
        }
        _nextRoomObject = targetRoom;
        StartCoroutine(nameof(RoomSwitchFadeCoroutine));
    }
    private IEnumerator RoomSwitchFadeCoroutine()
    {
        leking.UIManager.HideCanvas();
        _nextRoomObject.transform.position += fadeRoomOffset;
        for (float i = 0; i <= 1; i += 0.01f)
        {
            roomMask.transform.position =
                Vector3.Lerp(roomMaskPos.transform.position, roomRoot.transform.position, Mathf.Sqrt(i));
            yield return new WaitForSeconds(1 / 100f);
        }

        _nextRoomObject.transform.position = roomRoot.transform.position;
        Destroy(_currentRoomObject);
        onSwitched();
        _currentRoomObject = _nextRoomObject;
        for (float i = 0; i <= 1; i += 0.01f)
        {
            roomMask.transform.position =
                Vector3.Lerp(roomRoot.transform.position,roomMaskPos.transform.position , Mathf.Pow(i,2));
            yield return new WaitForSeconds(1 / 100f);
        }
        leking.UIManager.ShowCanvas();
    }
    private IEnumerator RoomSwitchRollCoroutine()
    {
        _nextRoomObject.transform.position -= rollRoomOffset;
        var position = roomRoot.transform.position;
        var targetPosNext = position;
        var targetPosCurr = position + rollRoomOffset;
        for (float i = 0; i <= 1; i += 0.001f)
        {
            _currentRoomObject.transform.position =
                Vector3.Lerp(_currentRoomObject.transform.position, targetPosCurr, i);
            _nextRoomObject.transform.position =
                Vector3.Lerp(_nextRoomObject.transform.position, targetPosNext, i);
            yield return new WaitForSeconds(1 / 100f);
        }
        Destroy(_currentRoomObject);
        _currentRoomObject = _nextRoomObject;
        onSwitched();
    }
}
