using CmdrCompanion.Core;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CmdrCompanion.Interface
{
    public class PluginContainer : INotifyPropertyChanged
    {
        public string AssemblyName { get; set; }
        public string AssemblyPath { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAsync { get; set; }

        public IDataFeeder Instance { get; set; }
        public IAsyncDataFeeder AsyncInstance { get; set; }

        private bool _isInitialized;
        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
            set
            {
                if(value != _isInitialized)
                {
                    _isInitialized = value;
                    OnPropertyChanged("IsInitialized");
                }
            }
        }

        private bool _isActive;
        public bool IsActive 
        { 
            get
            {
                return _isActive;
            }
            private set
            {
                if(value != _isActive)
                {
                    _isActive = value;
                    OnPropertyChanged("IsActive");
                }
            }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            private set
            {
                if(value != _isRunning)
                {
                    _isRunning = value;
                    OnPropertyChanged("IsRunning");
                }
            }
        }

        private DispatcherTimer Timer { get; set; }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            Assembly asm = Assembly.LoadFrom(AssemblyPath);
            Type t = asm.GetType(TypeName);
            if (IsAsync) 
                AsyncInstance = Activator.CreateInstance(t) as IAsyncDataFeeder;
            else
                Instance = Activator.CreateInstance(t) as IDataFeeder;

            IsInitialized = true;
        }

        public void Activate()
        {
            if (!IsInitialized)
                throw new InvalidOperationException();

            if (IsActive)
                return;

            int interval = 0;
            if (IsAsync)
                interval = AsyncInstance.Start(ServiceLocator.Current.GetInstance<EliteEnvironment>());
            else
                interval = Instance.Start(ServiceLocator.Current.GetInstance<EliteEnvironment>());

            if (interval <= 0)
            {
                IsActive = false;
                return;
            }

            Timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = new TimeSpan(0, 0, interval),
            };

            Timer.Tick += timer_Tick;

            IsActive = true;

            DoUpdate();
        }

        public void Deactivate()
        {
            IsActive = false;
            if(Timer != null)
                Timer.Stop();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            DoUpdate();
        }

        private void DoUpdate()
        {
            if (!IsActive)
                return;


            if (IsAsync)
            {
                IsRunning = true;
                Task<bool> task = AsyncInstance.Update(ServiceLocator.Current.GetInstance<EliteEnvironment>());
                task.ContinueWith((t =>
                {
                    IsRunning = false;
                    if (task.Result)
                        // TODO : Thread safety on the Timer object !
                        Timer.Start();
                }));
            }
            else
            {
                if (Instance.Update(ServiceLocator.Current.GetInstance<EliteEnvironment>()))
                    Timer.Start();
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
