﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "RCS1077:Optimize LINQ method call.", Justification = "Leave as-is for clarity", Scope = "member", Target = "~P:InformedProteomics.FeatureFinding.IsotopicEnvelope.ObservedIsotopeEnvelope.MinMzPeak")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.LcMsPeakMatrix.CorrectChargeState(InformedProteomics.FeatureFinding.IsotopicEnvelope.ObservedIsotopeEnvelope,InformedProteomics.Backend.Data.Spectrometry.Spectrum)~System.Boolean")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.LcMsPeakMatrix.GetDetectableMinMaxCharge(System.Double,System.Double,System.Double)~System.Tuple{System.Int32,System.Int32}")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.LcMsPeakMatrix.GetMs1EvidenceScore(System.Int32,System.Double,System.Int32)~System.Double")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.LcMsFeatureFinderLauncher.ProcessDirectory(System.String)~System.Int32")]
[assembly: SuppressMessage("Roslynator", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.Data.Ms1Spectrum.GetLocalMzWindow(System.Double)~InformedProteomics.FeatureFinding.Data.LocalMzWindow")]
[assembly: SuppressMessage("Roslynator", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.Data.Ms1Spectrum.PreArrangeLocalMzWindows")]
[assembly: SuppressMessage("Roslynator", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.MaxEntDeconvoluter.CollectMultiplyChargedPeaks(System.Double,InformedProteomics.Backend.Data.Spectrometry.Spectrum)~System.Collections.Generic.List{InformedProteomics.Backend.Data.Spectrometry.DeconvolutedPeak}")]
[assembly: SuppressMessage("Roslynator", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.MaxEntDeconvoluter.GetWeightingFactor(System.Collections.Generic.IList{InformedProteomics.Backend.Data.Spectrometry.DeconvolutedPeak})~System.Double[]")]
[assembly: SuppressMessage("Roslynator", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.Graphics.LcMsFeatureMap.#ctor(System.Collections.Generic.IList{InformedProteomics.FeatureFinding.Data.LcMsFeature},System.String,System.Double,System.Double,System.Double,System.Double)")]
[assembly: SuppressMessage("Roslynator", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.Graphics.LcMsFeatureMap.GetColorBreaksTable(System.Int32,System.Collections.Generic.IList{InformedProteomics.FeatureFinding.Data.LcMsFeature})~System.Double[]")]
[assembly: SuppressMessage("Roslynator", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.Scoring.LcMsPeakScorer.CheckChargeState(InformedProteomics.FeatureFinding.IsotopicEnvelope.ObservedIsotopeEnvelope)~System.Boolean")]
[assembly: SuppressMessage("Usage", "RCS1146:Use conditional access.", Justification = "Leave as-is for clarity", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.Data.LocalMzWindow.GetRankSumTestPValue(InformedProteomics.FeatureFinding.Data.Ms1Peak[],System.Int32)~System.Double")]
[assembly: SuppressMessage("Usage", "RCS1146:Use conditional access.", Justification = "Leave as-is for clarity", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.LcMsFeatureFinderLauncher.FilterFeaturesWithOutput(InformedProteomics.FeatureFinding.FeatureDetection.LcMsFeatureContainer,InformedProteomics.FeatureFinding.FeatureDetection.LcMsPeakMatrix,System.String,System.Collections.Generic.IList{System.Int32})~System.Collections.Generic.IEnumerable{InformedProteomics.Backend.FeatureFindingResults.Ms1FtEntry}")]
[assembly: SuppressMessage("Usage", "RCS1146:Use conditional access.", Justification = "Leave as-is for clarity", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.Scoring.LcMsPeakScorer.PreformStatisticalSignificanceTest(InformedProteomics.FeatureFinding.IsotopicEnvelope.ObservedIsotopeEnvelope)~InformedProteomics.FeatureFinding.Scoring.IsotopeEnvelopeStatisticalInfo")]
[assembly: SuppressMessage("Usage", "RCS1246:Use element access.", Justification = "Prefer to use .First()", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.LcMsPeakMatrix.BuildFeatureMatrix(System.Double)")]
[assembly: SuppressMessage("Usage", "RCS1246:Use element access.", Justification = "Prefer to use .First()", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.LcMsPeakMatrix.DetermineFeatureRange(System.Double,System.Int32,System.Int32,System.Int32)~System.Tuple{System.Int32,System.Int32,System.Int32,System.Int32}")]
[assembly: SuppressMessage("Usage", "RCS1246:Use element access.", Justification = "Prefer to use .First()", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.LcMsPeakMatrix.GetElutionWindow(System.Int32,System.Double,System.Int32)~System.Tuple{System.Int32,System.Int32}")]
[assembly: SuppressMessage("Usage", "RCS1246:Use element access.", Justification = "Prefer to use .First()", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.LcMsPeakMatrix.GetLcMs1PeakClusters(System.Int32)~System.Collections.Generic.IList{InformedProteomics.FeatureFinding.Clustering.LcMsPeakCluster}")]
[assembly: SuppressMessage("Usage", "RCS1246:Use element access.", Justification = "Prefer to use .First()", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.LcMsPeakMatrix.GetLcMsPeakCluster(System.Double,System.Int32,System.Int32,System.Int32,System.Boolean)~InformedProteomics.FeatureFinding.Clustering.LcMsPeakCluster")]
[assembly: SuppressMessage("Usage", "RCS1246:Use element access.", Justification = "Prefer to use .First()", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.LcMsPeakMatrix.ShiftColumnByElutionTime(System.Int32,System.Double)~System.Int32")]
[assembly: SuppressMessage("Usage", "RCS1246:Use element access.", Justification = "Prefer to use .First()", Scope = "member", Target = "~M:InformedProteomics.FeatureFinding.FeatureDetection.MaxEntDeconvoluter.CollectMultiplyChargedPeaks(System.Double,InformedProteomics.Backend.Data.Spectrometry.Spectrum)~System.Collections.Generic.List{InformedProteomics.Backend.Data.Spectrometry.DeconvolutedPeak}")]
