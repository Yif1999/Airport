using System;
using System.IO;
using UnityEngine;

public class SHArray : MonoBehaviour
{
    public Transform SHball;
    public Transform box;
    public String filePath;
    private int instances = 16*16*16;
    void Start()
    {
        float[][] shBuffer = LoadSHCoefficientsFromFile(filePath);
        // Debug.Log(shBuffer.Length);
        
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
                    shCoefficients[0] = new Vector4(0,0,0,0);
                    shCoefficients[1] = new Vector4(0,0,0,0);
                    shCoefficients[2] = new Vector4(0, 0, 0, 0);
                    properties.SetVectorArray("_SHCoefficients", shCoefficients);
                    instance.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
                }
            }
            
            
        }
    }
    
    float[][] LoadSHCoefficientsFromFile(string path)
    {
        // 检查文件是否存在
        if (!File.Exists(path))
        {
            Debug.LogError("File does not exist: " + path);
            return null;
        }

        string[] lines = File.ReadAllLines(path);
        float[][] coefficients = new float[lines.Length][];

        for (int i = 0; i < lines.Length; i++)
        {
            string[] stringValues = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            coefficients[i] = new float[stringValues.Length];

            for (int j = 0; j < stringValues.Length; j++)
            {
                if (float.TryParse(stringValues[j], out float value))
                {
                    coefficients[i][j] = value;
                }
                else
                {
                    Debug.LogError("Parsing failed at line " + (i + 1) + " value " + (j + 1));
                }
            }
        }

        return coefficients;
    }
    
}