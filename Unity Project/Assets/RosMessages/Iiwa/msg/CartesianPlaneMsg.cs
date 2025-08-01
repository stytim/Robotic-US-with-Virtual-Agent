//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Iiwa
{
    [Serializable]
    public class CartesianPlaneMsg : Message
    {
        public const string k_RosMessageName = "iiwa_msgs/CartesianPlane";
        public override string RosMessageName => k_RosMessageName;

        public const int XY = 1;
        public const int XZ = 2;
        public const int YZ = 3;

        public CartesianPlaneMsg()
        {
        }
        public static CartesianPlaneMsg Deserialize(MessageDeserializer deserializer) => new CartesianPlaneMsg(deserializer);

        private CartesianPlaneMsg(MessageDeserializer deserializer)
        {
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
        }

        public override string ToString()
        {
            return "CartesianPlaneMsg: ";
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
