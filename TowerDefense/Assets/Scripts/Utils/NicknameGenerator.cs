using UnityEngine;

public static class NicknameGenerator
{
    private static readonly string[] _adjectives =
    {
        "번개같은", "용감한", "강력한", "신비로운", "날카로운",
        "무적의", "전설의", "빠른", "거침없는", "불꽃같은",
        "얼음같은", "그림자같은", "폭풍같은", "황금빛", "어둠의",
    };

    private static readonly string[] _nouns =
    {
        "기사", "전사", "마법사", "궁수", "파수꾼",
        "영웅", "수호자", "방랑자", "사냥꾼", "검객",
        "탐험가", "용병", "성기사", "암살자", "소환사",
    };

    public static string Generate()
    {
        string adj  = _adjectives[Random.Range(0, _adjectives.Length)];
        string noun = _nouns[Random.Range(0, _nouns.Length)];
        int    num  = Random.Range(1000, 9999);
        return $"{adj}{noun}{num}";
    }
}
