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
            StationsView.SortDescriptions.Add(new SortDescription("Star.Name", ListSortDirection.Ascending));

            SelectAnyCommand = new RelayCommand(SelectAny);
            SelectCurrentCommand = new RelayCommand(SelectCurrent, CanSelectCurrent);

            Environment.CurrentSituation.PropertyChanged += CurrentSituation_PropertyChanged;

            IsSelectAnyEnabled = true;
            IsSelectCurrentEnabled = true;
        }

        private bool _userCanSelectAny;
        public bool UserCanSelectAny
        {
            get { return _userCanSelectAny; }
            set
            {
                if (value != _userCanSelectAny)
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
                if (value != _userCanSelectCurrent)
                {
                    _userCanSelectCurrent = value;
                    RaisePropertyChanged("UserCanSelectCurrent");
                }
            }
        }

        private bool _isSelectCurrentEnabled;
        public bool IsSelectCurrentEnabled
        {
            get { return _isSelectCurrentEnabled; }
            set
            {
                if (value != _isSelectCurrentEnabled)
                {
                    _isSelectCurrentEnabled = value;
                    RaisePropertyChanged("SelectCurrentEnabled");
                }
            }
        }

        private bool _isSelectAnyEnabled;
        public bool IsSelectAnyEnabled
        {
            get { return _isSelectAnyEnabled; }
            set 
            { 
                if(value != _isSelectAnyEnabled)
                {
                    _isSelectAnyEnabled = value;
                    RaisePropertyChanged("SelectAnyEnabled");
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

        private Predicate<Station> _filter;
        public Predicate<Station> Filter
        {
            get
            {
                return _filter;
            }

            set
            {
                if (value != _filter)
                {
                    _filter = value;
                    if (value == null)
                        StationsView.Filter = null;
                    else if(StationsView.Filter == null)
                        StationsView.Filter = FilterInternal;
                    else
                        StationsView.Refresh();

                    RaisePropertyChanged("Filter");
                }
            }
        }

        public ListCollectionView StationsView { get; private set; }

        public RelayCommand SelectAnyCommand { get; private set; }

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


        private bool FilterInternal(object s)
        {
            if (Filter == null)
                return true;

            return Filter((Station)s);
        }

        private void CurrentSituation_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentLocation":
                    SelectCurrentCommand.RaiseCanExecuteChanged();
                    break;
            }
        }

        public void Refresh()
        {
            StationsView.Refresh();
        }

        public override void Cleanup()
        {
            Environment.CurrentSituation.PropertyChanged -= CurrentSituation_PropertyChanged;
            base.Cleanup();
        }
    }
}
