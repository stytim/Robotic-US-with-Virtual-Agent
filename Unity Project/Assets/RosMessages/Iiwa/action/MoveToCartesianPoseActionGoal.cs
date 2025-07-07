using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Iiwa
{
    public class MoveToCartesianPoseActionGoal : ActionGoal<MoveToCartesianPoseGoal>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveToCartesianPoseActionGoal";
        public override string RosMessageName => k_RosMessageName;


        public MoveToCartesianPoseActionGoal() : base()
        {
            this.goal = new MoveToCartesianPoseGoal();
        }

        public MoveToCartesianPoseActionGoal(HeaderMsg header, GoalIDMsg goal_id, MoveToCartesianPoseGoal goal) : base(header, goal_id)
        {
            this.goal = goal;
        }
        public static MoveToCartesianPoseActionGoal Deserialize(MessageDeserializer deserializer) => new MoveToCartesianPoseActionGoal(deserializer);

        MoveToCartesianPoseActionGoal(MessageDeserializer deserializer) : base(deserializer)
        {
            this.goal = MoveToCartesianPoseGoal.Deserialize(deserializer);
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
