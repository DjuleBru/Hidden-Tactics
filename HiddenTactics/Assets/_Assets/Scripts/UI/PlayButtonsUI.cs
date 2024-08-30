using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayButtonsUI : MonoBehaviour
{
    [SerializeField] GameObject mainMenuButtonsGameObject;
    [SerializeField] GameObject updatesPanelGameObject;
    [SerializeField] GameObject lobbyPanelGameObject;
    [SerializeField] GameObject playButtonsGameObject;

    [SerializeField] private Animator leftPanelAnimator;
    [SerializeField] private Animator mainMenuButtonsAnimator;
    [SerializeField] private Animator playButtonsAnimator;

    [SerializeField] private Button backButton;

    private void Awake() {
        backButton.onClick.AddListener(() => {
            StartCoroutine(BackToMainMenu(.7f));
        });
    }

    private IEnumerator BackToMainMenu(float delatToLeftPanelDown) {

        leftPanelAnimator.SetTrigger("Up");
        playButtonsAnimator.SetTrigger("Fold");

        yield return new WaitForSeconds(delatToLeftPanelDown);

        mainMenuButtonsGameObject.SetActive(true);
        lobbyPanelGameObject.SetActive(false);
        updatesPanelGameObject.SetActive(true);
        playButtonsGameObject.SetActive(false);

        leftPanelAnimator.SetTrigger("Down");
        mainMenuButtonsAnimator.SetTrigger("Unfold");

    }

}
