#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AgentVI.ViewModels;
using AgentVI.Services;
using AgentVI.Interfaces;
using AgentVI.Utils;
using AgentVI.Models;

namespace AgentVI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CameraEventsPage : ContentPage, INotifyContentViewChanged, IBindable, IPopulableView
    {
        public IBindableVM BindableViewModel => SensorEventsListVM;
        public ContentPage ContentPage => this;
        private SensorEventsListViewModel SensorEventsListVM = null;
        public event EventHandler<UpdatedContentEventArgs> RaiseContentViewUpdateEvent;

        public CameraEventsPage()
        {
            InitializeComponent();
        }

        public CameraEventsPage(Sensor i_Sensor) : this()
        {
            SensorEventsListVM = new SensorEventsListViewModel(i_Sensor);
            SensorEventsListVM.EventsRouter += eventsRouter;
            cameraEventsListView.BindingContext = SensorEventsListVM;
        }

        public async void PopulateView()
        {
            await Task.Factory.StartNew(() => SensorEventsListVM?.PopulateCollection());
        }

        private async void OnRefresh(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(() => SensorEventsListVM.PopulateCollection());
            ((ListView)sender).IsRefreshing = false;
        }

        private void onCameraEventBackButtonTapped(object sender, EventArgs e)
        {
            RaiseContentViewUpdateEvent?.Invoke(this, new UpdatedContentEventArgs(UpdatedContentEventArgs.EContentUpdateType.Pop));
        }

        private async void cameraEventsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            RaiseContentViewUpdateEvent?.Invoke(this, null);
            EventModel selectedSensorEvent = e.SelectedItem as EventModel;
            UpdatedContentEventArgs updatedContentEventArgs = null;
            EventDetailsPage eventDetailsPageBuf = null;

            await Task.Factory.StartNew(() =>
            {
                eventDetailsPageBuf = new EventDetailsPage(selectedSensorEvent);
                eventDetailsPageBuf.RaiseContentViewUpdateEvent += eventsRouter;
                updatedContentEventArgs = new UpdatedContentEventArgs(
                UpdatedContentEventArgs.EContentUpdateType.Push, eventDetailsPageBuf, eventDetailsPageBuf.BindableViewModel
                );
                RaiseContentViewUpdateEvent?.Invoke(this, updatedContentEventArgs);
            });
            
        }

        private void eventsRouter(object sender, UpdatedContentEventArgs e)
        {
            RaiseContentViewUpdateEvent?.Invoke(this, e);
        }
    }
}