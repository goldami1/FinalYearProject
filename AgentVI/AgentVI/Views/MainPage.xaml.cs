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

[assembly: Dependency(typeof(MainPage))]
namespace AgentVI.Views
{
    public partial class MainPage : ContentPage
    {
        private enum Tabs : int
        {
            [StringValue("Events")]
            Events,
            [StringValue("Sensors")]
            Sensors,
            [StringValue("Health")]
            Health,
            [StringValue("Settings")]
            Setting
        };
        private LoginPageViewModel m_LoginPageViewModel = null;
        private FilterIndicatorViewModel m_FilterIndicatorViewModel = null;
        private Page m_FilterPage = null;
        private IProgress<ProgressReportModel> m_ProgressReporter = null;
        private readonly object contentViewUpdateLock = new object();
        private WaitingPagexaml waitingPage = new WaitingPagexaml();
        private Dictionary<String, Tuple<ContentPage, Button, SvgCachedImage, Label>> pageCollection;
        private const String k_TabSelectionColor = "#BABABA";
        private const short k_NumberOfInitializations = 8;
        private Stack<View> contentViewStack;

        public MainPage()
        {

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

            PlaceHolder.Content = pageCollection["EventsPage"].Item1.Content;
            markSelectedTab(pageCollection["EventsPage"].Item2);
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
            pageCollection = new Dictionary<String, Tuple<ContentPage, Button, SvgCachedImage, Label>>();

            updateReporter("Fetching Cameras...", i_Report);
            CamerasPage camerasPageInstance = new CamerasPage();
            camerasPageInstance.RaiseContentViewUpdateEvent += OnContentViewUpdateEvent;
            pageCollection.Add("CamerasPage", new Tuple<ContentPage, Button, SvgCachedImage, Label>(camerasPageInstance, FooterBarCamerasButton, FooterBarCamerasImage, FooterBarCamerasLabel));

            updateReporter("Fetching Settings...", i_Report);
            pageCollection.Add("SettingsPage", new Tuple<ContentPage, Button, SvgCachedImage, Label>(new SettingsPage(), FooterBarSettingsButton, FooterBarSettingsImage, FooterBarSettingsLabel));

            updateReporter("Fetching Events...", i_Report);
            EventsPage eventsPageBuf = new EventsPage();
            eventsPageBuf.RaiseContentViewUpdateEvent += OnContentViewUpdateEvent;
            pageCollection.Add("EventsPage", new Tuple<ContentPage, Button, SvgCachedImage, Label>(eventsPageBuf, FooterBarEventsButton, FooterBarEventsImage, FooterBarEventsLabel));
        }

        

        private void markSelectedTab(Button i_SelectedTab)
        {
            foreach (var kvPair in pageCollection)
            {
                if (kvPair.Value.Item2 != i_SelectedTab)
                {
                    if (kvPair.Value.Item4 != null)
                    {
                        kvPair.Value.Item4.BackgroundColor = Color.Transparent;
                    }
                    if (kvPair.Value.Item3 != null)
                    {
                        if (kvPair.Key == "EventsPage")
                        {
                            kvPair.Value.Item3.Source = Settings.EventsTabSVGPath;
                        }
                        if (kvPair.Key == "CamerasPage")
                        {
                            kvPair.Value.Item3.Source = Settings.SensorsTabSVGPath;
                        }
                        if (kvPair.Key == "HealthPage")
                        {
                            kvPair.Value.Item3.Source = Settings.SensorsTabSVGPath;
                        }
                        if (kvPair.Key == "SettingsPage")
                        {
                            kvPair.Value.Item3.Source = Settings.SettingsTabSVGPath;
                        }
                    }
                }
                else
                {
                    if (kvPair.Value.Item4 != null)
                    {
                        kvPair.Value.Item4.BackgroundColor = Color.FromHex(k_TabSelectionColor);
                    }

                    if (kvPair.Value.Item3 != null)
                    {
                        if (kvPair.Key == "EventsPage")
                        {
                            kvPair.Value.Item3.Source = Settings.EventsTabSelectedSVGPath;
                        }
                        if (kvPair.Key == "CamerasPage")
                        {
                            kvPair.Value.Item3.Source = Settings.SensorsTabSelectedSVGPath;
                        }
                        if (kvPair.Key == "HealthPage")
                        {
                            kvPair.Value.Item3.Source = Settings.SensorsTabSelectedSVGPath;
                        }
                        if (kvPair.Key == "SettingsPage")
                        {
                            kvPair.Value.Item3.Source = Settings.SettingsTabSelectedSVGPath; ;
                        }
                    }
                }
            }
        }

        void FilterStateIndicator_Tapped(object i_Sender, EventArgs i_EventArgs)
        {
            Navigation.PushModalAsync(m_FilterPage);
        }

        void FooterBarEvents_Clicked(object i_Sender, EventArgs i_EventArgs)
        {
            PlaceHolder.Content = pageCollection["EventsPage"].Item1.Content;
            markSelectedTab(i_Sender as Button);
            resetContentViewStack();
        }

        void FooterBarCameras_Clicked(object i_Sender, EventArgs i_EventArgs)
        {
            PlaceHolder.Content = pageCollection["CamerasPage"].Item1.Content;
            markSelectedTab(i_Sender as Button);
            resetContentViewStack();
        }

        void FooterBarHealth_Clicked(object i_Sender, EventArgs i_EventArgs)
        {
            markSelectedTab(i_Sender as Button);
        }

        void FooterBarSettings_Clicked(object i_Sender, EventArgs i_EventArgs)
        {
            PlaceHolder.Content = pageCollection["SettingsPage"].Item1.Content;
            markSelectedTab(i_Sender as Button);
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
                if (e == null)
                {
                    addToContentViewStack(PlaceHolder.Content);
                    PlaceHolder.Content = waitingPage.Content;
                }
                else if(e != null && e.IsStackPopRequested)
                {
                    popFromControlViewStack();
                }
                else
                {
                    PlaceHolder.Content = e.UpdatedContent.Content;
                }
            }
            
        }

        private void addToContentViewStack(View i_updatedContent)
        {
            if(contentViewStack == null)
            {
                contentViewStack = new Stack<View>();
            }
            contentViewStack.Push(i_updatedContent);
        }
        
        private void resetContentViewStack()
        {
            contentViewStack = null;
        }

        private void popFromControlViewStack()
        {
                View stackTop = contentViewStack.Pop();
                if (stackTop != null)
                {
                    PlaceHolder.Content = stackTop;
                }
                else
                {
                    throw new Exception("MainPage.PopFromControlViewStack called unexpectedely");
                }
        }
    }
}