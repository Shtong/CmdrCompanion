/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:CmdrCompanion.Interface"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;

namespace CmdrCompanion.Interface.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private List<ViewModelBase> ViewModels { get; set; }

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            ViewModels = new List<ViewModelBase>();
            RegisterViewModel<MainViewModel>();
            RegisterViewModel<DataSheetsHomeViewModel>();
            RegisterViewModel<TravelDesignerViewModel>();
            RegisterViewModel<JumpFinderViewModel>();
            RegisterViewModel<OptionsViewModel>();
            RegisterViewModel<SituationViewModel>();
        }

        private void RegisterViewModel<TViewModel>() where TViewModel:ViewModelBase
        {
            try
            {
                SimpleIoc.Default.Register<TViewModel>();
                ViewModels.Add((ViewModelBase)ServiceLocator.Current.GetInstance<TViewModel>());
            }
            catch(Exception ex)
            {
                throw new Exception(String.Format("Could not initialize view model {0}", typeof(TViewModel).FullName), ex);
            }
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public DataSheetsHomeViewModel CommodityAnalyzer
        {
            get
            {
                return ServiceLocator.Current.GetInstance<DataSheetsHomeViewModel>();
            }
        }

        public TravelDesignerViewModel TravelDesigner
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TravelDesignerViewModel>();
            }
        }

        public JumpFinderViewModel JumpFinder
        {
            get
            {
                return ServiceLocator.Current.GetInstance<JumpFinderViewModel>();
            }
        }

        public OptionsViewModel Options
        {
            get
            {
                return ServiceLocator.Current.GetInstance<OptionsViewModel>();
            }
        }

        public SituationViewModel Situation
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SituationViewModel>();
            }
        }
        
        public void Cleanup()
        {
            foreach (ViewModelBase vm in ViewModels)
                vm.Cleanup();
        }
    }
}