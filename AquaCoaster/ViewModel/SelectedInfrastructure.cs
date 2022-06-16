using AquaCoaster.Model;
using AquaCoaster.Model.Entities;
using AquaCoaster.Model.Enums;
using System;


namespace AquaCoaster.ViewModel
{
    public class SelectedInfrastructure : ViewModelBase
    {
        Infrastructure _infrastructure;
        public Infrastructure Infrastructure
        {
            get => _infrastructure;
            set
            {
                if (_infrastructure != value)
                {
                    _infrastructure = value;
                    OnPropertyChanged();
                }

                if (value != null)
                {
                    Status = value.Status;

                    if (value is Facility f)
                    {
                        InWaitingQueue = f.WaitingQueue.Count;
                        CurrentUsers = f.CurrentUsers.Count;
                        MinimumCapacity = f.MinimumCapacity;
                        UseFee = f.UseFee;
                    }

                    if (value is Gate g)
                    {
                        EntryFee = g.EntryFee;
                    }
                }
            }
        }

        InfrastructureStatus _status;
        public InfrastructureStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusString));
                }
            }
        }
        public String StatusString => InfrastructureStatusExtensions.DisplayName(_status);

        public String CapacityString => (Infrastructure is Facility f) ? $"{CurrentUsers}/{f.Capacity}" : "";

        Int32 _inWaitingQueue;
        public Int32 InWaitingQueue
        {
            get => _inWaitingQueue;
            set
            {
                if (_inWaitingQueue != value)
                {
                    _inWaitingQueue = value;
                    OnPropertyChanged();
                }
            }
        }

        Int32 _currentUsers;
        public Int32 CurrentUsers
        {
            get => _currentUsers;
            set
            {
                if (_currentUsers != value)
                {
                    _currentUsers = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CapacityString));
                }
            }
        }

        private Int32? _minimumCapacity;
        public Int32? MinimumCapacity
        {
            get => _minimumCapacity;
            set
            {
                if (_minimumCapacity != value)
                {
                    Facility f = Infrastructure as Facility;
                    _minimumCapacity = value;

                    if (value < 0)
                        f.MinimumCapacity = 0;
                    if (value > f.Capacity)
                        f.MinimumCapacity = f.Capacity;
                    else
                        f.MinimumCapacity = value;

                    OnPropertyChanged();
                }
            }
        }

        Int32 _useFee;
        public Int32 UseFee
        {
            get => _useFee;
            set
            {
                if (_useFee != value)
                {
                    Facility f = Infrastructure as Facility;
                    _useFee = value;

                    if (value < 0)
                        f.UseFee = 0;
                    if (value > GameModel.MAX_USE_FEE)
                        f.UseFee = GameModel.MAX_USE_FEE;
                    else
                        f.UseFee = value;

                    OnPropertyChanged();
                }
            }
        }

        Int32 _entryFee;
        public Int32 EntryFee
        {
            get => _entryFee;
            set
            {
                if (_entryFee != value)
                {
                    Gate g = Infrastructure as Gate;
                    _entryFee = value;

                    if (value < 0)
                        g.EntryFee = 0;
                    if (value > GameModel.MAX_ENTRY_FEE)
                        g.EntryFee = GameModel.MAX_ENTRY_FEE;
                    else
                        g.EntryFee = value;

                    OnPropertyChanged();
                }
            }
        }
    }
}
