using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Iiwa
{
    public class MoveAlongSplineActionGoal : ActionGoal<MoveAlongSplineGoal>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveAlongSplineActionGoal";
        public override string RosMessageName => k_RosMessageName;


        public MoveAlongSplineActionGoal() : base()
        {
            this.goal = new MoveAlongSplineGoal();
        }

        public MoveAlongSplineActionGoal(HeaderMsg header, GoalIDMsg goal_id, MoveAlongSplineGoal goal) : base(header, goal_id)
        {
            this.goal = goal;
        }
        public static MoveAlongSplineActionGoal Deserialize(MessageDeserializer deserializer) => new MoveAlongSplineActionGoal(deserializer);

        MoveAlongSplineActionGoal(MessageDeserializer deserializer) : base(deserializer)
        {
            this.goal = MoveAlongSplineGoal.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.goal_id);
            serializer.Write(this.goal);
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
