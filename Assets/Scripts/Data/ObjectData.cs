using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectData : MonoBehaviour, IObjectData
{
    [field: SerializeField] public IntVariable Id{get; private set;}
    [field: SerializeField] public StringVariable Name{get; private set;}
    [field: SerializeField] public StringVariable Type{get; private set;}
    [field: SerializeField] public IntVariable RoomNum{get; private set;}
    [field: SerializeField] public Vector2IntVariable Position{get; private set;}
    [SerializeField] private ObjectDataRuntimeSet _objectDataRuntimeSet;
    [SerializeField] private GameEvent onPositionChanged;

    public event System.Action<IObjectData> OnObjectUpdated;

    void OnEnable() {
        _objectDataRuntimeSet.Add(this);
    }

    void OnDisable() {
        _objectDataRuntimeSet.Remove(this);
    }

    public void SetPosition(Vector2Int position) {
        Position.SetValue(position);
        SetRoomNum(TileManager.i.LookupRoomNum(position));
        onPositionChanged.Raise();
        OnObjectUpdated?.Invoke(this);
    }

    public void SetRoomNum(int roomNum) {
        RoomNum.SetValue(roomNum);
        OnObjectUpdated?.Invoke(this);
    }
    
    public void SetName(string name) {
        Name.SetValue(name);
        OnObjectUpdated?.Invoke(this);
    }
    
    public void SetType(string type) {
        Type.SetValue(type);
        OnObjectUpdated?.Invoke(this);
    }
    
    public void SetId(int id) {
        Id.SetValue(id);
        OnObjectUpdated?.Invoke(this);
    }
    
    
    //SOインスタンス生成用
    public void CreateSOInstance() {
        Id = ScriptableObject.CreateInstance<IntVariable>();
        Name = ScriptableObject.CreateInstance<StringVariable>();
        Type = ScriptableObject.CreateInstance<StringVariable>();
        RoomNum = ScriptableObject.CreateInstance<IntVariable>();
        Position = ScriptableObject.CreateInstance<Vector2IntVariable>();
    }

}
