using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum RoomType
{
    BattleRoom,
    NormalRoom,
    EncounterRoom,
    TitleRoom,
    BossRoom,
}

public class RoomManager : MonoBehaviour
{
    private static RoomManager _instants;
    public TitleRoom titleRoom;
    public List<BattleRoom> battleRoomsPool;
    public List<NormalRoom> normalRoomsPool;
    public List<EncounterRoom> encounterRoomsPool;
    public List<BossRoom> bossRoomPool;
    private bool _readyNextRoom;
    private int _currentFloor;
    private int _currentStep;
    public GameObject roomRoot;
    public GameObject readRoomPrefab;
    public GameObject floorReadRoomPrefab;
    public GameObject endingRoomPrefab;
    public Vector3 rollRoomOffset;
    public Vector3 fadeRoomOffset;
    public Vector3 doorOffset;
    public GameObject roomMask;
    public GameObject roomMaskPos;
    public Door doorPrefab;

    public static bool IsSwitchRoom;
    private RoomType _currentRoomType;
    private RoomType _recordCurrentRoomType;

    private List<RoomAsset> _recordNextRooms = new ();
    private List<RoomAsset> _nextRooms = new ();
    private RoomAsset _currentRoomAsset;

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
    

    public static int GetStepNumber()
    {
        return _instants._currentStep;
    }
    public static int GetFloorNumber()
    {
        return _instants._currentFloor;
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
        foreach (var roomAsset in _nextRooms)
        {
            _recordNextRooms.Add(roomAsset);
        }
        _nextRooms.Clear();
        if (_currentStep == 5)
        {
            if (bossRoomPool.Count((room)=>room.floor == _currentFloor) > 0)
            {
                _nextRooms.Add(bossRoomPool.Where((room)=>room.floor == _currentFloor).ToList()[0]);
            }
            return;
        }
        if (_currentRoomType == RoomType.BossRoom)
        {
            var nextFloorRooms = battleRoomsPool.Where((room) => room.floor == _currentFloor + 1);
            if(!nextFloorRooms.Any()) return;
            _nextRooms.Add(nextFloorRooms.ToList()[0]);
        }
        var roomCount = Random.Range(2, 4);
        var battleFloorRoom = battleRoomsPool.Where((room) => room.floor == _currentFloor ||room.floor == 0).ToList();
        var encounterFloorRoom = encounterRoomsPool.Where((room) => room.floor == _currentFloor ||room.floor == 0).ToList();
        for (int i = 0; i < roomCount; i++)
        {
            switch (Random.Range(0,2))
            {
                case 0:
                    _nextRooms.Add(battleFloorRoom[Random.Range(0,battleFloorRoom.Count)]);
                    break;
                case 1:
                    _nextRooms.Add(encounterFloorRoom[Random.Range(0,encounterFloorRoom.Count)]);
                    break;
            }
        }
    }

    public void ToBattleRoom(BattleRoom battleRoom)
    {
        leking.UIManager.HideTitleUI();
        foreach (var monster in battleRoom.monsters)
        {
            BattleManager.AddMonster(monster);
        }
        var vr= Instantiate(battleRoom.roomPrefab,_instants.roomRoot.transform);
        onSwitched = BattleManager.StartBattle;
        RoomSwitchFade(vr);
    }
    public void ToEncounterRoom(EncounterRoom encounterRoom)
    {
        leking.UIManager.HideTitleUI();
        var vr= Instantiate(encounterRoom.roomPrefab,_instants.roomRoot.transform);
        //onSwitched = leking.UIManager.ShowNextRoomButton;
        RoomSwitchFade(vr);
    }
    public void ToBossRoom(BossRoom bossRoom)
    {
        leking.UIManager.HideTitleUI();
        foreach (var monster in bossRoom.monsters)
        {
            BattleManager.AddMonster(monster);
        }
        var vr= Instantiate(bossRoom.roomPrefab,_instants.roomRoot.transform);
        onSwitched = BattleManager.StartBattle;
        RoomSwitchFade(vr);
    }

    public static void NextFloor()
    {
        if(IsSwitchRoom) return;
        _instants._currentFloor += 1;
        _instants._currentStep = 1;
        NextRoom(0);
    }
    //前往下一个房间
    public static void NextRoom(int roomIndex)
    {
        if(IsSwitchRoom) return;
        if (roomIndex > _instants._nextRooms.Count-1) return;
        GameObject.FindWithTag("Player").GetComponent<Player>().RecordState();
        _instants._recordCurrentRoomType = _instants._currentRoomType;
        _instants._currentRoomAsset = _instants._nextRooms[roomIndex];
        SpellsManager.AddMagicAmount(5);
        switch (_instants._currentRoomAsset.roomType)
        {
            case RoomType.BattleRoom:
                _instants._currentRoomType = RoomType.BattleRoom;
                _instants.ToBattleRoom(_instants._currentRoomAsset as BattleRoom);
                break;
            case RoomType.EncounterRoom:
                _instants._currentRoomType = RoomType.EncounterRoom;
                _instants.ToEncounterRoom(_instants._currentRoomAsset as EncounterRoom);
                break;
            case RoomType.BossRoom:
                _instants._currentRoomType = RoomType.BossRoom;
                _instants.ToBossRoom(_instants._currentRoomAsset as BossRoom);
                break;
        }
        _instants._currentStep += 1;
        _instants.SpawnNextRoom();
    }
    public void StartGame()
    {
        leking.UIManager.HideTitleUI();
        _currentStep = 0;
        _currentFloor = 1;
        _currentRoomAsset = battleRoomsPool[Random.Range(0, battleRoomsPool.Count)];
        _currentStep++;
        _currentRoomType = RoomType.BattleRoom;
        GameObject.FindWithTag("Player").GetComponent<Player>().RecordState();
        ToBattleRoom(_currentRoomAsset as BattleRoom);
        SpawnNextRoom();
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

    public static void ReadyNextRoom()
    {
        leking.UIManager.ShowNextRoomButton();
    }
    public static void ToReadyRoom()
    {
        leking.UIManager.HideNextRoomButton();
        GameObject rr;
        if (_instants._currentRoomType == RoomType.BossRoom)
        {
            if (_instants._currentFloor == 4)
            {
                rr = Instantiate(_instants.endingRoomPrefab, _instants.roomRoot.transform);
            }
            else
            {
                rr = Instantiate(_instants.floorReadRoomPrefab,_instants.roomRoot.transform);
            }
        }
        
        else
        {
            rr = Instantiate(_instants.readRoomPrefab,_instants.roomRoot.transform);
            for (int i = 0; i < _instants._nextRooms.Count; i++)
            {
                var door = Instantiate(_instants.doorPrefab, rr.transform.Find("Doors").transform);
                door.transform.position = rr.transform.Find("Doors").transform.position + (i-1)*_instants.doorOffset;
                door.roomIndex = i;
                door.type = _instants._nextRooms[i].roomType;
            }
        }
        
        _instants.RoomSwitchRoll(rr);
    }
    private GameObject _currentRoomObject;
    private GameObject _nextRoomObject;
    private void RoomSwitchRoll(GameObject targetRoom)
    {
        Player.PlayerWalk();
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

    public void RollBack()
    {
        RollBackBattalStart();
    }
    public void RollBackBig()
    {
        RollBackRoom();
    }
    private static void RollBackBattalStart()
    {
        BattleManager.EndBattle();
        GameObject.FindWithTag("Player").GetComponent<Player>().RollbackState();
        switch (_instants._currentRoomType)
        {
            case RoomType.BattleRoom:
                _instants.ToBattleRoom(_instants._currentRoomAsset as BattleRoom);
                break;
        }
    }
    private static void RollBackRoom()
    {
        BattleManager.EndBattle();
        GameObject.FindWithTag("Player").GetComponent<Player>().RollbackState();
        _instants._currentStep -= 1;
        _instants._nextRooms.Clear();
        foreach (var roomAsset in _instants._recordNextRooms)
        {
            _instants._nextRooms.Add(roomAsset);
        }
        leking.UIManager.HideNextRoomButton();
        GameObject rr;
        if (_instants._recordCurrentRoomType == RoomType.BossRoom)
        {
            rr = Instantiate(_instants.floorReadRoomPrefab,_instants.roomRoot.transform);
        }
        else
        {
            rr = Instantiate(_instants.readRoomPrefab,_instants.roomRoot.transform);
            for (int i = 0; i < _instants._nextRooms.Count; i++)
            {
                var door = Instantiate(_instants.doorPrefab, rr.transform.Find("Doors").transform);
                door.transform.position = rr.transform.Find("Doors").transform.position + (i-1)*_instants.doorOffset;
                door.roomIndex = i;
                door.type = _instants._nextRooms[i].roomType;
            }
        }
        
        _instants.RoomSwitchFade(rr);
    }
    private IEnumerator RoomSwitchFadeCoroutine()
    {
        if (!IsSwitchRoom)
        {
            IsSwitchRoom = true;
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
            onSwitched = () => { };
            _currentRoomObject = _nextRoomObject;
            for (float i = 0; i <= 1; i += 0.01f)
            {
                roomMask.transform.position =
                    Vector3.Lerp(roomRoot.transform.position,roomMaskPos.transform.position , Mathf.Pow(i,2));
                yield return new WaitForSeconds(1 / 100f);
            }
            leking.UIManager.ShowCanvas();
            IsSwitchRoom = false;
        }
    }
    private IEnumerator RoomSwitchRollCoroutine()
    {
        if (!IsSwitchRoom)
        {
            IsSwitchRoom = true;
            _nextRoomObject.transform.position -= rollRoomOffset;
            var position = roomRoot.transform.position;
            var targetPosNext = position;
            var targetPosCurr = position + rollRoomOffset;
            var initNextPos = _nextRoomObject.transform.position;
            var initCurrPos = _currentRoomObject.transform.position;
            for (float i = 0; i <= 1; i += 0.01f)
            {
                _currentRoomObject.transform.position =
                    Vector3.Lerp(initCurrPos, targetPosCurr, i);
                _nextRoomObject.transform.position =
                    Vector3.Lerp(initNextPos, targetPosNext, i);
                yield return new WaitForSeconds(1 / 100f);
            }
            print(_currentRoomObject.name);
            Destroy(_currentRoomObject);
            _currentRoomObject = _nextRoomObject;
            onSwitched();
            onSwitched = () => { };
            IsSwitchRoom = false;
            Player.PlayerIdle();
        }
    }
}
