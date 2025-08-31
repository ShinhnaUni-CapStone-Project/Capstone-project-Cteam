using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelicUI : MonoBehaviour
{
    [SerializeField] private Image image;
    public Relic Relic { get; private set; }
    public void Setup(Relic relic)
    {
        image.sprite = relic.Image;

    }
}
