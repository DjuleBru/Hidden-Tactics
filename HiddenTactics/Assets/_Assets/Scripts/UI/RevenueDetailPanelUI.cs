using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RevenueDetailPanelUI : MonoBehaviour
{
    public static RevenueDetailPanelUI Instance;

    [SerializeField] private Transform revenueDetailContainer;
    [SerializeField] private Transform revenueDetailTemplate;

    [SerializeField] private TextMeshProUGUI totalIncomeAmountText;

    private List<int> revenueDetailIntList;
    private List<string> revenueDetailTextList;

    private int playerVillagesDestroyed;

    private void Awake() {
        Instance = this;
        revenueDetailIntList = new List<int>();
        revenueDetailTextList = new List<string>();
    }

    private void Start() {
        VillageManager.Instance.OnPlayerVillageDestroyed += VillageManager_OnPlayerVillageDestroyed;

        revenueDetailIntList.Add(PlayerGoldManager.Instance.GetPlayerBaseIncome());
        revenueDetailTextList.Add("Base income");

        revenueDetailIntList.Add(0);
        revenueDetailTextList.Add("Villages lost" + " (" + playerVillagesDestroyed + ")");
        UpdateRevenueDetailBreakdown();
    }


    public void UpdateRevenueDetailPanelUI(int newGold) {
        UpdateRevenueDetailBreakdown();
        totalIncomeAmountText.text = newGold.ToString();
    }

    private void UpdateRevenueDetailBreakdown() {
        revenueDetailTemplate.gameObject.SetActive(false);

        foreach (Transform child in revenueDetailContainer) {
            if(child != revenueDetailTemplate) {
                Destroy(child.gameObject);
            }
        }

        for(int i = 0; i < revenueDetailIntList.Count; i++) {
            Transform revenueDetailInstantiated = Instantiate(revenueDetailTemplate, revenueDetailContainer);
            revenueDetailInstantiated.gameObject.SetActive(true);
            TextMeshProUGUI revenueDetailText = revenueDetailInstantiated.Find("RevenueTitle").GetComponent<TextMeshProUGUI>();
            revenueDetailText.text = revenueDetailTextList[i];

            TextMeshProUGUI revenueDetailAmount = revenueDetailInstantiated.Find("RevenueAmountText").GetComponent<TextMeshProUGUI>();
            revenueDetailAmount.text = "+" + revenueDetailIntList[i].ToString();
        }
    }

    public void AddRevenueElement(string newRevenueElementName, int revenueAmount) {
        int i = 0;
        bool foundRevenueElementName = false;
        int revenueElementAmount = 0;

        foreach (string revenueElementText in revenueDetailTextList) {
            string revenueElementName = revenueElementText.Substring(0, revenueElementText.Length - 4);

            if (revenueElementName == newRevenueElementName) {
                i = revenueDetailTextList.IndexOf(revenueElementText);
                foundRevenueElementName = true;

                string numberOfType = revenueElementText.Substring(revenueElementText.Length - 2, 1);

                revenueElementAmount = int.Parse(numberOfType) + 1;
                break;
            }
        }

        if (foundRevenueElementName) {
            revenueDetailIntList[i] = revenueAmount * revenueElementAmount;
            revenueDetailTextList[i] = newRevenueElementName + "(x" + revenueElementAmount + ")";
            UpdateRevenueDetailBreakdown();
        } else {
            revenueDetailIntList.Add(revenueAmount);
            revenueDetailTextList.Add(newRevenueElementName + "(x1)");
            UpdateRevenueDetailBreakdown();
        }
    }

    public void RemoveRevenueElement(string revenueElementName, int revenueAmount) {
        int i = 0;
        foreach(string text in revenueDetailTextList) {
            if(text == revenueElementName) {
                i = revenueDetailTextList.IndexOf(text);
            }
        }

        revenueDetailIntList.Remove(revenueDetailIntList[i]);
        revenueDetailTextList.Remove(revenueDetailTextList[i]);
        UpdateRevenueDetailBreakdown();
    }

    private void VillageManager_OnPlayerVillageDestroyed(object sender, System.EventArgs e) {
        playerVillagesDestroyed++;

        revenueDetailIntList[1] = playerVillagesDestroyed * PlayerGoldManager.Instance.GetPlayerVillageDestroyedBonusIncome();
        revenueDetailTextList[1] = "Villages lost" + "(" + playerVillagesDestroyed + ")";
        UpdateRevenueDetailBreakdown();
    }

    private void OnEnable() {
        UpdateRevenueDetailBreakdown();
    }
}
