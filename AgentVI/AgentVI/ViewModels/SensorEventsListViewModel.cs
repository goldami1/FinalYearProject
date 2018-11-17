#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using System.Linq;
using AgentVI.Models;
using System.Collections.ObjectModel;
using AgentVI.Views;
using AgentVI.Utils;
using System.Threading.Tasks;
using AgentVI.Interfaces;
using Xamarin.Forms;

namespace AgentVI.ViewModels
{
    public class SensorEventsListViewModel : FilterDependentViewModel<EventModel>
    {
        public DropdownMenuPage DropdownMenu { get; private set; }
        public Sensor SensorSource { get; private set; }
        public string SensorName => SensorSource.Name;

        public SensorEventsListViewModel(Sensor i_Sensor) : base()
        {
            SensorSource = i_Sensor;
            DropdownMenu = buildDropdownMenu(i_Sensor);
        }

        private DropdownMenuPage buildDropdownMenu(Sensor i_Sensor)
        {
            return DropdownMenuPage.FactoryMethod().AddActionItem(new Tuple<string, Action>(
                "Live", async () =>
                {
                    eventsRouter(this, null);
                    EventDetailsPage eventDetailsPageBuf = null;
                    await Task.Factory.StartNew(() =>
                    {
                        eventDetailsPageBuf = new EventDetailsPage(i_Sensor);
                        eventDetailsPageBuf.RaiseContentViewUpdateEvent += eventsRouter;
                    });
                    eventsRouter(this, new UpdatedContentEventArgs(
                        UpdatedContentEventArgs.EContentUpdateType.Push,
                        eventDetailsPageBuf, eventDetailsPageBuf.BindableViewModel));
                }))
                .AddActionItem(new Tuple<string, Action>(
                    "Health", () =>
                    {
                        HealthPage healthPageToPush = new HealthPage(SensorSource);
                        healthPageToPush.PopulateView();
                        (App.Current.MainPage as NavigationPage).PushAsync(healthPageToPush);
                    })).Build();
        }

        public override void OnFilterStateUpdated(object source, EventArgs e)
        {
            return;
        }

        public override void PopulateCollection()
        {
            base.PopulateCollection();
            enumerableCollection = SensorSource.SensorEvents.Select(sensorEvent =>
            {
                return EventModel.FactoryMethod(sensorEvent);
            });
            FetchCollection();
        }
    }
}