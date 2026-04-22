namespace KingsHandsProject.Services.Interfaces
{
    public interface IUiDispatcher
    {
        void Invoke(Action action);

        void BeginInvoke(Action action);
    }
}