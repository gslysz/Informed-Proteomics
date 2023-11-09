using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using InformedProteomics.Tests.Base;
using NUnit.Framework;
using PRISM;

namespace InformedProteomics.Tests.FunctionalTests
{
    [TestFixture]
    public class TestReadingAgilent
    {
        private readonly string _testRawFilePath = Path.Combine("T:\\Data", @"enolase-Chip-final.d");


        [Test]
        public void TestAgilentDatafileType()
        {
            string datafilePath = _testRawFilePath;
            var msFileType= MassSpecDataReaderFactory.GetMassSpecDataType(datafilePath);

            Assert.AreEqual(MassSpecDataType.AgilentD, msFileType);
        }


        [Test]
        public void TestAgilentReaderCreation()
        {
            string datafilePath = _testRawFilePath;
            using (var reader = MassSpecDataReaderFactory.GetMassSpecDataReader(datafilePath))
            {
                Assert.AreEqual(typeof(AgilentReader), reader.GetType());
            }
        }

        [Test]
        public void TestReadMsMsFromAgilent()
        {
            string datafilePath = _testRawFilePath;

            Spectrum ms;
            using (AgilentReader reader = new AgilentReader(datafilePath))
            {
                int scanNum = 1000;
                ms = reader.ReadMassSpectrum(scanNum);
            }

            Assert.IsNotNull(ms);
            Assert.AreEqual("scanId=215409",ms.NativeId);
            Assert.AreEqual(3.590m, (decimal)Math.Round (ms.ElutionTime, 3));

            Assert.AreEqual(2, ms.MsLevel);
            Assert.AreEqual(296, ms.Peaks.Length);
            Assert.AreEqual(20204, (int)ms.TotalIonCurrent);   
        }


        [Test]
        public void TestReadAllSpectraFromAgilent()
        {
            string datafilePath = _testRawFilePath;

            int scanNum;
            Stopwatch watch;
            List<Spectrum> msList;
            long elapsedMs;
            using (AgilentReader reader = new AgilentReader(datafilePath))
            {
                scanNum = 1000;

                watch = Stopwatch.StartNew();

                msList = reader.ReadAllSpectra().ToList();

                elapsedMs = watch.ElapsedMilliseconds;
            }
            
            var ms = msList.First(p => p.ScanNum == scanNum);

            Assert.IsNotNull(ms);
            Assert.AreEqual("scanId=215409",ms.NativeId);
            Assert.AreEqual(3.590m, (decimal)Math.Round (ms.ElutionTime, 3));

            Assert.AreEqual(2, ms.MsLevel);
            Assert.AreEqual(296, ms.Peaks.Length);
            Assert.AreEqual(20204, (int)ms.TotalIonCurrent);   

            Console.WriteLine($"Read {msList.Count} in {elapsedMs} ms.");
        }

        [Test]
        public async Task CreatePbfFromAgilentTest1()
        {
            string datasetFilePath = _testRawFilePath;

            string pbfFilePath = datasetFilePath.Replace(".d", ".pbf");

            if (File.Exists(pbfFilePath))
                File.Delete(pbfFilePath);

            var reader = MassSpecDataReaderFactory.GetMassSpecDataReader(datasetFilePath);
            var progress = new Progress<ProgressData>(p =>
            {
            });

            progress.ProgressChanged += OnProgressChanged;

            const double precursorSignalToNoiseRatioThreshold = 0.0;
            const double productSignalToNoiseRatioThreshold = 0.0;
            const bool keepDataReaderOpen = false;

            var startScan = 950;
            var stopScan = 1050;

            await Task.Run(() =>
            {
                var run = new PbfLcMsRun(
                    datasetFilePath, reader, pbfFilePath,
                    precursorSignalToNoiseRatioThreshold, productSignalToNoiseRatioThreshold,
                    progress, keepDataReaderOpen, startScan, stopScan);
            });

            Assert.IsTrue(File.Exists(pbfFilePath));

        }

        private void OnProgressChanged(object sender, ProgressData e)
        {
            Console.WriteLine($"Progress:  {e.Percent} - {e.Status}");
        }
    }
}
