using KingsHandsProject.Services.Interfaces;

namespace KingsHandsProject.Services
{
    public sealed class FolderDialogService : IFolderDialogService
    {
        public string? SelectFolder()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папка с логами",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };
            DialogResult result = dialog.ShowDialog();
            return result == DialogResult.OK ? dialog.SelectedPath : null;
        }
    }
}