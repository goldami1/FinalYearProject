using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Extended;
using System.Threading.Tasks;
using AgentVI.Interfaces;
using System.Threading;
using System.Runtime.Serialization;

namespace AgentVI.ViewModels
{
    public abstract class FilterDependentViewModel<T> : IBindableVM
    {
        protected IEnumerable<T> enumerableCollection;
        private const int pageSize = 4;
        private IEnumerator<T> collectionEnumerator;
        protected bool canLoadMore = false;
        protected bool IsFilterStateChanged { get; set; }
        public ObservableCollection<T> ObservableCollection { get; set; }
        private bool _isStillLoading = true;
        public override bool IsStillLoading
        {
            get => _isStillLoading;
            set
            {
                _isStillLoading = value;
                if (!_isStillLoading && IsEmptyFolder)
                    IsEmptyView = true;
                else
                    IsEmptyView = false;
                OnPropertyChanged(nameof(IsStillLoading));
            }
        }
        private bool _isEmptyView = false;
        public override bool IsEmptyView
        {
            get => _isEmptyView;
            set
            {
                _isEmptyView = value;
                OnPropertyChanged(nameof(IsEmptyView));
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                updateIsStillLoading();
                OnPropertyChanged(nameof(IsBusy));
            }
        }
        private bool _isEmptyFolder = true;
        public bool IsEmptyFolder
        {
            get => _isEmptyFolder;
            set
            {
                _isEmptyFolder = value;
                updateIsStillLoading();
                OnPropertyChanged(nameof(IsEmptyFolder));
            }
        }

        public void updateIsStillLoading()
        {
            IsStillLoading = IsBusy && IsEmptyFolder;
        }

        private void updateFolderState()
        {
            if (ObservableCollection.Count == 0)
            {
                IsEmptyFolder = true;
            }
            else
            {
                IsEmptyFolder = false;
            }
        }


        public FilterDependentViewModel()
        {
            ObservableCollection = new InfiniteScrollCollection<T>()
            {
                OnLoadMore = async () =>
                {
                    IsBusy = true;
                    await Task.Factory.StartNew(() => FetchCollection());
                    IsBusy = false;
                    return null;
                },
                OnCanLoadMore = () =>
                {
                    return canLoadMore;
                }
            };
        }

        protected virtual void FetchCollection()
        {
            bool hasNext = true;
            int fetchedItems = 0;
            IsBusy = true;
            if (collectionEnumerator == null || IsFilterStateChanged)
            {
                IsFilterStateChanged = false;
                collectionEnumerator = enumerableCollection.GetEnumerator();
            }

            try
            {
                Console.WriteLine("####Logger####   -   in FetchCollection() @before MoveNext" + ">>> " + typeof(T).Name);
                var task = Task.Run(() => collectionEnumerator.MoveNext());
                if (task.Wait(TimeSpan.FromMilliseconds(5000)))
                    hasNext = task.Result;
                else
                    hasNext = false;
                Console.WriteLine("####Logger####   -   in FetchCollection() @after MoveNext" + ">>> " + typeof(T).Name);

                while (hasNext && canLoadMore)
                {
                    Console.WriteLine("####Logger####   -   in FetchCollection() getting collectionEnumerator.Current @before" + ">>> " + typeof(T).Name);
                    ObservableCollection.Add(collectionEnumerator.Current);
                    Console.WriteLine("####Logger####   -   in FetchCollection() getting collectionEnumerator.Current @after" + ">>> " + typeof(T).Name);
                    if (IsEmptyFolder)
                        IsEmptyFolder = !IsEmptyFolder;
                    if (fetchedItems++ == pageSize)
                    {
                        break;
                    }
                    hasNext = collectionEnumerator.MoveNext();
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                hasNext = false;
                safeEnumerableReset();
            }

            if (hasNext == false)
            {
                canLoadMore = false;
                safeEnumerableReset();
            }

            updateFolderState();
            IsBusy = false;
        }

        private void safeEnumerableReset()
        {
            try
            {
                collectionEnumerator.Reset();
            }catch (NotSupportedException) { }
        }

        public virtual void PopulateCollection()
        {
            ObservableCollection.Clear();
            IsFilterStateChanged = true;
            canLoadMore = true;
        }

        public virtual void OnFilterStateUpdated(object source, EventArgs e)
        {
            IsFilterStateChanged = true;
        }
    }
}
