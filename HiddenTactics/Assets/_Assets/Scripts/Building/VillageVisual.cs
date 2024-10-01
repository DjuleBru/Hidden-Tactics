using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageVisual : MonoBehaviour
{
    private Village village;
    private BuildingHP villageHP;
    private Animator villageAnimator;

    [SerializeField] private SpriteRenderer villageCleanRenderer;
    [SerializeField] private SpriteRenderer villageBurningRenderer;
    [SerializeField] Material villageDestroyedMaterial;
    [SerializeField] private ParticleSystem villageDestroyedPS;

    private bool villageDestroyed;

    private void Awake() {
        village = GetComponentInParent<Village>();
        villageHP = GetComponentInParent<BuildingHP>();
        villageAnimator = GetComponent<Animator>();

        BattleGridVisual.Instance.OnPlayerSpritesLoaded += BattleGridVisual_OnPlayerSpritesLoaded;
        BattleGridVisual.Instance.OnOpponentSpritesLoaded += BattleGridVisual_OnOpponentSpritesLoaded;
    }

    private void Start() {
        villageHP.OnHealthChanged += VillageHP_OnHealthChanged;
    }

    private void BattleGridVisual_OnPlayerSpritesLoaded(object sender, System.EventArgs e) {
        SetVillageSprite();
    }
    private void BattleGridVisual_OnOpponentSpritesLoaded(object sender, System.EventArgs e) {
        SetVillageSprite();
    }

    public void SetVillageSprite() {
        if (village.IsOwnedByPlayer()) {
            villageCleanRenderer.sprite = BattleGridVisual.Instance.GetRandomPlayerVillageSprite();
        }
        else {
            villageCleanRenderer.sprite = BattleGridVisual.Instance.GetRandomOpponentVillageSprite();
        }

        villageBurningRenderer.sprite = villageCleanRenderer.sprite;

    }

    private void Update() {

        if(villageDestroyed) {

            if (villageAnimator.GetCurrentAnimatorStateInfo(0).IsName("Village_Burn3") && villageAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > .8) {
                // Unit animator has finished playing burn3 animation
                villageCleanRenderer.material = villageDestroyedMaterial;
                villageDestroyed = false;
            }

        }
    }

    [Button]
    public void SetBurning1() {
        villageAnimator.SetTrigger("Burn1");
    }


    [Button]
    public void SetBurning12() {
        villageAnimator.SetTrigger("Burn1");
        villageAnimator.SetTrigger("Burn2");
    }

    [Button]
    public void SetBurning123() {
        villageAnimator.SetTrigger("Burn1");
        villageAnimator.SetTrigger("Burn2");
        villageAnimator.SetTrigger("Burn3");
    }
    private void VillageHP_OnHealthChanged(object sender, BuildingHP.OnHealthChangedEventArgs e) {

        if(e.previousHealth == 3) {
            if (e.newHealth == 2) {
                villageAnimator.SetTrigger("Burn1");
            }

            if (e.newHealth == 1) {
                villageAnimator.SetTrigger("Burn1");
                villageAnimator.SetTrigger("Burn2");
            }

            if (e.newHealth <= 0) {
                villageAnimator.SetTrigger("Burn1");
                villageAnimator.SetTrigger("Burn2");
                villageAnimator.SetTrigger("Burn3");
                villageAnimator.SetTrigger("Fade");
            }
        }

        if(e.previousHealth == 2) {
            if (e.newHealth == 1) {
                villageAnimator.SetTrigger("Burn2");
            }

            if (e.newHealth <= 0) {
                villageAnimator.SetTrigger("Burn2");
                villageAnimator.SetTrigger("Burn3");
                villageAnimator.SetTrigger("Fade");
            }
        }

        if(e.previousHealth == 1) {
            villageAnimator.SetTrigger("Burn3");
            villageAnimator.SetTrigger("Fade");
        }

        if(e.newHealth <= 0) {
            villageDestroyed = true;

            if(!village.IsOwnedByPlayer()) {
                villageDestroyedPS.Play();
            }
        }
    }

    public void SetVillageSprite(Sprite sprite) {
        villageCleanRenderer.sprite = sprite;
    }

}
