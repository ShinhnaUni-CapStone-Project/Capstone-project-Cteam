using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckLoader : MonoBehaviour
{
    private void Awake()
    {
        // 레거시: DeckController 인스턴스를 생성하던 스크립트입니다.
        // 현재는 GameInitializer + IDeckService가 덱을 관리하므로 더 이상 필요하지 않습니다.
        Debug.LogWarning("[DeckLoader] 레거시 스크립트입니다. GameInitializer/IDeckService가 덱을 관리합니다. 컴포넌트를 제거해도 됩니다.", this);
        enabled = false;
    }
}
