using CmdrCompanion.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace CmdrCompanion.Interface.ViewModel
{
    public class TravelDesignerViewModel : LocalViewModelBase
    {
        public TravelDesignerViewModel()
        {
            ComputeCommand = new RelayCommand(Compute, CanCompute);
            ShowDetailedResultCommand = new RelayCommand<TravelDetailsViewModel>(ShowDetailedResults);

            StationsView = new ListCollectionView(Environment.Stations);

            ResultsList = new ObservableCollection<TravelDetailsViewModel>();
            ResultsView = new ListCollectionView(ResultsList);
            ResultsView.SortDescriptions.Add(new SortDescription("AvgProfitPerJump", ListSortDirection.Descending));

            Cargo = 4;
            Budget = 1000;
            MaxJumpsPerTravel = 4;
            MaxDistanceFromOrigin = 500;
            MaxDistancePerjump = 10;
        }

        public ListCollectionView StationsView { get; private set; }

        public int Cargo { get; set; }

        public int Budget { get; set; }

        public int MaxJumpsPerTravel { get; set; }

        public float MaxDistanceFromOrigin { get; set; }

        public float MaxDistancePerjump { get; set; }

        private Station _selectedOrigin;
        public Station SelectedOrigin 
        {
            get { return _selectedOrigin; }
            set
            {
                if(value != _selectedOrigin)
                {
                    _selectedOrigin = value;
                    RaisePropertyChanged("SelectedOrigin");
                    ComputeCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private ObservableCollection<TravelDetailsViewModel> ResultsList { get; set; }

        public ListCollectionView ResultsView { get; private set; }

        public RelayCommand ComputeCommand { get; private set; }

        public bool CanCompute()
        {
            return SelectedOrigin != null;
        }

        public void Compute()
        {
            ResultsList.Clear();

            Station[] stations = Environment.Stations.Where(s => s.Star.KnownStarProximities.ContainsKey(SelectedOrigin.Star) && s.Star.KnownStarProximities[SelectedOrigin.Star] < MaxDistanceFromOrigin).ToArray();
            ThreadPool.QueueUserWorkItem(ComputeWorker, new ComputeArgs() {
                stations = stations,
                cargo = Cargo,
                budget = Budget,
                maxJumps = MaxJumpsPerTravel,
                origin = SelectedOrigin,
                maxDistPerJump = MaxDistancePerjump,
            });
        }

        public RelayCommand<TravelDetailsViewModel> ShowDetailedResultCommand { get; private set; }

        public void ShowDetailedResults(TravelDetailsViewModel detailedModel)
        {
            // Ask the interface to open the details window
            MessengerInstance.Send(new ShowDetailedResultsMessage(detailedModel));
        }

        #region Messages
        public class ShowDetailedResultsMessage : GenericMessage<TravelDetailsViewModel>
        {
            internal ShowDetailedResultsMessage(TravelDetailsViewModel content) : base (content)
            {

            }
        }

        #endregion

        # region Path calculation stuff
        private class ComputeArgs
        {
            public Station[] stations;
            public int cargo;
            public float budget;
            public int maxJumps;
            public Station origin;
            public float maxDistPerJump;
        }

        private void ComputeWorker(object oArgs)
        {
            // TODO : Check that more than 2 stations are selected
            ComputeArgs args = (ComputeArgs)oArgs;

            EliteEnvironment env = ServiceLocator.Current.GetInstance<EliteEnvironment>(); // TODO : Any thread safety issue here ?
            Station[] stations = args.stations;

            // Generate ideal trades for each link

            Dictionary<int, Dictionary<int, TradeJumpData>> jumps = new Dictionary<int, Dictionary<int, TradeJumpData>>();
            for (int i = 0; i < stations.Length; ++i)
            {
                Dictionary<int, TradeJumpData> currentToDict = new Dictionary<int, TradeJumpData>();
                jumps.Add(i, currentToDict);
                for (int j = 0; j < stations.Length; ++j)
                {
                    if (i == j)
                        continue;

                    // Don't include this link if no distance is known
                    if (!stations[i].Star.KnownStarProximities.ContainsKey(stations[j].Star))
                        continue;
                    
                    // Don't include this link if the distance is too long
                    if (stations[i].Star.KnownStarProximities[stations[j].Star] > args.maxDistPerJump)
                        continue;

                    TradeJumpData tjd = env.FindBestProfit(stations[i], stations[j], Cargo, Budget); // TODO : Thread safety
                    if (tjd == null || tjd.TotalProfit < 1)
                        continue;

                    currentToDict.Add(j, tjd);
                }
            }

            // Scan for routes :
            // - Select the first station as the starting point of the first route
            // - Check all paths from this point
            // - Once all paths from point 1 are scanned, do the same starting from station 2
            // - EXCEPT that any path going through station 1 can be abandonned (we already have it)

            ComputeData cd = new ComputeData()
            {
                stations = stations,
                currentPath = new Stack<int>(args.maxJumps),
                results = new SortedDictionary<float, int[]>(),
                jumpData = jumps,
                maxLength = args.maxJumps,
            };

            // No need to scan the last point since all of its paths have
            // already been scanned from the other points
            for (int i = 0; i < stations.Length - 1; ++i)
            {
                cd.currentPath.Clear();
                cd.currentPath.Push(i);
                cd.currentStart = i;
                CheckJumpRecursive(cd);
            }
        }

        private class ComputeData
        {
            public Station[] stations;
            public Stack<int> currentPath;
            public int currentStart;
            public SortedDictionary<float, int[]> results;
            public Dictionary<int, Dictionary<int, TradeJumpData>> jumpData;
            public int maxLength;
        }

        private void CheckJumpRecursive(ComputeData data)
        {
            int currentNode = data.currentPath.Peek();

            for(int i = data.currentStart; i < data.stations.Length; ++i)
            {
                // Is there actually a link between these two nodes ?
                if (!data.jumpData[currentNode].ContainsKey(i))
                    continue;

                // Am I back at the start ?
                if(i == data.currentStart)
                {
                    // If so, we have a loop.
                    // Send the results and go to the next point

                    TradeJumpData[] jumps = new TradeJumpData[data.currentPath.Count];
                    int[] pathArray = data.currentPath.ToArray(); // We can't iterate directly on currentPath because it would iterate in reversed order
                    Array.Reverse(pathArray);
                    for(int j = 0; j < pathArray.Length - 1; ++j)
                        jumps[j] = data.jumpData[pathArray[j]][pathArray[j+1]];
                    jumps[jumps.Length - 1] = data.jumpData[currentNode][i];

                    DispatcherHelper.UIDispatcher.BeginInvoke(new Action<TravelDetailsViewModel>(RegisterComputeResult), DispatcherPriority.Background, new TravelDetailsViewModel(jumps));
                    continue;
                }

                // Should we keep digging ?
                if (data.currentPath.Count >= data.maxLength)
                    // This path is getting too long
                    continue;

                // Keep digging
                data.currentPath.Push(i);
                CheckJumpRecursive(data);
                data.currentPath.Pop();
            }
        }

        private void RegisterComputeResult(TravelDetailsViewModel result)
        {
            ResultsList.Add(result);
        }
        #endregion
    }
}
