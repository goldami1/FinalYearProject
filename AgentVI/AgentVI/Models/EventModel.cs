using System;
using System.Threading.Tasks;
using InnoviApiProxy;

namespace AgentVI.Models
{
    public class EventModel
    {
        public string SensorName { get; private set; }
        public SensorEvent.eBehaviorType SensorEventRuleName { get; private set; }
        public ulong SensorEventDateTime { get; private set; }
        public string SensorEventImage { get; private set; }
        public string SensorEventClip { get; private set; }
        public SensorEvent.eObjectType SensorEventObjectType { get; private set; }
        public Sensor.eSensorEventTag SensorEventTag { get; private set; }
        private Lazy<bool> IsSensorClipAvailableLazyHelper { get; set; }
        public bool IsClipAvailable
        {
            get
            {
                var res = IsSensorClipAvailableLazyHelper.Value;
                return res;
            }
        }
        private Sensor m_SensorHolder;
        private Lazy<Sensor> _SensorLazyHelper;
        private Lazy<Sensor> SensorLazyHelper
        {
            get => _SensorLazyHelper;
            set
            {
                _SensorLazyHelper = value;
                Task.Factory.StartNew(() => m_SensorHolder = _SensorLazyHelper.Value);
            }
        }
        public Sensor Sensor
        {
            get
            {
                var res = m_SensorHolder == null ? SensorLazyHelper.Value : m_SensorHolder;
                return res;
            }
        }
        public SensorEvent SensorEvent { get; private set; }

        internal static EventModel FactoryMethod(SensorEvent i_SensorEvent)
        {
            EventModel res = new EventModel()
            {
                SensorName = i_SensorEvent.SensorName,
                SensorEventClip = i_SensorEvent.ClipPath,
                SensorEventDateTime = i_SensorEvent.StartTime,
                SensorEventImage = i_SensorEvent.ImagePath,
                SensorEventRuleName = i_SensorEvent.RuleName,
                SensorEventObjectType = i_SensorEvent.ObjectType,
                SensorEventTag = i_SensorEvent.Tag,
                SensorLazyHelper = new Lazy<Sensor>(() => i_SensorEvent.EventSensor),
                IsSensorClipAvailableLazyHelper = new Lazy<bool>(() => i_SensorEvent.IsClipAvailable),
                SensorEvent = i_SensorEvent
            };
            Task.Factory.StartNew(() =>
            {
                bool buffer = res.IsClipAvailable;
            });
            return res;
        }
    }
}