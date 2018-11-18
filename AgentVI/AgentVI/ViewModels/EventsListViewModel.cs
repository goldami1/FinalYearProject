#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using System.Linq;
using AgentVI.Services;
using AgentVI.Models;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace AgentVI.ViewModels
{
    public class EventsListViewModel : FilterDependentViewModel<EventModel>
    {
        public override void OnFilterStateUpdated(object source, EventArgs e)
        {
            base.OnFilterStateUpdated(source, e);
            PopulateCollection();
        }

        public override void PopulateCollection()
        {
            base.PopulateCollection();
            enumerableCollection = ServiceManager.Instance.FilterService.FilteredEvents.Select(sensorEvent =>
            {
                var res = EventModel.FactoryMethod(sensorEvent);
                return res;
            });
            FetchCollection();
        }
    }
}