﻿using AgentVI.Models;
using AgentVI.Services;
using AgentVI.Utils;
using AgentVI.ViewModels;
using FFImageLoading.Svg.Forms;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using AgentVI.Interfaces;
using AgentVI.Views;
using Plugin.DeviceOrientation;
using Plugin.DeviceOrientation.Abstractions;

namespace AgentVI.Views
{
    public partial class MainPage : ContentPage
    {
        private enum AppTab{EventsPage, SensorsPage, SettingsPage};
        private enum Selection { Active, Passive};
        private LoginPageViewModel m_LoginPageViewModel = null;
        private FilterIndicatorViewModel m_FilterIndicatorViewModel = null;
        private Page m_FilterPage = null;
        private IProgress<ProgressReportModel> m_ProgressReporter = null;
        private readonly object contentViewUpdateLock = new object();
        private WaitingPage waitingPage = new WaitingPage();
        private Dictionary<AppTab, Tuple<ContentPage, SvgCachedImage>> pageCollection;
        private const string k_TabSelectionColor = "#BABABA";
        private const short k_NumberOfInitializations = 8;
        private Stack<ContentPage> contentViewStack;
        private ContentPage currentPageInContentView = null;

        public MainPage()
        {
            CrossDeviceOrientation.Current.LockOrientation(DeviceOrientations.Portrait);
        }

        public MainPage(IProgress<ProgressReportModel> i_ProgressReporter):this()
        {
            m_ProgressReporter = i_ProgressReporter;
            ProgressReportModel report = new ProgressReportModel(k_NumberOfInitializations);

            updateReporter("Initializing filter...", report);
            m_FilterIndicatorViewModel = new FilterIndicatorViewModel();
            m_FilterPage = new FilterPage(m_FilterIndicatorViewModel);
            InitializeComponent();
            FilterStateIndicatorListView.BindingContext = m_FilterIndicatorViewModel;
            updateReporter("Filter initialized correctly", report);

            updateReporter("Fetching account...", report);
            m_LoginPageViewModel = new LoginPageViewModel();
            m_LoginPageViewModel.InitializeFields(ServiceManager.Instance.LoginService.LoggedInUser);
            BindingContext = m_LoginPageViewModel;
            updateReporter("Account initialized.", report);

            initPagesCollectionHelper(report);

            NavigationPage.SetHasNavigationBar(this, false);

            FooterBarEvents_Clicked(null, null);
        }

        private string EnumToSVGPath(AppTab i_TabPage, Selection i_SelectionStatus)
        {
            string res;

            if(i_TabPage == AppTab.EventsPage && i_SelectionStatus == Selection.Active)
            {
                res = Settings.EventsTabSelectedSVGPath;
            }
            else if(i_TabPage == AppTab.EventsPage && i_SelectionStatus == Selection.Passive)
            {
                res = Settings.EventsTabSVGPath;
            }
            else if(i_TabPage == AppTab.SensorsPage && i_SelectionStatus == Selection.Active)
            {
                res = Settings.SensorsTabSelectedSVGPath;
            }
            else if(i_TabPage == AppTab.SensorsPage && i_SelectionStatus == Selection.Passive)
            {
                res = Settings.SensorsTabSVGPath;
            }
            else if(i_TabPage == AppTab.SettingsPage&& i_SelectionStatus == Selection.Active)
            {
                res = Settings.SettingsTabSelectedSVGPath;
            }
            else if(i_TabPage == AppTab.SettingsPage && i_SelectionStatus == Selection.Passive)
            {
                res = Settings.SettingsTabSVGPath;
            }
            else
            {
                res = null;
            }

            return res;
        }

        private void updateReporter(String i_StageCompleted, ProgressReportModel i_Report)
        {
            if (m_ProgressReporter != null && i_Report != null)
            {
                i_Report.Progress();
                i_Report.AddStage(i_StageCompleted);
                m_ProgressReporter.Report(i_Report);
            }
        }

        private void initPagesCollectionHelper(ProgressReportModel i_Report)
        {
            updateReporter("Initializing app pages...", i_Report);
            pageCollection = new Dictionary<AppTab, Tuple<ContentPage, SvgCachedImage>>();

            updateReporter("Fetching Cameras...", i_Report);
            CamerasPage camerasPageInstance = new CamerasPage();
            camerasPageInstance.RaiseContentViewUpdateEvent += OnContentViewUpdateEvent;
            pageCollection.Add(AppTab.SensorsPage, new Tuple<ContentPage, SvgCachedImage>(camerasPageInstance, FooterBarCamerasImage));

            updateReporter("Fetching Settings...", i_Report);
            pageCollection.Add(AppTab.SettingsPage, new Tuple<ContentPage, SvgCachedImage>(new SettingsPage(), FooterBarSettingsImage));

            updateReporter("Fetching Events...", i_Report);
            EventsPage eventsPageBuf = new EventsPage();
            eventsPageBuf.RaiseContentViewUpdateEvent += OnContentViewUpdateEvent;
            pageCollection.Add(AppTab.EventsPage, new Tuple<ContentPage, SvgCachedImage>(eventsPageBuf, FooterBarEventsImage));
        }

        private void markSelectedTab(AppTab i_SelectedTab)
        {
            pageCollection[i_SelectedTab].Item2.Source = EnumToSVGPath(i_SelectedTab, Selection.Active);
            foreach (KeyValuePair<AppTab, Tuple<ContentPage, SvgCachedImage>> kvPair in pageCollection)
            {
                if (kvPair.Key != i_SelectedTab)
                {
                    kvPair.Value.Item2.Source = EnumToSVGPath(kvPair.Key, Selection.Passive);
                }
            }
        }

        void FilterStateIndicator_Tapped(object i_Sender, EventArgs i_EventArgs)
        {
            Navigation.PushModalAsync(m_FilterPage);
        }

        void FooterBarEvents_Clicked(object i_Sender, EventArgs i_EventArgs)
        {
            currentPageInContentView = pageCollection[AppTab.EventsPage].Item1;
            PlaceHolder.Content = currentPageInContentView.Content;
            
            markSelectedTab(AppTab.EventsPage);
            resetContentViewStack();
        }

        void FooterBarCameras_Clicked(object i_Sender, EventArgs i_EventArgs)
        {
            currentPageInContentView = pageCollection[AppTab.SensorsPage].Item1;
            PlaceHolder.Content = currentPageInContentView.Content;
            markSelectedTab(AppTab.SensorsPage);
            resetContentViewStack();
        }

        void FooterBarSettings_Clicked(object i_Sender, EventArgs i_EventArgs)
        {
            currentPageInContentView = pageCollection[AppTab.SettingsPage].Item1;
            PlaceHolder.Content = currentPageInContentView.Content;
            markSelectedTab(AppTab.SettingsPage);
            resetContentViewStack();
        }

        protected override bool OnBackButtonPressed()
        {
            DependencyService.Get<IBackButtonPressed>().NativeOnBackButtonPressed();
            return true;
        }

        private void OnContentViewUpdateEvent(object sender, UpdatedContentEventArgs e)
        {
            lock(contentViewUpdateLock)
            {
                if (e == null || e.ContentUpdateType == UpdatedContentEventArgs.EContentUpdateType.Buffering)
                {
                    addToContentViewStack(currentPageInContentView);
                    PlaceHolder.Content = waitingPage.Content;
                }
                else if(e.ContentUpdateType == UpdatedContentEventArgs.EContentUpdateType.Pop)
                {
                    popFromControlViewStack();
                }
                else if(e.ContentUpdateType == UpdatedContentEventArgs.EContentUpdateType.PushAsync)
                {
                    addToContentViewStack(currentPageInContentView);
                    try
                    {
                        (e.UpdatedContent as IFocusable).Refocus();
                    }catch(NullReferenceException ex) { Console.WriteLine(ex.Message); }
                    Navigation.PushAsync(e.UpdatedContent);
                }
                else if(e.ContentUpdateType == UpdatedContentEventArgs.EContentUpdateType.PopAsync)
                {
                    Navigation.PopAsync();
                    ContentPage stackTop = contentViewStack.Pop();
                    try
                    {
                        (stackTop as IFocusable).Refocus();
                    }catch (NullReferenceException ex) { Console.WriteLine(ex.Message); }
                }
                else                                        //UpdatedContentEventArgs.EContentUpdateType.Push
                {
                    currentPageInContentView = e.UpdatedContent;
                    PlaceHolder.Content = currentPageInContentView.Content;
                }
            }
        }

        private void addToContentViewStack(ContentPage i_updatedContent)
        {
            if(contentViewStack == null)
            {
                contentViewStack = new Stack<ContentPage>();
            }
            contentViewStack.Push(i_updatedContent);
        }
        
        private void resetContentViewStack()
        {
            contentViewStack = null;
            CrossDeviceOrientation.Current.LockOrientation(DeviceOrientations.Portrait);
        }

        private void popFromControlViewStack()
        {
            ContentPage stackTop = contentViewStack.Pop();
            if (stackTop != null)
            {
                currentPageInContentView = stackTop;
                PlaceHolder.Content = stackTop.Content;
            }
            else
            {
                throw new Exception("MainPage.PopFromControlViewStack called unexpectedely");
            }
        }
    }
}