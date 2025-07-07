using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Iiwa
{
    public class MoveAlongSplineActionFeedback : ActionFeedback<MoveAlongSplineFeedback>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveAlongSplineActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public MoveAlongSplineActionFeedback() : base()
        {
            this.feedback = new MoveAlongSplineFeedback();
        }

        public MoveAlongSplineActionFeedback(HeaderMsg header, GoalStatusMsg status, MoveAlongSplineFeedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static MoveAlongSplineActionFeedback Deserialize(MessageDeserializer deserializer) => new MoveAlongSplineActionFeedback(deserializer);

        MoveAlongSplineActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = MoveAlongSplineFeedback.Deserialize(deserializer);
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
