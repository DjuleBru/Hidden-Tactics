using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TroopSO : ScriptableObject {

    public Sprite troopIllustrationSlotSprite;
    public Sprite troopDescriptionSlotSprite;
    public Sprite troopTypeIconSprite;

    public GameObject troopPrefab;
    public GameObject unitPrefab;

    public AttackSO mainTroopAttackSO;

    public int spawnTroopCost;
    public int upgradeTroopCost;
    public int buyAdditionalUnitsCost;

    public bool troopIsImplemented;

}
