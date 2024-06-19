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
        
        Vector3 boxSize = box.localScale; // 获取Box的大小
        int instancePerSide = Mathf.FloorToInt(Mathf.Pow(instances, 1f/3f)); // 计算每边需要排布的球体数量

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
                    
                    int index = x + y * instancePerSide + z * instancePerSide * instancePerSide;
                    Matrix4x4 shCoefficients = new Matrix4x4(
                        new Vector4(shBuffer[index][0], shBuffer[index][1], shBuffer[index][2], shBuffer[index][3]), // 第1列
                        new Vector4(shBuffer[index][4], shBuffer[index][5], shBuffer[index][6], shBuffer[index][7]), // 第2列
                        new Vector4(shBuffer[index][8], 0f, 0f, 0f),                                                 // 第3列
                        new Vector4(0f, 0f, 0f, 1f)                                                                  // 第4列
                    );
                    properties.SetMatrix("_SHCoefficients", shCoefficients);
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