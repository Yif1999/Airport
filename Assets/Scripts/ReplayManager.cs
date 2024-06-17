using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class ReplayManager : MonoBehaviour
{
    public String filePath;
    [FormerlySerializedAs("RPYoffset")] public Vector3 rpyOffset;
    [FormerlySerializedAs("RPYscale")] public Vector3 rpyScale;
    [Range(0,1)]
    public float damping = 1.0f;
    private Vector3 position;
    private Quaternion rotation;
    private List<string[]> rowData = new List<string[]>();
    private int currentLine = 1;
    private float nextTime = 0;

    void Start()
    {
        ReadCSVFile(filePath);
    }

    // 读取CSV文件
    void ReadCSVFile(string filePath)
    {
        using (StreamReader sr = File.OpenText(filePath))
        {
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                // 使用‘，’将每一行数据进行分割
                string[] rowDataArrays = line.Split(',');
                
                // 将该行数据加入到list中
                rowData.Add(rowDataArrays);
            }
        }
    }

    void Update()
    {
        if (Time.time > nextTime)
        {
            
            position = new Vector3(float.Parse(rowData[currentLine][4]),
                                    float.Parse(rowData[currentLine][6]),
                                    -float.Parse(rowData[currentLine][5]));
            rotation = Quaternion.Euler(new Vector3(
                rpyOffset.z+rpyScale.z*float.Parse(rowData[currentLine][3]), 
                rpyOffset.y+rpyScale.y*float.Parse(rowData[currentLine][2]),
                rpyOffset.x+rpyScale.x*float.Parse(rowData[currentLine][1])));
            currentLine++;
            nextTime = float.Parse(rowData[currentLine][0]);
        }
        
        transform.position = Vector3.Lerp(transform.position, position, Mathf.Max(0.01f,1 - damping));
        transform.rotation = Quaternion.Slerp(transform.rotation,rotation, Mathf.Max(0.01f,1 - damping));
    }
}

