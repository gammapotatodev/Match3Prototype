using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 10;
    public int height = 10;
    [SerializeField] private float cellSize = 1.2f;
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    [Header("Level Configuration")]
    [SerializeField] private LevelPattern levelPattern; // перетаскиваем в инспекторе

    // [Header("Prefabs")]
    // [SerializeField] private GameObject redPrefab;
    // [SerializeField] private GameObject yellowPrefab;
    // private int width;
    // private int height;

    //public List<List<GameObject>> gridCells = new();
    public List<List<BlockStack>> gridCells = new List<List<BlockStack>>();
    public bool IsBusy { get; private set; }

    public void Lock()
    {
        IsBusy = true;
    }

    public void Unlock()
    {
        IsBusy = false;
    }


    public event Action OnAllColumnsShiftedComplete;
    public Action OnGridChanged;

    private void Start()
    {
        GenerateGrid();
    }

    // private void GenerateGrid()
    // {
    //     // gridCells.Clear();

    //     // for (int x = 0; x < width; x++)
    //     // {
    //     //     gridCells.Add(new List<GameObject>());

    //     //     for (int y = 0; y < height; y++)
    //     //     {
    //     //         Vector3 pos = gridOrigin + new Vector3(x * cellSize, y * cellSize, 0);
    //     //         GameObject prefab = x < width / 2 ? yellowPrefab : redPrefab;

    //     //         GameObject cube = Instantiate(prefab, pos, Quaternion.identity, transform);
    //     //         cube.GetComponent<BlockProperties>().colorType =
    //     //             prefab == redPrefab ? BlockColor.Red : BlockColor.Yellow;

    //     //         gridCells[x].Add(cube);
    //     //     }
    //     // }
    //     gridCells.Clear();

    // // Можно переопределить размеры из паттерна
    //     if (levelPattern != null)
    //     {
    //         width = levelPattern.width;
    //         height = levelPattern.height;
    //     }

    //     for (int x = 0; x < width; x++) //int x = 0; x < width; x++
    //     {
    //         gridCells.Add(new List<GameObject>());

    //         for (int y = 0; y < height; y++) //int y = 0; y < height; y++
    //         {
    //             Vector3 pos = gridOrigin + new Vector3(x * cellSize, y * cellSize, 0);

    //             GameObject prefab = levelPattern?.GetPrefab(x, y);

    //             if (prefab == null)
    //             {
    //                 // Можно оставить пустую клетку или поставить дефолтный блок
    //                 gridCells[x].Add(null);
    //                 continue;
    //             }

    //             GameObject cube = Instantiate(prefab, pos, Quaternion.identity, transform);
    //             cube.GetComponent<BlockProperties>().colorType = levelPattern.GetColorType(x, y);

    //             gridCells[x].Add(cube);
    //         }
    //     }
    // }
    private void GenerateGrid()
    {
        gridCells.Clear();

        width = levelPattern.width;
        height = levelPattern.height;

        for (int x = 0; x < width; x++)
        {
            gridCells.Add(new List<BlockStack>());

            for (int y = 0; y < height; y++)
            {
                var stack = new BlockStack();
                Vector3 basePos = gridOrigin + new Vector3(x * cellSize, y * cellSize, 0);

                // Базовый слой (нижний) — красный
                GameObject basePrefab = levelPattern.GetPrefabFromLayer(0, x, y);
                if (basePrefab != null)
                {
                    stack.baseBlock = Instantiate(basePrefab, basePos, Quaternion.identity, transform);
                    var props = stack.baseBlock.GetComponent<BlockProperties>();
                    props.colorType = levelPattern.GetColorFromLayer(0, x, y);
                    props.layerType = BlockLayer.Base;
                }

                // Верхний слой (жёлтый) — всегда создаём на том же родителе
                GameObject topPrefab = levelPattern.GetPrefabFromLayer(1, x, y);
                if (topPrefab != null)
                {
                    // Сдвигаем по Z ближе к камере (меньше Z = ближе)
                    // Чем больше Z-координата — тем дальше объект от камеры
                    Vector3 topPos = basePos + Vector3.forward * 1f;  // ← ПОЗИТИВНЫЙ сдвиг!

                    stack.topBlock = Instantiate(topPrefab, topPos, Quaternion.identity, transform);
                    var props = stack.topBlock.GetComponent<BlockProperties>();
                    props.colorType = levelPattern.GetColorFromLayer(1, x, y);
                    props.layerType = BlockLayer.Top;
                }

                gridCells[x].Add(stack);
            }
        }
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    // Удаляет верхний блок в клетке (x,y)
    // Возвращает true, если что-то удалили
    public bool RemoveBlockAt(int x, int y)
    {
        if (!IsValidPosition(x, y)) return false;

        var stack = gridCells[x][y];
        bool removed = stack.RemoveTop();

        if (removed)
        {
            OnGridChanged?.Invoke();
        }

        return removed;
    }


    // // ЛОГИЧЕСКИЙ сдвиг без анимации
    // public void ShiftColumnInstant(int column)
    // {
    //     var col = gridCells[column];
    //     int writeY = 0;

    //     for (int readY = 0; readY < height; readY++)
    //     {
    //         if (col[readY] != null)
    //         {
    //             if (readY != writeY)
    //             {
    //                 col[writeY] = col[readY];
    //                 col[readY] = null;
    //             }
    //             writeY++;
    //         }
    //     }
    //     OnGridChanged?.Invoke();
    // }
    public void ShiftColumnInstant(int column)
    {
        var col = gridCells[column];

        int writeY = 0;

        for (int readY = 0; readY < height; readY++)
        {
            var stack = col[readY];
            if (!stack.IsEmpty)
            {
                // Перемещаем весь стек вниз
                if (readY != writeY)
                {
                    col[writeY] = stack;
                    col[readY] = new BlockStack(); // очищаем старую позицию
                }
                writeY++;
            }
        }

        // Очищаем верхние пустые ячейки
        for (int y = writeY; y < height; y++)
        {
            col[y] = new BlockStack();
        }

        OnGridChanged?.Invoke();
    }

    // // ОДНОВРЕМЕННАЯ анимация всех кубов
    // public IEnumerator AnimateAllCubes(float duration = 0.15f)
    // {
    //     float t = 0f;

    //     Dictionary<GameObject, Vector3> start = new();
    //     Dictionary<GameObject, Vector3> target = new();

    //     for (int x = 0; x < width; x++)
    //     for (int y = 0; y < height; y++)
    //     {
    //         var cube = gridCells[x][y];
    //         if (!cube) continue; // ВАЖНО: Unity null-check

    //         start[cube] = cube.transform.position;
    //         target[cube] = gridOrigin + new Vector3(x * cellSize, y * cellSize, 0);
    //     }

    //     while (t < 1f)
    //     {
    //         t += Time.deltaTime / duration;
    //         float ease = Mathf.SmoothStep(0, 1, t);

    //         foreach (var kv in start)
    //         {
    //             if (!kv.Key) continue; // <-- защита от Destroy
                
    //             kv.Key.transform.position =
    //                 Vector3.Lerp(kv.Value, target[kv.Key], ease);
    //         }
    //         yield return null;
    //     }

    //     foreach (var kv in target)
    //     {
    //         if (!kv.Key) continue;
    //         kv.Key.transform.position = kv.Value;
    //     }

    //     OnAllColumnsShiftedComplete?.Invoke();
    // }

    public IEnumerator AnimateAllCubes(float duration = 0.15f)
    {
        var startPositions = new Dictionary<GameObject, Vector3>();
        var targetPositions = new Dictionary<GameObject, Vector3>();

        // Собираем все существующие блоки и их целевые позиции
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var stack = gridCells[x][y];
                if (stack.HasBase)
                {
                    Vector3 target = gridOrigin + new Vector3(x * cellSize, y * cellSize, 0);
                    startPositions[stack.baseBlock] = stack.baseBlock.transform.position;
                    targetPositions[stack.baseBlock] = target;
                }
                if (stack.HasTop)
                {
                    Vector3 target = gridOrigin + new Vector3(x * cellSize, y * cellSize, 0) + Vector3.forward * -0.1f;
                    startPositions[stack.topBlock] = stack.topBlock.transform.position;
                    targetPositions[stack.topBlock] = target;
                }
            }
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float ease = Mathf.SmoothStep(0, 1, t);

            foreach (var block in startPositions.Keys)
            {
                if (block == null) continue;
                block.transform.position = Vector3.Lerp(startPositions[block], targetPositions[block], ease);
            }
            yield return null;
        }

        // Финальная точная установка
        foreach (var block in startPositions.Keys)
        {
            if (block == null) continue;
            block.transform.position = targetPositions[block];
        }

        OnAllColumnsShiftedComplete?.Invoke();
    }

    // public int GetRemainingBlocksCount()
    // {
    //     int count = 0;

    //     for (int x = 0; x < width; x++)
    //     {
    //         for (int y = 0; y < height; y++)
    //         {
    //             if (gridCells[x][y] != null)
    //                 count++;
    //         }
    //     }

    //     return count;
    // }

    public int GetRemainingBlocksCount()
    {
        int count = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var stack = gridCells[x][y];
                count += (stack.HasBase ? 1 : 0) + (stack.HasTop ? 1 : 0);
            }
        return count;
    }

    // Внутри класса GridSystem
    public bool TryRemoveBlock(GameObject block)
    {
        if (block == null) return false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var stack = gridCells[x][y];
                if (stack == null || stack.IsEmpty) continue;

                if (stack.topBlock == block)
                {
                    Destroy(stack.topBlock);          // ← так
                    stack.topBlock = null;
                    OnGridChanged?.Invoke();
                    return true;
                }
                else if (stack.baseBlock == block)
                {
                    // Удаляем верхний, если он есть (опционально)
                    if (stack.topBlock != null)
                    {
                        Destroy(stack.topBlock);
                        stack.topBlock = null;
                    }
                    Destroy(stack.baseBlock);         // ← так
                    stack.baseBlock = null;
                    OnGridChanged?.Invoke();
                    return true;
                }
            }
        }
        return false;
    }

}

