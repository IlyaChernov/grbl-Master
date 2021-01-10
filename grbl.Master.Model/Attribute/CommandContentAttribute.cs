namespace grbl.Master.Model.Attribute
{
    using grbl.Master.Model.Enum;
    using System;
    using System.Linq;

    public static class ExtensionOperation
    {
        public static int? GetCommandContentInt(this GrblCommands commandType)
        {
            var enumType = commandType.GetType();
            var memberInfos = enumType.GetMember(commandType.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var attributes = enumValueMemberInfo?.GetCustomAttributes(typeof(CommandContentIntAttribute), false);
            return  (attributes?.FirstOrDefault() as CommandContentIntAttribute)?.Command;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CommandContentIntAttribute : CommandContentAttribute
    {
        public int Command { get; internal set; }

        public CommandContentIntAttribute(int command)
        {
            Command = command;
        }
    }

    public class CommandContentAttribute : Attribute
    {
    }


}
