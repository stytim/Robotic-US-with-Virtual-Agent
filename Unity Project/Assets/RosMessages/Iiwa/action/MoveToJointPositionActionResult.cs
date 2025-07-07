using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Iiwa
{
    public class MoveToJointPositionActionResult : ActionResult<MoveToJointPositionResult>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveToJointPositionActionResult";
        public override string RosMessageName => k_RosMessageName;


        public MoveToJointPositionActionResult() : base()
        {
            this.result = new MoveToJointPositionResult();
        }

        public MoveToJointPositionActionResult(HeaderMsg header, GoalStatusMsg status, MoveToJointPositionResult result) : base(header, status)
        {
            this.result = result;
        }
        public static MoveToJointPositionActionResult Deserialize(MessageDeserializer deserializer) => new MoveToJointPositionActionResult(deserializer);

        MoveToJointPositionActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = MoveToJointPositionResult.Deserialize(deserializer);
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
