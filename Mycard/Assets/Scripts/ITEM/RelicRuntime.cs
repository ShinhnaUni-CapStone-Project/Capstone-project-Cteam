using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RelicRuntime
{

    //public static int TotalAttackBonus { get; private set; }
    //public static int ExtraDrawPerTurn { get; private set; }
    //public static int ExtraManaPerTurn { get; private set; }
    public static void Apply(string relicId, int delta)
    {
        switch (relicId)
        {

            case "quick_thinking":    // ��: �� �� ���� ��ο� +1
                //ExtraDrawPerTurn += 1 * delta;
                break;

            case "mana_font":         // ��: �� �� ���� �߰� ���� +1
                //ExtraManaPerTurn += 1 * delta;
                break;

            default:
                // ���� ȿ�� ������ Relic ID
                break;
        }
    }
}
