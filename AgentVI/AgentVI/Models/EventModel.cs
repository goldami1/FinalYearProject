﻿using System;
using System.ComponentModel;
using InnoviApiProxy;
using Xamarin.Forms;

namespace AgentVI.Models
{
    public class EventModel : INotifyPropertyChanged
    {
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

        private SensorEvent.eBehaviorType m_SensorEventRuleName;
        public SensorEvent.eBehaviorType SensorEventRuleName
        {
            get { return m_SensorEventRuleName; }
            private set
            {
                m_SensorEventRuleName = value;
                OnPropertyChanged("SensorEventRuleName");
            }
        }

        private ulong m_SensorEventDateTime;
        public ulong SensorEventDateTime
        {
            get { return m_SensorEventDateTime; }
            private set
            {
                m_SensorEventDateTime = value;
                OnPropertyChanged("SensorEventDateTime");
            }
        }

        private string m_SensorEventImage;
        public string SensorEventImage
        {
            get { return "https://picsum.photos/200/200"; }
            private set
            {
                m_SensorEventImage = value;
                OnPropertyChanged("SensorEventImage");
            }
        }

        private string m_SensorEventClip;
        public string SensorEventClip
        {
            get { return m_SensorEventClip; }
            private set
            {
                m_SensorEventClip = value;
                OnPropertyChanged("SensorEventClip");
            }
        }

        private SensorEvent.eObjectType m_SensorEventObjectType;
        public SensorEvent.eObjectType SensorEventObjectType
        {
            get { return m_SensorEventObjectType; }
            private set
            {
                m_SensorEventObjectType = value;
                OnPropertyChanged("SensorEventObjectType");
            }
        }

        private Sensor.eSensorEventTag m_SensorEventTag;
        public Sensor.eSensorEventTag SensorEventTag
        {
            get { return m_SensorEventTag; }
            private set
            {
                m_SensorEventTag = value;
                OnPropertyChanged("SensorEventTag");
            }
        }

        private EventModel()
        {

        }

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
                SensorEventTag = i_SensorEvent.Tag
            };

            return res;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}