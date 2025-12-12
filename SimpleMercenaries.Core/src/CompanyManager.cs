using RimWorld;
using Verse;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;

namespace SimpleMercenaries.Core
{
    public class CompanyManager : GameComponent
    {
        private List<Company> companies = null;

        public IEnumerable<Company> Companies
        {
            get
            {
                return companies;
            }
        }

        public CompanyManager(Game game) { }


        public override void StartedNewGame()
        {
            companies = DefDatabase<CompanyDef>.AllDefs
                .Select(d => new Company(d))
                .ToList();
        }

        public static IEnumerable<Company> GetAllCompanies()
        {
            return Current.Game.GetComponent<CompanyManager>().Companies;
        }

        public static Company GetCompanyByFaction(Faction faction)
        {
            return GetAllCompanies().Single(c => c.def.factionDef == faction.def);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look<Company>(ref companies, "companies", LookMode.Deep);
        }
    }
}