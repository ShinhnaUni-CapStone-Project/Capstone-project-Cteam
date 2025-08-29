using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckLoader : MonoBehaviour
{
    public DeckController theDeck;

    private void Awake()
    {
        if (FindObjectOfType<DeckController>() == null)
        {
            DeckController.instance = Instantiate(theDeck);
            DontDestroyOnLoad(DeckController.instance.gameObject);
            //�� ��ȯ�ÿ��� ����
        }
    }
}
