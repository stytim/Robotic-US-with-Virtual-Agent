using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Iiwa
{
    public class MoveToJointPositionActionGoal : ActionGoal<MoveToJointPositionGoal>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveToJointPositionActionGoal";
        public override string RosMessageName => k_RosMessageName;


        public MoveToJointPositionActionGoal() : base()
        {
            this.goal = new MoveToJointPositionGoal();
        }

        public MoveToJointPositionActionGoal(HeaderMsg header, GoalIDMsg goal_id, MoveToJointPositionGoal goal) : base(header, goal_id)
        {
            this.goal = goal;
        }
        public static MoveToJointPositionActionGoal Deserialize(MessageDeserializer deserializer) => new MoveToJointPositionActionGoal(deserializer);

        MoveToJointPositionActionGoal(MessageDeserializer deserializer) : base(deserializer)
        {
            this.goal = MoveToJointPositionGoal.Deserialize(deserializer);
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
