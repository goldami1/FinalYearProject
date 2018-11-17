using AgentVI.Models;
using AgentVI.Converters;
using AgentVI.Utils;
using System;
using AgentVI.Interfaces;
using AgentVI.Views;
using System.Windows.Input;
using Xamarin.Forms;
using Rg.Plugins.Popup.Extensions;
using System.Threading.Tasks;
using InnoviApiProxy;

namespace AgentVI.ViewModels
{
    public class EventDetailsViewModel : IBindableVM
    {
        public DropdownMenuPage DropdownMenu { get; private set; }
        public string SensorName => isLive ? StreamingSensor.Name : EventModel.SensorName;
        public string SensorEventRuleName => isLive ? string.Empty : EventModel.SensorEventRuleName.convertEnumToString();
        public string SensorEventBehavior => isLive ? string.Empty : EventModel.SensorEventRuleName.BehaviorToString();
        public string SensorEventDateTime => isLive ? string.Empty : new TimestampConverter().Convert(EventModel.SensorEventDateTime, typeof(String), null, null).ToString();
        public string SensorEventTag => isLive ? string.Empty : EventModel.SensorEventTag.convertEnumToString();
        public string SensorEventClipPath => isLive ? StreamingSensor.LiveView : EventModel.SensorEventClip;
        public string SensorEventObjectType => isLive ? string.Empty : new EnumObjectTypeSVGConverter().Convert(EventModel.SensorEventObjectType, EventModel.SensorEventObjectType.GetType(), null, null).ToString();
        public bool IsClipAvailable => isLive ? true : EventModel.IsClipAvailable && IsPlayerVisible;
        public bool IsPlayerVisible { get; set; } = true;
        private EventModel EventModel { get; set; }
        private Sensor StreamingSensor { get; set; }
        private bool isLive;

        public EventDetailsViewModel(EventModel i_EventModel, bool i_IsLiveAvailable = false)
        {
            EventModel = i_EventModel;
            isLive = false;
            DropdownMenu = null;
            if (i_IsLiveAvailable)
            {
                DropdownMenu = buildDropdownMenu(i_EventModel.Sensor);
            }
        }

        public EventDetailsViewModel(Sensor i_StreamingSensor)
        {
            StreamingSensor = i_StreamingSensor;
            isLive = true;
            DropdownMenu = null;
        }

        private DropdownMenuPage buildDropdownMenu(Sensor i_StreamingSensor)
        {
            return DropdownMenuPage.FactoryMethod()
                .AddActionItem(new Tuple<string, Action>(
                    "Live", async () =>
                    {
                        eventsRouter(this, null);
                        EventDetailsPage eventDetailsPageBuf = null;
                        await Task.Factory.StartNew(() =>
                        {
                            eventDetailsPageBuf = new EventDetailsPage(i_StreamingSensor);
                            eventDetailsPageBuf.RaiseContentViewUpdateEvent += eventsRouter;
                        });
                        eventsRouter(this, new UpdatedContentEventArgs(
                                                                    UpdatedContentEventArgs.EContentUpdateType.Push,
                                                                    eventDetailsPageBuf, eventDetailsPageBuf.BindableViewModel));
                    }))
                .AddActionItem(new Tuple<string, Action>(
                    "Health", () =>
                    {
                        HealthPage healthPageToPush = new HealthPage(EventModel.Sensor);
                        healthPageToPush.PopulateView();
                        (App.Current.MainPage as NavigationPage).PushAsync(healthPageToPush);
                    }
                    ))
                .Build();
        }
    }
}