using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class RelicIconUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text stackText;

    public void Setup(Sprite sprite, int stacks)
    {
        if (icon) icon.sprite = sprite;
        SetStacks(stacks);
    }

    public void SetStacks(int stacks)
    {
        if (!stackText) return;
        bool show = stacks > 1;
        stackText.gameObject.SetActive(show);
        if (show) stackText.text = stacks.ToString();
    }
}
