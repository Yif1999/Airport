using UnityEngine;

public class SHArray : MonoBehaviour
{
    public Transform SHball;
    public Transform box;
    private int instances = 16*16*16;
    void Start()
    {
        Vector3 boxSize = box.localScale; // 获取Box的大小
        int instancePerSide = Mathf.CeilToInt(Mathf.Pow(instances, 1f/3f)); // 计算每边需要排布的球体数量

        // 计算均匀分布的间隔
        float spacingX = boxSize.x / instancePerSide;
        float spacingY = boxSize.y / instancePerSide;
        float spacingZ = boxSize.z / instancePerSide;

        // 生成球体阵列
        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        for (int x = 0; x < instancePerSide; x++)
        {
            for (int y = 0; y < instancePerSide; y++)
            {
                for (int z = 0; z < instancePerSide; z++)
                {
                    Transform instance = Instantiate(SHball);
                    // 计算每个实例的局部位置，使其在Box的内部均匀分布
                    Vector3 localPos = new Vector3((x + 0.5f) * spacingX, (y + 0.5f) * spacingY, (z + 0.5f) * spacingZ) - boxSize / 2 + box.position;
                    instance.localPosition = localPos;
                    instance.SetParent(transform);
                    
                    Vector4[] shCoefficients = new Vector4[3];
                    properties.SetVectorArray("_SHCoefficients", shCoefficients);
                    instance.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
                }
            }
            
            
        }
    }
}