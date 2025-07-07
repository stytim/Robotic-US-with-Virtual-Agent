using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Iiwa
{
    public class MoveToCartesianPoseActionFeedback : ActionFeedback<MoveToCartesianPoseFeedback>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveToCartesianPoseActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public MoveToCartesianPoseActionFeedback() : base()
        {
            this.feedback = new MoveToCartesianPoseFeedback();
        }

        public MoveToCartesianPoseActionFeedback(HeaderMsg header, GoalStatusMsg status, MoveToCartesianPoseFeedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static MoveToCartesianPoseActionFeedback Deserialize(MessageDeserializer deserializer) => new MoveToCartesianPoseActionFeedback(deserializer);

        MoveToCartesianPoseActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = MoveToCartesianPoseFeedback.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.feedback);
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
