using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BlockStack
{
    public GameObject baseBlock;   // нижний слой (Base)
    public GameObject topBlock;    // верхний слой (Top)

    public bool HasTop => topBlock != null;
    public bool HasBase => baseBlock != null;
    public bool IsEmpty => !HasTop && !HasBase;

    // Самый верхний видимый блок (для матчинга, удаления и т.д.)
    public GameObject TopMost => HasTop ? topBlock : baseBlock;

    // Удаляет верхний блок и возвращает true, если что-то удалили
    public bool RemoveTop()
    {
        if (HasTop)
        {
            if (topBlock != null) Object.Destroy(topBlock);
            topBlock = null;
            return true;
        }
        return false;
    }
}