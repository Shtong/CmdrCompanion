using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// This class is used as the root access point for all the data of a CmdrCompanion session.
    /// </summary>
    public class EliteEnvironment : CoreObject
    {
        /// <summary>
        /// Creates a new instance of <see cref="EliteEnvironment"/>.
        /// </summary>
        public EliteEnvironment()
        {
            AutoDistanceEnabled = true;

            Stars = new ObservableCollection<Star>();

            InternalCommodities = new ObservableCollection<Commodity>();
            Commodities = new ReadOnlyObservableCollection<Commodity>(InternalCommodities);

            InternalStations = new ObservableCollection<Station>();
            Stations = new ReadOnlyObservableCollection<Station>(InternalStations);
        }

        /// <summary>
        /// Gets a list of all the known stars
        /// </summary>
        /// <seealso cref="CreateStar"/>
        public ObservableCollection<Star> Stars { get; private set; }

        internal ObservableCollection<Station> InternalStations { get; private set; }

        /// <summary>
        /// Gets a list of all the known stations
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is actually a shortcut to the list of stations contained in all the <see cref="Stars"/>
        /// of this environment.
        /// </para>
        /// </remarks>
        public ReadOnlyObservableCollection<Station> Stations { get; set; }

        internal ObservableCollection<Commodity> InternalCommodities { get; private set; }
        /// <summary>
        /// Gets a list of all known tradable commodities
        /// </summary>
        /// <seealso cref="CreateCommodity"/>
        public ReadOnlyObservableCollection<Commodity> Commodities { get; private set; }

        /// <summary>
        /// Finds a specific star based on its name
        /// </summary>
        /// <param name="name">The name of a star</param>
        /// <param name="comparisonOptions">Options for comparing the star names</param>
        /// <returns>The <see cref="Star"/> instance that was found, or null if no star could be found with that name.</returns>
        /// <exception cref="ArgumentNullException">The provided name is null.</exception>
        public Star FindStarByName(string name, StringComparison comparisonOptions = StringComparison.InvariantCultureIgnoreCase)
        {
            if (name == null)
                throw new ArgumentNullException("name", "A star name cannot be null");

            return Stars.Where(s => s.Name.Equals(name, comparisonOptions)).FirstOrDefault();
        }

        /// <summary>
        /// Creates a new <see cref="Star"/> instance and adds it into this environment
        /// </summary>
        /// <param name="name">The name of the new star system</param>
        /// <returns>The created <see cref="Star"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The provided name is null</exception>
        /// <exception cref="ArgumentException">The provided name is already used by an existing star</exception>
        public Star CreateStar(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name", "A star name cannot be null");

            if (FindStarByName(name) != null)
                throw new ArgumentException("A star with the same name already exists");

            Star result = new Star(name, this);

            if(AutoDistanceEnabled)
            {
                float distance = 0;
                foreach(Star otherStar in Stars)
                {
                    if (DistancesDB.TryGetDistance(result, otherStar, out distance))
                        result.RegisterDistanceFrom(otherStar, distance);
                }
            }

            Stars.Add(result);
            return result;
        }

        /// <summary>
        /// Finds a commodity that has the sspecified name
        /// </summary>
        /// <param name="name">A commodity name to look for</param>
        /// <param name="comparisonOptions">Options used to compare the commodity names</param>
        /// <returns>The <see cref="Commodity"/> that was found, or null if the provided name does not match any commodity.</returns>
        /// <exception cref="ArgumentNullException">The provided name is null.</exception>
        public Commodity FindCommodityByName(string name, StringComparison comparisonOptions = StringComparison.InvariantCultureIgnoreCase)
        {
            if (name == null)
                throw new ArgumentNullException("name", "A commodity name cannot be null");

            return InternalCommodities.Where(c => c.Name.Equals(name, comparisonOptions)).FirstOrDefault();
        }

        /// <summary>
        /// Creates a new <see cref="Commodity"/> instance and adds it into this environment
        /// </summary>
        /// <param name="name">The name of the new commodity</param>
        /// <param name="category">An opptionnal category name</param>
        /// <returns>The newly created <see cref="Commodity"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The provided name is null.</exception>
        public Commodity CreateCommodity(string name, string category = null)
        {
            if (name == null)
                throw new ArgumentNullException("name", "The commodity name cannot be null");

            Commodity result = new Commodity(name, category);
            InternalCommodities.Add(result);
            return result;
        }

        /// <summary>
        /// Gets the standard Elite:Dangerous time
        /// </summary>
        public DateTime Now
        {
            get
            {
                return DateTime.UtcNow.AddYears(1286);
            }
        }

        /// <summary>
        /// Computes the most profitable trading jump given the specified constraints
        /// </summary>
        /// <param name="from">The <see cref="Station"/> to buy the goods from</param>
        /// <param name="to">The <see cref="Station"/> to sell the goods at</param>
        /// <param name="cargo">The maximum amount of units that can be bought</param>
        /// <param name="budget">The maximum price that can be paid for buying the goods</param>
        /// <returns>A <see cref="TradeJumpData"/> instance describing the ideal trade operation</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="from"/> or <paramref name="to"/> arguments are null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        ///     <item>
        ///         <description>The <paramref name="cargo"/> parameter is negative or zero.</description>
        ///     </item>
        ///     <item>
        ///         <description>The <paramref name="budget"/> parameter is negative or zero.</description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">The <paramref name="from"/> and <paramref name="to"/> parameters are equal</exception>
        public TradeJumpData FindBestProfit(Station from, Station to, int cargo, int budget)
        {
            if (from == null)
                throw new ArgumentNullException("from");
            if (to == null)
                throw new ArgumentNullException("to");
            if (from == to)
                throw new ArgumentException("The from and to stations cannot be the same");
            if (cargo <= 0)
                throw new ArgumentOutOfRangeException("cargo");
            if (budget <= 0)
                throw new ArgumentOutOfRangeException("budget");

            TradeJumpData result = new TradeJumpData();
            result.TotalProfit = Single.MinValue;

            foreach(Trade t in from.Trades)
            {
                if(!t.CanSell)
                    continue;

                Trade t2 = to.FindCommodity(t.Commodity);
                if (t2 == null || !t2.CanBuy)
                    continue;

                float profitPerUnit = t2.BuyingPrice - t.SellingPrice;
                int cargoSize = Math.Min(cargo, (int)(budget / t.SellingPrice));
                float totalProfit = profitPerUnit * cargoSize;

                if(totalProfit > result.TotalProfit)
                    result.Fill(t, t2, cargoSize);
            }

            if (result.Commodity == null)
                return null;
            return result;
        }

        private bool _autoDistanceEnabled;
        /// <summary>
        /// Gets or sets whether all distances are automatically filled out when adding a new star in this system.
        /// </summary>
        public bool AutoDistanceEnabled
        {
            get { return _autoDistanceEnabled; }
            set
            {
                if(value != _autoDistanceEnabled)
                {
                    _autoDistanceEnabled = value;
                    OnPropertyChanged("AutoDistanceEnabled");
                }
            }
        }
    }
}
