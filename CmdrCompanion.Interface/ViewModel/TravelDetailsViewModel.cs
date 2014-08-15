using CmdrCompanion.Core;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace CmdrCompanion.Interface.ViewModel
{
    public class TravelDetailsViewModel : LocalViewModelBase
    {
        internal TravelDetailsViewModel(IEnumerable<TradeJumpData> jumps)
        {
            Jumps = jumps;
            DataDate = DateTime.Now;
            int jCount = 0;

            List<string> stationNames = new List<string>(32);
            foreach (TradeJumpData tjd in Jumps)
            {
                ++jCount;
                TotalProfit += tjd.TotalProfit;
                stationNames.Add(tjd.From.Station.Name);
                if (tjd.DataDate < DataDate)
                    DataDate = tjd.DataDate;
            }

            AvgProfitPerJump = TotalProfit / jCount;

            stationNames.Add(stationNames[0]);
            PlanText = String.Join(" -> ", stationNames);
            JumpCount = jCount;

            _ageUpdateTimer = new DispatcherTimer();
            _ageUpdateTimer.Interval = new TimeSpan(0, 1, 0);
            _ageUpdateTimer.Tick += UpdateAge;
            _ageUpdateTimer.Start();
        }

        private DispatcherTimer _ageUpdateTimer;

        public IEnumerable<TradeJumpData> Jumps { get; private set; }

        public float AvgProfitPerJump { get; private set; }

        public float TotalProfit { get; private set; }

        public string PlanText { get; private set; }

        public int JumpCount { get; private set; }

        public DateTime DataDate { get; private set; }

        public TimeSpan DataAge { get; private set; }

        public string DataAgeText { get; private set; }

        private List<TravelStep> _stepsList;
        public List<TravelStep> StepsList 
        { 
            get
            {
                // This is lazy loaded because it's only displayed in the detailed view 
                // and I didn't want to put this in the constructor, which would have added
                // more work to the already heavy path generation algorithm
                if(_stepsList == null)
                {
                    _stepsList = new List<TravelStep>();
                    TradeJumpData previousData = null;
                    int i = 0;
                    foreach(TradeJumpData data in Jumps)
                    {
                        if(i == 0)
                        {
                            StepsList.Add(new TravelStep()
                            {
                                Type = "initial",
                                Text = String.Format("Start at {0} - {1}", data.From.Station.Star.Name, data.From.Station.Name),
                            });
                        }
                        StepsList.Add(new TravelStep()
                        {
                            Type = "buy",
                            Text = String.Format("Buy {0} units of {1} for a total cost of {2}", data.CargoSize, data.Commodity.Name, data.TotalPrice),
                        });
                        StepsList.Add(new TravelStep()
                        {
                            Type = "jump",
                            Text = String.Format("Jump to {0} - {1}", data.To.Station.Star.Name, data.To.Station.Name),
                        });
                        StepsList.Add(new TravelStep()
                        {
                            Type = "sell",
                            Text = String.Format("Sell all your stock of {0} for a profit of {1}", data.Commodity.Name, data.TotalProfit),
                        });
                        ++i;
                        previousData = data;
                    }
                    StepsList.Add(new TravelStep()
                    {
                        Type = "final",
                        Text = "Rinse and repeat !",
                    });
                }

                return _stepsList;
            }
        }

        private ListCollectionView _stepsView;
        public ListCollectionView StepsView 
        {
            get
            {
                if(_stepsView == null)
                {
                    _stepsView = new ListCollectionView(StepsList);
                }

                return _stepsView;
            }
        }

        public class TravelStep
        {
            public string Type { get; set; }

            public string Text { get; set; }
        }

        public override void Cleanup()
        {
            _ageUpdateTimer.Stop();
            _ageUpdateTimer = null;

            base.Cleanup();
        }

        private void UpdateAge(object sender, EventArgs e)
        {
            // Even though the actual update date did not change,
            // we trigger this even every minute so that the text
            // explaining the data age updates
            RaisePropertyChanged("DataDate");
        }
    }
}
