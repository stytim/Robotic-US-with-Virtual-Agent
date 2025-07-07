using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;


namespace RosMessageTypes.Iiwa
{
    public class MoveAlongSplineAction : Action<MoveAlongSplineActionGoal, MoveAlongSplineActionResult, MoveAlongSplineActionFeedback, MoveAlongSplineGoal, MoveAlongSplineResult, MoveAlongSplineFeedback>
    {
        public const string k_RosMessageName = "iiwa_msgs/MoveAlongSplineAction";
        public override string RosMessageName => k_RosMessageName;


        public MoveAlongSplineAction() : base()
        {
            this.action_goal = new MoveAlongSplineActionGoal();
            this.action_result = new MoveAlongSplineActionResult();
            this.action_feedback = new MoveAlongSplineActionFeedback();
        }

        public static MoveAlongSplineAction Deserialize(MessageDeserializer deserializer) => new MoveAlongSplineAction(deserializer);

        MoveAlongSplineAction(MessageDeserializer deserializer)
        {
            this.action_goal = MoveAlongSplineActionGoal.Deserialize(deserializer);
            this.action_result = MoveAlongSplineActionResult.Deserialize(deserializer);
            this.action_feedback = MoveAlongSplineActionFeedback.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

    }
}
