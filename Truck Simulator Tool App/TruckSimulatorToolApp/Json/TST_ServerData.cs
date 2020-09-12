using System.Drawing;

namespace Truck_Simulator_Tool_App
{
    class TST_ServerData
    {
            public string connectionStatusText { get; set; }
            public string connectionStatusBrush { get; set; }
            public string contractStatusText { get; set; }
            public string contractStatusBrush { get; set; }
            public string shiftStatusText { get; set; }
            public string shiftStatusBrush { get; set; }
            public string currentArrival_dtText { get; set; }
            public string currentArrival_tsText { get; set; }
            public string currentArrivalBrush { get; set; }
            public string currentBestArrival_dtText { get; set; }
            public string currentBestArrival_tsText { get; set; }
            public string bestArrival_dtText { get; set; }
            public string bestArrival_tsText { get; set; }
            public string nextPauseTimeText { get; set; }
            public string nextPauseTimeBrush { get; set; }
            public string remainingTimeText { get; set; }
            public string remainingTimeBrush { get; set; }
            public string jobInfo_FreightText { get; set; }
            public string jobInfo_MassText { get; set; }
            public string jobInfo_IncomeText { get; set; }
            public string sourceText { get; set; }
            public string destinationText { get; set; }
            public string progressBarPercentage { get; set; }
            public string timebufferText { get; set; }
            public string timebufferBrush { get; set; }
            public string remainingDistanceText { get; set; }
            public string timescaleText { get; set; }
            public float pb_distanceProgress { get; set; }
            public string pb_distanceText { get; set; }
            public float pb_damageProgress { get; set; }
            public string pb_damageText { get; set; }
            public bool hasShift { get; set; }
            public string nextShiftEvent { get; set; }
            public string nextShiftPause { get; set; }
            public string shiftTimeLeft { get; set; }
    }
}