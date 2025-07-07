using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;


namespace RosMessageTypes.Iiwa
{
    public class MoveToCartesianPoseAction : Action<MoveToCartesianPoseActionGoal, MoveToCartesianPoseActionResult, MoveToCartesianPoseActionFeedback, MoveToCartesianPoseGoal, MoveToCartesianPoseResult, MoveToCartesianPoseFeedback>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveToCartesianPoseAction";
        public override string RosMessageName => k_RosMessageName;


        public MoveToCartesianPoseAction() : base()
        {
            this.action_goal = new MoveToCartesianPoseActionGoal();
            this.action_result = new MoveToCartesianPoseActionResult();
            this.action_feedback = new MoveToCartesianPoseActionFeedback();
        }

        public static MoveToCartesianPoseAction Deserialize(MessageDeserializer deserializer) => new MoveToCartesianPoseAction(deserializer);

        MoveToCartesianPoseAction(MessageDeserializer deserializer)
        {
            this.action_goal = MoveToCartesianPoseActionGoal.Deserialize(deserializer);
            this.action_result = MoveToCartesianPoseActionResult.Deserialize(deserializer);
            this.action_feedback = MoveToCartesianPoseActionFeedback.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

    }
}
