using System.Collections.Generic;
using InformedProteomics.Backend.Data.Enum;

namespace InformedProteomics.Backend.Data.Sequence
{
    public class ModificationParams
    {
        private List<ModificationInstance> _modList;

        public ModificationParams(int maxNumMods)
        {
            MaxNumMods = maxNumMods;
        }

        public int MaxNumMods
        {
            get;
            private set;
        }

        public int GetNumMassShifts()
        {
            throw new System.NotImplementedException();
        }

        public int[] GetModificationIndices(int prevIndex, AminoAcid aa, SequenceLocation? location)
        {
            throw new System.NotImplementedException();
        }

        public Composition GetModificationComposition(int modConfIndex)
        {
            throw new System.NotImplementedException();
        }

        public Composition[] GetModificationCompositions()
        {
            throw new System.NotImplementedException();
        }
    }
}