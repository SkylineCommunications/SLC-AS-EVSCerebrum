namespace Skyline.DataMiner.CommunityLibrary.Automation.Extensions
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.CommunityLibrary.Automation.Api;

    #region Classes

    public static class EngineExtensions
    {
        #region Methods

        public static CorrelationAlarmInfo GetCorrelationAlarmInfo(this Engine engine)
        {
            var scriptParam = engine.GetScriptParam(65006);
            string[] alarmInfo = scriptParam.Value.Split('|');

            return CorrelationAlarmInfo.Load(alarmInfo);
        }

        #endregion
    }

    #endregion
}