using System.Linq;

// ReSharper disable UnusedMember.Global
namespace InformedProteomics.Backend.Data.Biology
{
    /// <summary>
    /// Enzyme, specifically digestion enzymes
    /// </summary>
    public sealed class Enzyme
    {
        // Ignore Spelling: Arg, Chymotrypsin, Glu, Lys

        private Enzyme(string name, string residues, bool isNTerm, string description, string psiCvAccession)
        {
            Name = name;
            Residues = residues.ToCharArray();
            IsNTerm = isNTerm;
            Description = description;
            PsiCvAccession = psiCvAccession;
        }

        /// <summary>
        /// Name of the enzyme
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Residues cleaved by the enzyme
        /// </summary>
        public char[] Residues { get; }

        /// <summary>
        /// If the enzyme affects the N-Terminus
        /// </summary>
        public bool IsNTerm { get; }

        /// <summary>
        /// Description of the enzyme
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// PSI-MS CV Accession for the enzyme
        /// </summary>
        public string PsiCvAccession { get; }

        /// <summary>
        /// Test if the enzyme will cleave at <paramref name="residue"/>
        /// </summary>
        /// <param name="residue"></param>
        /// <returns>True if this residue is a cleavage point</returns>
        public bool IsCleavable(char residue)
        {
            if (Residues.Length == 0)
            {
                return true;
            }

            return Residues.Any(r => r == residue);
        }

        /// <summary>
        /// Unspecific cleavage
        /// </summary>
        public static readonly Enzyme UnspecificCleavage = new("UnspecificCleavage", string.Empty, false, "unspecific cleavage", "MS:1001956");

        /// <summary>
        /// Trypsin enzyme
        /// </summary>
        public static readonly Enzyme Trypsin = new("Trypsin", "KR", false, "Trypsin", "MS:1001251");

        /// <summary>
        /// Chymotrypsin enzyme
        /// </summary>
        // ReSharper disable once StringLiteralTypo
        public static readonly Enzyme Chymotrypsin = new("Chymotrypsin", "FYWL", false, "Chymotrypsin", "MS:1001306");

        /// <summary>
        /// LysC enzyme
        /// </summary>
        public static readonly Enzyme LysC = new("LysC", "K", false, "Lys-C", "MS:1001309");

        /// <summary>
        /// LysN enzyme
        /// </summary>
        public static readonly Enzyme LysN = new("LysN", "K", true, "Lys-N", null);

        /// <summary>
        /// GluC enzyme
        /// </summary>
        public static readonly Enzyme GluC = new("GluC", "E", false, "Glu-C", "MS:1001917");

        /// <summary>
        /// ArgC enzyme
        /// </summary>
        public static readonly Enzyme ArgC = new("ArgC", "R", false, "Arg-C", "MS:1001303");

        /// <summary>
        /// AspN enzyme
        /// </summary>
        public static readonly Enzyme AspN = new("AspN", "D", true, "Asp-N", "MS:1001304");

        /// <summary>
        /// Alp enzyme
        /// </summary>
        public static readonly Enzyme Alp = new("aLP", string.Empty, false, "alphaLP", null);

        /// <summary>
        /// No cleavage enzyme
        /// </summary>
        public static readonly Enzyme NoCleavage = new("NoCleavage", string.Empty, false, "no cleavage", "MS:1001955");
    }
}
