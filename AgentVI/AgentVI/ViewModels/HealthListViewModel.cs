#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using System.ComponentModel;
using System.Collections.Generic;

using Xamarin.Forms;
using AgentVI.Models;

using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AgentVI.ViewModels
{
    public class HealthListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private SensorModel m_SensorModel;
        public SensorModel SensorModel
        {
            get { return m_SensorModel; }
            private set
            {
                m_SensorModel = value;
                OnPropertyChanged("SensorModel");
            }
        }

        private List<HealthModel> m_HealthsList;
        public List<HealthModel> HealthsList
        {
            get { return m_HealthsList; }
            private set
            {
                m_HealthsList = value;
                OnPropertyChanged("HealthsList");
            }
        }

        public HealthListViewModel(SensorModel i_SensorModel)
        {
            SensorModel = i_SensorModel;
            HealthsList = GetHealthSensorList(i_SensorModel);
        }

        private List<HealthModel> GetHealthSensorList(SensorModel i_Sensor)
        {
            List<HealthModel> healthList = new List<HealthModel>();
            List<Sensor.Health> healths = i_Sensor.SensorHealthHistory;
            foreach (Sensor.Health sh in healths)
            {
                HealthModel hm = new HealthModel();
                hm.HealthTime = sh.StatusTimeStamp;
                hm.HealthDescription = sh.DetailedDescription;
                if (sh != healths[healths.Count - 1])
                {
                    long currentTime = sh.StatusTimeStamp;
                    long nextTime = healths[healths.IndexOf(sh) + 1].StatusTimeStamp;
                    hm.HealthDuration = getHealthDurationTime(currentTime, nextTime); //The duration time displayed as hh:mm format
                }
                healthList.Add(hm);
            }
            return healthList;
        }

        private string getHealthDurationTime(long i_currentTime, long i_nextTime)
        {
            int minutes, hours;
            long duration = i_nextTime - i_currentTime;
            TimeSpan timeDuration = new TimeSpan(duration * 10000);
            StringBuilder durationTimeText = new StringBuilder();
            minutes = (int)timeDuration.TotalMinutes;
            hours = minutes / 60;
            minutes = minutes % 60;
            if (hours < 10)
            {
                durationTimeText.Append("0" + hours + ":");
            }
            else if (hours < 100)
            {
                durationTimeText.Append(hours + ":");
            }
            else
            {
                durationTimeText.Append("00:");
            }
            if (minutes < 10)
            {
                durationTimeText.Append("0" + minutes);
            }
            else if (minutes < 100)
            {
                durationTimeText.Append(minutes);
            }
            else //never get here (cause always 'minutes' mod 60 < 60)
            {
                durationTimeText.Append("00");
            }

            return durationTimeText.ToString();
        }

        public void UpdateHealthList() //need to implement!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        {
            //HealthsList.Clear();
            //HealthsList = GetHealthSensorList(SensorModel);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}