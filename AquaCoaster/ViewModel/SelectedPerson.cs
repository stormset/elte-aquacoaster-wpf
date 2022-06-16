using AquaCoaster.Model;
using AquaCoaster.Model.Entities;
using AquaCoaster.Model.Enums;
using System;


namespace AquaCoaster.ViewModel
{
    public class SelectedPerson : ViewModelBase
    {
        Person _person;
        public Person Person
        {
            get => _person;
            set
            {
                if (_person != value)
                {
                    _person = value;
                    OnPropertyChanged();
                }

                if (value != null)
                {
                    Status = value.Status;

                    if (value is Visitor v)
                    {
                        Money = v.Money;
                        Food = v.Food;
                        Mood = v.Mood;
                    }
                }
            }
        }

        PersonStatus _status;
        public PersonStatus Status
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
        public String StatusString => PersonStatusExtensions.DisplayName(_status);


        Int32 _money;
        public Int32 Money
        {
            get => _money;
            set
            {
                if (_money != value)
                {
                    _money = value;
                    OnPropertyChanged();
                }
            }
        }

        float _mood;
        public float Mood
        {
            get => _mood * 100;
            set
            {
                if (_mood != value)
                {
                    _mood = value;
                    OnPropertyChanged();
                }
            }
        }
        public String MoodString => $"{(Int32)Mood} %";

        float _food;
        public float Food
        {
            get => _food * 100;
            set
            {
                if (_food != value)
                {
                    _food = value;
                    OnPropertyChanged();
                }
            }
        }
        public String FoodString => $"{(Int32)Food} %";
    }
}
