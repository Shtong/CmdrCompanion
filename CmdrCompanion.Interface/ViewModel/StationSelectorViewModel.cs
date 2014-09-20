using CmdrCompanion.Core;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.ViewModel
{
    public sealed class StationSelectorViewModel : LocalViewModelBase
    {
        public StationSelectorViewModel()
        {
            StationsView = new ListCollectionView(Environment.Stations);
            StationsView.CurrentChanged += (sender, e) => RaisePropertyChanged("SelectedStation");
            StationsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            SelectAnyCommand = new RelayCommand(SelectAny);
            SelectCurrentCommand = new RelayCommand(SelectCurrent, CanSelectCurrent);

            Environment.CurrentSituation.PropertyChanged += CurrentSituation_PropertyChanged;
        }

        private bool _userCanSelectAny;
        public bool UserCanSelectAny
        {
            get { return _userCanSelectAny; }
            set
            {
                if(value != _userCanSelectAny)
                {
                    _userCanSelectAny = value;
                    RaisePropertyChanged("UserCanSelectAny");
                }
            }
        }

        private bool _userCanSelectCurrent;
        public bool UserCanSelectCurrent
        {
            get { return _userCanSelectCurrent; }
            set
            {
                if(value != _userCanSelectCurrent)
                {
                    _userCanSelectCurrent = value;
                    RaisePropertyChanged("UserCanSelectCurrent");
                }
            }
        }

        public Station SelectedStation
        {
            get
            {
                return StationsView.CurrentItem as Station;
            }
            set
            {
                if (value == null)
                    StationsView.MoveCurrentToPosition(-1);
                else
                    StationsView.MoveCurrentTo(value);
                RaisePropertyChanged("SelectedStation");
            }
        }

        public ListCollectionView StationsView { get; private set; }

        public RelayCommand SelectAnyCommand { get; private set;}

        public void SelectAny()
        {
            if (!UserCanSelectAny)
                return;

            StationsView.MoveCurrentToPosition(-1);
        }

        public RelayCommand SelectCurrentCommand { get; private set; }

        public void SelectCurrent()
        {
            if (!UserCanSelectCurrent || 
                Environment.CurrentSituation.CurrentLocation == null || 
                !(Environment.CurrentSituation.CurrentLocation is Station))
                return;

            StationsView.MoveCurrentTo(Environment.CurrentSituation.CurrentLocation);
        }

        public bool CanSelectCurrent()
        {
            return Environment.CurrentSituation.CurrentLocation != null && Environment.CurrentSituation.CurrentLocation is Station;
        }




        private void CurrentSituation_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "CurrentLocation":
                    SelectCurrentCommand.RaiseCanExecuteChanged();
                    break;
            }
        }

        public override void Cleanup()
        {
            Environment.CurrentSituation.PropertyChanged -= CurrentSituation_PropertyChanged;
            base.Cleanup();
        }
    }
}
