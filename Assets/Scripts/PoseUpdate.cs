using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Pose = RosMessageTypes.Std.Float32MultiArrayMsg;

public class PoseUpdate : MonoBehaviour
{
    float damping = 5.0f;
    Vector3 position;
    Quaternion rotation;

    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<Pose>("/unity/pose", PoseSync);
    }

    void PoseSync(Pose msg)
    {
        position = new Vector3((float)msg.data[0], (float)msg.data[1], (float)msg.data[2]);
        rotation = new Quaternion((float)msg.data[3], (float)msg.data[4], (float)msg.data[5], (float)msg.data[6]);
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * damping);
        transform.rotation = Quaternion.Slerp(transform.rotation,rotation,Time.deltaTime*damping);
    }
}