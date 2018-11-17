using System;
using System.ComponentModel;
using InnoviApiProxy;
using Xamarin.Forms;
using System.Text;
namespace AgentVI.Models
{
    public class HealthModel
    {
        private HealthModel()
        {
        }
        internal static HealthModel FactoryMethod(Sensor.Health i_Health)
        {
            HealthModel res = new HealthModel()
            {
                HealthTime = (ulong)i_Health.StatusTimeStamp,
                HealthDescription = getNoEmptyDescription(i_Health.DetailedDescription),
                HealthDuration = getHealthDurationTime(i_Health.Duration),
                CameraName = i_Health.SensorName
            };
            return res;
        }
        public ulong HealthTime { get; set; }
        public string HealthDescription { get; set; }
        public string HealthDuration { get; set; }
        public string CameraName { get; set; }
        private static string getNoEmptyDescription(string i_description)
        {
            string description = i_description;
            if (string.IsNullOrEmpty(description))
            {
                description = "OK";
            }
            return description;
        }
        private static string getHealthDurationTime(long i_duration)
        {
            int minutes, hours;
            TimeSpan timeDuration = new TimeSpan(i_duration * 10000);
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
    }
}