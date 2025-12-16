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
        private List<Company> companies = new List<Company>();

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
            foreach(CompanyDef companyDef in DefDatabase<CompanyDef>.AllDefs)
            {
                Company company = new Company(companyDef);
                companies.Add(company);
                
                //Hidden factions normally don't have leaders so we have to generate one manually
                if(company.Faction.leader == null)
                {
                    PawnGenerationRequest request = MercenaryGenerator.GetGenerationRequest(companyDef.factionDef.fixedLeaderKinds.RandomElement());
                    company.Faction.leader = MercenaryGenerator.Generate(request);
                }
            }
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