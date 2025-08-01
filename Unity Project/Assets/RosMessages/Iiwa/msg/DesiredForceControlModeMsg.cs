//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Iiwa
{
    [Serializable]
    public class DesiredForceControlModeMsg : Message
    {
        public const string k_RosMessageName = "iiwa_msgs/DesiredForceControlMode";
        public override string RosMessageName => k_RosMessageName;

        //  The degree of freedom on which the desired force
        public int cartesian_dof;
        //  The value of the desired force. In [N].
        public double desired_force;
        //  The value of the stiffness. In [N/m].
        public double desired_stiffness;

        public DesiredForceControlModeMsg()
        {
            this.cartesian_dof = 0;
            this.desired_force = 0.0;
            this.desired_stiffness = 0.0;
        }

        public DesiredForceControlModeMsg(int cartesian_dof, double desired_force, double desired_stiffness)
        {
            this.cartesian_dof = cartesian_dof;
            this.desired_force = desired_force;
            this.desired_stiffness = desired_stiffness;
        }

        public static DesiredForceControlModeMsg Deserialize(MessageDeserializer deserializer) => new DesiredForceControlModeMsg(deserializer);

        private DesiredForceControlModeMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.cartesian_dof);
            deserializer.Read(out this.desired_force);
            deserializer.Read(out this.desired_stiffness);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.cartesian_dof);
            serializer.Write(this.desired_force);
            serializer.Write(this.desired_stiffness);
        }

        public override string ToString()
        {
            return "DesiredForceControlModeMsg: " +
            "\ncartesian_dof: " + cartesian_dof.ToString() +
            "\ndesired_force: " + desired_force.ToString() +
            "\ndesired_stiffness: " + desired_stiffness.ToString();
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
