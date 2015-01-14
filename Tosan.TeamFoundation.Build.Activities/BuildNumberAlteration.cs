using System.Activities;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Build.Client;

namespace Tosan.TeamFoundation.Build.Activities
{
    [BuildActivity(HostEnvironmentOption.All)]
    public class BuildNumberAlteration : CodeActivity
    {
        //[RequiredArgument, DisplayName("Build Type"), Description("Which part of build number should be changed?")]
        public InArgument<string> BuildType { get; set; }
        //[RequiredArgument, DisplayName("Build Number"), Description("The build number itself.")]
        public InArgument<string> BuildNumber { get; set; }
        public OutArgument<string> AlteredBuildNumber { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            var buildDetails = context.GetExtension<IBuildDetail>();

            var buildNumber = buildDetails.BuildNumber.ToString();
            const string regExPattern = @"\d+\.\d+\.\d+\.\d+";

            var regEx = new Regex(regExPattern);

            var versionNumber = regEx.Match(buildNumber);

            if (!versionNumber.Success) return;

            var versionNumberByPart = versionNumber.Value.Split('.');
            switch (BuildType.Get(context).ToLower())
            {

                case "major":
                    {
                        versionNumberByPart[0] = (int.Parse(versionNumberByPart[0]) + 1).ToString();
                        break;
                    }
                case "minor":
                    {
                        versionNumberByPart[1] = (int.Parse(versionNumberByPart[1]) + 1).ToString();
                        break;
                    }
                case "path":
                    {
                        versionNumberByPart[2] = (int.Parse(versionNumberByPart[2]) + 1).ToString();
                        break;
                    }
                case "build":
                    {
                        versionNumberByPart[3] = (int.Parse(versionNumberByPart[3]) + 1).ToString();
                        break;
                    }
            }
            var versionNumberPartedJoined = string.Format("{0}.{1}.{2}.{3}", versionNumberByPart[0], versionNumberByPart[1],
                versionNumberByPart[2], versionNumberByPart[3]);

            var versionNumberResult = regEx.Replace(buildNumber, versionNumberPartedJoined);
            AlteredBuildNumber.Set(context, versionNumberResult);
        }
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.RequireExtension(typeof(IBuildDetail));
        } 
    }
}
