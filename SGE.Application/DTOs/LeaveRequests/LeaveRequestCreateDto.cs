using SGE.Core.Enums;

namespace SGE.Application.DTOs.LeaveRequests;

public class LeaveRequestCreateDto
{
    private readonly int _leaveType;
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public int LeaveType
    {
        get => _leaveType;
        init
        {
            if (!Enum.IsDefined(typeof(LeaveType), value))
                throw new ArgumentOutOfRangeException(nameof(value), "LeaveType value must be a valid LeaveType enum value.");
            _leaveType = value;
        }
    }

    public DateTime StartDate
    {
        get => _startDate;
        init
        {
            if (value < DateTime.UtcNow.Date)
                throw new ArgumentException("StartDate cannot be in the past.", nameof(value));
            _startDate = value;
        }
    }

    public DateTime EndDate
    {
        get => _endDate;
        init
        {
            if (value < _startDate)
                throw new ArgumentException("EndDate must be after StartDate.", nameof(value));
            _endDate = value;
        }
    }

    public string Reason { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
}