using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair : MonoBehaviour, IMenuActionAdapter
{
    [SerializeField] ObjectData objectData;
    private Vector2 moveOffset = new Vector2(.5f, .5f);
    [field:SerializeField] public SubmitMenuSet submitMenuSet{get;private set;}
    [SerializeField] GameEvent onFootStair;
    [SerializeField] CurrentSelectedObjectSO currentSelectedObjectSO;

    public void Initialize() {
        CreateSOInstance();
        InitializeStairStatus();
    }

    public void MovePosition() {
        transform.position = objectData.Position.Value.ToVector2() + moveOffset;
    }

    private void CreateSOInstance() {
        objectData.CreateSOInstance();
    }

    private void InitializeStairStatus() {
        //objectData.SetId(CharacterManager.GetUniqueID());
        objectData.Name.SetValue("階段");
        objectData.Type.SetValue("Stair");
        objectData.Position.SetValue(objectData.Position.Value);
    }

    public void OnSelected() {
        currentSelectedObjectSO.Object = gameObject;
        onFootStair.Raise();
    }
}
