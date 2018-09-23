﻿#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using AgentVI.ViewModels;
using Xamarin.Forms;
using System.Linq;
using AgentVI.Services;

namespace AgentVI.Views
{
    public partial class CamerasPage : ContentPage
    {
        private SensorsListViewModel SensorsListVM = null;

        public CamerasPage()
        {
            InitializeComponent();

            SensorsListVM = new SensorsListViewModel();
            initOnFilterStateUpdatedEventHandler();
            SensorsListVM.InitializeList(ServiceManager.Instance.LoginService.LoggedInUser);
            cameraListView.BindingContext = SensorsListVM;
        }

        public void initOnFilterStateUpdatedEventHandler()
        {
            ServiceManager.Instance.FilterService.FilterStateUpdated += SensorsListVM.OnFilterStateUpdated;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SensorsListVM.UpdateCameras();
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            SensorsListVM.UpdateCameras();
            ((ListView)sender).IsRefreshing = false; //end the refresh state
        }

        private void onCameraNameTapped(object sender, EventArgs e)
        {
            (Application.Current.MainPage as NavigationPage).PushAsync(new CameraEventsPage(SensorsListVM.SensorsList.Where(sensor => sensor.SensorName == (sender as Grid).FindByName<Label>("SensorName").Text).First().Sensor));
        }
    }
}