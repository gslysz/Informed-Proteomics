﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Enum;
using InformedProteomics.Backend.Data.Sequence;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.Utils;
using NUnit.Framework;

namespace InformedProteomics.Test
{
    [TestFixture]
    internal class TestUtils
    {
        [Test]
        public void TestSequenceGraph()
        {
            var phosPhoS = new SearchModification(Modification.Phosphorylation, 'S', SequenceLocation.Everywhere, false);
            var phosPhoT = new SearchModification(Modification.Phosphorylation, 'T', SequenceLocation.Everywhere, false);
            var phosPhoY = new SearchModification(Modification.Phosphorylation, 'Y', SequenceLocation.Everywhere, false);
            var oxM = new SearchModification(Modification.Oxidation, 'M', SequenceLocation.Everywhere, false);
            var fixCarbamidomethylC = new SearchModification(Modification.Carbamidomethylation, 'C', SequenceLocation.Everywhere, true);

            var searchModifications = new List<SearchModification> { phosPhoS, phosPhoT, phosPhoY, oxM, fixCarbamidomethylC };
            //var searchModifications = new List<SearchModification> { phosPhoT, fixCarbamidomethylC };
            const int numMaxModsPepPeptide = 2;

            var aaSet = new AminoAcidSet(searchModifications, numMaxModsPepPeptide);
            const string pepSeq = "PEPCTISDSMER";
            Console.WriteLine(aaSet.GetComposition(pepSeq));
            var graph = new SequenceGraph(aaSet, pepSeq);
            Console.WriteLine(graph.GetUnmodifiedSequenceComposition());
            Assert.AreEqual(graph.GetUnmodifiedSequenceComposition(), aaSet.GetComposition(pepSeq));

            Console.WriteLine("Sequence Compositions:");
            foreach (var composition in graph.GetSequenceCompositions())
            {
                Console.WriteLine(composition);
            }
        }

        [Test]
        public void TestCompositions()
        {
            var comp1 = Modification.Oxidation.Composition;
            var comp2 = new Composition(1, 2, 3, 4, 5,
                                        new[]
                                            {
                                                new Tuple<Atom, short>(Atom.Get("Au"), 2),
                                                new Tuple<Atom, short>(Atom.Get("P"), 2)
                                            });
            var comp3 = comp1 + comp2;

            Console.WriteLine(comp1);
            Console.WriteLine(comp2);
            Console.WriteLine(comp3);
        }

        [Test]
        public void TestModificationParams()
        {
            var modifications = new[] { Modification.Acetylation, Modification.Phosphorylation, Modification.Oxidation}; //, Modification.PyroGluQ };
            var modParams = new ModificationParams(modifications, 3);
            int numCombinations = modParams.NumModificationCombinations;
            for (int modCombIndex = 0; modCombIndex < numCombinations; modCombIndex++)
            {
                var modCombination = modParams.GetModificationCombination(modCombIndex);
                Console.WriteLine("{0}: {1} {2} {3}", modCombIndex, modCombination.ToString(), modCombination.Composition, modCombination.Composition.GetMass());
            }

            Console.WriteLine(modParams.GetModificationCombinationIndex(8, 0));
            Console.WriteLine(modParams.GetModificationCombinationIndex(19, 1));
        }

        [Test]
        public void TestGeneratingCombinations()
        {
            const int n = 5;
            const int k = 3;
            var combinations = SimpleMath.GetCombinationsWithRepetition(n, k);
            int count = 0;
            foreach (var combination in combinations)
            {
                ++count;
                Console.WriteLine((count-1)+": "+string.Join(",", combination));
            }
            Console.WriteLine("Count: " + count);
            Console.WriteLine("Count2: " + SimpleMath.NChooseK(n + k - 1, k));
        }

        [Test]
        public void TestCSharpSyntax()
        {
            const string path = @"C:\cygwin\home\kims336\Developments\InformedProteomics\InformedProteomics.Test\TestFiles\BSA_10ugml_IMS6_TOF03_CID_27Aug12_Frodo_Collision_Energy_Collapsed.UIMF";
            Console.WriteLine(Path.GetFileName(path));
            Console.WriteLine(Path.GetFileNameWithoutExtension(path));
            Console.WriteLine(Path.GetExtension(path));
            Console.WriteLine(Path.GetDirectoryName(path));

            var test = new Dictionary<int, int>();
            test[1] = 2;
            Console.WriteLine(test.Equals(null));
        }

        [Test]
        public void TestPeptide()
        {
            const string sequence = "PEPTIDE";
            var aaSet = new AminoAcidSet(Modification.Carbamidomethylation);
            var composition = aaSet.GetComposition(sequence);

            Console.WriteLine(composition);
            Console.WriteLine(composition.GetMass());
            Console.WriteLine(composition.GetNominalMass());
            // 2nd isotope
            Console.WriteLine(composition.GetIsotopeMass(0));
            Console.WriteLine(composition.GetIsotopeMass(1));
            Console.WriteLine(composition.GetIsotopeMass(2));
            Assert.AreEqual(composition.ToString(), "C34H51N7O14S0");

            foreach (var e in composition.GetApproximatedIsotopomerEnvelop())
                Console.WriteLine(e);
        }

        [Test]
        public void TestIonTypeGeneration()
        {
            var ionTypeFactory = new IonTypeFactory();
            int index = 0;
            foreach (var ionType in ionTypeFactory.GetAllKnownIonTypes())
            {
                Console.WriteLine(++index + ": " + ionType);
            }
            var yIon = ionTypeFactory.GetIonType("y2-H2O");
            Console.WriteLine(yIon.GetMz(0));
        }
	}
}
