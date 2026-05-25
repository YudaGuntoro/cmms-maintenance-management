using MaintenanceApp.Mapping.Cmms;
using MaintenanceApp.Repositories;
using MaintenanceApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MaintenanceApp.ViewsModel
{
    public class CmmsViewModelBase : INotifyPropertyChanged
    {
        private bool _isBusy;
        private string? _errorMessage;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected static ICmmsRepository CreateRepository()
        {
            return new CmmsRepository(new ApiClient());
        }
    }

    public class DashboardMetric
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class CmmsDashboardViewModel : CmmsViewModelBase
    {
        private readonly ICmmsRepository _repository;
        private ReliabilityKpiDto? _kpi;

        public ObservableCollection<DashboardMetric> Metrics { get; } = new();
        public ObservableCollection<AssetMetricDto> TopDowntimeAssets { get; } = new();
        public ObservableCollection<AssetMetricDto> TopFailureAssets { get; } = new();

        public ReliabilityKpiDto? Kpi
        {
            get => _kpi;
            set => SetProperty(ref _kpi, value);
        }

        public CmmsDashboardViewModel() : this(CreateRepository())
        {
        }

        public CmmsDashboardViewModel(ICmmsRepository repository)
        {
            _repository = repository;
        }

        public async Task LoadAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = null;
                var summary = await _repository.GetDashboardSummaryAsync();
                Kpi = await _repository.GetReliabilityKpiAsync();

                Metrics.Clear();
                if (summary != null)
                {
                    Metrics.Add(new DashboardMetric { Title = "Total Assets", Value = summary.TotalAssets.ToString() });
                    Metrics.Add(new DashboardMetric { Title = "Open WO", Value = summary.OpenWorkOrders.ToString() });
                    Metrics.Add(new DashboardMetric { Title = "Overdue PM", Value = summary.OverduePreventiveMaintenance.ToString() });
                    Metrics.Add(new DashboardMetric { Title = "Low Stock", Value = summary.LowStockSpareparts.ToString() });
                }

                if (Kpi != null)
                {
                    Metrics.Add(new DashboardMetric { Title = "MTTR", Value = $"{Kpi.MttrMinutes:0.##} min" });
                    Metrics.Add(new DashboardMetric { Title = "MTBF", Value = $"{Kpi.MtbfMinutes:0.##} min" });
                }

                TopDowntimeAssets.Clear();
                TopFailureAssets.Clear();
                foreach (var item in summary?.TopAssetsByDowntime ?? new List<AssetMetricDto>())
                {
                    TopDowntimeAssets.Add(item);
                }

                foreach (var item in summary?.TopAssetsByFailureCount ?? new List<AssetMetricDto>())
                {
                    TopFailureAssets.Add(item);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public class AssetListViewModel : CmmsViewModelBase
    {
        private readonly ICmmsRepository _repository;
        private string? _searchText;
        private string _statusFilter = "ALL";

        public ObservableCollection<AssetDto> Assets { get; } = new();
        public List<string> StatusOptions { get; } = new() { "ALL", "ACTIVE", "INACTIVE", "UNDER_MAINTENANCE", "RETIRED" };

        public string? SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string StatusFilter
        {
            get => _statusFilter;
            set => SetProperty(ref _statusFilter, value);
        }

        public bool IsEmpty => !IsBusy && Assets.Count == 0;

        public AssetListViewModel() : this(CreateRepository())
        {
        }

        public AssetListViewModel(ICmmsRepository repository)
        {
            _repository = repository;
            Assets.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
        }

        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                var status = StatusFilter == "ALL" ? null : StatusFilter;
                var data = await _repository.GetAssetsAsync(SearchText, status);
                Assets.Clear();
                foreach (var asset in data)
                {
                    Assets.Add(asset);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(IsEmpty));
            }
        }
    }

    public class AssetDetailViewModel : CmmsViewModelBase
    {
        private readonly ICmmsRepository _repository;
        private AssetDto? _asset;
        private ReliabilityKpiDto? _kpi;

        public ObservableCollection<WorkOrderDto> WorkOrders { get; } = new();

        public AssetDto? Asset
        {
            get => _asset;
            set => SetProperty(ref _asset, value);
        }

        public ReliabilityKpiDto? Kpi
        {
            get => _kpi;
            set => SetProperty(ref _kpi, value);
        }

        public AssetDetailViewModel() : this(CreateRepository())
        {
        }

        public AssetDetailViewModel(ICmmsRepository repository)
        {
            _repository = repository;
        }

        public async Task LoadAsync(int assetId)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                Asset = await _repository.GetAssetAsync(assetId);
                Kpi = await _repository.GetReliabilityKpiAsync(assetId);
                var workOrders = await _repository.GetWorkOrdersAsync(assetId);
                WorkOrders.Clear();
                foreach (var workOrder in workOrders)
                {
                    WorkOrders.Add(workOrder);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public class WorkOrderListViewModel : CmmsViewModelBase
    {
        private readonly ICmmsRepository _repository;
        private string _statusFilter = "ALL";
        private string _priorityFilter = "ALL";
        private string _typeFilter = "ALL";

        public ObservableCollection<WorkOrderDto> WorkOrders { get; } = new();
        public List<string> StatusOptions { get; } = new() { "ALL", "DRAFT", "OPEN", "ASSIGNED", "IN_PROGRESS", "PENDING", "COMPLETED", "CLOSED", "CANCELLED" };
        public List<string> PriorityOptions { get; } = new() { "ALL", "LOW", "MEDIUM", "HIGH", "URGENT" };
        public List<string> TypeOptions { get; } = new() { "ALL", "BREAKDOWN", "CORRECTIVE", "PREVENTIVE", "PREDICTIVE", "INSPECTION", "CONTRACTOR_SUPERVISION" };

        public string StatusFilter
        {
            get => _statusFilter;
            set => SetProperty(ref _statusFilter, value);
        }

        public string PriorityFilter
        {
            get => _priorityFilter;
            set => SetProperty(ref _priorityFilter, value);
        }

        public string TypeFilter
        {
            get => _typeFilter;
            set => SetProperty(ref _typeFilter, value);
        }

        public bool IsEmpty => !IsBusy && WorkOrders.Count == 0;

        public WorkOrderListViewModel() : this(CreateRepository())
        {
        }

        public WorkOrderListViewModel(ICmmsRepository repository)
        {
            _repository = repository;
            WorkOrders.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
        }

        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                var data = await _repository.GetWorkOrdersAsync(
                    status: StatusFilter == "ALL" ? null : StatusFilter,
                    priority: PriorityFilter == "ALL" ? null : PriorityFilter,
                    maintenanceType: TypeFilter == "ALL" ? null : TypeFilter);

                WorkOrders.Clear();
                foreach (var workOrder in data)
                {
                    WorkOrders.Add(workOrder);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(IsEmpty));
            }
        }
    }

    public class WorkOrderDetailViewModel : CmmsViewModelBase
    {
        private readonly ICmmsRepository _repository;
        private WorkOrderDto? _workOrder;

        public WorkOrderDto? WorkOrder
        {
            get => _workOrder;
            set => SetProperty(ref _workOrder, value);
        }

        public WorkOrderDetailViewModel() : this(CreateRepository())
        {
        }

        public WorkOrderDetailViewModel(ICmmsRepository repository)
        {
            _repository = repository;
        }

        public async Task LoadAsync(int id)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                WorkOrder = await _repository.GetWorkOrderAsync(id);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task AssignAsync(int technicianId)
        {
            if (WorkOrder == null)
            {
                return;
            }

            WorkOrder = await _repository.AssignWorkOrderAsync(WorkOrder.Id, technicianId);
        }

        public async Task StartAsync()
        {
            if (WorkOrder == null)
            {
                return;
            }

            WorkOrder = await _repository.StartWorkOrderAsync(WorkOrder.Id);
        }

        public async Task CompleteAsync(string? failureCode, string? rootCause, string? actionTaken)
        {
            if (WorkOrder == null)
            {
                return;
            }

            WorkOrder = await _repository.CompleteWorkOrderAsync(WorkOrder.Id, new CompleteWorkOrderRequest
            {
                CompletedAt = DateTime.Now,
                FailureCode = failureCode,
                RootCause = rootCause,
                ActionTaken = actionTaken
            });
        }

        public async Task CloseAsync(string? failureCode, string? rootCause, string? actionTaken)
        {
            if (WorkOrder == null)
            {
                return;
            }

            WorkOrder = await _repository.CloseWorkOrderAsync(WorkOrder.Id, new CloseWorkOrderRequest
            {
                ClosedAt = DateTime.Now,
                FailureCode = failureCode,
                RootCause = rootCause,
                ActionTaken = actionTaken
            });
        }
    }

    public class CreateWorkOrderViewModel : CmmsViewModelBase
    {
        private readonly ICmmsRepository _repository;

        public ObservableCollection<AssetDto> Assets { get; } = new();
        public AssetDto? SelectedAsset { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = "CORRECTIVE";
        public string Priority { get; set; } = "MEDIUM";
        public DateTime ScheduledDate { get; set; } = DateTime.Today;
        public List<string> TypeOptions { get; } = new() { "BREAKDOWN", "CORRECTIVE", "PREVENTIVE", "PREDICTIVE", "INSPECTION", "CONTRACTOR_SUPERVISION" };
        public List<string> PriorityOptions { get; } = new() { "LOW", "MEDIUM", "HIGH", "URGENT" };

        public CreateWorkOrderViewModel() : this(CreateRepository())
        {
        }

        public CreateWorkOrderViewModel(ICmmsRepository repository)
        {
            _repository = repository;
        }

        public async Task LoadAssetsAsync()
        {
            var assets = await _repository.GetAssetsAsync();
            Assets.Clear();
            foreach (var asset in assets)
            {
                Assets.Add(asset);
            }
        }

        public async Task<WorkOrderDto?> SubmitAsync()
        {
            if (SelectedAsset == null)
            {
                throw new InvalidOperationException("Asset wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                throw new InvalidOperationException("Title wajib diisi.");
            }

            var request = new WorkOrderDto
            {
                AssetId = SelectedAsset.Id,
                Title = Title,
                Description = Description,
                MaintenanceType = MaintenanceType,
                Priority = Priority,
                Status = "OPEN",
                ReportedBy = "Android User",
                ReportedAt = DateTime.Now,
                ScheduledAt = ScheduledDate
            };

            return await _repository.CreateWorkOrderAsync(request);
        }
    }

    public class ContractorMonitoringViewModel : CmmsViewModelBase
    {
        private readonly ICmmsRepository _repository;
        private string? _vendorFilter;
        private string? _areaFilter;
        private string? _picMtcFilter;
        private string _statusFilter = "ALL";
        private string _riskFilter = "ALL";

        public ObservableCollection<ContractorWorkPlanDto> Plans { get; } = new();
        public ObservableCollection<ContractorWorkReminderDto> Reminders { get; } = new();
        public List<string> StatusOptions { get; } = new() { "ALL", "PLANNED", "WAITING_PERMIT_DOCUMENT", "READY_TO_START", "ONGOING", "FINISHED", "CANCELLED", "EXPIRED" };
        public List<string> RiskOptions { get; } = new() { "ALL", "high_risk", "working_at_height", "hot_work", "welding", "electrical_work", "confined_space", "heavy_equipment_activity", "chemical_handling", "shutdown_activity", "loto_required", "need_safety_standby" };

        public string? VendorFilter
        {
            get => _vendorFilter;
            set => SetProperty(ref _vendorFilter, value);
        }

        public string? AreaFilter
        {
            get => _areaFilter;
            set => SetProperty(ref _areaFilter, value);
        }

        public string? PicMtcFilter
        {
            get => _picMtcFilter;
            set => SetProperty(ref _picMtcFilter, value);
        }

        public string StatusFilter
        {
            get => _statusFilter;
            set => SetProperty(ref _statusFilter, value);
        }

        public string RiskFilter
        {
            get => _riskFilter;
            set => SetProperty(ref _riskFilter, value);
        }

        public int TotalPlans => Plans.Count;
        public int ReadyPlans => Plans.Count(x => x.Status == "READY_TO_START" || x.Status == "ONGOING");
        public int PermitAttention => Plans.Count(x => x.PermitDocumentStatus != "UPLOADED");
        public int HighRiskPlans => Plans.Count(x => x.HasHighRisk);
        public bool IsEmpty => !IsBusy && Plans.Count == 0;

        public ContractorMonitoringViewModel() : this(CreateRepository())
        {
        }

        public ContractorMonitoringViewModel(ICmmsRepository repository)
        {
            _repository = repository;
            Plans.CollectionChanged += (_, _) => RefreshCounters();
        }

        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                var plans = await _repository.GetContractorWorkPlansAsync(
                    VendorFilter,
                    AreaFilter,
                    StatusFilter == "ALL" ? null : StatusFilter,
                    RiskFilter == "ALL" ? null : RiskFilter,
                    PicMtcFilter);
                var reminders = await _repository.GetContractorWorkRemindersAsync();

                Plans.Clear();
                foreach (var plan in plans)
                {
                    Plans.Add(plan);
                }

                Reminders.Clear();
                foreach (var reminder in reminders.Take(5))
                {
                    Reminders.Add(reminder);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
                RefreshCounters();
            }
        }

        private void RefreshCounters()
        {
            OnPropertyChanged(nameof(TotalPlans));
            OnPropertyChanged(nameof(ReadyPlans));
            OnPropertyChanged(nameof(PermitAttention));
            OnPropertyChanged(nameof(HighRiskPlans));
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    public class SparepartListViewModel : CmmsViewModelBase
    {
        private readonly ICmmsRepository _repository;

        public ObservableCollection<SparepartDto> Spareparts { get; } = new();

        public SparepartListViewModel() : this(CreateRepository())
        {
        }

        public SparepartListViewModel(ICmmsRepository repository)
        {
            _repository = repository;
        }

        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                var data = await _repository.GetSparepartsAsync();
                Spareparts.Clear();
                foreach (var sparepart in data)
                {
                    Spareparts.Add(sparepart);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public class TechnicianListViewModel : CmmsViewModelBase
    {
        private readonly ICmmsRepository _repository;

        public ObservableCollection<TechnicianDto> Technicians { get; } = new();

        public TechnicianListViewModel() : this(CreateRepository())
        {
        }

        public TechnicianListViewModel(ICmmsRepository repository)
        {
            _repository = repository;
        }

        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                var data = await _repository.GetTechniciansAsync();
                Technicians.Clear();
                foreach (var technician in data)
                {
                    Technicians.Add(technician);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
