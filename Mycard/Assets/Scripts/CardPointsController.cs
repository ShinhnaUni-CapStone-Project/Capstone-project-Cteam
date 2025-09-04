using System.Collections;
using UnityEngine;

public class CardPointsController : MonoBehaviour
{   //ī���� �ϵ����� �ൿ�� ����ϴ� ��ũ��Ʈ�Դϴ�

    public static CardPointsController instance;

    private void Awake()
    {
        instance = this;
    }

    public CardPlacePoint[] playerCardPoints, enemyCardPoints, enemyStayPoints;

    public float timeBetweenAttacks = .25f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayerAttack()
    {
        
        StartCoroutine(PlayerAttackCo());
        CameraController.instance.MoveTo(CameraController.instance.battleTransform);

    }

    IEnumerator PlayerAttackCo()
    {

        
        yield return new WaitForSeconds(timeBetweenAttacks);

        for (int i = 0; i < playerCardPoints.Length; i++)
        {   // �߰� +++ ���ݷ� ���� ü�� ����
            var playerCard = (i < playerCardPoints.Length && playerCardPoints[i] != null)
                ? playerCardPoints[i].activeCard : null;
            int baseAtk = playerCard?.attackPower ?? 0;
            int finalAtk = GameEvents.ModifyPlayerAttack?.Invoke(baseAtk) ?? baseAtk;
            // �߰� +++ ���ݷ� ���� ü�� ����
            if (playerCardPoints[i].activeCard != null) //1�� ĭ�� ������
             {

                if (enemyCardPoints[i].activeCard != null) //��ī������Ʈ�� 1��ĭ�� ������
                {
                    //��ī�����
                    //enemyCardPoints[i].activeCard.DamageCard(playerCardPoints[i].activeCard.attackPower); //����
                    enemyCardPoints[i].activeCard.DamageCard(finalAtk);//finalAtk������ �߰��Ǹ� ���ݷ� �߰�

                }
                else
                {
                    //BattleController.instance.DamageEnemy(playerCardPoints[i].activeCard.attackPower); //ī�尡 ���ٸ� �������� ����
                    //��ī����üü��
                    BattleController.instance.DamageEnemy(finalAtk);//finalAtk������ �߰��Ǹ� ���ݷ� �߰�
                }

                playerCardPoints[i].activeCard.anim.SetTrigger("Attack");//Attack�ҷ�����

               
                yield return new WaitForSeconds(timeBetweenAttacks);
            }

            if (BattleController.instance.battleEnded == true)
            {
                i = playerCardPoints.Length;
            }
        }

        CheckAssignedCards();

        BattleController.instance.AdvanceTurn();
    }

    public void EnemyAttack()
    {
        
        StartCoroutine(EnemyAttackCo());
        


    }
    IEnumerator EnemyAttackCo()
    {
        


        yield return new WaitForSeconds(timeBetweenAttacks);


        for (int i = 0; i < enemyCardPoints.Length; i++)
        {


            if (enemyCardPoints[i].activeCard != null)
            {
                if (playerCardPoints[i].activeCard != null)
                {
                    //�÷��̾�ī�����
                    playerCardPoints[i].activeCard.DamageCard(enemyCardPoints[i].activeCard.attackPower);

                    
                }
                else
                {
                    //�÷��̾���üü��
                    BattleController.instance.DamagePlayer(enemyCardPoints[i].activeCard.attackPower);
                }

                enemyCardPoints[i].activeCard.anim.SetTrigger("Attack");//Attack�ҷ�����

                

                yield return new WaitForSeconds(timeBetweenAttacks);
            }

            if(BattleController.instance.battleEnded == true)
            {
                i = enemyCardPoints.Length;
            }
        }

        CheckAssignedCards();

        GameEvents.OnTurnEnd?.Invoke(false);  // �߰� +++ �� �� ����
        BattleController.instance.AdvanceTurn();
    }

    public void CheckAssignedCards()
    {
        foreach(CardPlacePoint point in enemyCardPoints)
        {
            if (point.activeCard != null)
            {
                if(point.activeCard.currentHealth <= 0)
                {
                    point.activeCard = null;
                }
            }
        }
        
        foreach (CardPlacePoint point in playerCardPoints)
        {
            if (point.activeCard != null)
            {
                if (point.activeCard.currentHealth <= 0)
                {
                    point.activeCard = null;
                }
            }
        }
    }
}
