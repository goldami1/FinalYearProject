#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using AgentVI.ViewModels;
using Xamarin.Forms;
using AgentVI.Services;
using AgentVI.Interfaces;
using AgentVI.Utils;
using AgentVI.Models;
using System.Threading.Tasks;
using System.Collections;

namespace AgentVI.Views
{
    public partial class EventsPage : ContentPage, INotifyContentViewChanged, IBindable, IPopulableView
    {
        public IBindableVM BindableViewModel => SensorsEventsListVM;
        public ContentPage ContentPage => this;
        private EventsListViewModel SensorsEventsListVM = null;
        public event EventHandler<UpdatedContentEventArgs> RaiseContentViewUpdateEvent;
        public Command<EventModel> Command => new Command<EventModel>(val =>
        {
            onSensorNameTapped(val);
        });

        public EventsPage()
        {
            InitializeComponent();
            SensorsEventsListVM = new EventsListViewModel();
            initOnFilterStateUpdatedEventHandler();
            eventListView.BindingContext = SensorsEventsListVM;
        }

        public async void PopulateView()
        {
            await Task.Factory.StartNew(() => SensorsEventsListVM?.PopulateCollection());
        }

        private void initOnFilterStateUpdatedEventHandler()
        {
            ServiceManager.Instance.FilterService.FilterStateUpdated += SensorsEventsListVM.OnFilterStateUpdated;
        }

        private async void OnRefresh(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(() => SensorsEventsListVM.PopulateCollection());
            ((ListView)sender).IsRefreshing = false;
        }
        
        private async void onEventTapped(object sender, ItemTappedEventArgs e)
        {
            RaiseContentViewUpdateEvent?.Invoke(this, null);
            UpdatedContentEventArgs updatedContentEventArgs = null;
            EventDetailsPage eventDetailsPageBuf = null;

            await Task.Factory.StartNew(() =>
            {
                EventModel selectedEvent = e.Item as EventModel;
                eventDetailsPageBuf = new EventDetailsPage(selectedEvent);
                eventDetailsPageBuf.RaiseContentViewUpdateEvent += eventsRouter;
                updatedContentEventArgs = new UpdatedContentEventArgs(
                    UpdatedContentEventArgs.EContentUpdateType.Push, eventDetailsPageBuf,
                    eventDetailsPageBuf.BindableViewModel);

            });

            RaiseContentViewUpdateEvent?.Invoke(this, updatedContentEventArgs);
        }

        private async void onSensorNameTapped(EventModel val)
        {
            RaiseContentViewUpdateEvent?.Invoke(this, new UpdatedContentEventArgs(UpdatedContentEventArgs.EContentUpdateType.Buffering));
            UpdatedContentEventArgs updatedContentEventArgs = null;
            CameraEventsPage cameraEventsPageBuf = null;
            Sensor sensorBuffer = val.Sensor;

            await Task.Factory.StartNew(() =>
            {
                cameraEventsPageBuf = new CameraEventsPage(sensorBuffer);
                cameraEventsPageBuf.RaiseContentViewUpdateEvent += eventsRouter;
            });
            await Task.Factory.StartNew(() => 
            updatedContentEventArgs = new UpdatedContentEventArgs(
                UpdatedContentEventArgs.EContentUpdateType.Push, cameraEventsPageBuf, cameraEventsPageBuf.BindableViewModel
                ));
            RaiseContentViewUpdateEvent?.Invoke(this, updatedContentEventArgs);
        }

        private void eventsRouter(object sender, UpdatedContentEventArgs e)
        {
            RaiseContentViewUpdateEvent?.Invoke(this, e);
        }
    }
}