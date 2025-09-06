using System.Collections.Generic;
using Game.Save;

/// <summary>
/// 게임의 모든 무작위성을 관리하고 재현성을 보장하는 서비스의 인터페이스입니다.
/// </summary>
public interface IRngService
{
    /// <summary>
    /// 지정된 도메인의 시드를 설정합니다. 도메인을 사용하기 전에 반드시 호출되어야 합니다.
    /// </summary>
    /// <param name="domain">"deck-shuffle", "enemy-ai" 등 무작위성을 사용할 기능 영역</param>
    /// <param name="seed">재현성을 위한 시드 값 (0은 내부적으로 1로 보정됨)</param>
    void Seed(string domain, uint seed);

    /// <summary>
    /// 지정된 도메인에서 다음 무작위 정수(uint)를 반환합니다.
    /// </summary>
    uint NextUInt(string domain);

    /// <summary>
    /// 지정된 도메인에서 [minInclusive, maxExclusive) 범위의 무작위 정수(int)를 반환합니다. (상한 배타)
    /// </summary>
    /// <param name="domain">도메인 키</param>
    /// <param name="minInclusive">결과에 포함될 수 있는 최소값</param>
    /// <param name="maxExclusive">결과보다 항상 큰 값(포함되지 않음)</param>
    int NextInt(string domain, int minInclusive, int maxExclusive);

    /// <summary>
    /// 지정된 도메인에서 [0.0f, 1.0f) 범위의 무작위 실수(float)를 반환합니다. (상한 배타)
    /// </summary>
    float NextFloat(string domain);

    /// <summary>
    /// 현재 모든 RNG 상태를 DB 저장용 DTO 목록으로 내보냅니다.
    /// 주의: 반환되는 항목에는 RunId가 설정되어 있지 않으므로, 저장 측에서 RunId를 채운 뒤 저장해야 합니다.
    /// </summary>
    List<RngState> GetStatesForSave();
}
