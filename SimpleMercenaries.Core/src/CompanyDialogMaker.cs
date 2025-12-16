using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SimpleMercenaries.Core
{
    public static class CompanyDialogMaker
    {
        public static DiaNode DialogFor(Pawn negotiator, Faction faction)
        {
            Map map = negotiator.Map;
            Pawn pawn;
            string text;
            if (faction.leader != null)
            {
                pawn = faction.leader;
                text = faction.leader.Name.ToStringFull.Colorize(ColoredText.NameColor);
            }
            else
            {
                Log.Error($"Faction {faction} has no leader.");
                pawn = negotiator;
                text = faction.Name;
            }

            DiaNode root;
            if (faction.PlayerRelationKind == FactionRelationKind.Hostile)
            {
                string key = ((faction.def.permanentEnemy || !"FactionGreetingHostileAppreciative".CanTranslate()) ? ((!string.IsNullOrEmpty(faction.def.dialogFactionGreetingHostile)) ? faction.def.dialogFactionGreetingHostile : "FactionGreetingHostile") : ((!string.IsNullOrEmpty(faction.def.dialogFactionGreetingHostileAppreciative)) ? faction.def.dialogFactionGreetingHostileAppreciative : "FactionGreetingHostileAppreciative"));
                root = new DiaNode(key.Translate(text).AdjustedFor(pawn));
            }
            else if (faction.PlayerRelationKind == FactionRelationKind.Neutral)
            {
                string key2 = "FactionGreetingWary";
                if (!string.IsNullOrEmpty(faction.def.dialogFactionGreetingWary))
                {
                    key2 = faction.def.dialogFactionGreetingWary;
                }

                root = new DiaNode(key2.Translate(text, negotiator.LabelShort, negotiator.Named("NEGOTIATOR"), pawn.Named("LEADER")).AdjustedFor(pawn));
            }
            else
            {
                string key3 = "FactionGreetingWarm";
                if (!string.IsNullOrEmpty(faction.def.dialogFactionGreetingWarm))
                {
                    key3 = faction.def.dialogFactionGreetingWarm;
                }

                root = new DiaNode(key3.Translate(text, negotiator.LabelShort, negotiator.Named("NEGOTIATOR"), pawn.Named("LEADER")).AdjustedFor(pawn));
            }

            if (map != null && map.IsPlayerHome)
            {
                Company company = CompanyManager.GetCompanyByFaction(faction);

                AddAndDecorateOption(HireMercenariesOption(map, faction, negotiator), needsSocial: true);

                if (company.def.orbital)
                {
                    AddAndDecorateOption(RequestMilitaryAidOption(map, faction, negotiator), needsSocial: true);
                }
            }

            if (Prefs.DevMode)
            {
                foreach (DiaOption item3 in DebugOptions(faction, negotiator))
                {
                    AddAndDecorateOption(item3, needsSocial: false);
                }
            }

            AddAndDecorateOption(new DiaOption("(" + "Disconnect".Translate() + ")")
            {
                resolveTree = true
            }, needsSocial: false);

            return root;

            void AddAndDecorateOption(DiaOption opt, bool needsSocial)
            {
                if (opt != null)
                {
                    if (needsSocial && negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
                    {
                        opt.Disable("WorkTypeDisablesOption".Translate(SkillDefOf.Social.label));
                    }

                    root.options.Add(opt);
                }
            }
        }

        private static IEnumerable<DiaOption> DebugOptions(Faction faction, Pawn negotiator)
        {
            DiaOption diaOption = new DiaOption("(Debug) Goodwill +10");
            diaOption.action = delegate
            {
                faction.TryAffectGoodwillWith(Faction.OfPlayer, 10, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.DebugGoodwill);
            };
            diaOption.linkLateBind = () => DialogFor(negotiator, faction);
            yield return diaOption;
            DiaOption diaOption2 = new DiaOption("(Debug) Goodwill -10");
            diaOption2.action = delegate
            {
                faction.TryAffectGoodwillWith(Faction.OfPlayer, -10, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.DebugGoodwill);
            };
            diaOption2.linkLateBind = () => DialogFor(negotiator, faction);
            yield return diaOption2;
        }

        private static int AmountSendableSilver(Map map)
        {
            return (from t in TradeUtility.AllLaunchableThingsForTrade(map)
                    where t.def == ThingDefOf.Silver
                    select t).Sum((Thing t) => t.stackCount);
        }

        private static DiaOption HireMercenariesOption(Map map, Faction faction, Pawn negotiator)
        {
            Company company = CompanyManager.GetCompanyByFaction(faction);
            DiaOption diaOption = new DiaOption("Hire mercenaries");
            
            diaOption.action = delegate
            {
                Find.WindowStack.Add(new Dialog_HireMercenaries(negotiator, company));
                ResetToRoot(faction, negotiator);
            };
            
            return diaOption;
        }

        private static DiaOption RequestMilitaryAidOption(Map map, Faction faction, Pawn negotiator)
        {
            string text = "Request immediate military aid (cost: 2000 silver)";

            if (AmountSendableSilver(map) < 2000)
            {
                DiaOption diaOption = new DiaOption(text);
                diaOption.Disable("Not enough silver");
                return diaOption;
            }

            if (!faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
            {
                DiaOption diaOption2 = new DiaOption(text);
                diaOption2.Disable("BadTemperature".Translate());
                return diaOption2;
            }

            int num = faction.lastMilitaryAidRequestTick + 60000 - Find.TickManager.TicksGame;

            if (num > 0)
            {
                DiaOption diaOption3 = new DiaOption(text);
                diaOption3.Disable("WaitTime".Translate(num.ToStringTicksToPeriod()));
                return diaOption3;
            }

            DiaOption diaOption5 = new DiaOption(text);

            /*IEnumerable<Faction> source = (from x in map.attackTargetsCache.TargetsHostileToColony
                                        where GenHostility.IsActiveThreatToPlayer(x)
                                        select ((Thing)x).Faction into x
                                        where x != null && !x.HostileTo(faction)
                                        select x).Distinct();
            if (source.Any())
            {
                DiaNode diaNode = new DiaNode("MilitaryAidConfirmMutualEnemy".Translate(faction.Name, source.Select((Faction fa) => fa.Name).ToCommaList(useAnd: true)));
                DiaOption diaOption6 = new DiaOption("CallConfirm".Translate());
                diaOption6.action = delegate
                {
                    CallForAid(map, faction);
                };
                diaOption6.link = FightersSent(faction, negotiator);
                DiaOption diaOption7 = new DiaOption("CallCancel".Translate());
                diaOption7.linkLateBind = ResetToRoot(faction, negotiator);
                diaNode.options.Add(diaOption6);
                diaNode.options.Add(diaOption7);
                diaOption5.link = diaNode;
            }
            else
            {*/
                diaOption5.action = delegate
                {
                    CallForAid(map, faction);
                };
                diaOption5.link = FightersSent(faction, negotiator);
            //}

            return diaOption5;
        }

        public static DiaNode CantMakeItInTime(Faction faction, Pawn negotiator)
        {
            return new DiaNode("CantSendMilitaryAidInTime".Translate(faction.leader).CapitalizeFirst())
            {
                options = { OKToRoot(faction, negotiator) }
            };
        }

        public static DiaNode FightersSent(Faction faction, Pawn negotiator)
        {
            string key = "MilitaryAidSent";
            if (faction.def.dialogMilitaryAidSent != null && faction.def.dialogMilitaryAidSent != "")
            {
                key = faction.def.dialogMilitaryAidSent;
            }

            return new DiaNode(key.Translate(faction.leader).CapitalizeFirst())
            {
                options = { OKToRoot(faction, negotiator) }
            };
        }

        private static void CallForAid(Map map, Faction faction)
        {
            int amount = 2000;

            //"Pay" the money required
            foreach(Thing silver in TradeUtility.AllLaunchableThingsForTrade(map).Where(t => t.def == ThingDefOf.Silver))
            {
                if(amount >= silver.stackCount)
                {
                    amount -= silver.stackCount;
                    silver.Destroy();
                }
                else
                {
                    amount = 0;
                    silver.SplitOff(amount).Destroy();
                }

                if(amount <= 0)
                {
                    break;
                }
            }
            
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = map;
            incidentParms.faction = faction;
            incidentParms.raidArrivalModeForQuickMilitaryAid = true;
            incidentParms.points = DiplomacyTuning.RequestedMilitaryAidPointsRange.RandomInRange;
            faction.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
            IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
        }

        private static DiaOption OKToRoot(Faction faction, Pawn negotiator)
        {
            return new DiaOption("OK".Translate())
            {
                linkLateBind = ResetToRoot(faction, negotiator)
            };
        }

        public static Func<DiaNode> ResetToRoot(Faction faction, Pawn negotiator)
        {
            return () => DialogFor(negotiator, faction);
        }
    }
}