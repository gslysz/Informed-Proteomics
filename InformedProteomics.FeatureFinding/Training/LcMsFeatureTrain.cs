﻿using System;
using System.Collections.Generic;
using System.Linq;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Sequence;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using InformedProteomics.FeatureFinding.SpectrumMatching;
using InformedProteomics.FeatureFinding.Util;

namespace InformedProteomics.FeatureFinding.Training
{
    public static class LcMsFeatureTrain
    {
        internal class PrsmComparer : INodeComparer<ProteinSpectrumMatch>
        {
            public PrsmComparer(LcMsRun run)
            {
                _run = run;
                _elutionLength = _run.GetElutionTime(_run.MaxLcScan);
            }

            private readonly LcMsRun _run;
            private readonly double _elutionLength;

            public bool SameCluster(ProteinSpectrumMatch prsm1, ProteinSpectrumMatch prsm2)
            {
                var tol = new Tolerance(10);
                //if (!prsm1.ProteinName.Equals(prsm2.ProteinName)) return false;
                var massDiff = Math.Abs(prsm1.Mass - prsm2.Mass);
                if (massDiff > tol.GetToleranceAsMz(prsm1.Mass))
                {
                    return false;
                }

                var elutionDiff = Math.Abs(_run.GetElutionTime(prsm1.ScanNum) - _run.GetElutionTime(prsm2.ScanNum));
                if (prsm1.SequenceText.Equals(prsm2.SequenceText))
                {
                    if (elutionDiff > _elutionLength * 0.02)
                    {
                        return false;
                    }
                }
                else
                {
                    if (elutionDiff > _elutionLength * 0.005)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public static ICollection<ProteinSpectrumMatchSet> CollectTrainSet(string pbfFilePath, string idFilePath)
        {
            Modification.RegisterAndGetModification(Modification.Cysteinyl.Name, Modification.Cysteinyl.Composition);

            const double fdrCutoff = 0.01;
            var prsmReader = new ProteinSpectrumMatchReader(fdrCutoff);

            var prsmList = prsmReader.LoadIdentificationResult(idFilePath);
            var run = PbfLcMsRun.GetLcMsRun(pbfFilePath);

            var groupedPrsmList = GroupingByPrsm(0, prsmList, new PrsmComparer(run));

            var finalPrsmGroups = new List<ProteinSpectrumMatchSet>();

            foreach (var prsmSet in groupedPrsmList)
            {
                if (prsmSet.Count < 2)
                {
                    continue;
                }

                var isGood = false;
                var sequence = prsmSet[0].GetSequence();
                if (sequence == null)
                {
                    continue;
                }

                foreach (var scan in prsmSet.Select(prsm => prsm.ScanNum))
                {
                    if (run.GetSpectrum(scan) is not ProductSpectrum spectrum)
                    {
                        continue;
                    }

                    if (IsGoodTarget(spectrum, sequence))
                    {
                        isGood = true;
                        break;
                    }
                }

                if (isGood)
                {
                    finalPrsmGroups.Add(prsmSet);
                }
            }
            return finalPrsmGroups;
        }

        public static List<ProteinSpectrumMatchSet> GroupingByPrsm(int dataId, IEnumerable<ProteinSpectrumMatch> matches, INodeComparer<ProteinSpectrumMatch> prsmComparer)
        {
            var prsmSet = new NodeSet<ProteinSpectrumMatch>();
            prsmSet.AddRange(matches);
            var groupList = prsmSet.ConnectedComponents(prsmComparer);
            return groupList.ConvertAll(group => new ProteinSpectrumMatchSet(dataId, group));
        }

        private static bool IsGoodTarget(ProductSpectrum ms2Spec, Sequence sequence)
        {
            var BaseIonTypesCID = new[] { BaseIonType.B, BaseIonType.Y };
            var BaseIonTypesETD = new[] { BaseIonType.C, BaseIonType.Z };
            var tolerance = new Tolerance(10);

            const int minCharge = 1;
            const int maxCharge = 20;

            var baseIonTypes = ms2Spec.ActivationMethod != ActivationMethod.ETD ? BaseIonTypesCID : BaseIonTypesETD;
            var cleavages = sequence.GetInternalCleavages();
            const double relativeIsotopeIntensityThreshold = 0.7d;

            // Unused: var nTheoreticalIonPeaks = 0;
            var nObservedIonPeaks = 0;
            var nObservedPrefixIonPeaks = 0;
            var nObservedSuffixIonPeaks = 0;

            foreach (var c in cleavages)
            {
                foreach (var baseIonType in baseIonTypes)
                {
                    var fragmentComposition = baseIonType.IsPrefix
                                  ? c.PrefixComposition + baseIonType.OffsetComposition
                                  : c.SuffixComposition + baseIonType.OffsetComposition;

                    for (var charge = minCharge; charge <= maxCharge; charge++)
                    {
                        var ion = new Ion(fragmentComposition, charge);
                        if (ms2Spec.ContainsIon(ion, tolerance, relativeIsotopeIntensityThreshold))
                        {
                            if (baseIonType.IsPrefix)
                            {
                                nObservedPrefixIonPeaks++;
                            }
                            else
                            {
                                nObservedSuffixIonPeaks++;
                            }

                            nObservedIonPeaks++;
                        }
                        // nTheoreticalIonPeaks++;
                    }
                }
            }

            if (sequence.Composition.Mass > 3000)
            {
                if ((double)nObservedPrefixIonPeaks / nObservedIonPeaks > 0.85 ||
                    (double)nObservedSuffixIonPeaks / nObservedIonPeaks > 0.85)
                {
                    return false;
                }

                if (nObservedPrefixIonPeaks < 3 || nObservedSuffixIonPeaks < 3)
                {
                    return false;
                }
            }
            else
            {
                if (nObservedPrefixIonPeaks < 1 || nObservedSuffixIonPeaks < 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
