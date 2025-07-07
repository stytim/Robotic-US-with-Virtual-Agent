using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Iiwa
{
    public class MoveToJointPositionActionFeedback : ActionFeedback<MoveToJointPositionFeedback>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveToJointPositionActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public MoveToJointPositionActionFeedback() : base()
        {
            this.feedback = new MoveToJointPositionFeedback();
        }

        public MoveToJointPositionActionFeedback(HeaderMsg header, GoalStatusMsg status, MoveToJointPositionFeedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static MoveToJointPositionActionFeedback Deserialize(MessageDeserializer deserializer) => new MoveToJointPositionActionFeedback(deserializer);

        MoveToJointPositionActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = MoveToJointPositionFeedback.Deserialize(deserializer);
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
