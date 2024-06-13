using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MercenariesUI : MonoBehaviour
{
    private Animator mercenariesAnimator;
    [SerializeField] private ItemTemplateUI_BattleDeck level1MercenarySlot;
    [SerializeField] private ItemTemplateUI_BattleDeck level2MercenarySlot;
    [SerializeField] private ItemTemplateUI_BattleDeck level3MercenarySlot;
    [SerializeField] private ItemTemplateUI_BattleDeck level4MercenarySlot;

    private float changeMercenaryImageTimer;
    private float changeMercenaryImageRate = .1f;
    private float changeMercenaryLevelTimer;
    private float changeMercenaryLevelRate = 1.5f;
    private bool randomizingMercenary;
    private int mercenaryRandomized = 1;

    private int mercenaryImageIndex;

    private void Awake() {
        mercenariesAnimator = GetComponent<Animator>();

        level1MercenarySlot.gameObject.SetActive(false);
        level2MercenarySlot.gameObject.SetActive(false);
        level3MercenarySlot.gameObject.SetActive(false);
        level4MercenarySlot.gameObject.SetActive(false);
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Update() {
        if(randomizingMercenary) {
            changeMercenaryImageTimer += Time.deltaTime;
            changeMercenaryLevelTimer += Time.deltaTime;

            if(changeMercenaryImageTimer > changeMercenaryImageRate) {
                RandomizeMercenaryImage(mercenaryRandomized);
                changeMercenaryImageTimer = 0;
            }

            if(changeMercenaryLevelTimer > changeMercenaryLevelRate) {
                SetMercenary(mercenaryRandomized);
                mercenaryRandomized++;
                changeMercenaryLevelTimer = 0;

                if(mercenaryRandomized > 4) {
                    randomizingMercenary = false;
                    mercenariesAnimator.SetTrigger("Shrink");
                }

            }
        }
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if (BattleManager.Instance.IsPreparationPhase()) {
                Debug.Log(BattleManager.Instance.GetCurrentTurn());
            if (BattleManager.Instance.GetCurrentTurn() == 0) {
                mercenariesAnimator.SetTrigger("Grow");
                randomizingMercenary = true;
            }
        }
    }

    private void RandomizeMercenaryImage(int mercenaryLevel) {
        if(mercenaryLevel == 1) {
            level1MercenarySlot.gameObject.SetActive(true);
            level1MercenarySlot.GetSlotImage().gameObject.SetActive(true);
            level1MercenarySlot.GetSlotImage().sprite = BattleDataManager.Instance.GetLevel1MercenaryTroopSOList()[mercenaryImageIndex].troopIllustrationSlotSprite;

            mercenaryImageIndex++;
            if (mercenaryImageIndex == BattleDataManager.Instance.GetLevel1MercenaryTroopSOList().Count) {
                mercenaryImageIndex = 0;
            }
        }

        if (mercenaryLevel == 2) {
            level2MercenarySlot.gameObject.SetActive(true);
            level2MercenarySlot.GetSlotImage().gameObject.SetActive(true);
            level2MercenarySlot.GetSlotImage().sprite = BattleDataManager.Instance.GetLevel2MercenaryTroopSOList()[mercenaryImageIndex].troopIllustrationSlotSprite;

            mercenaryImageIndex++;
            if (mercenaryImageIndex == BattleDataManager.Instance.GetLevel2MercenaryTroopSOList().Count) {
                mercenaryImageIndex = 0;
            }
        }

        if (mercenaryLevel == 3) {
            level3MercenarySlot.gameObject.SetActive(true);
            level3MercenarySlot.GetSlotImage().gameObject.SetActive(true);
            level3MercenarySlot.GetSlotImage().sprite = BattleDataManager.Instance.GetLevel3MercenaryTroopSOList()[mercenaryImageIndex].troopIllustrationSlotSprite;

            mercenaryImageIndex++;
            if (mercenaryImageIndex == BattleDataManager.Instance.GetLevel3MercenaryTroopSOList().Count) {
                mercenaryImageIndex = 0;
            }
        }

        if (mercenaryLevel == 4) {
            level4MercenarySlot.gameObject.SetActive(true);
            level4MercenarySlot.GetSlotImage().gameObject.SetActive(true);
            level4MercenarySlot.GetSlotImage().sprite = BattleDataManager.Instance.GetLevel4MercenaryTroopSOList()[mercenaryImageIndex].troopIllustrationSlotSprite;

            mercenaryImageIndex++;
            if (mercenaryImageIndex == BattleDataManager.Instance.GetLevel4MercenaryTroopSOList().Count) {
                mercenaryImageIndex = 0;
            }
        }

    }

    private void SetMercenary(int mercenaryLevel) {
        if (mercenaryLevel == 1) {
            level1MercenarySlot.SetTroopSO(BattleManager.Instance.GetLevel1Mercenary());
            level1MercenarySlot.GetComponentInChildren<SpawnIPlaceableButton>().SetTroopToSpawn(BattleManager.Instance.GetLevel1Mercenary());
        }
        if (mercenaryLevel == 2) {
            level2MercenarySlot.GetComponentInChildren<SpawnIPlaceableButton>().SetTroopToSpawn(BattleManager.Instance.GetLevel2Mercenary());
        }
        if (mercenaryLevel == 3) {
            level3MercenarySlot.GetComponentInChildren<SpawnIPlaceableButton>().SetTroopToSpawn(BattleManager.Instance.GetLevel3Mercenary());
        }
        if (mercenaryLevel == 4) {
            level4MercenarySlot.GetComponentInChildren<SpawnIPlaceableButton>().SetTroopToSpawn(BattleManager.Instance.GetLevel4Mercenary());
        }
    }
}
