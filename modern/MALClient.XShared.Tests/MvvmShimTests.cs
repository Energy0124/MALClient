using System.Collections.Generic;
using System.Windows.Input;
using MALClient.XShared.Mvvm;
using Xunit;

namespace MALClient.XShared.Tests
{
    public class MvvmShimTests
    {
        private class SampleViewModel : ViewModelBase
        {
            private int _count;

            public int Count
            {
                get => _count;
                set
                {
                    _count = value;
                    RaisePropertyChanged(() => Count);
                }
            }

            public string Name
            {
                get => null;
                set => RaisePropertyChanged();
            }

            private int _value;

            public int Value
            {
                get => _value;
                set => Set(ref _value, value);
            }
        }

        private static List<string> TrackChanges(ViewModelBase viewModel)
        {
            var changes = new List<string>();
            viewModel.PropertyChanged += (sender, args) => changes.Add(args.PropertyName);
            return changes;
        }

        [Fact]
        public void ExpressionRaisesPropertyName()
        {
            var vm = new SampleViewModel();
            var changes = TrackChanges(vm);

            vm.Count = 5;

            Assert.Equal(new[] {nameof(SampleViewModel.Count)}, changes);
        }

        [Fact]
        public void ParameterlessRaiseUsesCallerName()
        {
            var vm = new SampleViewModel();
            var changes = TrackChanges(vm);

            vm.Name = "x";

            Assert.Equal(new[] {nameof(SampleViewModel.Name)}, changes);
        }

        [Fact]
        public void SetOnlyRaisesOnChange()
        {
            var vm = new SampleViewModel();
            var changes = TrackChanges(vm);

            vm.Value = 1;
            vm.Value = 1;

            Assert.Equal(new[] {nameof(SampleViewModel.Value)}, changes);
        }

        [Fact]
        public void GenericCommandConvertsXamlStringParameters()
        {
            var received = -1;
            ICommand command = new RelayCommand<int>(value => received = value);

            command.Execute("2");

            Assert.Equal(2, received);
        }

        [Fact]
        public void GenericCommandPassesDefaultForNullValueType()
        {
            var received = -1;
            ICommand command = new RelayCommand<int>(value => received = value);

            command.Execute(null);

            Assert.Equal(0, received);
        }

        [Fact]
        public void GenericCommandRespectsCanExecute()
        {
            var executed = false;
            var command = new RelayCommand<int>(value => executed = true, value => value > 5);

            Assert.False(((ICommand) command).CanExecute(1));
            command.Execute(1);
            Assert.False(executed);

            Assert.True(((ICommand) command).CanExecute(6));
            command.Execute(6);
            Assert.True(executed);
        }

        [Fact]
        public void ObjectCommandReceivesParameterAsIs()
        {
            object received = null;
            var parameter = new object();
            ICommand command = new RelayCommand<object>(value => received = value);

            command.Execute(parameter);

            Assert.Same(parameter, received);
        }

        [Fact]
        public void CommandRaisesCanExecuteChanged()
        {
            var raised = 0;
            var command = new RelayCommand(() => { });
            command.CanExecuteChanged += (sender, args) => raised++;

            command.NotifyCanExecuteChanged();
            command.RaiseCanExecuteChanged();

            Assert.Equal(2, raised);
        }
    }
}
