#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using AgentVI.ViewModels;
using AgentVI.Models;
using AgentVI.Services;
using AgentVI.Interfaces;
using AgentVI.Utils;
using System.Threading.Tasks;
using System.Collections;
namespace AgentVI.Views
{
    public partial class HealthPage : ContentPage, IBindable, IPopulableView
    {
        private HealthListViewModel HealthPageVM = null;
        private bool isHealthForSpecificSensor = false;
        private Sensor sensor = null;
        public IBindableVM BindableViewModel => HealthPageVM;
        public ContentPage ContentPage => this;
        public HealthPage(Sensor i_sensor)
        {
            InitializeComponent();
            HealthPageVM = new HealthListViewModel();
            sensor = i_sensor;
            if (i_sensor != null)
            {
                isHealthForSpecificSensor = true;
                string cameraName = i_sensor.Name;
                cameraNameHeader.IsVisible = true;
                cameraNameHeader.Text = "Health for " + cameraName;
                Title = cameraName;
            }
            else
            {
                cameraNameHeader.IsVisible = false;
                isHealthForSpecificSensor = false;
                initOnFilterStateUpdatedEventHandler();
            }
            healthListView.BindingContext = HealthPageVM;
        }
        public void PopulateView()
        {
            if (isHealthForSpecificSensor)
            {
                HealthPageVM.PopulateCollection(sensor);
            }
            else
            {
                HealthPageVM.PopulateCollection();
            }
        }
        private void initOnFilterStateUpdatedEventHandler()
        {
            ServiceManager.Instance.FilterService.FilterStateUpdated += HealthPageVM.OnFilterStateUpdated;
        }
        private async void OnRefresh(object sender, EventArgs e)
        {
            if (isHealthForSpecificSensor)
            {
                try
                {
                    await Task.Factory.StartNew(() => HealthPageVM.PopulateCollection(sensor));
                    ((ListView)sender).IsRefreshing = false; //end the refresh state
                }
                catch (AggregateException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                try
                {
                    await Task.Factory.StartNew(() => HealthPageVM.PopulateCollection());
                    ((ListView)sender).IsRefreshing = false; //end the refresh state
                }
                catch (AggregateException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}