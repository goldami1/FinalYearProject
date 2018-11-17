#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using System.ComponentModel;
using System.Collections.Generic;
using Xamarin.Forms;
using AgentVI.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentVI.Services;
namespace AgentVI.ViewModels
{
    public class HealthListViewModel : FilterDependentViewModel<HealthModel>
    {
        public override void OnFilterStateUpdated(object source, EventArgs e)
        {
            base.OnFilterStateUpdated(source, e);
            PopulateCollection();
        }
        public override void PopulateCollection()
        {
            base.PopulateCollection();
            enumerableCollection = ServiceManager.Instance.FilterService.
                                                 FilteredHealth.Select(health => HealthModel.FactoryMethod(health));
            enumerableCollection.OrderBy(health => health.HealthTime);
            FetchCollection();
        }
        public void PopulateCollection(Sensor i_Sensor)
        {
            base.PopulateCollection();
            List<HealthModel> healthModelList = new List<HealthModel>();
            List<Sensor.Health> healthProxyList = i_Sensor.SensorHealthArray;
            foreach (Sensor.Health health in healthProxyList)
            {
                healthModelList.Add(HealthModel.FactoryMethod(health));
            }
            healthModelList.Reverse();
            enumerableCollection = healthModelList;
            FetchCollection();
        }
    }
}