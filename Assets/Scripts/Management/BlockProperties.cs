using UnityEngine;

public enum BlockLayer
{
    Base = 0,
    Top = 1
}
public class BlockProperties : MonoBehaviour
{
    public BlockColor colorType;
    public BlockLayer layerType;
}
