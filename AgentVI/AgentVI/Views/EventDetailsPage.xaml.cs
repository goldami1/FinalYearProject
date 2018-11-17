using AgentVI.Interfaces;
using AgentVI.Models;
using AgentVI.Utils;
using AgentVI.ViewModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.DeviceOrientation;
using Plugin.DeviceOrientation.Abstractions;
using InnoviApiProxy;

namespace AgentVI.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EventDetailsPage : ContentPage, INotifyContentViewChanged, IFocusable, IBindable, IDisposable
    {
        public event EventHandler<UpdatedContentEventArgs> RaiseContentViewUpdateEvent;
        public IBindableVM BindableViewModel => eventDetailsViewModel;
        public ContentPage ContentPage => this;
        private EventDetailsViewModel eventDetailsViewModel = null;
        private LandscapeEventDetailsPage landscapeEventDetailsPage = null;
        

        private EventDetailsPage()
        {
            InitializeComponent();
        }

        public EventDetailsPage(EventModel i_EventModel) : this()
        {
            Task.Factory.StartNew(() =>
            {
                landscapeEventDetailsPage = new LandscapeEventDetailsPage(i_EventModel);
                landscapeEventDetailsPage.RaiseContentViewUpdateEvent += eventsRouter;
            });
            eventDetailsViewModel = new EventDetailsViewModel(i_EventModel, true);
            eventDetailsViewModel.EventsRouter += eventsRouter;
            initOrientationState();
        }

        public EventDetailsPage(Sensor i_StreamingSensor) : this()
        {
            Task.Factory.StartNew(() =>
            {
                landscapeEventDetailsPage = new LandscapeEventDetailsPage(i_StreamingSensor);
                landscapeEventDetailsPage.RaiseContentViewUpdateEvent += eventsRouter;
            });
            eventDetailsViewModel = new EventDetailsViewModel(i_StreamingSensor);
            eventDetailsViewModel.EventsRouter += eventsRouter;
            initOrientationState();
        }

        private void initOrientationState()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                CrossDeviceOrientation.Current.UnlockOrientation();
                CrossDeviceOrientation.Current.OrientationChanged += handleOrientationChanged;
                checkLockOrientation();
            });
        }

        private void handleOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == DeviceOrientations.Landscape && landscapeEventDetailsPage != null)
            {
                quitClipLoading();
                RaiseContentViewUpdateEvent?.Invoke(
                    this, new UpdatedContentEventArgs(
                    UpdatedContentEventArgs.EContentUpdateType.PushAsync, landscapeEventDetailsPage, landscapeEventDetailsPage.BindableViewModel
                    ));
            }
        }

        private void quitClipLoading()
        {
            if (eventDetailsViewModel != null)
            {
                eventDetailsViewModel.IsPlayerVisible = false;
            }
        }

        private void restartClipLoading()
        {
            if (eventDetailsViewModel != null)
            {
                eventDetailsViewModel.IsPlayerVisible = true;
            }
        }

        private async void onEventDetailsBackButtonTapped(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(() => RaiseContentViewUpdateEvent?.Invoke(this, new UpdatedContentEventArgs(UpdatedContentEventArgs.EContentUpdateType.Buffering)));
            await Task.Factory.StartNew(()=> RaiseContentViewUpdateEvent?.Invoke(this, new UpdatedContentEventArgs(UpdatedContentEventArgs.EContentUpdateType.Pop)));
            await Task.Factory.StartNew(()=> RaiseContentViewUpdateEvent?.Invoke(this, new UpdatedContentEventArgs(UpdatedContentEventArgs.EContentUpdateType.Pop, this, BindableViewModel)));
            CrossDeviceOrientation.Current.LockOrientation(DeviceOrientations.Portrait);
        }

        private bool checkLockOrientation()
        {
            bool res = true;
            if (!eventDetailsViewModel.IsClipAvailable)
            {
                res = false;
                CrossDeviceOrientation.Current.LockOrientation(DeviceOrientations.Portrait);
            }
            return res;
        }

        private void eventsRouter(object sender, UpdatedContentEventArgs e)
        {
            if (landscapeEventDetailsPage != null)
            {
                RaiseContentViewUpdateEvent?.Invoke(this, e);
            }
        }

        public void Refocus()
        {
            restartClipLoading();
        }

        public void Dispose()
        {
            landscapeEventDetailsPage.RaiseContentViewUpdateEvent -= eventsRouter;
            landscapeEventDetailsPage = null;
            eventDetailsViewModel.EventsRouter -= eventsRouter;
            eventDetailsViewModel = null;
        }
    }
}