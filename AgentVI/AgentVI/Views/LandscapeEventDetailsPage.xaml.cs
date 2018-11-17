using AgentVI.Interfaces;
using AgentVI.Models;
using AgentVI.Utils;
using AgentVI.ViewModels;
using InnoviApiProxy;
using Plugin.DeviceOrientation;
using Plugin.DeviceOrientation.Abstractions;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AgentVI.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LandscapeEventDetailsPage : ContentPage, INotifyContentViewChanged, IFocusable, IBindable
    {
        public IBindableVM BindableViewModel => landscapeEventDetailsPageVM;
        public ContentPage ContentPage => this;
        public event EventHandler<UpdatedContentEventArgs> RaiseContentViewUpdateEvent;
        private LandscapeEventDetailsPageVM landscapeEventDetailsPageVM = null;

        public LandscapeEventDetailsPage ()
		{
			InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            CrossDeviceOrientation.Current.UnlockOrientation();
            CrossDeviceOrientation.Current.OrientationChanged += handleOrientationChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            onAreaTapped(null, null);
        }

        public LandscapeEventDetailsPage(EventModel i_EventModel) :this()
        {
            landscapeEventDetailsPageVM = new LandscapeEventDetailsPageVM(i_EventModel);
            BindingContext = landscapeEventDetailsPageVM;
        }

        public LandscapeEventDetailsPage(Sensor i_StreamingSensor) : this()
        {
            landscapeEventDetailsPageVM = new LandscapeEventDetailsPageVM(i_StreamingSensor);
            BindingContext = landscapeEventDetailsPageVM;
        }

        private void handleOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == DeviceOrientations.Portrait)
            {
                quitClipLoading();
                RaiseContentViewUpdateEvent?.Invoke(
                    this, new UpdatedContentEventArgs(UpdatedContentEventArgs.EContentUpdateType.PopAsync)
                    );
            }
            
        }
        
        private void quitClipLoading()
        {
            landscapeEventDetailsPageVM.IsPlayerVisible = false;
        }

        private void restartClipLoading()
        {
            landscapeEventDetailsPageVM.IsPlayerVisible = true;
        }

        public void Refocus()
        {
            restartClipLoading();
        }

        private async void onAreaTapped(object sender, EventArgs e)
        {
            await firstLineLabel.FadeTo(100, 400);
            await secondLineLabel.FadeTo(100, 400);
            await firstLineLabel.FadeTo(0, 1000);
            await secondLineLabel.FadeTo(0, 1000);
        }
    }
}