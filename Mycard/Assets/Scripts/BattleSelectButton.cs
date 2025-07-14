using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//�� ��ũ��Ʈ ��ü�� ���߿� �� �������� ���� ���� �߿��� �κ��� �ƴϴ�.
public class BattleSelectButton : MonoBehaviour
{
    public string levelToLoad;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.instance.PlayBattleSelectMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SelectBattle()
    {
        SceneManager.LoadScene(levelToLoad);

        AudioManager.instance.PlaySFX(0);
    }
}
