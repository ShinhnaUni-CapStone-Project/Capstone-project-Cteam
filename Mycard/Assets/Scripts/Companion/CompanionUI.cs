using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CompanionUI : MonoBehaviour
{
    public static CompanionUI instance;
    private bool hasPressed = false;
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CompanionCardAdd1()
    {
        if (hasPressed) return;
        hasPressed = true;
        DeckController.instance.AddCardToDeckById("Knight", 1);
        //DeckController.instance.SaveDeck();
    }
}
