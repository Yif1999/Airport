using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ExampleEditorScript : MonoBehaviour
{
    [MenuItem("Tools/3DTexture")]
    static void CreateTexture3D()
    {
        // 配置纹理
        int size = 64;
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode =  TextureWrapMode.Clamp;

        // 创建纹理并应用配置
        Texture3D texture = new Texture3D(size, size, size, format, false);
        texture.wrapMode = wrapMode;

        // 创建 3 维数组以存储颜色数据
        Color[] colors = new Color[size * size * size];
        
        // 读取 numpy 数组文件
        string path = "Assets/Dataset/density3d.txt";
        var streamReader = new StreamReader(path);
        float[] values = new float[size * size * size];
        int id = 0;
        while (!streamReader.EndOfStream)
        {
            string line = streamReader.ReadLine();
            if (line != null)
            {
                values[id] = Convert.ToSingle(line);
                id++;
            }
        }
        streamReader.Close();
        Debug.Log(values.Length);
        
        // 填充数组，使纹理的 x、y 和 z 值映射为红色、蓝色和绿色
        float inverseResolution = 1.0f / (size - 1.0f);
        for (int z = 0; z < size; z++)
        {
            int zOffset = z * size * size;
            for (int y = 0; y < size; y++)
            {
                int yOffset = y * size;
                for (int x = 0; x < size; x++)
                {
                    colors[x + yOffset + zOffset] = new Color(
                        1,
                        1, 
                        1,
                        Mathf.Max(0.0f,values[x + yOffset + zOffset]));
                }
            }
        }

        // 将颜色值复制到纹理
        texture.SetPixels(colors);

        // 将更改应用到纹理，然后将更新的纹理上传到 GPU
        texture.Apply();        

        // 将纹理保存到 Unity 项目
        AssetDatabase.CreateAsset(texture, "Assets/Textures/3DTexture.asset");
    }
}