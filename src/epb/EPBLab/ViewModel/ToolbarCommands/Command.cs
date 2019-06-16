
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Command;

namespace EPBLab.ViewModel.ToolbarCommands
{
    public class Command
    {
        public string Name { get; set; }
        public string Icon { get => _icon; set => _icon = $"pack://application:,,,/Images/ToolbarCommandIcons/{value}"; }
        private string _icon;
        public RelayCommand<Command> Select { get; set; }
        public RelayCommand Accept { get; set; }
        public RelayCommand Cancel { get; set; }
        public Command Self { get => this; }
        public List<Parameter> Parameters { get; set; }

        public Parameter ParameterByName(string name)
        {
            if (Parameters == null)
            {
                return null;
            }
            return Parameters.Where(p => p.Name == name).FirstOrDefault();
        }
    }
}
