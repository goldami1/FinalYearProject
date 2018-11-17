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
                Console.WriteLine("####Logger####   -   Getting IsClipAvailable @ Begin");
                var res = IsSensorClipAvailableLazyHelper.Value;
                Console.WriteLine("####Logger####   -   Getting IsClipAvailable @ End");
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
                Console.WriteLine("####Logger####   -   Getting Sensor @ Begin");
                var res = m_SensorHolder == null ? SensorLazyHelper.Value : m_SensorHolder;
                Console.WriteLine("####Logger####   -   Getting Sensor @ End");
                return res;
            }
        }
        public SensorEvent SensorEvent { get; private set; }

        internal static EventModel FactoryMethod(SensorEvent i_SensorEvent)
        {
            Console.WriteLine("####Logger####   -   FactoryMethod @ Begin");
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
            Console.WriteLine("####Logger####   -   FactoryMethod @ End");
            return res;
        }
    }
}