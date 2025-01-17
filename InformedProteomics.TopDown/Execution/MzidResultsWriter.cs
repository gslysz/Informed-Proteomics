﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Enum;
using InformedProteomics.Backend.Data.Sequence;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.Database;
using InformedProteomics.Backend.MassSpecData;
using InformedProteomics.Backend.SearchResults;
using PRISM;
using PSI_Interface.CV;
using PSI_Interface.IdentData;
using PSI_Interface.IdentData.IdentDataObjs;
using PSI_Interface.IdentData.mzIdentML;

namespace InformedProteomics.TopDown.Execution
{
    /// <summary>
    /// Creation and writing of an mzid file.
    /// </summary>
    public class MzidResultsWriter
    {
        // Ignore Spelling: Cvid, mzid, mzIdentML

        private readonly FastaDatabase database;
        private readonly LcMsRun lcmsRun;
        private readonly MsPfParameters options;

        public MzidResultsWriter(FastaDatabase db, LcMsRun run, MsPfParameters options)
        {
            this.database = db;
            this.lcmsRun = run;
            this.options = options;
        }

        public void WriteResultsToMzid(IEnumerable<DatabaseSearchResultData> matches, string outputFilePath)
        {
            var datasetName = Path.GetFileNameWithoutExtension(outputFilePath);
            var creator = new IdentDataCreator("MSPathFinder_" + datasetName, "MSPathFinder_" + datasetName);
            var soft = creator.AddAnalysisSoftware("Software_1", "MSPathFinder", System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString(), CV.CVID.MS_MSPathFinder, "MSPathFinder");
            var settings = creator.AddAnalysisSettings(soft, "Settings_1", CV.CVID.MS_ms_ms_search);
            var searchDb = creator.AddSearchDatabase(database.GetFastaFilePath(), database.GetNumEntries(), Path.GetFileNameWithoutExtension(database.GetFastaFilePath()), CV.CVID.CVID_Unknown,
                CV.CVID.MS_FASTA_format);

            if (options.TargetDecoySearchMode.HasFlag(DatabaseSearchMode.Decoy))
            {
                searchDb.CVParams.AddRange(new CVParamObj[]
                {
                    new CVParamObj() { Cvid = CV.CVID.MS_DB_composition_target_decoy, },
                    new CVParamObj() { Cvid = CV.CVID.MS_decoy_DB_accession_regexp, Value = "^XXX", },
                    //new CVParamObj() { Cvid = CV.CVID.MS_decoy_DB_type_reverse, },
                    new CVParamObj() { Cvid = CV.CVID.MS_decoy_DB_type_randomized, },
                });
            }

            // store the settings...
            CreateMzidSettings(settings);

            var path = options.SpecFilePath;
            if (lcmsRun is PbfLcMsRun run)
            {
                var rawPath = run.RawFilePath;
                if (!string.IsNullOrWhiteSpace(rawPath))
                {
                    path = rawPath;
                }
            }
            // TODO: fix this to match correctly to the original file - May need to modify the PBF format to add an input format specifier
            // TODO: Should probably? request a CV Term for the PBF format?
            var nativeIdFormat = lcmsRun.NativeIdFormat;
            if (nativeIdFormat == CV.CVID.CVID_Unknown)
            {
                nativeIdFormat = CV.CVID.MS_scan_number_only_nativeID_format;
            }
            var specData = creator.AddSpectraData(path, datasetName, nativeIdFormat, lcmsRun.NativeFormat);

            // Get the search modifications as they were passed into the AminoAcidSet constructor, so we can retrieve masses from them
            var modDict = new Dictionary<string, Modification>();
            foreach (var mod in options.AminoAcidSet.SearchModifications)
            {
                if (!modDict.ContainsKey(mod.Modification.Name))
                {
                    modDict.Add(mod.Modification.Name, mod.Modification);
                }
                else if (!modDict[mod.Modification.Name].Composition.Equals(mod.Modification.Composition))
                {
                    throw new System.Exception(
                        "ERROR: Cannot have modifications with the same name and different composition/mass! Fix input modifications! Duplicated modification name: " +
                        mod.Modification.Name);
                }
            }

            var missingKeys = new SortedSet<string>();

            var scanOrder = new List<int>();
            var matchesByScan = new Dictionary<int, List<DatabaseSearchResultData>>();

            foreach (var match in matches)
            {
                if (matchesByScan.TryGetValue(match.ScanNum, out var scanMatches))
                {
                    scanMatches.Add(match);
                    continue;
                }

                matchesByScan.Add(match.ScanNum, new List<DatabaseSearchResultData> { match });
                scanOrder.Add(match.ScanNum);
            }

            foreach (var scanMatches in scanOrder.Select(scanNumber => matchesByScan[scanNumber]))
            {
                for (var rank = 1; rank <= scanMatches.Count; rank++)
                {
                    var match = scanMatches[rank - 1];

                    var scanNum = match.ScanNum;
                    var spec = lcmsRun.GetSpectrum(scanNum, false);
                    var matchIon = new Ion(Composition.Parse(match.Composition), match.Charge);

                    var nativeId = spec.NativeId;
                    if (string.IsNullOrWhiteSpace(spec.NativeId))
                    {
                        nativeId = "scan=" + spec.ScanNum;
                    }

                    var specIdent = creator.AddSpectrumIdentification(specData, nativeId, spec.ElutionTime, match.MostAbundantIsotopeMz,
                        match.Charge, rank, double.NaN);

                    specIdent.CalculatedMassToCharge = matchIon.GetMonoIsotopicMz();
                    var pep = new PeptideObj(match.Sequence);

                    var modText = match.Modifications;
                    if (!string.IsNullOrWhiteSpace(modText))
                    {
                        try
                        {
                            var mods = modText.Split(',');
                            foreach (var mod in mods)
                            {
                                var tokens = mod.Split(' ');

                                if (!modDict.TryGetValue(tokens[0], out var modInfo))
                                {
                                    if (!missingKeys.Contains(tokens[0]))
                                    {
                                        ConsoleMsgUtils.ShowWarning("Modification dictionary does not have key {0}", tokens[0]);
                                        Console.WriteLine("Keys in the dictionary:");
                                        foreach (var key in modDict.Keys)
                                        {
                                            Console.WriteLine(key);
                                        }

                                        ConsoleMsgUtils.ShowWarning("Results in the .mzid file will be inaccurate");

                                        missingKeys.Add(tokens[0]);
                                    }
                                }
                                else
                                {
                                    var modObj = new ModificationObj(CV.CVID.MS_unknown_modification, modInfo.Name, int.Parse(tokens[1]),
                                        modInfo.Mass);
                                    pep.Modifications.Add(modObj);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ConsoleMsgUtils.ShowError("Error parsing mods while writing .mzid", ex);
                        }
                    }

                    specIdent.Peptide = pep;

                    var proteinName = match.ProteinName;
                    var protLength = match.ProteinLength;
                    var proteinDescription = match.ProteinDescription;
                    var dbSeq = new DbSequenceObj(searchDb, protLength, proteinName, proteinDescription);

                    var start = match.Start;
                    var end = match.End;
                    var pepEv = new PeptideEvidenceObj(dbSeq, pep, start, end, match.Pre, match.Post, match.ProteinName.StartsWith("XXX"));
                    specIdent.AddPeptideEvidence(pepEv);

                    var probability = match.Probability;

                    specIdent.CVParams.Add(new CVParamObj() { Cvid = CV.CVID.MS_chemical_formula, Value = match.Composition, });
                    //specIdent.CVParams.Add(new CVParamObj() { Cvid = CV.CVID.MS_number_of_matched_peaks, Value = match.NumMatchedFragments.ToString(), });
                    specIdent.CVParams.Add(new CVParamObj()
                    { Cvid = CV.CVID.MS_MSPathFinder_RawScore, Value = probability.ToString(CultureInfo.InvariantCulture), });
                    specIdent.CVParams.Add(new CVParamObj()
                    { Cvid = CV.CVID.MS_MSPathFinder_SpecEValue, Value = match.SpecEValue.ToString(CultureInfo.InvariantCulture), });
                    specIdent.CVParams.Add(new CVParamObj()
                    { Cvid = CV.CVID.MS_MSPathFinder_EValue, Value = match.EValue.ToString(CultureInfo.InvariantCulture), });
                    if (match.HasTdaScores)
                    {
                        specIdent.CVParams.Add(new CVParamObj()
                        { Cvid = CV.CVID.MS_MSPathFinder_QValue, Value = match.QValue.ToString(CultureInfo.InvariantCulture), });
                        specIdent.CVParams.Add(new CVParamObj()
                        { Cvid = CV.CVID.MS_MSPathFinder_PepQValue, Value = match.PepQValue.ToString(CultureInfo.InvariantCulture), });
                    }
                    // MS-GF+ similarity: find/add isotope error?
                    // MS-GF+ similarity: find/add assumed dissociation method?
                    //specIdent.UserParams.Add(new UserParamObj() {Name = "Assumed Dissociation Method", Value = match.});

                }
            }

            var identData = creator.GetIdentData();

            MzIdentMlReaderWriter.Write(new MzIdentMLType(identData), outputFilePath);
        }

        private void CreateMzidSettings(SpectrumIdentificationProtocolObj settings)
        {
            settings.AdditionalSearchParams.Items.AddRange(new ParamBaseObj[]
            {
                new CVParamObj(CV.CVID.MS_parent_mass_type_mono),
                new CVParamObj(CV.CVID.MS_fragment_mass_type_mono),
                new UserParamObj() { Name = "TargetDecoyApproach", Value = (options.TargetDecoySearchMode == DatabaseSearchMode.Both).ToString()},
                new UserParamObj() { Name = "MinSequenceLength", Value = options.MinSequenceLength.ToString() },
                new UserParamObj() { Name = "MaxSequenceLength", Value = options.MaxSequenceLength.ToString() },
                new UserParamObj() { Name = "MaxNumNTermCleavages", Value = options.MaxNumNTermCleavages.ToString() },
                new UserParamObj() { Name = "MaxNumCTermCleavages", Value = options.MaxNumCTermCleavages.ToString() },
                new UserParamObj() { Name = "MinPrecursorIonCharge", Value = options.MinPrecursorIonCharge.ToString() },
                new UserParamObj() { Name = "MaxPrecursorIonCharge", Value = options.MaxPrecursorIonCharge.ToString() },
                new UserParamObj() { Name = "MinProductIonCharge", Value = options.MinProductIonCharge.ToString() },
                new UserParamObj() { Name = "MaxProductIonCharge", Value = options.MaxProductIonCharge.ToString() },
                new UserParamObj() { Name = "MinSequenceMass", Value = options.MinSequenceMass.ToString(CultureInfo.InvariantCulture) },
                new UserParamObj() { Name = "MaxSequenceMass", Value = options.MaxSequenceMass.ToString(CultureInfo.InvariantCulture) },
                new UserParamObj() { Name = "PrecursorIonTolerance", Value = options.PrecursorIonTolerance.ToString() },
                new UserParamObj() { Name = "ProductIonTolerance", Value = options.ProductIonTolerance.ToString() },
                new UserParamObj() { Name = "SearchMode", Value = options.InternalCleavageMode.ToString() },
                new UserParamObj() { Name = "MatchesPerSpectrumToKeepInMemory", Value = options.MatchesPerSpectrumToKeepInMemory.ToString() },
                new UserParamObj() { Name = "MatchesPerSpectrumToReport", Value = options.MatchesPerSpectrumToReport.ToString() },
                new UserParamObj() { Name = "TagBasedSearch", Value = options.TagBasedSearch.ToString() }
            });

            var activationMethod = options.ActivationMethod.ToString();
            if (options.ActivationMethod == ActivationMethod.Unknown)
            {
                activationMethod = $"Determined By Spectrum ({options.ActivationMethod})";
            }
            settings.AdditionalSearchParams.Items.Add(new UserParamObj() { Name = "SpecifiedActivationMethod", Value = activationMethod });

            // Add search type, if not a target-decoy search
            if (options.TargetDecoySearchMode != DatabaseSearchMode.Both)
            {
                settings.AdditionalSearchParams.Items.Add(new UserParamObj() { Name = "SearchType", Value = options.TargetDecoySearchMode.ToString() });
            }

            // Get the search modifications as they were passed into the AminoAcidSet constructor...
            foreach (var mod in options.AminoAcidSet.SearchModifications)
            {
                var modObj = new SearchModificationObj()
                {
                    FixedMod = mod.IsFixedModification,
                    MassDelta = (float)mod.Modification.Mass,
                    Residues = mod.TargetResidue.ToString(),
                };
                // "*" is used for wildcard residue N-Term or C-Term modifications. mzIdentML standard says that "." should be used instead.
                if (modObj.Residues.Contains("*"))
                {
                    modObj.Residues = modObj.Residues.Replace("*", ".");
                }
                // Really only using this for the modification name parsing for CVParams that exists with ModificationObj
                var tempMod = new ModificationObj(CV.CVID.MS_unknown_modification, mod.Modification.Name, 0, modObj.MassDelta);
                modObj.CVParams.Add(tempMod.CVParams.First());

                if (mod.Location != SequenceLocation.Everywhere)
                {
                    // specificity rules should be added
                    var rule = new SpecificityRulesListObj();
                    switch (mod.Location)
                    {
                        case SequenceLocation.PeptideNTerm:
                            rule.CVParams.Add(new CVParamObj(CV.CVID.MS_modification_specificity_peptide_N_term));
                            break;
                        case SequenceLocation.PeptideCTerm:
                            rule.CVParams.Add(new CVParamObj(CV.CVID.MS_modification_specificity_peptide_C_term));
                            break;
                        case SequenceLocation.ProteinNTerm:
                            rule.CVParams.Add(new CVParamObj(CV.CVID.MS_modification_specificity_protein_N_term));
                            break;
                        case SequenceLocation.ProteinCTerm:
                            rule.CVParams.Add(new CVParamObj(CV.CVID.MS_modification_specificity_protein_C_term));
                            break;
                        case SequenceLocation.Everywhere:
                            // not needed, the enclosing if should prevent ever hitting this
                            break;
                        default:
                            // Limited by enum...
                            break;
                    }
                    modObj.SpecificityRules.Add(rule);
                }

                settings.ModificationParams.Add(modObj);
            }

            // No enzyme for top-down search
            //settings.Enzymes.Enzymes.Add(new EnzymeObj());

            settings.ParentTolerances.AddRange(new CVParamObj[]
            {
                new CVParamObj(CV.CVID.MS_search_tolerance_plus_value, options.PrecursorIonTolerancePpm.ToString(CultureInfo.InvariantCulture)) { UnitCvid = CV.CVID.UO_parts_per_million },
                new CVParamObj(CV.CVID.MS_search_tolerance_minus_value, options.PrecursorIonTolerancePpm.ToString(CultureInfo.InvariantCulture)) { UnitCvid = CV.CVID.UO_parts_per_million },
            });
            settings.FragmentTolerances.AddRange(new CVParamObj[]
            {
                new CVParamObj(CV.CVID.MS_search_tolerance_plus_value, options.ProductIonTolerancePpm.ToString(CultureInfo.InvariantCulture)) { UnitCvid = CV.CVID.UO_parts_per_million },
                new CVParamObj(CV.CVID.MS_search_tolerance_minus_value, options.ProductIonTolerancePpm.ToString(CultureInfo.InvariantCulture)) { UnitCvid = CV.CVID.UO_parts_per_million },
            });
            settings.Threshold.Items.Add(new CVParamObj(CV.CVID.MS_no_threshold));
        }
    }
}
