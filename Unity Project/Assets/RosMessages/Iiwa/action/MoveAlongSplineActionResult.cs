using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Iiwa
{
    public class MoveAlongSplineActionResult : ActionResult<MoveAlongSplineResult>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveAlongSplineActionResult";
        public override string RosMessageName => k_RosMessageName;


        public MoveAlongSplineActionResult() : base()
        {
            this.result = new MoveAlongSplineResult();
        }

        public MoveAlongSplineActionResult(HeaderMsg header, GoalStatusMsg status, MoveAlongSplineResult result) : base(header, status)
        {
            this.result = result;
        }
        public static MoveAlongSplineActionResult Deserialize(MessageDeserializer deserializer) => new MoveAlongSplineActionResult(deserializer);

        MoveAlongSplineActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = MoveAlongSplineResult.Deserialize(deserializer);
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
