using KingsHandsProject.Infrastructure;
using KingsHandsProject.Models;
using KingsHandsProject.Services.Interfaces;
using KingsHandsProject.ViewModels;
using System.Collections.ObjectModel;
using System.IO;

namespace KingsHandsProject.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        private readonly IFolderDialogService _folderDialogService;
        private readonly IPokerLogScannerService _scannerService;
        private readonly IUiDispatcher _uiDispatcher;

        private readonly Dictionary<string, List<PokerHand>> _handsByTable = new();

        private string _selectedFolderPath = string.Empty;
        private string _statusText = "Выберите папку и нажмите «Начать сканирование».";
        private bool _isScanning;
        private string? _selectedTableName;
        private PokerHand? _selectedHand;
        private string _tablePlaceholderText = "Нет данных. Запустите сканирование.";
        private string _handPlaceholderText = "Выберите стол.";
        private string _detailsPlaceholderText = "Выберите раздачу.";

        private Thread? _scanThread;

        public MainWindowViewModel(
            IFolderDialogService folderDialogService,
            IPokerLogScannerService scannerService,
            IUiDispatcher uiDispatcher)
        {
            _folderDialogService = folderDialogService ?? throw new ArgumentNullException(nameof(folderDialogService));
            _scannerService = scannerService ?? throw new ArgumentNullException(nameof(scannerService));
            _uiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));

            TableNames = new ObservableCollection<string>();
            HandItems = new ObservableCollection<PokerHand>();

            BrowseFolderCommand = new RelayCommand(_ => BrowseFolder(), _ => !IsScanning);
            StartScanCommand = new RelayCommand(_ => StartScan(), _ => CanStartScan());
        }

        public ObservableCollection<string> TableNames { get; }

        public ObservableCollection<PokerHand> HandItems { get; }

        public RelayCommand BrowseFolderCommand { get; }

        public RelayCommand StartScanCommand { get; }

        public string SelectedFolderPath
        {
            get => _selectedFolderPath;
            set
            {
                if (SetProperty(ref _selectedFolderPath, value))
                {
                    StartScanCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        public bool IsScanning
        {
            get => _isScanning;
            private set
            {
                if (SetProperty(ref _isScanning, value))
                {
                    BrowseFolderCommand.RaiseCanExecuteChanged();
                    StartScanCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string? SelectedTableName
        {
            get => _selectedTableName;
            set
            {
                if (SetProperty(ref _selectedTableName, value))
                {
                    UpdateHandItems();
                    SelectedHand = null;
                }
            }
        }

        public PokerHand? SelectedHand
        {
            get => _selectedHand;
            set
            {
                if (SetProperty(ref _selectedHand, value))
                {
                    UpdateDetailsPlaceholder();
                }
            }
        }

        public string TablePlaceholderText
        {
            get => _tablePlaceholderText;
            private set => SetProperty(ref _tablePlaceholderText, value);
        }

        public string HandPlaceholderText
        {
            get => _handPlaceholderText;
            private set => SetProperty(ref _handPlaceholderText, value);
        }

        public string DetailsPlaceholderText
        {
            get => _detailsPlaceholderText;
            private set => SetProperty(ref _detailsPlaceholderText, value);
        }

        public bool HasTables => TableNames.Count > 0;

        public bool HasHandItems => HandItems.Count > 0;

        public bool HasSelectedHand => SelectedHand is not null;

        public string PlayersText =>
            SelectedHand is null
                ? string.Empty
                : (SelectedHand.Players.Count > 0
                    ? string.Join(", ", SelectedHand.Players)
                    : "Нет данных");

        public string WinnersText =>
            SelectedHand is null
                ? string.Empty
                : (SelectedHand.Winners.Count > 0
                    ? string.Join(", ", SelectedHand.Winners)
                    : "Нет данных");

        public string WinAmountText =>
            SelectedHand is null
                ? string.Empty
                : (!string.IsNullOrWhiteSpace(SelectedHand.WinAmount)
                    ? SelectedHand.WinAmount
                    : "Нет данных");

        private bool CanStartScan()
        {
            return !IsScanning && !string.IsNullOrWhiteSpace(SelectedFolderPath);
        }

        private void BrowseFolder()
        {
            string? selectedPath = _folderDialogService.SelectFolder();
            if (!string.IsNullOrWhiteSpace(selectedPath))
            {
                SelectedFolderPath = selectedPath;
            }
        }

        private void StartScan()
        {
            if (IsScanning)
                return;

            string folderPath = SelectedFolderPath?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                StatusText = "Укажите папку для сканирования.";
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                StatusText = $"Папка не найдена: {folderPath}";
                return;
            }

            ResetDataBeforeScan();

            IsScanning = true;
            StatusText = "Сканирование...";

            _scanThread = new Thread(() => ScanWorker(folderPath))
            {
                IsBackground = true,
                Name = "PokerLogScannerThread"
            };

            _scanThread.Start();
        }

        private void ScanWorker(string folderPath)
        {
            try
            {
                ScanResult result = _scannerService.Scan(
                    folderPath,
                    progress =>
                    {
                        _uiDispatcher.BeginInvoke(() =>
                        {
                            StatusText = progress.TotalFiles > 0
                                ? $"Сканирование... {progress.ProcessedFiles}/{progress.TotalFiles}"
                                : "Сканирование...";
                        });
                    });

                _uiDispatcher.BeginInvoke(() =>
                {
                    ApplyScanResult(result);
                });
            }
            catch (ThreadInterruptedException ex)
            {
                DebugLogger.LogException(ex);

                _uiDispatcher.BeginInvoke(() =>
                {
                    IsScanning = false;
                    StatusText = "Сканирование прервано.";
                });
            }
            catch (Exception ex)
            {
                DebugLogger.LogException(ex);

                _uiDispatcher.BeginInvoke(() =>
                {
                    IsScanning = false;
                    StatusText = $"Ошибка: {ex.Message}";
                    UpdatePlaceholdersAfterError();
                });
            }
        }

        private void ResetDataBeforeScan()
        {
            _handsByTable.Clear();

            TableNames.Clear();
            HandItems.Clear();

            SelectedTableName = null;
            SelectedHand = null;

            TablePlaceholderText = "Сканирование данных...";
            HandPlaceholderText = "Дождитесь завершения сканирования.";
            DetailsPlaceholderText = "Дождитесь завершения сканирования.";

            OnPropertyChanged(nameof(HasTables));
            OnPropertyChanged(nameof(HasHandItems));
            OnPropertyChanged(nameof(HasSelectedHand));
            OnPropertyChanged(nameof(PlayersText));
            OnPropertyChanged(nameof(WinnersText));
            OnPropertyChanged(nameof(WinAmountText));
        }

        private void ApplyScanResult(ScanResult result)
        {
            _handsByTable.Clear();
            TableNames.Clear();
            HandItems.Clear();

            foreach (PokerHand hand in result.Hands)
            {
                if (!_handsByTable.TryGetValue(hand.TableName, out List<PokerHand>? hands))
                {
                    hands = new List<PokerHand>();
                    _handsByTable[hand.TableName] = hands;
                }

                hands.Add(hand);
            }

            foreach (KeyValuePair<string, List<PokerHand>> pair in _handsByTable)
            {
                pair.Value.Sort((left, right) => left.HandId.CompareTo(right.HandId));
            }

            foreach (string tableName in _handsByTable.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                TableNames.Add(tableName);
            }

            IsScanning = false;

            if (result.Errors.Count > 0)
            {
                foreach (string error in result.Errors)
                {
                    DebugLogger.LogError(error);
                }
            }

            if (TableNames.Count == 0)
            {
                StatusText = result.SkippedFilesCount > 0
                    ? $"Готово (0 валидных записей, обработано файлов: {result.ProcessedFilesCount}, пропущено: {result.SkippedFilesCount})"
                    : $"Готово ({result.ProcessedFilesCount} файлов обработано, данные не найдены)";

                TablePlaceholderText = "Нет данных для отображения.";
                HandPlaceholderText = "Нет доступных раздач.";
                DetailsPlaceholderText = "Нет данных.";
            }
            else
            {
                StatusText = result.SkippedFilesCount > 0
                    ? $"Готово ({result.ProcessedFilesCount} файлов обработано, {result.SkippedFilesCount} пропущено)"
                    : $"Готово ({result.ProcessedFilesCount} файлов обработано)";

                TablePlaceholderText = string.Empty;
                HandPlaceholderText = "Выберите стол.";
                DetailsPlaceholderText = "Выберите раздачу.";

                SelectedTableName = TableNames[0];
            }

            OnPropertyChanged(nameof(HasTables));
            OnPropertyChanged(nameof(HasHandItems));
            OnPropertyChanged(nameof(HasSelectedHand));
        }

        private void UpdateHandItems()
        {
            HandItems.Clear();

            if (string.IsNullOrWhiteSpace(SelectedTableName))
            {
                HandPlaceholderText = HasTables ? "Выберите стол." : "Нет доступных раздач.";
                OnPropertyChanged(nameof(HasHandItems));
                return;
            }

            if (_handsByTable.TryGetValue(SelectedTableName, out List<PokerHand>? hands))
            {
                foreach (PokerHand hand in hands)
                {
                    HandItems.Add(hand);
                }
            }

            HandPlaceholderText = HandItems.Count == 0
                ? "Для выбранного стола раздачи не найдены."
                : string.Empty;

            OnPropertyChanged(nameof(HasHandItems));
        }

        private void UpdateDetailsPlaceholder()
        {
            DetailsPlaceholderText = SelectedHand is null
                ? "Выберите раздачу."
                : string.Empty;

            OnPropertyChanged(nameof(HasSelectedHand));
            OnPropertyChanged(nameof(PlayersText));
            OnPropertyChanged(nameof(WinnersText));
            OnPropertyChanged(nameof(WinAmountText));
        }

        private void UpdatePlaceholdersAfterError()
        {
            if (TableNames.Count == 0)
            {
                TablePlaceholderText = "Не удалось загрузить данные.";
                HandPlaceholderText = "Нет доступных раздач.";
                DetailsPlaceholderText = "Нет данных.";
            }

            OnPropertyChanged(nameof(HasTables));
            OnPropertyChanged(nameof(HasHandItems));
            OnPropertyChanged(nameof(HasSelectedHand));
            OnPropertyChanged(nameof(PlayersText));
            OnPropertyChanged(nameof(WinnersText));
            OnPropertyChanged(nameof(WinAmountText));
        }
    }
}