using UnityEngine;
using System;
using UnityEngine.PlayerLoop;

public class GameContext : MonoBehaviour
{
    public static GameContext I { get; private set; } //I는 Instance의 I?

    [Header("Session")]
    public string ProfileId = "P1"; // 임시 기본값
    public string RunId;
    public string SelectedCompanionId; // "WARRIOR" 처럼 저장

    //public DeckController DeckController { get; private set; }
    
    //게임매니저 같은걸 만드신거라 생각하고 약간 수정해봤습니다 내용은 유사하고 주석 처리 해놓은 것은 GameContext인 게임매니저가 실행될때 덱 컨트롤러를 자동으로 불러오는 코드를 만들었습니다
    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);

        /*if (I == null)
        {
            I= this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
            
        }else if(I != this)
        {
            Destroy(gameObject);
        }*/
    }


    /*
    private void InitializeManager()
    {
        DeckController = GetComponent<DeckController>();

        if (DeckController != null)
        {
            GameObject prefab = Resources.Load<GameObject>("prefabs/DeckController"); //덱 컨트롤러를 프리팹에서 찾습니다
            if(prefab == null)
            {
                Debug.Log("DeckController Prefabs not found");

            }
            else
            {
                Instantiate(prefab,transform.position, Quaternion.identity, transform);
                DeckController = GetComponentInChildren<DeckController>();
                //덱 컨트롤러를 게임 콘텍스트 밑에 불러옵니다 만약 잘 작동하면 배틀의 덱 컨트롤러를 지워도 될겁니다
            }
        }
    }*/


}