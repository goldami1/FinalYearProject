#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AgentVI.Utils;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AgentVI.Models;
using Xamarin.Forms;
using System.Text;

namespace AgentVI.Services
{
    public partial class ServiceManager
    {
        private class FilterServiceS : IFilterService
        {
            public Account CurrentAccount { get; private set; }
            public List<Account> UserAccounts { get; private set; }
            public IEnumerable<Sensor> FilteredSensorCollection { get; private set; }
            public IEnumerable<FolderModel> CurrentLevel { get; private set; }
            public IEnumerable<SensorEvent> FilteredEvents { get; private set; }
            public IEnumerable<Sensor.Health> FilteredHealth => FilteredHealthHelper.Value;
            private Lazy<IEnumerable<Sensor.Health>> FilteredHealthHelper { get; set; }
            public bool IsAtRootLevel { get; private set; }
            public List<Folder> CurrentPath { get; private set; }
            public string CurrentStringPath => currenPathToString(CurrentPath);
            private IEnumerable<Folder> RootFolders { get; set; }
            private Dictionary<Folder, ObservableCollection<FolderModel>> cachedHierarchy { get; set; }
            private Dictionary<Folder, ObservableCollection<FolderModel>> _cachedHierarchy { get; set; }
            private ObservableCollection<FolderModel> rootFoldersCache;
            private readonly object cacheLock;
            private readonly TimeSpan defaultCachingTimespan;
            public event EventHandler FilterStateUpdated;



            public FilterServiceS()
            {
                cacheLock = new object();
                defaultCachingTimespan = new TimeSpan(0, 5, 0);
                IsAtRootLevel = true;
                CurrentAccount = null;
                UserAccounts = null;
                FilteredSensorCollection = null;
                RootFolders = null;
                CurrentPath = new List<Folder>();
                CurrentLevel = null;
            }

            public bool InitServiceModule(User i_User = null)
            {
                bool res = false;
                if (ServiceManager.Instance.LoginService != null &&
                    ServiceManager.Instance.LoginService.LoggedInUser != null)
                {
                    UserAccounts = ServiceManager.Instance.LoginService.LoggedInUser.Accounts;
                    CurrentAccount = ServiceManager.Instance.LoginService.LoggedInUser.Accounts[0];
                    IsAtRootLevel = true;
                    FilteredSensorCollection = ServiceManager.Instance.LoginService.LoggedInUser.GetDefaultAccountSensors();
                    RootFolders = ServiceManager.Instance.LoginService.LoggedInUser.GetDefaultAccountFolders();
                    CurrentPath = new List<Folder>();
                    CurrentLevel = foldersToFolderModels(RootFolders);
                    FilteredHealthHelper = new Lazy<IEnumerable<Sensor.Health>>(fetchHealthArray);
                    setFilteredEvents();                             //keeps FilteredEvents updated
                    OnFilterStateUpdated();
                    Task.Factory.StartNew(() => fetchFoldersHierarchy());
                    Device.BeginInvokeOnMainThread(() =>
                    Device.StartTimer(defaultCachingTimespan, () =>
                    {
                        Task.Factory.StartNew(() => fetchFoldersHierarchy());
                        return true;
                    }));
                    res = true;
                }

                return res;
            }

            public void SelectRootLevel(bool i_TriggerOnFilterUpdatedEvent = false)
            {
                FilteredSensorCollection = ServiceManager.Instance.LoginService.LoggedInUser.GetDefaultAccountSensors();
                IsAtRootLevel = true;
                CurrentPath = new List<Folder>();
                CurrentLevel = getRootFolders();
                FilteredHealthHelper = new Lazy<IEnumerable<Sensor.Health>>(fetchHealthArray);
                setFilteredEvents();
                if (i_TriggerOnFilterUpdatedEvent)
                {
                    OnFilterStateUpdated();
                }
            }

            public void SelectFolder(Folder i_FolderSelected, bool i_TriggerOnFilterUpdatedEvent = false)
            {
                FilteredSensorCollection = i_FolderSelected.GetAllSensors();
                updatePath(i_FolderSelected);                               //keeps CurrentPath, CurrentPathStr updated
                CurrentLevel = getCurrentLevelFromSelectedFolder(i_FolderSelected);
                FilteredHealthHelper = new Lazy<IEnumerable<Sensor.Health>>(fetchHealthArray);
                setFilteredEvents();
                if (i_FolderSelected.Depth >= 0)
                {
                    IsAtRootLevel = false;
                }
                if (i_TriggerOnFilterUpdatedEvent)
                {
                    OnFilterStateUpdated();
                }
            }

            public void SwitchAccount(Account i_SelectedAccount)
            {
                i_SelectedAccount.SetAsDefaultAccount();
                CurrentAccount = i_SelectedAccount;
                RootFolders = ServiceManager.Instance.LoginService.LoggedInUser.GetDefaultAccountFolders();
                CurrentPath = new List<Folder>();
                SelectRootLevel();
            }

            protected virtual void OnFilterStateUpdated()
            {
                FilterStateUpdated?.Invoke(this, EventArgs.Empty);
            }

            private void setFilteredEvents()
            {
                if (IsAtRootLevel)
                {
                    Console.WriteLine("####Logger####   -   FilteredEvents by DefaultAccountEvents");
                    FilteredEvents = ServiceManager.Instance.LoginService.LoggedInUser.GetDefaultAccountEvents().Clone();
                }
                else
                {
                    Console.WriteLine("####Logger####   -   FilteredEvents by FolderEvents");
                    FilteredEvents = CurrentPath[CurrentPath.Count - 1].FolderEvents.Clone();
                }
            }

            private void updatePath(Folder i_FolderSelected)
            {
                if (i_FolderSelected.Depth == 0)
                {
                    CurrentPath = new List<Folder>() { i_FolderSelected };
                }
                else if (i_FolderSelected.Depth == CurrentPath.Count)
                {
                    CurrentPath.Add(i_FolderSelected);
                }
                else if (i_FolderSelected.Depth < CurrentPath.Count)
                {
                    CurrentPath.RemoveRange(i_FolderSelected.Depth, CurrentPath.Count - 1);
                    CurrentPath.Add(i_FolderSelected);
                }
                else    //i_FolderSelected.Depth > CurrentPath.Count is impossible scenario! something bad happened!
                {
                    throw new Exception("Corrupted hierarchy navigation.");
                }
            }

            private static IEnumerable<FolderModel> foldersToFolderModels(IEnumerable<Folder> i_Folders)
            {
                return i_Folders.Select<Folder, FolderModel>(currentFolder => FolderModel.FactoryMethod(currentFolder));
            }

            private IEnumerable<FolderModel> getRootFolders()
            {
                IEnumerable<FolderModel> res = null;

                if (rootFoldersCache != null)
                {
                    res = rootFoldersCache;
                }
                else
                {
                    res = foldersToFolderModels(RootFolders);
                }

                return res;
            }

            private IEnumerable<FolderModel> getCurrentLevelFromSelectedFolder(Folder i_SelectedFolder)
            {
                IEnumerable<FolderModel> res = null;

                if (cachedHierarchy != null)
                {
                    lock (cacheLock)
                    {
                        if (cachedHierarchy.ContainsKey(i_SelectedFolder))
                        {
                            res = cachedHierarchy[i_SelectedFolder];
                        }
                    }
                }

                if (cachedHierarchy == null || res == null)
                {
                    res = foldersToFolderModels(i_SelectedFolder.Folders);
                }

                return res;
            }

            private void fetchFoldersHierarchy()
            {
                InnoviObjectCollection<Folder> rootFolders = ServiceManager.Instance.LoginService.LoggedInUser.GetDefaultAccountFolders().Clone();
                _cachedHierarchy = new Dictionary<Folder, ObservableCollection<FolderModel>>();
                rootFoldersCache = new ObservableCollection<FolderModel>(foldersToFolderModels(rootFolders));
                //<ImproveIt><begin> change to foreach loop - for now some issue arrises <begin><ImproveIt>
                IEnumerator<Folder> rootFoldersEnumerator = rootFolders.GetEnumerator();
                bool hasNext = true;
                while (hasNext = rootFoldersEnumerator.MoveNext())
                {
                    _cachedHierarchy.Add(rootFoldersEnumerator.Current, new ObservableCollection<FolderModel>(
                        foldersToFolderModels(rootFoldersEnumerator.Current.Folders)));
                    fetchFoldersHierarchyHelper(rootFoldersEnumerator.Current);
                }
                lock (cacheLock)
                {
                    cachedHierarchy = _cachedHierarchy;
                }
                //<ImproveIt><end> change to foreach loop <end><ImproveIt>
            }

            private void fetchFoldersHierarchyHelper(Folder i_Folder)
            {
                foreach (Folder folder in i_Folder.Folders)
                {
                    ObservableCollection<FolderModel> folderFoldersCollection =
                        new ObservableCollection<FolderModel>(foldersToFolderModels(folder.Folders));
                    _cachedHierarchy.Add(folder, folderFoldersCollection);
                    if (folderFoldersCollection != null)
                    {
                        fetchFoldersHierarchyHelper(folder);
                    }
                }
            }

            private string currenPathToString(IEnumerable<Folder> i_FolderCollection)
            {
                string res;
                string separator = "/";
                string prefix = separator;

                if (i_FolderCollection != null)
                {
                    StringBuilder resBuilder = new StringBuilder();

                    foreach (Folder folder in i_FolderCollection)
                    {
                        resBuilder.Append(prefix).Append(folder.Name);
                    }

                    res = resBuilder.ToString();
                }
                else
                {
                    res = prefix;
                }

                return res;
            }

            private List<Sensor.Health> fetchHealthArray()
            {
                List<Sensor.Health> fetchedHealthArray = null;
                if (Settings.IsHealthFetchingEnabled)
                {
                    fetchedHealthArray = new List<Sensor.Health>();
                    foreach (Sensor sensor in FilteredSensorCollection)
                    {
                        fetchedHealthArray.AddRange(sensor.SensorHealthArray);
                    }
                }
                return fetchedHealthArray;
            }
        }
    }
}