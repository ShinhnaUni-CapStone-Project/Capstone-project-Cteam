﻿using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class UIController : MonoBehaviour
{
    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }

    public TMP_Text playerManaText, playerHealthText, enemyHealthText, enemyManaText;

    public GameObject manawarning;
    public float manawarningTime;
    private float manawarningCounter;
    public GameObject drawCardButton, endTurnButton;

    public UIDamageIndicator playerDamage, enemyDamage;

    public GameObject battleEndScreen_win, battleEndScreen_lose;
    public TMP_Text battleResultText1, battleResultText2;

    public string mainMenuScene, battleSelectScene;

    public GameObject PauseScreen;
    public GameObject FieldShowButton;
    public GameObject FieldBackButton;

    public GameObject EnemyUI;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (manawarningCounter > 0)
        {
            manawarningCounter -= Time.deltaTime;

            if (manawarningCounter <= 0)
            {
                manawarning.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnPause();
        }
    }

    public void SetPlayerManaText(int manaAmount)
    {
        playerManaText.text = "" + manaAmount + "/" + BattleController.instance.playermaxMana;
    }
    public void SetEnemyManaText(int manaAmount)
    {
        enemyManaText.text = "" + manaAmount;
    }

    public void setPlayerHealthText(int healthAmount)
    {
        playerHealthText.text = "" + healthAmount;
    }
    public void setEnemyHealthText(int healthAmount)
    {
        enemyHealthText.text = "" + healthAmount;
    }

    public void ShowManaWarning()
    {
        manawarning.SetActive(true);
        manawarningCounter = manawarningTime;
    }

    public void DrawCard()
    {
        BattleController.instance.AttemptPlayerDraw();
        AudioManager.instance.PlaySFX(0);
    }

    public void EndPlayerTurn()
    {


        BattleController.instance.EndPlayerTurn();

        AudioManager.instance.PlaySFX(0);
    }

    public void AddBanana()
    {
        RelicSystem.Instance.AddRelicById("ManaBoostGem", stacks: 1);
    }

    public void AddSword()
    {
        /*GameObject SwordObject = new GameObject("SwordItem"); //아이템 오브젝트를 새로 만들어줍니다
        Sword SwordItem = SwordObject.AddComponent<Sword>(); //아이템 스크립트를 불러와요
        Inventory.Instance.AddItem(SwordItem);
        SwordItem.OnAddItem();*/
    }

    public void AddWarBanner()
    {
        /*GameObject WarBannerObject = new GameObject("WarBannerItem");
        WarBanner banner = WarBannerObject.AddComponent<WarBanner>();
        Inventory.Instance.AddItem(banner);
        banner.OnAddItem();*/
    }

    public void FieldButton()
    {
        if (FieldBackButton.activeSelf == false)
        {
            CameraController.instance.MoveTo(CameraController.instance.battleTransform);
            endTurnButton.SetActive(false);
            FieldShowButton.SetActive(false);
            FieldBackButton.SetActive(true);
            EnemyUI.SetActive(false);

        }
    }

    public void FieldBack()
    {
        if (FieldShowButton.activeSelf == false)
        {
            CameraController.instance.MoveTo(CameraController.instance.homeTransform);
            endTurnButton.SetActive(true);
            FieldBackButton.SetActive(false);
            FieldShowButton.SetActive(true);

            Invoke("EnableEnemyUI", .4f);
        }
    }

    void EnableEnemyUI()
    {
        EnemyUI.SetActive(true);
    }

    public void ChAdd()
    {
        // 1) Resources/Characters 폴더 안의 "Cat.asset" 파일 불러오기
        CharacterSO newChar = Resources.Load<CharacterSO>("Characters/Cat");

        if (newChar != null)
        {
            // 2) PortraitInventory에 캐릭터 추가
            PortraitInventory.instance.AddCharacter(newChar);

            Debug.Log(newChar.characterName + " 추가됨!");
        }
        else
        {
            Debug.LogError("캐릭터 ScriptableObject를 찾을 수 없습니다!");
        }
    }

    public void CardAdd1()
    {
        // 레거시 DeckController 경로 제거: 이 기능은 신규 덱/보상 시스템으로 대체되어야 합니다.
        Debug.LogWarning("[UIController] CardAdd1은 레거시입니다. 덱 추가는 보상/상점/동료 선택 로직을 통해 처리하세요.");
    }
    
    public void AddRelic()
    {
        RelicSystem.Instance.AddRelicById("WarBanner", stacks: 1);
    }

    public void Pauseup()
    {
        PauseUnPause();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);

        Time.timeScale = 1f;

        AudioManager.instance.PlaySFX(0);
    }
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        Time.timeScale = 1f;

        AudioManager.instance.PlaySFX(0);
    }
    public void ChooseNewBattle()
    {
        SceneManager.LoadScene(battleSelectScene);

        Time.timeScale = 1f;

        AudioManager.instance.PlaySFX(0);
    }

    public void PauseUnPause()
    {
        if(PauseScreen.activeSelf == false)
        {
            PauseScreen.SetActive(true);

            Time.timeScale = 0f;
        }
        else
        {
            PauseScreen.SetActive(false);
            Time.timeScale = 1f;
        }
        AudioManager.instance.PlaySFX(0);
    }
}
