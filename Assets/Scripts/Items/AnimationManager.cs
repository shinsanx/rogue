using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
public class AnimationManager : MonoBehaviour
{
    private static AnimationManager _instance;
    public static AnimationManager i {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<AnimationManager>();
            } if (_instance == null) {
                Debug.LogError("AnimationManagerがシーンに存在しません。");
            }
            return _instance;
        }
    }

    private Vector3 itemOffset = new Vector3(0.5f, 0.5f, 0);


    public async Task throwItemAnimation(ItemSO item, Vector2Int orgPosition, Vector2Int targetPosition){
        float distance = Vector2Int.Distance(orgPosition, targetPosition);
        GameObject itemObject = CharacterManager.i.GetItemPrefab(item.id);
        GameObject itemInstance = Instantiate(itemObject, orgPosition.ToVector3() + itemOffset, Quaternion.identity);
        itemInstance.transform.DOMove(targetPosition.ToVector3() + itemOffset, distance / 20f).SetEase(Ease.Linear);
        await Task.Delay((int)(distance / 20f * 1000));
        Destroy(itemInstance);
    }
    
}
