﻿#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using AgentVI.ViewModels;
using Xamarin.Forms;
using System.Linq;
using AgentVI.Services;
using AgentVI.Interfaces;
using AgentVI.Models;
using AgentVI.Utils;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AgentVI.Views
{
    public partial class CamerasPage : ContentPage, INotifyContentViewChanged
    {
        private SensorsListViewModel SensorsListVM = null;
        public event EventHandler<UpdatedContentEventArgs> RaiseContentViewUpdateEvent;

        public CamerasPage()
        {
            InitializeComponent();

            SensorsListVM = new SensorsListViewModel();
            initOnFilterStateUpdatedEventHandler();
            SensorsListVM.UpdateCameras();
            cameraListView.BindingContext = SensorsListVM;
        }

        private void initOnFilterStateUpdatedEventHandler()
        {
            ServiceManager.Instance.FilterService.FilterStateUpdated += SensorsListVM.OnFilterStateUpdated;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SensorsListVM.UpdateCameras();
        }

        private async void OnRefresh(object sender, EventArgs e)
        {
            await System.Threading.Tasks.Task.Factory.StartNew(() => SensorsListVM.UpdateCameras());
            ((ListView)sender).IsRefreshing = false; //end the refresh state
        }

        private async void OnSensor_Tapped(object sender, EventArgs e)
        {
            RaiseContentViewUpdateEvent?.Invoke(this, null);
            UpdatedContentEventArgs updatedContentEventArgs = null;
            Sensor sensorBuffer = null;
            await Task.Factory.StartNew(() =>
            {
                Label labelObj = (sender as Grid).FindByName<Label>("SensorName");
                IEnumerable<SensorModel> sensorEnumerable = SensorsListVM.ObservableCollection.Where(sensor => sensor.SensorName == labelObj.Text);
                sensorBuffer = sensorEnumerable.First().Sensor;
            });
            await Task.Factory.StartNew(() => updatedContentEventArgs = new UpdatedContentEventArgs(new CameraEventsPage(sensorBuffer)));
            RaiseContentViewUpdateEvent?.Invoke(this, updatedContentEventArgs);
        }
    }
}