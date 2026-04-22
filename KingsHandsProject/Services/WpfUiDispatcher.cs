using KingsHandsProject.Services.Interfaces;
using System.Windows;

namespace KingsHandsProject.Services
{
    public sealed class WpfUiDispatcher : IUiDispatcher
    {
        public void Invoke(Action action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            System.Windows.Application.Current.Dispatcher.Invoke(action);
        }

        public void BeginInvoke(Action action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
        }
    }
}