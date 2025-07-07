using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Iiwa
{
    public class MoveToCartesianPoseActionResult : ActionResult<MoveToCartesianPoseResult>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveToCartesianPoseActionResult";
        public override string RosMessageName => k_RosMessageName;


        public MoveToCartesianPoseActionResult() : base()
        {
            this.result = new MoveToCartesianPoseResult();
        }

        public MoveToCartesianPoseActionResult(HeaderMsg header, GoalStatusMsg status, MoveToCartesianPoseResult result) : base(header, status)
        {
            this.result = result;
        }
        public static MoveToCartesianPoseActionResult Deserialize(MessageDeserializer deserializer) => new MoveToCartesianPoseActionResult(deserializer);

        MoveToCartesianPoseActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = MoveToCartesianPoseResult.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.result);
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
