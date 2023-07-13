namespace Skyline.DataMiner.CommunityLibrary.Automation.Api
{
    using System;
    using System.Collections.Generic;

    #region Classes

    public class CorrelationAlarmInfo
    {
        #region Constructors

        private CorrelationAlarmInfo(string[] alarmInfo)
        {
            Parse(alarmInfo);
        }

        #endregion

        #region Classes

        public class Property
        {
            #region Constructors

            internal Property(string name, string value)
            {
                Name = name;
                Value = value;
            }

            #endregion

            #region Properties

            public string Name { get; }

            public string Value { get; }

            #endregion
        }

        #endregion

        #region Properties

        public int AlarmId { get; private set; }

        public int DmaId { get; private set; }

        public int ElementId { get; private set; }

        public int ParameterId { get; private set; }

        public string ParameterIdx { get; private set; }

        public int RootAlarmId { get; private set; }

        public int PrevAlarmId { get; private set; }

        public int Severity { get; private set; }

        public int Type { get; private set; }

        public int Status { get; private set; }

        public string AlarmValue { get; private set; }

        public DateTime AlarmTime { get; private set; }

        public int ServiceRca { get; private set; }

        public int ElementRca { get; private set; }

        public int ParameterRca { get; private set; }

        public int SeverityRange { get; private set; }

        public int SourceId { get; private set; }

        public int UserStatus { get; private set; }

        public string Owner { get; private set; }

        public string ImpactedServices { get; private set; }

        public IReadOnlyList<Property> Properties { get; private set; }

        #endregion

        #region Methods

        internal static CorrelationAlarmInfo Load(string[] alarmInfo)
        {
            if (alarmInfo == null)
            {
                throw new ArgumentNullException(nameof(alarmInfo));
            }

            if (alarmInfo.Length < 21)
            {
                throw new ArgumentException("Invalid length.", nameof(alarmInfo));
            }

            return new CorrelationAlarmInfo(alarmInfo);
        }

        private void Parse(string[] alarmInfo)
        {
            AlarmId = Convert.ToInt32(alarmInfo[0]);
            DmaId = Convert.ToInt32(alarmInfo[1]);
            ElementId = Convert.ToInt32(alarmInfo[2]);
            ParameterId = Convert.ToInt32(alarmInfo[3]);
            ParameterIdx = alarmInfo[4];
            RootAlarmId = Convert.ToInt32(alarmInfo[5]);
            PrevAlarmId = Convert.ToInt32(alarmInfo[6]);
            Severity = Convert.ToInt32(alarmInfo[7]);
            Type = Convert.ToInt32(alarmInfo[8]);
            Status = Convert.ToInt32(alarmInfo[9]);
            AlarmValue = alarmInfo[10];
            AlarmTime = DateTime.Parse(alarmInfo[11]);
            ServiceRca = Convert.ToInt32(alarmInfo[12]);
            ElementRca = Convert.ToInt32(alarmInfo[13]);
            ParameterRca = Convert.ToInt32(alarmInfo[14]);
            SeverityRange = Convert.ToInt32(alarmInfo[15]);
            SourceId = Convert.ToInt32(alarmInfo[16]);
            UserStatus = Convert.ToInt32(alarmInfo[17]);
            Owner = alarmInfo[18];
            ImpactedServices = alarmInfo[19];

            ParseProperties(alarmInfo);
        }

        private void ParseProperties(string[] alarmInfo)
        {
            var amount = Convert.ToInt32(alarmInfo[20]);
            var properties = new List<Property>(amount);

            for (var i = 0; i < amount; i++)
            {
                int propertyIdx = 21 + (i * 2);
                var property = new Property(
                    alarmInfo[propertyIdx],
                    alarmInfo[propertyIdx + 1]);

                properties.Add(property);
            }

            Properties = properties.AsReadOnly();
        }

        #endregion
    }

    #endregion
}