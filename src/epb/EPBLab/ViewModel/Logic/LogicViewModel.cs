
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EPBLib;
using EPBLib.Logic;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel.Logic
{
    public class LogicViewModel : ViewModelBase
    {
        protected EpBlueprint Blueprint;

        #region Properties
        public ObservableCollection<SignalSourceViewModel> SignalSources
        {
            get => _SignalSources;
            set => Set (ref _SignalSources, value);
        }
        protected ObservableCollection<SignalSourceViewModel> _SignalSources;

        public ObservableCollection<SignalTargetViewModel> SignalTargets
        {
            get => _SignalTargets;
            set => Set(ref _SignalTargets, value);
        }
        protected ObservableCollection<SignalTargetViewModel> _SignalTargets;

        public ObservableCollection<SignalOperatorViewModel> SignalOperators
        {
            get => _SignalOperators;
            set => Set(ref _SignalOperators, value);
        }
        protected ObservableCollection<SignalOperatorViewModel> _SignalOperators;

        public ObservableCollection<ConnectorViewModel> Connectors
        {
            get => _Connectors;
            set => Set(ref _Connectors, value);
        }
        protected ObservableCollection<ConnectorViewModel> _Connectors;
        #endregion Properties

        public LogicViewModel(EpBlueprint blueprint)
        {
            Blueprint = blueprint;

            Update();
        }

        public void Update()
        {
            //SignalSources = new ObservableCollection<SignalSourceViewModel>(blueprint.SignalSources.Select(x => new SignalSourceViewModel(blueprint, x) {X = rnd.NextDouble() * 400, Y = rnd.NextDouble() * 400 }));
            SignalSources = new ObservableCollection<SignalSourceViewModel>();
            SignalTargets = new ObservableCollection<SignalTargetViewModel>();
            SignalOperators = new ObservableCollection<SignalOperatorViewModel>();
            Connectors = new ObservableCollection<ConnectorViewModel>();

            for (int i = 0; i < Blueprint.SignalSources.Count; i++)
            {
                SignalSources.Add(new SignalSourceViewModel(Blueprint, Blueprint.SignalSources[i], GetPosition(PositionType.Source, i)));
            }


            for (int o = 0; o < Blueprint.SignalOperators.Count; o++)
            {
                EpbSignalOperator op = Blueprint.SignalOperators[o];
                SignalOperatorViewModel opVm = new SignalOperatorViewModel(Blueprint, op, GetPosition(PositionType.Operator, o));
                SignalOperators.Add(opVm);
                for (int i = 0; i < opVm.Inputs.Count; i++)
                {
                    SignalSourceViewModel sourceVm = (from x in SignalSources where x.Name == ((EpbBlockTagString)op.Tags[$"InSig{i}"]).Value select x).FirstOrDefault();
                    if (sourceVm != null)
                    {
                        Connectors.Add(new ConnectorViewModel(sourceVm.Outputs[0], opVm.Inputs[i]));
                    }
                }
            }

            // Add connectors from operator to operator:
            foreach (SignalOperatorViewModel opVm in SignalOperators)
            {
                for (int i = 0; i < opVm.Inputs.Count; i++)
                {
                    SignalOperatorViewModel sourceVm = (from x in SignalOperators where x.OutSig == ((EpbBlockTagString)opVm.Operator.Tags[$"InSig{i}"]).Value select x).FirstOrDefault();
                    if (sourceVm != null)
                    {
                        Connectors.Add(new ConnectorViewModel(sourceVm.Outputs[0], opVm.Inputs[i]));
                    }
                }
            }



            for (int i = 0; i < Blueprint.SignalTargets.Count; i++)
            {
                EpbSignalTarget target = Blueprint.SignalTargets[i];
                SignalTargetViewModel targetVm = new SignalTargetViewModel(Blueprint, target, GetPosition(PositionType.Source, i));
                SignalTargets.Add(targetVm);

                SignalOperatorViewModel operatorVm = (from x in SignalOperators where x.OutSig == target.SignalName select x).FirstOrDefault();
                if (operatorVm != null)
                {
                    Connectors.Add(new ConnectorViewModel(operatorVm.Outputs[0], targetVm.Inputs[0]));
                }

                SignalSourceViewModel sourceVm = (from x in SignalSources where x.Name == target.SignalName select x).FirstOrDefault();
                if (sourceVm != null)
                {
                    Connectors.Add(new ConnectorViewModel(sourceVm.Outputs[0], targetVm.Inputs[0]));
                }
            }
        }


        protected enum PositionType
        {
            Source,
            Target,
            Operator
        }
        protected Random Rnd = new Random();

        /// <summary>
        /// Returns the saved position for this node.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected Point GetPosition(PositionType type, int index)
        {
            // TODO: Look up the position in a file given to the constructor.
            return new Point(Rnd.NextDouble() * 400, Rnd.NextDouble() * 400);
        }
    }
}
