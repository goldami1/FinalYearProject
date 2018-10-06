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
                    int minutes, hours;
                    long currentTime = sh.StatusTimeStamp;
                    long nextTime = healths[healths.IndexOf(sh) + 1].StatusTimeStamp;
                    long duration = nextTime - currentTime;
                    TimeSpan dur = new TimeSpan(duration * 10000);
                    minutes = (int)dur.TotalMinutes;
                    hours = minutes / 60;
                    minutes = minutes % 60;

                    hm.HealthDuration = hours + ":" + minutes;
                }
                healthList.Add(hm);
            }
            return healthList;
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