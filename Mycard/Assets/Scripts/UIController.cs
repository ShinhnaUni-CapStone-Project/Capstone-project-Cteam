using UnityEngine;
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
        DeckController.instance.DrawCardForMana();

        AudioManager.instance.PlaySFX(0);
    }

    public void EndPlayerTurn()
    {


        BattleController.instance.EndPlayerTurn();

        AudioManager.instance.PlaySFX(0);
    }

    public void AddBanana()
    {
        GameObject bananaObject = new GameObject("BananaItem"); //������ ������Ʈ�� ���� ������ݴϴ�
        Banana bananaItem = bananaObject.AddComponent<Banana>(); //������ ��ũ��Ʈ�� �ҷ��Ϳ�
        Inventory.Instance.AddItem(bananaItem);
        bananaItem.OnAddItem();
    }

    public void AddSword()
    {
        GameObject SwordObject = new GameObject("SwordItem"); //������ ������Ʈ�� ���� ������ݴϴ�
        Sword SwordItem = SwordObject.AddComponent<Sword>(); //������ ��ũ��Ʈ�� �ҷ��Ϳ�
        Inventory.Instance.AddItem(SwordItem);
        SwordItem.OnAddItem();
    }

    public void AddWarBanner()
    {
        GameObject WarBannerObject = new GameObject("WarBannerItem");
        WarBanner banner = WarBannerObject.AddComponent<WarBanner>();
        Inventory.Instance.AddItem(banner);
        banner.OnAddItem();
    }

    public void FieldButton()
    {
        if (FieldBackButton.activeSelf == false)
        {
            CameraController.instance.MoveTo(CameraController.instance.battleTransform);
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
        // 1) Resources/Characters ���� ���� "Cat.asset" ���� �ҷ�����
        CharacterSO newChar = Resources.Load<CharacterSO>("Characters/Cat");

        if (newChar != null)
        {
            // 2) PortraitInventory�� ĳ���� �߰�
            PortraitInventory.instance.AddCharacter(newChar);

            Debug.Log(newChar.characterName + " �߰���!");
        }
        else
        {
            Debug.LogError("ĳ���� ScriptableObject�� ã�� �� �����ϴ�!");
        }
    }

    public void CardAdd1()
    {
        DeckController.instance.AddCardToDeckById("Knight", 1);
        //DeckController.instance.SaveDeck();
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
