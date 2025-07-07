using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.UI;

public class CompressedImageStreamSubscriber : MonoBehaviour
{
    ROSConnection m_Ros;
    public string m_TopicName = "/image/compressed";
    private Texture2D texture;
    public RawImage image;
    // Start is called before the first frame update
    void Start()
    {
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.Subscribe<CompressedImageMsg>(m_TopicName, SetImage);
        texture = new Texture2D(1, 1);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetImage(CompressedImageMsg msg)
    {
        // Get the image data from the message
        byte[] imageData = msg.data;
        // Load the image data into the texture
        texture.LoadImage(imageData);
        // Set the texture to the material
        image.texture = texture;
    }
}
