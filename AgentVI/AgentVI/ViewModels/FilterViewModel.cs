﻿#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AgentVI.Services;
using Xamarin.Forms;
using AgentVI.Interfaces;

namespace AgentVI.ViewModels
{
    public class FilterViewModel : IBindableVM, INotifyPropertyChanged
    {
        private bool _isFetching = false;
        public bool IsFetching
        {
            get => _isFetching;
            set
            {
                _isFetching = value;
                OnPropertyChanged(nameof(IsFetching));
            }
        }
        public string ActiveAccountName => ServiceManager.Instance.FilterService.CurrentAccount.Name;
        private ObservableCollection<Folder> _selectedFoldersCache;
        public ObservableCollection<Folder> SelectedFoldersCache
        {
            get => _selectedFoldersCache;
            set
            {
                _selectedFoldersCache = value;
                OnPropertyChanged(nameof(SelectedFoldersCache));
            }
        }
        private ObservableCollection<FilteringPageViewModel> _filteringPagesContent;
        public ObservableCollection<FilteringPageViewModel> FilteringPagesContent
        {
            get => _filteringPagesContent;
            private set
            {
                _filteringPagesContent = value;
                OnPropertyChanged(nameof(FilteringPagesContent));
            }
        }
        public Folder CurrentlySelectedFolder { get; set; }
        public int CurrentPageNumber { get; private set; }

        public FilterViewModel()
        {
            IsFetching = false;
            FilteringPagesContent = new ObservableCollection<FilteringPageViewModel>();
            ServiceManager.Instance.FilterService.SelectRootLevel(true);
            SelectedFoldersCache = new ObservableCollection<Folder>(ServiceManager.Instance.FilterService.CurrentPath);
            FilteringPageViewModel currentFiltrationLevel = new FilteringPageViewModel(0);
            currentFiltrationLevel.PopulateCollection();
            FilteringPagesContent.Add(currentFiltrationLevel);
        }

        public void FetchCurrentFilteringDepth(Folder i_SelectedFolder)
        {
            IsFetching = true;
            ServiceManager.Instance.FilterService.SelectFolder(i_SelectedFolder, true);
            SelectedFoldersCache = new ObservableCollection<Folder>(ServiceManager.Instance.FilterService.CurrentPath);
            IsFetching = false;
        }

        public void FetchNextFilteringDepth(Folder i_SelectedFolder)
        {
            CurrentPageNumber++;
            IsFetching = true;
            for (int i = FilteringPagesContent.Count - 1; i > i_SelectedFolder.Depth; i--)
            {
                FilteringPagesContent.RemoveAt(i);
            }
            ServiceManager.Instance.FilterService.SelectFolder(i_SelectedFolder);
            if (i_SelectedFolder.Folders != null && !i_SelectedFolder.Folders.IsEmpty())
            {
                FilteringPageViewModel currentFiltrationLevel = new FilteringPageViewModel(i_SelectedFolder.Depth + 1);
                currentFiltrationLevel.PopulateCollection();
                FilteringPagesContent.Add(currentFiltrationLevel);
            }
            SelectedFoldersCache = new ObservableCollection<Folder>(ServiceManager.Instance.FilterService.CurrentPath);
            IsFetching = false;
        }

        public int GetPreviousPageIndex()
        {
            try
            {
                IsFetching = true;
                if (CurrentPageNumber == 0)
                {
                    ServiceManager.Instance.FilterService.SelectRootLevel(true);
                }
                else
                {
                    if (--CurrentPageNumber != 0)
                    {
                        ServiceManager.Instance.FilterService.SelectFolder(SelectedFoldersCache[--CurrentPageNumber], false);
                    }
                    else
                    {
                        ServiceManager.Instance.FilterService.SelectRootLevel();
                    }
                }
                IsFetching = false;
                return CurrentPageNumber;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
    }
}
