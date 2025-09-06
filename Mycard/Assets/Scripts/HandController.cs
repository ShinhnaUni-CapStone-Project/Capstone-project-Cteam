using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public static HandController instance;

    private void Awake()
    {
        instance = this;
    }

    public List<Card> heldCards = new List<Card>();

    public Transform minpos, maxpos;
    public List<Vector3> cardPositions = new List<Vector3>();
    


    void Start()
    {
        SetCardPositionsInHand();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetCardPositionsInHand()
    {
        cardPositions.Clear();

        Vector3 distanceBetweenPoints = Vector3.zero;
        if(heldCards.Count > 1)
        {
            distanceBetweenPoints = (maxpos.position - minpos.position) / (heldCards.Count - 1);
        }

        for(int i = 0; i < heldCards.Count; i++)
        {
            cardPositions.Add(minpos.position + (distanceBetweenPoints * i));

            //heldCards[i].transform.position = cardPositions[i];
            //heldCards[i].transform.rotation = minpos.rotation;
            
            //카드가 움직이면 사용됩니다
            heldCards[i].MoveToPoint(cardPositions[i], minpos.rotation);

            heldCards[i].inHand = true;
            heldCards[i].handPosition = i;

        }
    }

    public void RemoveCardFromHand(Card cardToRemove)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        int before = heldCards.Count;
        Debug.Log($"[HandController] RemoveCardFromHand begin: target={(cardToRemove!=null?cardToRemove.name:"<null>")}, instance={(cardToRemove!=null?cardToRemove.InstanceId:"<null>")}, beforeCount={before}");
#endif
        // 우선 위치 인덱스 기반 제거 시도
        if (cardToRemove != null && cardToRemove.handPosition >= 0 && cardToRemove.handPosition < heldCards.Count && heldCards[cardToRemove.handPosition] == cardToRemove)
        {
            heldCards.RemoveAt(cardToRemove.handPosition);
        }
        else
        {
            // 인덱스가 불일치할 수 있으므로 안전하게 객체 기반 제거를 시도한다.
            int idx = heldCards.IndexOf(cardToRemove);
            if (idx >= 0)
            {
                heldCards.RemoveAt(idx);
            }
            else
            {
                Debug.LogError("Card at position" + (cardToRemove!=null?cardToRemove.handPosition:-1) + " is not the card being removed from hand");
            }
        }

        SetCardPositionsInHand();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        int after = heldCards.Count;
        Debug.Log($"[HandController] RemoveCardFromHand end: afterCount={after}");
#endif
    }

    public void AddCardToHand(Card cardToAdd)
    {
        heldCards.Add(cardToAdd);
        SetCardPositionsInHand() ;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[HandController] AddCardToHand: added={(cardToAdd!=null?cardToAdd.name:"<null>")}, instance={(cardToAdd!=null?cardToAdd.InstanceId:"<null>")}, count={heldCards.Count}");
#endif
    }

    public void EmptyHand()
    {
        foreach(Card heldCard in heldCards)
        {
            heldCard.inHand = false;
            heldCard.MoveToPoint(BattleController.instance.discardPoint.position, heldCard.transform.rotation);

        }
        heldCards.Clear();
    }
}
