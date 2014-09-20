using CmdrCompanion.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.ViewModel
{
    public class DataSheetsHomeViewModel : LocalViewModelBase
    {
        public DataSheetsHomeViewModel()
        {
            CommodityDetails = new CommodityDetailsViewModel();
            PlaceDetails = new PlaceDetailsViewModel();

            CommoditiesView = new ListCollectionView(Environment.Commodities);
            CommoditiesView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            CommoditiesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            CommoditiesView.CurrentChanged += CommoditiesViewCurrentChanged;

            PlacesView = new ListCollectionView(Environment.Objects);
            PlacesView.GroupDescriptions.Add(new PropertyGroupDescription("Star"));
            PlacesView.SortDescriptions.Add(new SortDescription("Star.Name", ListSortDirection.Ascending));
            PlacesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            CommoditiesView.MoveCurrentToPosition(0);
            PlacesView.CurrentChanged += PlacesViewCurrentChanged;
        }

        void PlacesViewCurrentChanged(object sender, EventArgs e)
        {
            PlaceDetails.FillWithObject((AstronomicalObject)PlacesView.CurrentItem);
        }

        private void CommoditiesViewCurrentChanged(object sender, EventArgs e)
        {
            CommodityDetails.FillWithCommodity((Commodity)CommoditiesView.CurrentItem);
        }

        public override void Cleanup()
        {
            CommodityDetails.Cleanup();

            base.Cleanup();
        }

        public ListCollectionView CommoditiesView { get; private set; }

        public ListCollectionView PlacesView { get; private set; }

        public CommodityDetailsViewModel CommodityDetails { get; private set; }

        public PlaceDetailsViewModel PlaceDetails { get; private set; }
    }
}
