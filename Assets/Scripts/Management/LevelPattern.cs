// using UnityEngine;

// [CreateAssetMenu(fileName = "LevelPattern", menuName = "Grid/Level Pattern", order = 1)]
// public class LevelPattern : ScriptableObject
// {
//     public int width = 10;
//     public int height = 10;

//     // Предопределённые цвета для каждого типа блока (можно расширять)
//     [Header("Available Colors")]
//     public GameObject redPrefab;
//     public GameObject yellowPrefab;
//     public GameObject bluePrefab;
//     public GameObject greenPrefab;
//     // ... другие префабы

//     // Сам паттерн: 2D-массив индексов префабов (0 = red, 1 = yellow и т.д.)
//     [SerializeField] private int[] pattern; // размер = width * height

//     public GameObject GetPrefab(int x, int y)
//     {
//         if (pattern == null || pattern.Length != width * height)
//             return null;

//         int index = y * width + x; // row-major order (y сверху вниз)
//         int type = pattern[index];

//         return type switch
//         {
//             0 => redPrefab,
//             1 => yellowPrefab,
//             2 => bluePrefab,
//             3 => greenPrefab,
//             //_ => null
//         };
//     }

//     public BlockColor GetColorType(int x, int y)
//     {
//         // Аналогично, но возвращаем enum
//         int type = pattern[y * width + x];
//         return type switch
//         {
//             0 => BlockColor.Red,
//             1 => BlockColor.Yellow,
//             2 => BlockColor.Blue,
//             3 => BlockColor.Green,
//             //_ => BlockColor.None // или default
//         };
//     }
// }

using UnityEngine;
[CreateAssetMenu(fileName = "LevelPattern", menuName = "Grid/Level Pattern")]
public class LevelPattern : ScriptableObject
{
    public int width = 10;
    public int height = 10;

    [Header("Shared Prefabs")]
    public GameObject redPrefab;
    public GameObject yellowPrefab;
    public GameObject bluePrefab;
    public GameObject greenPrefab;
    public GameObject orangePrefab;
    public GameObject pinkPrefab;
    public GameObject lightBluePrefab;
    public GameObject purplePrefab;

    [Header("Layers")]
    [SerializeField] private int[] baseLayer;    // основной слой
    [SerializeField] private int[] secondLayer;  // второй слой (поверх)

    public GameObject GetPrefabFromLayer(int layer, int x, int y)
    {
        int index = y * width + x;

        int[] targetLayer = (layer == 0) ? baseLayer : secondLayer;

        // Если массив для этого слоя пустой или слишком короткий — возвращаем null
        if (targetLayer == null || index >= targetLayer.Length)
        {
            return null;
        }

        int type = targetLayer[index];

        return type switch
        {
            0 => null,
            1 => redPrefab,
            2 => yellowPrefab,
            3 => bluePrefab,
            4 => greenPrefab,
            5 => orangePrefab,
            6 => pinkPrefab,
            7 => lightBluePrefab,
            8 => purplePrefab,
            _ => null
        };
    }

    public BlockColor GetColorFromLayer(int layer, int x, int y)
{
    int index = y * width + x;

    int[] targetLayer = (layer == 0) ? baseLayer : secondLayer;

    // if (targetLayer == null || index >= targetLayer.Length)
    // {
    //     return BlockColor.None; // или default, например BlockColor.None
    // }

    int type = targetLayer[index];

    return type switch
    {
        1 => BlockColor.Red,
        2 => BlockColor.Yellow,
        3 => BlockColor.Blue,
        4 => BlockColor.Green,
        5 => BlockColor.Orange,
        6 => BlockColor.Pink,
        7 => BlockColor.LightBlue,
        8 => BlockColor.Purple,
       // _ => BlockColor.None
    };
}
}
