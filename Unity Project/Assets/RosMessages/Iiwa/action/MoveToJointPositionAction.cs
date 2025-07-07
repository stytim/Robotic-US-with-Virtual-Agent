using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;


namespace RosMessageTypes.Iiwa
{
    public class MoveToJointPositionAction : Action<MoveToJointPositionActionGoal, MoveToJointPositionActionResult, MoveToJointPositionActionFeedback, MoveToJointPositionGoal, MoveToJointPositionResult, MoveToJointPositionFeedback>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveToJointPositionAction";
        public override string RosMessageName => k_RosMessageName;


        public MoveToJointPositionAction() : base()
        {
            this.action_goal = new MoveToJointPositionActionGoal();
            this.action_result = new MoveToJointPositionActionResult();
            this.action_feedback = new MoveToJointPositionActionFeedback();
        }

        public static MoveToJointPositionAction Deserialize(MessageDeserializer deserializer) => new MoveToJointPositionAction(deserializer);

        MoveToJointPositionAction(MessageDeserializer deserializer)
        {
            this.action_goal = MoveToJointPositionActionGoal.Deserialize(deserializer);
            this.action_result = MoveToJointPositionActionResult.Deserialize(deserializer);
            this.action_feedback = MoveToJointPositionActionFeedback.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

    }
}
