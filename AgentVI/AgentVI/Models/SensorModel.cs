#if DPROXY
using DummyProxy;
#else
using AgentVI.Converters;
using InnoviApiProxy;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using static InnoviApiProxy.Sensor;

namespace AgentVI.Models
{
    public class SensorModel : INotifyPropertyChanged
    {
        private SensorModel()
        {

        }

        internal static SensorModel FactoryMethod(Sensor i_Sensor)
        {
            SensorModel res = new SensorModel()
            {
                Sensor = i_Sensor,
                SensorName = i_Sensor.Name,
                SensorHealth = i_Sensor.Status,
                SensorHealthHistory = i_Sensor.SensorHealthArray,
                SensorImage = i_Sensor.ReferenceImage
            };

            return res.UpdateSensorsHealthStatus();
        }

        public enum ESensorColorHealth
        {
            Black,
            Grey,
            Red,
            Yellow,
            Green
        };
        public event PropertyChangedEventHandler PropertyChanged;

        private Sensor m_Sensor;
        public Sensor Sensor
        {
            get { return m_Sensor; }
            private set
            {
                m_Sensor = value;
                OnPropertyChanged("Sensor");
            }
        }

        private string m_SensorName;
        public string SensorName
        {
            get { return m_SensorName; }
            private set
            {
                m_SensorName = value;
                OnPropertyChanged("SensorName");
            }
        }

        private eSensorStatus m_SensorHealth;
        public eSensorStatus SensorHealth
        {
            get { return m_SensorHealth; }
            private set
            {
                m_SensorHealth = value;
                OnPropertyChanged("SensorHealth");
            }
        }
        
        private List<Health> m_ensorHealthHistory;
        public List<Health> SensorHealthHistory
        {
            get { return m_ensorHealthHistory; }
            private set
            {
                m_ensorHealthHistory = value;
                OnPropertyChanged("SensorHealthHistory");
            }
        }

        private ESensorColorHealth m_SensorHealthStatus;
        public  ESensorColorHealth SensorHealthStatus
        {
            get { return m_SensorHealthStatus; }
            private set
            {
                m_SensorHealthStatus = value;
                OnPropertyChanged("SensorHealthStatus");
            }
        }

        private string m_SensorImage;
        public  string SensorImage
        {
            get { return m_SensorImage; }
            private set
            {
                m_SensorImage = value;
                OnPropertyChanged("SensorImage");
            }
        }

        private ESensorColorHealth GetEnumForValue(eSensorStatus i_value)
        {
            ESensorColorHealth res;

            switch(i_value)
            {
                case eSensorStatus.Active:
                    res = ESensorColorHealth.Green;
                    break;
                case eSensorStatus.Warning:
                    res = ESensorColorHealth.Yellow;
                    break;
                case eSensorStatus.Error:
                    res = ESensorColorHealth.Red;
                    break;
                case eSensorStatus.Inactive:
                    res = ESensorColorHealth.Black;
                    break;
                case eSensorStatus.Undefined:
                    res = ESensorColorHealth.Grey;
                    break;
                default:
                    res = ESensorColorHealth.Grey;
                    break;
            }

            return res;
        }

        private SensorModel UpdateSensorsHealthStatus()
        {
            SensorHealthStatus = GetEnumForValue(SensorHealth);
            return this;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}








