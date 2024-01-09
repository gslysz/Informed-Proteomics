using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Agilent.MassSpectrometry.DataAnalysis;
using Agilent.MassSpectrometry.MIDAC;
using InformedProteomics.Backend.Data.Spectrometry;
using PSI_Interface.CV;
using Spectrum = InformedProteomics.Backend.Data.Spectrometry.Spectrum;

namespace InformedProteomics.Backend.MassSpecData
{
    public class AgilentReader : IMassSpecDataReader
    {
        #region Constructors

        public AgilentReader(string dataFilePath)
        {
            var dataFile = new DirectoryInfo(dataFilePath);
            if (!dataFile.Exists) throw new DirectoryNotFoundException($"Agilent .D folder not found: {dataFilePath}");

            _dataAccess = new DataAccess();
            _dataAccess.OpenDataFile(dataFilePath);

            // create lookup table for BDA scan info, keyed by 'ScanId'
            _scanRecords = _dataAccess.GetScanRecordsInfo(MSScanType.All).Values.ToDictionary(p => p.ScanId, p => p);

            int scanNumCounter = 1;
            foreach (var kvp in _scanRecords)
            {
                var scanInfo = kvp.Value;
                var scanNum = scanNumCounter++;  // Scannum is 1-based in PBF files. 
                var msLevel = scanInfo.MsLevel == MSLevel.MS ? 1 : 2;
                var totalIonCurrent = scanInfo.TIC;
                var isolationWidth = scanInfo.MzIsolationWidth;

                var infoItem = new ScanInfoAdapter(scanNum, scanInfo.ScanId, scanInfo.ScanTime, msLevel, totalIonCurrent)
                {
                    IsolationWidth = isolationWidth,
                    PrecursorMz = scanInfo.MzOfInterest
                };

                ScanInfo.Add(scanNum, infoItem);
            }
            
            NumSpectra = _scanRecords.Count;

            FileFormatVersion = GetFileFormatVersion();

            FilePath = dataFilePath;

            _agilentMsPeakFinder = new FindPeaksMS();
        }

        #endregion

        #region Properties

        public Dictionary<int, ScanInfoAdapter> ScanInfo { get; } = new();

        

        #endregion

        #region Public Methods

        public static List<Peak> ConvertMassSpectrumToSpectralPeaks(IMassSpectrum massSpectrum)
        {
            var xvals = ((IFXArrayStore)massSpectrum.XYStore).GetXArray();
            var yvals = ((IFXArrayStore)massSpectrum.XYStore).GetYArray();
            return xvals.Select((t, i) => new Peak(t, yvals[i])).ToList();
        }

        #endregion


        public Spectrum GetSpectrum(int scanNum, bool includePeaks = true)
        {
            return ReadMassSpectrum(scanNum, includePeaks);
        }

        public void Dispose()
        {
            _dataAccess.CloseDataFile();
        }

        public IEnumerable<Spectrum> ReadAllSpectra(bool includePeaks = true)
        {
            List<Spectrum> msList = new List<Spectrum>();

            foreach (var kvp in ScanInfo)
            {
                var scanNum = kvp.Key;

                var ms = ReadMassSpectrum(scanNum);
                msList.Add(ms);
            }

            return msList;
        }

        public Spectrum ReadMassSpectrum(int scanNum, bool includePeaks = true)
        {
            if (!ScanInfo.ContainsKey(scanNum))
                return null;

            var scanId = ScanInfo[scanNum].ScanId;
            var rt = ScanInfo[scanNum].Rt;
            var msLevel = ScanInfo[scanNum].MsLevel;
            var tic = ScanInfo[scanNum].Tic;

            IPSetExtractSpectrum pset = new PSetExtractSpectrum();
            pset.ScanIds = new List<int> { scanId };
            pset.DesiredMSStorage = DesiredMSStorageType.PeakElseProfile;

            
            var massSpectrum = _dataAccess.ReadSpectrum(pset).FirstOrDefault() as IMassSpectrum;

            var storageMode = massSpectrum.Description.MSDetails.MSStorageMode;

            List<Peak> peaks;
            if (storageMode == MSStorageMode.ProfileSpectrum && msLevel == 1)
            {
                var findPeaksMsParameters = new FindPeaksMSParameters();
                findPeaksMsParameters.PeakFilterParameters = new PSetMSPeakFilter();

                findPeaksMsParameters.TOFParameters = new PSetTofPeakFinder();
                findPeaksMsParameters.TOFParameters.DoRestrictXRange = true;
                findPeaksMsParameters.TOFParameters.RestrictedXRange = new MinMaxRange(200, 2000);

                _agilentMsPeakFinder.FindPeaks(massSpectrum, findPeaksMsParameters, out var peakList);
                peaks = peakList.Peaks.Select(p => new Peak(p.CenterX, p.Height)).ToList();

            }
            else
            {
                peaks = ConvertMassSpectrumToSpectralPeaks(massSpectrum);
            }

            var nativeIdString = $"scanId={scanId}";

            if (msLevel == 1)
                return new Spectrum(peaks, scanNum)
                {
                    MsLevel = 1,
                    ElutionTime = rt,
                    NativeId = nativeIdString,
                    TotalIonCurrent = tic
                };

            IsolationWindow isolationWindow = GetIsolationWindow(scanNum);

            var msms = new ProductSpectrum(peaks, scanNum)
            {
                MsLevel = msLevel,
                ElutionTime = rt,
                NativeId = nativeIdString,
                TotalIonCurrent = tic,
                IsolationWindow = isolationWindow
            };

            return msms;
        }

        public int NumSpectra { get; }

        public void Close()
        {
            _dataAccess.CloseDataFile();
        }

        public bool TryMakeRandomAccessCapable()
        {
            return true;
        }

        public CV.CVID NativeIdFormat => CV.CVID.MS_Agilent_MassHunter_nativeID_format;
        public CV.CVID NativeFormat => CV.CVID.MS_Agilent_MassHunter_format;
        public string FilePath { get; }

        public string SrcFileChecksum
        {
            get
            {
                if (string.IsNullOrEmpty(_srcFileChecksum)) _srcFileChecksum = ComputeChecksum();

                return _srcFileChecksum;
            }
        }

        public string FileFormatVersion { get; }

        #region Private Methods

        private string ComputeChecksum()
        {
            // TODO: FINISH THI
            return "";
        }

        private string GetFileFormatVersion()
        {
            // TODO: FINISH THIS
            return string.Empty;
        }

        private IsolationWindow GetIsolationWindow(int scanNum)
        {
            if (!ScanInfo.ContainsKey(scanNum))
            {
                //TODO: FINISH THIS.  Should we throw an error?
                return null;
            }

            var scanInfo = ScanInfo[scanNum];
            var bdaScanInfo = _scanRecords[scanInfo.ScanId];
            var precursorMz = bdaScanInfo.MzOfInterest;
            var z = bdaScanInfo.ChargeState;
            var windowWidth = bdaScanInfo.MzIsolationWidth;

            var lowerOffset = windowWidth / 2;
            var upperOffset = windowWidth / 2;

            return new IsolationWindow(precursorMz, lowerOffset, upperOffset, charge: z);
        }

        #endregion

        private readonly IDataAccess _dataAccess;
        
        /// <summary>
        /// Contains the BaseDataAccess scan info; key = scanID  (not scanNum)
        /// </summary>
        private readonly Dictionary<int,IBdaMsScanRecInfo> _scanRecords;
        
        
        private string _srcFileChecksum;
        private readonly IFindPeaks _agilentMsPeakFinder;
    }

    public class ScanInfoAdapter
    {
        #region Constructors

        public ScanInfoAdapter(int scanNum, int scanId, double rt, int msLevel, double tic)
        {
            ScanNum = scanNum;
            ScanId = scanId;
            Rt = rt;
            MsLevel = msLevel;
            Tic = tic;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Simple scan number
        /// </summary>
        public int ScanNum { get; }

        /// <summary>
        ///     Native ID for scan
        /// </summary>
        public int ScanId { get; }

        /// <summary>
        ///     Retention time
        /// </summary>
        public double Rt { get; }

        public int MsLevel { get; }

        /// <summary>
        ///     Total ion current for this scan
        /// </summary>
        public double Tic { get; }

        public float IsolationWidth { get; set; }
        public double PrecursorMz { get; set; }

        #endregion
    }
}