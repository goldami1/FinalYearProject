﻿using System;
using System.Collections.Generic;
using System.Text;

namespace InnoviApiProxy
{
    public class SensorEvent : InnoviObject
    {
        private int m_EventId;
        private int m_SensorId;
        private int m_AccountId;
        public ulong StartTime { get; set; }
        public int ObjectType { get; set; }
        public int RuleId { get; set; }

        public string Name { get; private set; }
        public string ImagePath { get; set; }
        public string ClipPath { get; set; }
        public Sensor.eSensorEventTag Tag { get; set; }
    }
}
